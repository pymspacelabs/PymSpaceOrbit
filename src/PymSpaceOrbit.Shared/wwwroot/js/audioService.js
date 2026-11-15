// PymSpaceOrbit Cross-Platform Audio Engine
// Supports .NET 11 MAUI Blazor Hybrid (Android, Windows WebView2, iOS, MacCatalyst) + Blazor Web / WASM + GitHub Pages

const STORAGE_KEY = 'PymSpaceOrbit.AudioSettings.v2';
const MAX_CONCURRENT_SFX = 12;
const AMBIENT_VOLUME_SCALE = 0.42;

const INITIAL_ROUTE_TRACKS = {
    '': { key: 'MainMenu', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_main_menu.mp3' },
    'menu': { key: 'MainMenu', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_main_menu.mp3' },
    'options': { key: 'Options', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_options.mp3' },
    'info': { key: 'InfoCardSets', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_info_cardsets.mp3' },
    'play': { key: 'GameplayBuild', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_gameplay_build.mp3' }
};

// Use a single global engine singleton on window so multiple ES module imports never conflict
const engine = (typeof window !== 'undefined' && window.__pymAudioEngine)
    ? window.__pymAudioEngine
    : {
        state: {
            initialized: false,
            masterVolume: 0.85,
            musicVolume: 0.70,
            sfxVolume: 0.85,
            isMuted: false,
            currentTrackKey: null,
            currentTrackUrl: null,
            bgmLoop: true,
            bgmPaused: false,
            bgmLoading: false,
            pendingBgmPlay: false,
            currentAmbientKey: null,
            currentAmbientUrl: null,
            ambientLoop: true,
            ambientLoading: false,
            pendingAmbientPlay: false,
            wasBgmPlayingBeforeHide: false,
            wasAmbientPlayingBeforeHide: false,
            unlockListenersAttached: false,
            visibilityListenerAttached: false,
            requestCounter: 0,
            ambientRequestCounter: 0
        },
        bgmPlayer: null,
        ambientPlayer: null,
        activeSfx: new Set(),
        blobUrlCache: new Map(),
        blobFetchPromises: new Map(),
        audioBufferCache: new Map(),
        audioCtx: null
    };

if (typeof window !== 'undefined') {
    window.__pymAudioEngine = engine;
}

const state = engine.state;
const activeSfx = engine.activeSfx;
const blobUrlCache = engine.blobUrlCache;
const blobFetchPromises = engine.blobFetchPromises;
const audioBufferCache = engine.audioBufferCache;

function clamp01(value, fallback) {
    const num = Number(value);
    if (!Number.isFinite(num)) return fallback;
    return Math.min(1, Math.max(0, num));
}

function isMauiWebView() {
    if (typeof window === 'undefined' || !window.location) return false;
    const host = (window.location.hostname || '').toLowerCase();
    const proto = (window.location.protocol || '').toLowerCase();
    return host === '0.0.0.0' || host === '0.0.0.1' || proto === 'app:' || proto === 'ms-appx-web:';
}

function resolveAssetUrl(relativePath) {
    if (!relativePath || typeof relativePath !== 'string') {
        return '';
    }
    if (/^(https?:|blob:|data:|app:)/i.test(relativePath)) {
        return relativePath;
    }
    const cleanPath = relativePath.replace(/^(\.\/|\/)+/, '');
    try {
        const baseUri = (typeof document !== 'undefined' && document.baseURI)
            ? document.baseURI
            : (typeof window !== 'undefined' ? window.location.href : '/');
        return new URL(cleanPath, baseUri).href;
    } catch {
        return cleanPath;
    }
}

function getOrCreateAudioContext() {
    if (engine.audioCtx) return engine.audioCtx;
    if (typeof window === 'undefined') return null;
    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) return null;
    try {
        engine.audioCtx = new AudioCtx();
    } catch {
        engine.audioCtx = null;
    }
    return engine.audioCtx;
}

function tryResumeAudioContext() {
    const ctx = getOrCreateAudioContext();
    if (ctx && ctx.state === 'suspended' && typeof ctx.resume === 'function') {
        try {
            ctx.resume().catch(() => {});
        } catch {
            // Ignore resume restriction until next gesture
        }
    }
}

function fetchBlobUrl(url) {
    if (!url) return Promise.resolve('');
    if (url.startsWith('blob:') || url.startsWith('data:')) {
        return Promise.resolve(url);
    }
    if (blobUrlCache.has(url)) {
        return Promise.resolve(blobUrlCache.get(url));
    }
    if (blobFetchPromises.has(url)) {
        return blobFetchPromises.get(url);
    }
    if (typeof fetch !== 'function' || typeof URL === 'undefined' || typeof URL.createObjectURL !== 'function') {
        return Promise.resolve(url);
    }

    const promise = fetch(url, { credentials: 'same-origin' })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            return response.arrayBuffer();
        })
        .then(buffer => {
            if (!buffer || buffer.byteLength === 0) {
                throw new Error('Empty audio buffer');
            }
            const blob = new Blob([buffer], { type: 'audio/mpeg' });
            const objectUrl = URL.createObjectURL(blob);
            blobUrlCache.set(url, objectUrl);
            blobFetchPromises.delete(url);

            // If small SFX (< 1 MB), also pre-decode into Web Audio API AudioBuffer for 0ms playback
            if (buffer.byteLength < 1048576 && !audioBufferCache.has(url)) {
                const ctx = getOrCreateAudioContext();
                if (ctx && typeof ctx.decodeAudioData === 'function') {
                    ctx.decodeAudioData(buffer.slice(0))
                        .then(decoded => audioBufferCache.set(url, decoded))
                        .catch(() => {});
                }
            }

            return objectUrl;
        })
        .catch(() => {
            blobFetchPromises.delete(url);
            return url;
        });

    blobFetchPromises.set(url, promise);
    return promise;
}

function getEffectiveMusicVolume() {
    if (state.isMuted) return 0;
    return clamp01(state.masterVolume * state.musicVolume, 0);
}

function getEffectiveSfxVolume() {
    if (state.isMuted) return 0;
    return clamp01(state.masterVolume * state.sfxVolume, 0);
}

function getEffectiveAmbientVolume() {
    if (state.isMuted) return 0;
    return clamp01(state.masterVolume * state.sfxVolume * AMBIENT_VOLUME_SCALE, 0);
}

function loadPersistedSettings() {
    try {
        if (typeof window === 'undefined' || !window.localStorage) return;
        const raw = window.localStorage.getItem(STORAGE_KEY);
        if (!raw) return;
        const parsed = JSON.parse(raw);
        if (!parsed || typeof parsed !== 'object') return;

        state.masterVolume = clamp01(parsed.masterVolume, state.masterVolume);
        state.musicVolume = clamp01(parsed.musicVolume, state.musicVolume);
        state.sfxVolume = clamp01(parsed.sfxVolume, state.sfxVolume);
        state.isMuted = Boolean(parsed.isMuted);
    } catch {
        // Ignore storage read errors in restricted environments
    }
}

function savePersistedSettings() {
    try {
        if (typeof window === 'undefined' || !window.localStorage) return;
        const payload = JSON.stringify({
            masterVolume: state.masterVolume,
            musicVolume: state.musicVolume,
            sfxVolume: state.sfxVolume,
            isMuted: state.isMuted
        });
        window.localStorage.setItem(STORAGE_KEY, payload);
    } catch {
        // Ignore storage write errors in restricted environments
    }
}

function configureDomAudioElement(el, id) {
    el.id = id;
    el.preload = 'auto';
    el.loop = true;
    el.playsInline = true;
    el.setAttribute('playsinline', '');
    el.setAttribute('webkit-playsinline', '');
    el.style.display = 'none';
    el.setAttribute('aria-hidden', 'true');

    if (typeof document !== 'undefined' && document.body && !el.parentElement) {
        try {
            document.body.appendChild(el);
        } catch {
            // Ignore DOM append error if body not ready
        }
    }
}

function ensurePlayers() {
    if (typeof Audio === 'undefined' && typeof document === 'undefined') return false;

    if (!engine.bgmPlayer) {
        engine.bgmPlayer = (typeof document !== 'undefined' && document.getElementById('pym-bgm-player'))
            || (typeof document !== 'undefined' ? document.createElement('audio') : new Audio());
        configureDomAudioElement(engine.bgmPlayer, 'pym-bgm-player');
        engine.bgmPlayer.volume = getEffectiveMusicVolume();
        engine.bgmPlayer.muted = state.isMuted;
    } else if (typeof document !== 'undefined' && document.body && !engine.bgmPlayer.parentElement) {
        configureDomAudioElement(engine.bgmPlayer, 'pym-bgm-player');
    }

    if (!engine.ambientPlayer) {
        engine.ambientPlayer = (typeof document !== 'undefined' && document.getElementById('pym-ambient-player'))
            || (typeof document !== 'undefined' ? document.createElement('audio') : new Audio());
        configureDomAudioElement(engine.ambientPlayer, 'pym-ambient-player');
        engine.ambientPlayer.volume = getEffectiveAmbientVolume();
        engine.ambientPlayer.muted = state.isMuted;
    } else if (typeof document !== 'undefined' && document.body && !engine.ambientPlayer.parentElement) {
        configureDomAudioElement(engine.ambientPlayer, 'pym-ambient-player');
    }

    return true;
}

function applyVolumesToPlayers() {
    ensurePlayers();

    if (engine.bgmPlayer) {
        engine.bgmPlayer.muted = state.isMuted;
        engine.bgmPlayer.volume = getEffectiveMusicVolume();
    }
    if (engine.ambientPlayer) {
        engine.ambientPlayer.muted = state.isMuted;
        engine.ambientPlayer.volume = getEffectiveAmbientVolume();
    }
    const sfxVol = getEffectiveSfxVolume();
    for (const entry of activeSfx) {
        try {
            if (entry.gainNode) {
                entry.gainNode.gain.value = sfxVol;
            }
            if (entry.audio) {
                entry.audio.muted = state.isMuted;
                entry.audio.volume = sfxVol;
            }
        } catch {
            // Ignore element state error
        }
    }
}

function ensureActiveTracksResumedIfUnmuted() {
    if (state.isMuted) return;

    tryResumeAudioContext();

    if (engine.bgmPlayer && state.currentTrackKey && !state.bgmPaused && !state.bgmLoading && engine.bgmPlayer.paused) {
        if (engine.bgmPlayer.src) {
            startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.currentTrackUrl, state.requestCounter);
        } else if (state.currentTrackUrl) {
            const key = state.currentTrackKey;
            const url = state.currentTrackUrl;
            const loop = state.bgmLoop;
            state.currentTrackKey = null;
            playMusic(key, url, loop);
        }
    }

    if (engine.ambientPlayer && state.currentAmbientKey && !state.ambientLoading && engine.ambientPlayer.paused) {
        if (engine.ambientPlayer.src) {
            startAmbientElementPlayback(engine.ambientPlayer, state.currentAmbientKey, state.currentAmbientUrl, state.ambientRequestCounter);
        } else if (state.currentAmbientUrl) {
            const key = state.currentAmbientKey;
            const url = state.currentAmbientUrl;
            const loop = state.ambientLoop;
            state.currentAmbientKey = null;
            playAmbient(key, url, loop);
        }
    }
}

function checkAndDetachUnlockListeners() {
    if (!state.pendingBgmPlay && !state.pendingAmbientPlay) {
        detachUnlockListeners();
    }
}

function handleUserInteractionUnlock() {
    tryResumeAudioContext();

    if (engine.bgmPlayer && state.currentTrackKey && !state.bgmPaused && !state.bgmLoading) {
        if (state.pendingBgmPlay || engine.bgmPlayer.paused) {
            engine.bgmPlayer.muted = state.isMuted;
            engine.bgmPlayer.volume = getEffectiveMusicVolume();
            try {
                const playPromise = engine.bgmPlayer.play();
                if (playPromise && typeof playPromise.then === 'function') {
                    playPromise
                        .then(() => {
                            state.pendingBgmPlay = false;
                            checkAndDetachUnlockListeners();
                        })
                        .catch(err => {
                            if (err && err.name === 'AbortError') return;
                            state.pendingBgmPlay = true;
                        });
                } else {
                    state.pendingBgmPlay = false;
                    checkAndDetachUnlockListeners();
                }
            } catch {
                state.pendingBgmPlay = true;
            }
        }
    }

    if (engine.ambientPlayer && state.currentAmbientKey && !state.ambientLoading) {
        if (state.pendingAmbientPlay || engine.ambientPlayer.paused) {
            engine.ambientPlayer.muted = state.isMuted;
            engine.ambientPlayer.volume = getEffectiveAmbientVolume();
            try {
                const ambPromise = engine.ambientPlayer.play();
                if (ambPromise && typeof ambPromise.then === 'function') {
                    ambPromise
                        .then(() => {
                            state.pendingAmbientPlay = false;
                            checkAndDetachUnlockListeners();
                        })
                        .catch(err => {
                            if (err && err.name === 'AbortError') return;
                            state.pendingAmbientPlay = true;
                        });
                } else {
                    state.pendingAmbientPlay = false;
                    checkAndDetachUnlockListeners();
                }
            } catch {
                state.pendingAmbientPlay = true;
            }
        }
    }

    checkAndDetachUnlockListeners();
}

function handlePassiveActivationCheck() {
    if (typeof navigator !== 'undefined' && navigator.userActivation) {
        if (navigator.userActivation.isActive || navigator.userActivation.hasBeenActive) {
            handleUserInteractionUnlock();
        }
    }
}

function attachUnlockListeners() {
    if (state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = true;
    const opts = { capture: true, passive: true };
    document.addEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.addEventListener('mousedown', handleUserInteractionUnlock, opts);
    document.addEventListener('mouseup', handleUserInteractionUnlock, opts);
    document.addEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.addEventListener('touchend', handleUserInteractionUnlock, opts);
    document.addEventListener('keydown', handleUserInteractionUnlock, opts);
    document.addEventListener('keyup', handleUserInteractionUnlock, opts);
    document.addEventListener('click', handleUserInteractionUnlock, opts);
    document.addEventListener('pointermove', handlePassiveActivationCheck, opts);
    document.addEventListener('mousemove', handlePassiveActivationCheck, opts);
    if (typeof window !== 'undefined') {
        window.addEventListener('focus', handlePassiveActivationCheck, opts);
    }
}

function detachUnlockListeners() {
    if (!state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = false;
    const opts = { capture: true, passive: true };
    document.removeEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.removeEventListener('mousedown', handleUserInteractionUnlock, opts);
    document.removeEventListener('mouseup', handleUserInteractionUnlock, opts);
    document.removeEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.removeEventListener('touchend', handleUserInteractionUnlock, opts);
    document.removeEventListener('keydown', handleUserInteractionUnlock, opts);
    document.removeEventListener('keyup', handleUserInteractionUnlock, opts);
    document.removeEventListener('click', handleUserInteractionUnlock, opts);
    document.removeEventListener('pointermove', handlePassiveActivationCheck, opts);
    document.removeEventListener('mousemove', handlePassiveActivationCheck, opts);
    if (typeof window !== 'undefined') {
        window.removeEventListener('focus', handlePassiveActivationCheck, opts);
    }
}

function handleVisibilityChange() {
    if (typeof document === 'undefined') return;
    try {
        if (document.hidden) {
            state.wasBgmPlayingBeforeHide = Boolean(engine.bgmPlayer && !engine.bgmPlayer.paused && state.currentTrackKey && !state.bgmPaused);
            state.wasAmbientPlayingBeforeHide = Boolean(engine.ambientPlayer && !engine.ambientPlayer.paused && state.currentAmbientKey);
            if (state.wasBgmPlayingBeforeHide && engine.bgmPlayer) {
                engine.bgmPlayer.pause();
            }
            if (state.wasAmbientPlayingBeforeHide && engine.ambientPlayer) {
                engine.ambientPlayer.pause();
            }
        } else {
            if (state.wasBgmPlayingBeforeHide && engine.bgmPlayer && state.currentTrackKey && !state.bgmPaused) {
                state.wasBgmPlayingBeforeHide = false;
                startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.currentTrackUrl, state.requestCounter);
            }
            if (state.wasAmbientPlayingBeforeHide && engine.ambientPlayer && state.currentAmbientKey) {
                state.wasAmbientPlayingBeforeHide = false;
                startAmbientElementPlayback(engine.ambientPlayer, state.currentAmbientKey, state.currentAmbientUrl, state.ambientRequestCounter);
            }
        }
    } catch {
        // Ignore visibility transition errors
    }
}

function ensureVisibilityListener() {
    if (state.visibilityListenerAttached || typeof document === 'undefined') return;
    state.visibilityListenerAttached = true;
    document.addEventListener('visibilitychange', handleVisibilityChange);
}

function startBgmElementPlayback(player, expectedTrackKey, canonicalUrl, reqId) {
    if (!player || state.currentTrackKey !== expectedTrackKey || state.bgmPaused) return;
    if (reqId !== undefined && reqId !== state.requestCounter) return;

    player.muted = state.isMuted;
    player.volume = getEffectiveMusicVolume();

    try {
        const playPromise = player.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    if (state.currentTrackKey !== expectedTrackKey) return;
                    state.pendingBgmPlay = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(err => {
                    if (state.currentTrackKey !== expectedTrackKey) return;
                    if (reqId !== undefined && reqId !== state.requestCounter) return;

                    const errName = (err && err.name) ? err.name : '';

                    // Ignore AbortError caused by rapid track switching or concurrent load
                    if (errName === 'AbortError') {
                        return;
                    }

                    // If direct URL failed to decode in WebView, swap to Blob URL
                    if (errName === 'NotSupportedError' && canonicalUrl && !player.src.startsWith('blob:')) {
                        state.bgmLoading = true;
                        fetchBlobUrl(canonicalUrl).then(blobUrl => {
                            state.bgmLoading = false;
                            if (state.currentTrackKey === expectedTrackKey && reqId === state.requestCounter && blobUrl && blobUrl !== canonicalUrl) {
                                player.src = blobUrl;
                                player.loop = state.bgmLoop;
                                startBgmElementPlayback(player, expectedTrackKey, null, reqId);
                            }
                        });
                        return;
                    }

                    // Browser autoplay restriction (NotAllowedError): queue for first user interaction
                    state.pendingBgmPlay = true;
                    attachUnlockListeners();
                });
        }
    } catch {
        state.pendingBgmPlay = true;
        attachUnlockListeners();
    }
}

export function initialize() {
    if (!state.initialized) {
        loadPersistedSettings();
        ensurePlayers();
        ensureVisibilityListener();
        state.initialized = true;
    } else {
        ensurePlayers();
    }
    applyVolumesToPlayers();
    return {
        masterVolume: state.masterVolume,
        musicVolume: state.musicVolume,
        sfxVolume: state.sfxVolume,
        isMuted: state.isMuted
    };
}

export function playMusic(trackKey, relativePath, loop = true) {
    if (!state.initialized) {
        initialize();
    }
    if (!ensurePlayers()) return;

    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    const shouldLoop = loop !== false;
    const player = engine.bgmPlayer;

    // Never restart or abort the same track if it is already active, loading, or pending autoplay unlock
    if (state.currentTrackKey === trackKey && player) {
        player.loop = shouldLoop;
        state.bgmLoop = shouldLoop;
        player.muted = state.isMuted;
        player.volume = getEffectiveMusicVolume();

        if (!player.paused || state.bgmLoading) {
            return;
        }

        state.bgmPaused = false;
        if (player.src) {
            startBgmElementPlayback(player, trackKey, url, state.requestCounter);
        }
        return;
    }

    const reqId = ++state.requestCounter;

    // Stop any previously active track before switching
    try {
        player.pause();
        player.currentTime = 0;
    } catch {
        // Ignore reset error
    }

    state.currentTrackKey = trackKey;
    state.currentTrackUrl = url;
    state.bgmLoop = shouldLoop;
    state.bgmPaused = false;
    state.pendingBgmPlay = false;

    player.loop = shouldLoop;
    player.volume = getEffectiveMusicVolume();
    player.muted = state.isMuted;

    if (blobUrlCache.has(url)) {
        state.bgmLoading = false;
        player.src = blobUrlCache.get(url);
        startBgmElementPlayback(player, trackKey, url, reqId);
    } else if (isMauiWebView()) {
        state.bgmLoading = true;
        fetchBlobUrl(url).then(resolvedUrl => {
            if (state.currentTrackKey !== trackKey || reqId !== state.requestCounter || !engine.bgmPlayer) return;
            state.bgmLoading = false;
            engine.bgmPlayer.src = resolvedUrl || url;
            engine.bgmPlayer.loop = state.bgmLoop;
            startBgmElementPlayback(engine.bgmPlayer, trackKey, null, reqId);
        });
    } else {
        state.bgmLoading = false;
        player.src = url;
        startBgmElementPlayback(player, trackKey, url, reqId);
        fetchBlobUrl(url);
    }
}

export function stopMusic() {
    ++state.requestCounter;
    state.bgmLoading = false;
    state.pendingBgmPlay = false;
    state.bgmPaused = false;
    state.currentTrackKey = null;
    state.currentTrackUrl = null;

    if (!engine.bgmPlayer) return;
    try {
        engine.bgmPlayer.pause();
        engine.bgmPlayer.currentTime = 0;
        engine.bgmPlayer.removeAttribute('src');
        engine.bgmPlayer.load();
    } catch {
        // Ignore stop errors
    }
}

export function pauseMusic() {
    if (!engine.bgmPlayer) return;
    state.bgmPaused = true;
    state.pendingBgmPlay = false;
    try {
        if (!engine.bgmPlayer.paused) {
            engine.bgmPlayer.pause();
        }
    } catch {
        // Ignore pause errors
    }
}

export function resumeMusic() {
    if (!engine.bgmPlayer || !state.currentTrackKey) return;
    state.bgmPaused = false;
    engine.bgmPlayer.muted = state.isMuted;
    engine.bgmPlayer.volume = getEffectiveMusicVolume();

    if (!engine.bgmPlayer.paused) return;

    startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.currentTrackUrl, state.requestCounter);
}

export function playSoundEffect(effectKey, relativePath) {
    if (!state.initialized) {
        initialize();
    }
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    // Any SFX trigger coincides with a user interaction; unlock pending BGM/Ambient & AudioContext
    tryResumeAudioContext();
    if (state.pendingBgmPlay || state.pendingAmbientPlay || (engine.bgmPlayer && engine.bgmPlayer.paused && state.currentTrackKey && !state.bgmPaused)) {
        handleUserInteractionUnlock();
    }

    if (state.isMuted) return;

    // Enforce pool limit without interrupting BGM or newer SFX
    if (activeSfx.size >= MAX_CONCURRENT_SFX) {
        const oldest = activeSfx.values().next().value;
        if (oldest) {
            activeSfx.delete(oldest);
            try {
                if (oldest.sourceNode) {
                    oldest.sourceNode.stop();
                }
                if (oldest.audio) {
                    oldest.audio.pause();
                    oldest.audio.removeAttribute('src');
                }
            } catch {
                // Ignore cleanup error
            }
        }
    }

    // Fast path: if decoded in Web Audio API AudioBuffer cache and AudioContext is running, play with 0ms latency
    const ctx = getOrCreateAudioContext();
    if (ctx && ctx.state === 'running' && audioBufferCache.has(url)) {
        try {
            const sourceNode = ctx.createBufferSource();
            const gainNode = ctx.createGain();
            sourceNode.buffer = audioBufferCache.get(url);
            gainNode.gain.value = getEffectiveSfxVolume();
            sourceNode.connect(gainNode);
            gainNode.connect(ctx.destination);

            const entry = { key: effectKey, sourceNode, gainNode, audio: null };
            activeSfx.add(entry);
            sourceNode.onended = () => activeSfx.delete(entry);
            sourceNode.start(0);
            return;
        } catch {
            // Fall through to HTMLAudioElement
        }
    }

    const playResolvedSfx = (playableUrl) => {
        if (typeof Audio === 'undefined') return;
        try {
            const sfxAudio = new Audio(playableUrl);
            sfxAudio.preload = 'auto';
            sfxAudio.loop = false;
            sfxAudio.playsInline = true;
            sfxAudio.muted = state.isMuted;
            sfxAudio.volume = getEffectiveSfxVolume();

            const entry = { key: effectKey, audio: sfxAudio, sourceNode: null, gainNode: null };
            activeSfx.add(entry);

            const cleanup = () => {
                activeSfx.delete(entry);
            };

            sfxAudio.addEventListener('ended', cleanup, { once: true });
            sfxAudio.addEventListener('error', cleanup, { once: true });

            const playPromise = sfxAudio.play();
            if (playPromise && typeof playPromise.catch === 'function') {
                playPromise.catch(() => {
                    cleanup();
                    attachUnlockListeners();
                });
            }
        } catch {
            // Audio failures must never prevent gameplay
        }
    };

    if (blobUrlCache.has(url)) {
        playResolvedSfx(blobUrlCache.get(url));
    } else if (isMauiWebView()) {
        fetchBlobUrl(url).then(resolvedUrl => playResolvedSfx(resolvedUrl || url));
    } else {
        playResolvedSfx(url);
        fetchBlobUrl(url);
    }
}

export function stopSoundEffect(effectKey) {
    for (const entry of Array.from(activeSfx)) {
        if (entry.key === effectKey) {
            activeSfx.delete(entry);
            try {
                if (entry.sourceNode) {
                    entry.sourceNode.stop();
                }
                if (entry.audio) {
                    entry.audio.pause();
                    entry.audio.currentTime = 0;
                    entry.audio.removeAttribute('src');
                }
            } catch {
                // Ignore stop error
            }
        }
    }
}

export function stopAllSoundEffects() {
    for (const entry of Array.from(activeSfx)) {
        activeSfx.delete(entry);
        try {
            if (entry.sourceNode) {
                entry.sourceNode.stop();
            }
            if (entry.audio) {
                entry.audio.pause();
                entry.audio.currentTime = 0;
                entry.audio.removeAttribute('src');
            }
        } catch {
            // Ignore stop error
        }
    }
}

function startAmbientElementPlayback(player, expectedAmbientKey, canonicalUrl, reqId) {
    if (!player || state.currentAmbientKey !== expectedAmbientKey) return;
    if (reqId !== undefined && reqId !== state.ambientRequestCounter) return;

    player.muted = state.isMuted;
    player.volume = getEffectiveAmbientVolume();

    try {
        const playPromise = player.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    if (state.currentAmbientKey !== expectedAmbientKey) return;
                    state.pendingAmbientPlay = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(err => {
                    if (state.currentAmbientKey !== expectedAmbientKey) return;
                    if (reqId !== undefined && reqId !== state.ambientRequestCounter) return;

                    const errName = (err && err.name) ? err.name : '';
                    if (errName === 'AbortError') {
                        return;
                    }
                    if (errName === 'NotSupportedError' && canonicalUrl && !player.src.startsWith('blob:')) {
                        state.ambientLoading = true;
                        fetchBlobUrl(canonicalUrl).then(blobUrl => {
                            state.ambientLoading = false;
                            if (state.currentAmbientKey === expectedAmbientKey && reqId === state.ambientRequestCounter && blobUrl && blobUrl !== canonicalUrl) {
                                player.src = blobUrl;
                                player.loop = state.ambientLoop;
                                startAmbientElementPlayback(player, expectedAmbientKey, null, reqId);
                            }
                        });
                        return;
                    }
                    state.pendingAmbientPlay = true;
                    attachUnlockListeners();
                });
        }
    } catch {
        state.pendingAmbientPlay = true;
        attachUnlockListeners();
    }
}

export function playAmbient(ambientKey, relativePath, loop = true) {
    if (!state.initialized) {
        initialize();
    }
    if (!ensurePlayers()) return;
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    const shouldLoop = loop !== false;
    const player = engine.ambientPlayer;

    if (state.currentAmbientKey === ambientKey && player) {
        player.loop = shouldLoop;
        state.ambientLoop = shouldLoop;
        player.muted = state.isMuted;
        player.volume = getEffectiveAmbientVolume();

        if (!player.paused || state.ambientLoading) {
            return;
        }
        if (player.src) {
            startAmbientElementPlayback(player, ambientKey, url, state.ambientRequestCounter);
        }
        return;
    }

    const reqId = ++state.ambientRequestCounter;

    try {
        player.pause();
        player.currentTime = 0;
    } catch {
        // Ignore reset error
    }

    state.currentAmbientKey = ambientKey;
    state.currentAmbientUrl = url;
    state.ambientLoop = shouldLoop;
    state.pendingAmbientPlay = false;

    player.loop = shouldLoop;
    player.volume = getEffectiveAmbientVolume();
    player.muted = state.isMuted;

    if (blobUrlCache.has(url)) {
        state.ambientLoading = false;
        player.src = blobUrlCache.get(url);
        startAmbientElementPlayback(player, ambientKey, url, reqId);
    } else if (isMauiWebView()) {
        state.ambientLoading = true;
        fetchBlobUrl(url).then(resolvedUrl => {
            if (state.currentAmbientKey !== ambientKey || reqId !== state.ambientRequestCounter || !engine.ambientPlayer) return;
            state.ambientLoading = false;
            engine.ambientPlayer.src = resolvedUrl || url;
            engine.ambientPlayer.loop = state.ambientLoop;
            startAmbientElementPlayback(engine.ambientPlayer, ambientKey, null, reqId);
        });
    } else {
        state.ambientLoading = false;
        player.src = url;
        startAmbientElementPlayback(player, ambientKey, url, reqId);
        fetchBlobUrl(url);
    }
}

export function stopAmbient() {
    ++state.ambientRequestCounter;
    state.ambientLoading = false;
    state.pendingAmbientPlay = false;
    state.currentAmbientKey = null;
    state.currentAmbientUrl = null;

    if (!engine.ambientPlayer) return;
    try {
        engine.ambientPlayer.pause();
        engine.ambientPlayer.currentTime = 0;
        engine.ambientPlayer.removeAttribute('src');
        engine.ambientPlayer.load();
    } catch {
        // Ignore stop error
    }
}

export function setMasterVolume(volume) {
    state.masterVolume = clamp01(volume, state.masterVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
    ensureActiveTracksResumedIfUnmuted();
}

export function setMusicVolume(volume) {
    state.musicVolume = clamp01(volume, state.musicVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
    ensureActiveTracksResumedIfUnmuted();
}

export function setSoundEffectsVolume(volume) {
    state.sfxVolume = clamp01(volume, state.sfxVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
    ensureActiveTracksResumedIfUnmuted();
}

export function setMuted(muted) {
    state.isMuted = Boolean(muted);
    applyVolumesToPlayers();
    savePersistedSettings();
    if (!state.isMuted) {
        ensureActiveTracksResumedIfUnmuted();
    }
}

export function disposeAudio() {
    detachUnlockListeners();
    if (state.visibilityListenerAttached && typeof document !== 'undefined') {
        document.removeEventListener('visibilitychange', handleVisibilityChange);
        state.visibilityListenerAttached = false;
    }
    stopMusic();
    stopAmbient();
    stopAllSoundEffects();
}

function autoStartInitialScreenMusic() {
    if (typeof window === 'undefined' || !window.location) return;
    initialize();

    // Pre-cache and decode UI button tap SFX immediately
    fetchBlobUrl(resolveAssetUrl('_content/PymSpaceOrbit.Shared/audio/sfx/sfx_ui_button_tap.mp3'));

    if (state.currentTrackKey) return;

    const rawPath = (window.location.pathname || '').replace(/^\/+|\/+$/g, '');
    const segments = rawPath ? rawPath.split('/') : [];
    const lastSegment = segments.length > 0 ? segments[segments.length - 1].toLowerCase() : '';

    const routeTrack = INITIAL_ROUTE_TRACKS[lastSegment] || INITIAL_ROUTE_TRACKS[''];
    playMusic(routeTrack.key, routeTrack.path, true);
}

if (typeof window !== 'undefined') {
    window.pymAudioService = {
        initialize,
        playMusic,
        stopMusic,
        pauseMusic,
        resumeMusic,
        playSoundEffect,
        stopSoundEffect,
        stopAllSoundEffects,
        playAmbient,
        stopAmbient,
        setMasterVolume,
        setMusicVolume,
        setSoundEffectsVolume,
        setMuted,
        disposeAudio
    };

    if (typeof document !== 'undefined') {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', autoStartInitialScreenMusic, { once: true });
        } else {
            autoStartInitialScreenMusic();
        }
    }
}

// PymSpaceOrbit Cross-Platform Audio Engine
// Supports .NET 11 MAUI Blazor Hybrid (Android, Windows WebView2, iOS, MacCatalyst) + Blazor Web + GitHub Pages

const STORAGE_KEY = 'PymSpaceOrbit.AudioSettings.v1';
const MAX_CONCURRENT_SFX = 12;
const AMBIENT_VOLUME_SCALE = 0.42;

const INITIAL_ROUTE_TRACKS = {
    '': { key: 'MainMenu', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_main_menu.mp3' },
    'menu': { key: 'MainMenu', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_main_menu.mp3' },
    'options': { key: 'Options', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_options.mp3' },
    'info': { key: 'InfoCardSets', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_info_cardsets.mp3' },
    'play': { key: 'GameplayBuild', path: '_content/PymSpaceOrbit.Shared/audio/bgm/bgm_gameplay_build.mp3' }
};

const state = {
    initialized: false,
    masterVolume: 0.85,
    musicVolume: 0.70,
    sfxVolume: 0.85,
    isMuted: false,
    currentTrackKey: null,
    currentTrackUrl: null,
    bgmLoop: true,
    bgmPaused: false,
    pendingBgmPlay: false,
    autoMutedByBrowserPolicy: false,
    currentAmbientKey: null,
    currentAmbientUrl: null,
    ambientLoop: true,
    pendingAmbientPlay: false,
    autoMutedAmbientByPolicy: false,
    wasBgmPlayingBeforeHide: false,
    wasAmbientPlayingBeforeHide: false,
    unlockListenersAttached: false,
    visibilityListenerAttached: false
};

let bgmPlayer = null;
let ambientPlayer = null;
const activeSfx = new Set();
const blobUrlCache = new Map();
const blobFetchPromises = new Map();

function clamp01(value, fallback) {
    const num = Number(value);
    if (!Number.isFinite(num)) return fallback;
    return Math.min(1, Math.max(0, num));
}

function isMauiWebView() {
    if (typeof window === 'undefined' || !window.location) return false;
    const host = (window.location.hostname || '').toLowerCase();
    const proto = (window.location.protocol || '').toLowerCase();
    return host === '0.0.0.0' || proto === 'app:' || proto === 'ms-appx-web:';
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

function applyVolumesToPlayers() {
    if (bgmPlayer) {
        bgmPlayer.volume = getEffectiveMusicVolume();
        if (!state.autoMutedByBrowserPolicy) {
            bgmPlayer.muted = state.isMuted;
        }
    }
    if (ambientPlayer) {
        ambientPlayer.volume = getEffectiveAmbientVolume();
        if (!state.autoMutedAmbientByPolicy) {
            ambientPlayer.muted = state.isMuted;
        }
    }
    const sfxVol = getEffectiveSfxVolume();
    for (const entry of activeSfx) {
        try {
            entry.audio.volume = sfxVol;
            entry.audio.muted = state.isMuted;
        } catch {
            // Ignore element state error
        }
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

    if (!bgmPlayer) {
        bgmPlayer = (typeof document !== 'undefined' && document.getElementById('pym-bgm-player'))
            || (typeof document !== 'undefined' ? document.createElement('audio') : new Audio());
        configureDomAudioElement(bgmPlayer, 'pym-bgm-player');
        bgmPlayer.volume = getEffectiveMusicVolume();
    } else if (typeof document !== 'undefined' && document.body && !bgmPlayer.parentElement) {
        configureDomAudioElement(bgmPlayer, 'pym-bgm-player');
    }

    if (!ambientPlayer) {
        ambientPlayer = (typeof document !== 'undefined' && document.getElementById('pym-ambient-player'))
            || (typeof document !== 'undefined' ? document.createElement('audio') : new Audio());
        configureDomAudioElement(ambientPlayer, 'pym-ambient-player');
        ambientPlayer.volume = getEffectiveAmbientVolume();
    } else if (typeof document !== 'undefined' && document.body && !ambientPlayer.parentElement) {
        configureDomAudioElement(ambientPlayer, 'pym-ambient-player');
    }

    return true;
}

function checkAndDetachUnlockListeners() {
    if (!state.pendingBgmPlay && !state.pendingAmbientPlay && !state.autoMutedByBrowserPolicy && !state.autoMutedAmbientByPolicy) {
        detachUnlockListeners();
    }
}

function unlockBgmPlayback() {
    if (!bgmPlayer || !state.currentTrackKey || state.bgmPaused) return;
    if (!state.pendingBgmPlay && !state.autoMutedByBrowserPolicy && !bgmPlayer.paused) return;

    state.autoMutedByBrowserPolicy = false;
    bgmPlayer.muted = state.isMuted;
    bgmPlayer.volume = getEffectiveMusicVolume();

    try {
        const playPromise = bgmPlayer.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    state.pendingBgmPlay = false;
                    state.autoMutedByBrowserPolicy = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(() => {
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

function unlockAmbientPlayback() {
    if (!ambientPlayer || !state.currentAmbientKey) return;
    if (!state.pendingAmbientPlay && !state.autoMutedAmbientByPolicy && !ambientPlayer.paused) return;

    state.autoMutedAmbientByPolicy = false;
    ambientPlayer.muted = state.isMuted;
    ambientPlayer.volume = getEffectiveAmbientVolume();

    try {
        const ambPromise = ambientPlayer.play();
        if (ambPromise && typeof ambPromise.then === 'function') {
            ambPromise
                .then(() => {
                    state.pendingAmbientPlay = false;
                    state.autoMutedAmbientByPolicy = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(() => {
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

function handleUserInteractionUnlock() {
    unlockBgmPlayback();
    unlockAmbientPlayback();
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
            state.wasBgmPlayingBeforeHide = Boolean(bgmPlayer && !bgmPlayer.paused && state.currentTrackKey && !state.bgmPaused);
            state.wasAmbientPlayingBeforeHide = Boolean(ambientPlayer && !ambientPlayer.paused && state.currentAmbientKey);
            if (state.wasBgmPlayingBeforeHide && bgmPlayer) {
                bgmPlayer.pause();
            }
            if (state.wasAmbientPlayingBeforeHide && ambientPlayer) {
                ambientPlayer.pause();
            }
        } else {
            if (state.wasBgmPlayingBeforeHide && bgmPlayer && state.currentTrackKey && !state.bgmPaused) {
                state.wasBgmPlayingBeforeHide = false;
                bgmPlayer.volume = getEffectiveMusicVolume();
                const p = bgmPlayer.play();
                if (p && typeof p.catch === 'function') {
                    p.catch(() => {
                        state.pendingBgmPlay = true;
                        attachUnlockListeners();
                    });
                }
            }
            if (state.wasAmbientPlayingBeforeHide && ambientPlayer && state.currentAmbientKey) {
                state.wasAmbientPlayingBeforeHide = false;
                ambientPlayer.volume = getEffectiveAmbientVolume();
                const p = ambientPlayer.play();
                if (p && typeof p.catch === 'function') {
                    p.catch(() => {
                        state.pendingAmbientPlay = true;
                        attachUnlockListeners();
                    });
                }
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

function startBgmElementPlayback(player, expectedTrackKey, canonicalUrl) {
    if (!player || state.currentTrackKey !== expectedTrackKey || state.bgmPaused) return;

    state.autoMutedByBrowserPolicy = false;
    player.muted = state.isMuted;
    player.volume = getEffectiveMusicVolume();

    try {
        const playPromise = player.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    state.pendingBgmPlay = false;
                    state.autoMutedByBrowserPolicy = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(err => {
                    if (state.currentTrackKey !== expectedTrackKey) return;
                    const errName = (err && err.name) ? err.name : '';

                    // If direct virtual-host URL failed to decode in MAUI WebView, swap to Blob URL
                    if (errName === 'NotSupportedError' && canonicalUrl && !player.src.startsWith('blob:')) {
                        fetchBlobUrl(canonicalUrl).then(blobUrl => {
                            if (state.currentTrackKey === expectedTrackKey && blobUrl && blobUrl !== canonicalUrl) {
                                player.src = blobUrl;
                                player.loop = state.bgmLoop;
                                startBgmElementPlayback(player, expectedTrackKey, null);
                            }
                        });
                        return;
                    }

                    // Browser autoplay restriction (NotAllowedError):
                    // Start muted autoplay so track is buffered & rolling, then unmute on first user activation
                    state.pendingBgmPlay = true;
                    attachUnlockListeners();

                    if (!state.isMuted && !isMauiWebView()) {
                        try {
                            state.autoMutedByBrowserPolicy = true;
                            player.muted = true;
                            const mutedPromise = player.play();
                            if (mutedPromise && typeof mutedPromise.catch === 'function') {
                                mutedPromise.catch(() => {
                                    state.autoMutedByBrowserPolicy = false;
                                    player.muted = state.isMuted;
                                });
                            }
                        } catch {
                            state.autoMutedByBrowserPolicy = false;
                            player.muted = state.isMuted;
                        }
                    }
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

    // Do not restart the same track if already playing or queued for autoplay
    if (state.currentTrackKey === trackKey && bgmPlayer) {
        bgmPlayer.loop = shouldLoop;
        state.bgmLoop = shouldLoop;
        bgmPlayer.volume = getEffectiveMusicVolume();

        if (!bgmPlayer.paused && !state.autoMutedByBrowserPolicy) {
            return;
        }
        if (state.pendingBgmPlay && !state.bgmPaused) {
            unlockBgmPlayback();
            if (state.pendingBgmPlay) {
                attachUnlockListeners();
            }
            return;
        }
        if (state.bgmPaused) {
            state.bgmPaused = false;
            startBgmElementPlayback(bgmPlayer, trackKey, url);
            return;
        }
    }

    // Stop any previously active track before switching
    try {
        bgmPlayer.pause();
        bgmPlayer.currentTime = 0;
    } catch {
        // Ignore reset error
    }

    state.currentTrackKey = trackKey;
    state.currentTrackUrl = url;
    state.bgmLoop = shouldLoop;
    state.bgmPaused = false;
    state.pendingBgmPlay = false;
    state.autoMutedByBrowserPolicy = false;

    bgmPlayer.loop = shouldLoop;
    bgmPlayer.volume = getEffectiveMusicVolume();
    bgmPlayer.muted = state.isMuted;

    // In MAUI BlazorWebView (0.0.0.0 / app://), load via Blob URL so OS MediaPlayer/AVPlayer never fails on virtual origin
    if (blobUrlCache.has(url)) {
        bgmPlayer.src = blobUrlCache.get(url);
        startBgmElementPlayback(bgmPlayer, trackKey, url);
    } else if (isMauiWebView()) {
        fetchBlobUrl(url).then(resolvedUrl => {
            if (state.currentTrackKey !== trackKey || !bgmPlayer) return;
            bgmPlayer.src = resolvedUrl || url;
            bgmPlayer.loop = state.bgmLoop;
            startBgmElementPlayback(bgmPlayer, trackKey, null);
        });
    } else {
        bgmPlayer.src = url;
        startBgmElementPlayback(bgmPlayer, trackKey, url);
        // Pre-cache Blob URL in background for instant replay
        fetchBlobUrl(url);
    }
}

export function stopMusic() {
    state.pendingBgmPlay = false;
    state.autoMutedByBrowserPolicy = false;
    state.bgmPaused = false;
    state.currentTrackKey = null;
    state.currentTrackUrl = null;

    if (!bgmPlayer) return;
    try {
        bgmPlayer.pause();
        bgmPlayer.currentTime = 0;
        bgmPlayer.removeAttribute('src');
        bgmPlayer.load();
    } catch {
        // Ignore stop errors
    }
}

export function pauseMusic() {
    if (!bgmPlayer) return;
    state.bgmPaused = true;
    state.pendingBgmPlay = false;
    try {
        if (!bgmPlayer.paused) {
            bgmPlayer.pause();
        }
    } catch {
        // Ignore pause errors
    }
}

export function resumeMusic() {
    if (!bgmPlayer || !state.currentTrackKey) return;
    state.bgmPaused = false;
    bgmPlayer.volume = getEffectiveMusicVolume();

    if (!bgmPlayer.paused && !state.autoMutedByBrowserPolicy) return;

    startBgmElementPlayback(bgmPlayer, state.currentTrackKey, state.currentTrackUrl);
}

export function playSoundEffect(effectKey, relativePath) {
    if (!state.initialized) {
        initialize();
    }
    if (typeof Audio === 'undefined') return;
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    // Any SFX trigger often coincides with a user click; unlock pending BGM/Ambient too
    if (state.pendingBgmPlay || state.autoMutedByBrowserPolicy || state.pendingAmbientPlay) {
        handleUserInteractionUnlock();
    }

    // Enforce pool limit without interrupting BGM or newer SFX
    if (activeSfx.size >= MAX_CONCURRENT_SFX) {
        const oldest = activeSfx.values().next().value;
        if (oldest) {
            activeSfx.delete(oldest);
            try {
                oldest.audio.pause();
                oldest.audio.removeAttribute('src');
            } catch {
                // Ignore cleanup error
            }
        }
    }

    const playResolvedSfx = (playableUrl) => {
        try {
            const sfxAudio = new Audio(playableUrl);
            sfxAudio.preload = 'auto';
            sfxAudio.loop = false;
            sfxAudio.playsInline = true;
            sfxAudio.muted = state.isMuted;
            sfxAudio.volume = getEffectiveSfxVolume();

            const entry = { key: effectKey, audio: sfxAudio };
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
                entry.audio.pause();
                entry.audio.currentTime = 0;
                entry.audio.removeAttribute('src');
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
            entry.audio.pause();
            entry.audio.currentTime = 0;
            entry.audio.removeAttribute('src');
        } catch {
            // Ignore stop error
        }
    }
}

function startAmbientElementPlayback(player, expectedAmbientKey, canonicalUrl) {
    if (!player || state.currentAmbientKey !== expectedAmbientKey) return;

    state.autoMutedAmbientByPolicy = false;
    player.muted = state.isMuted;
    player.volume = getEffectiveAmbientVolume();

    try {
        const playPromise = player.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    state.pendingAmbientPlay = false;
                    state.autoMutedAmbientByPolicy = false;
                    checkAndDetachUnlockListeners();
                })
                .catch(err => {
                    if (state.currentAmbientKey !== expectedAmbientKey) return;
                    const errName = (err && err.name) ? err.name : '';
                    if (errName === 'NotSupportedError' && canonicalUrl && !player.src.startsWith('blob:')) {
                        fetchBlobUrl(canonicalUrl).then(blobUrl => {
                            if (state.currentAmbientKey === expectedAmbientKey && blobUrl && blobUrl !== canonicalUrl) {
                                player.src = blobUrl;
                                player.loop = state.ambientLoop;
                                startAmbientElementPlayback(player, expectedAmbientKey, null);
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

    if (state.currentAmbientKey === ambientKey && ambientPlayer) {
        ambientPlayer.loop = shouldLoop;
        state.ambientLoop = shouldLoop;
        ambientPlayer.volume = getEffectiveAmbientVolume();
        if (!ambientPlayer.paused && !state.autoMutedAmbientByPolicy) {
            return;
        }
        if (state.pendingAmbientPlay) {
            unlockAmbientPlayback();
            if (state.pendingAmbientPlay) {
                attachUnlockListeners();
            }
            return;
        }
    }

    try {
        ambientPlayer.pause();
        ambientPlayer.currentTime = 0;
    } catch {
        // Ignore reset error
    }

    state.currentAmbientKey = ambientKey;
    state.currentAmbientUrl = url;
    state.ambientLoop = shouldLoop;
    state.pendingAmbientPlay = false;
    state.autoMutedAmbientByPolicy = false;

    ambientPlayer.loop = shouldLoop;
    ambientPlayer.volume = getEffectiveAmbientVolume();
    ambientPlayer.muted = state.isMuted;

    if (blobUrlCache.has(url)) {
        ambientPlayer.src = blobUrlCache.get(url);
        startAmbientElementPlayback(ambientPlayer, ambientKey, url);
    } else if (isMauiWebView()) {
        fetchBlobUrl(url).then(resolvedUrl => {
            if (state.currentAmbientKey !== ambientKey || !ambientPlayer) return;
            ambientPlayer.src = resolvedUrl || url;
            ambientPlayer.loop = state.ambientLoop;
            startAmbientElementPlayback(ambientPlayer, ambientKey, null);
        });
    } else {
        ambientPlayer.src = url;
        startAmbientElementPlayback(ambientPlayer, ambientKey, url);
        fetchBlobUrl(url);
    }
}

export function stopAmbient() {
    state.pendingAmbientPlay = false;
    state.autoMutedAmbientByPolicy = false;
    state.currentAmbientKey = null;
    state.currentAmbientUrl = null;

    if (!ambientPlayer) return;
    try {
        ambientPlayer.pause();
        ambientPlayer.currentTime = 0;
        ambientPlayer.removeAttribute('src');
        ambientPlayer.load();
    } catch {
        // Ignore stop error
    }
}

export function setMasterVolume(volume) {
    state.masterVolume = clamp01(volume, state.masterVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
}

export function setMusicVolume(volume) {
    state.musicVolume = clamp01(volume, state.musicVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
}

export function setSoundEffectsVolume(volume) {
    state.sfxVolume = clamp01(volume, state.sfxVolume);
    applyVolumesToPlayers();
    savePersistedSettings();
}

export function setMuted(muted) {
    state.isMuted = Boolean(muted);
    if (!state.isMuted) {
        state.autoMutedByBrowserPolicy = false;
        state.autoMutedAmbientByPolicy = false;
    }
    applyVolumesToPlayers();
    savePersistedSettings();
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

    // Pre-cache UI tap SFX immediately
    fetchBlobUrl(resolveAssetUrl('_content/PymSpaceOrbit.Shared/audio/sfx/sfx_ui_button_tap.mp3'));

    if (state.currentTrackKey) return;

    const rawPath = (window.location.pathname || '').replace(/^\/+|\/+$/g, '');
    const segments = rawPath ? rawPath.split('/') : [];
    const lastSegment = segments.length > 0 ? segments[segments.length - 1].toLowerCase() : '';

    const routeTrack = INITIAL_ROUTE_TRACKS[lastSegment] || INITIAL_ROUTE_TRACKS[''];
    playMusic(routeTrack.key, routeTrack.path, true);
}

// Expose globally on window for direct synchronous/single-hop Blazor JS interop
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

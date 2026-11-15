// PymSpaceOrbit Web Audio Engine
// Uses audio assets hosted directly in PymSpaceOrbit.Web/wwwroot/audio/

const STORAGE_KEY = 'PymSpaceOrbit.AudioSettings.v2';
const MAX_CONCURRENT_SFX = 12;
const AMBIENT_VOLUME_SCALE = 0.42;

const INITIAL_ROUTE_TRACKS = {
    '': { key: 'MainMenu', path: 'audio/bgm/bgm_main_menu.mp3' },
    'menu': { key: 'MainMenu', path: 'audio/bgm/bgm_main_menu.mp3' },
    'options': { key: 'Options', path: 'audio/bgm/bgm_options.mp3' },
    'info': { key: 'InfoCardSets', path: 'audio/bgm/bgm_info_cardsets.mp3' },
    'play': { key: 'GameplayBuild', path: 'audio/bgm/bgm_gameplay_build.mp3' }
};

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
            pendingBgmPlay: false,
            currentAmbientKey: null,
            currentAmbientUrl: null,
            ambientLoop: true,
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
        activeSfx: new Set()
    };

if (typeof window !== 'undefined') {
    window.__pymAudioEngine = engine;
}

const state = engine.state;
const activeSfx = engine.activeSfx;

function clamp01(value, fallback) {
    const num = Number(value);
    if (!Number.isFinite(num)) return fallback;
    return Math.min(1, Math.max(0, num));
}

function resolveAssetUrl(relativePath) {
    if (!relativePath || typeof relativePath !== 'string') {
        return '';
    }
    if (/^(https?:|blob:|data:)/i.test(relativePath)) {
        return relativePath;
    }
    const cleanPath = relativePath.replace(/^(\.\/|\/)+/, '');
    try {
        const baseUri = (typeof document !== 'undefined' && document.baseURI)
            ? document.baseURI
            : (typeof window !== 'undefined' ? window.location.href : '/');
        return new URL(cleanPath, baseUri).href;
    } catch {
        return '/' + cleanPath;
    }
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
        // Ignore storage read errors
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
        // Ignore storage write errors
    }
}

function ensurePlayers() {
    if (typeof Audio === 'undefined') return false;

    if (!engine.bgmPlayer) {
        engine.bgmPlayer = new Audio();
        engine.bgmPlayer.preload = 'auto';
        engine.bgmPlayer.loop = true;
        engine.bgmPlayer.volume = getEffectiveMusicVolume();
        engine.bgmPlayer.muted = state.isMuted;
    }

    if (!engine.ambientPlayer) {
        engine.ambientPlayer = new Audio();
        engine.ambientPlayer.preload = 'auto';
        engine.ambientPlayer.loop = true;
        engine.ambientPlayer.volume = getEffectiveAmbientVolume();
        engine.ambientPlayer.muted = state.isMuted;
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

    if (engine.bgmPlayer && state.currentTrackKey && !state.bgmPaused && engine.bgmPlayer.paused) {
        if (engine.bgmPlayer.src) {
            startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.requestCounter);
        } else if (state.currentTrackUrl) {
            const key = state.currentTrackKey;
            const url = state.currentTrackUrl;
            const loop = state.bgmLoop;
            state.currentTrackKey = null;
            playMusic(key, url, loop);
        }
    }

    if (engine.ambientPlayer && state.currentAmbientKey && engine.ambientPlayer.paused) {
        if (engine.ambientPlayer.src) {
            startAmbientElementPlayback(engine.ambientPlayer, state.currentAmbientKey, state.ambientRequestCounter);
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
    if (engine.bgmPlayer && state.currentTrackKey && !state.bgmPaused) {
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

    if (engine.ambientPlayer && state.currentAmbientKey) {
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

function attachUnlockListeners() {
    if (state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = true;
    const opts = { capture: true, passive: true };
    document.addEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.addEventListener('mousedown', handleUserInteractionUnlock, opts);
    document.addEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.addEventListener('keydown', handleUserInteractionUnlock, opts);
    document.addEventListener('click', handleUserInteractionUnlock, opts);
}

function detachUnlockListeners() {
    if (!state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = false;
    const opts = { capture: true, passive: true };
    document.removeEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.removeEventListener('mousedown', handleUserInteractionUnlock, opts);
    document.removeEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.removeEventListener('keydown', handleUserInteractionUnlock, opts);
    document.removeEventListener('click', handleUserInteractionUnlock, opts);
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
                startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.requestCounter);
            }
            if (state.wasAmbientPlayingBeforeHide && engine.ambientPlayer && state.currentAmbientKey) {
                state.wasAmbientPlayingBeforeHide = false;
                startAmbientElementPlayback(engine.ambientPlayer, state.currentAmbientKey, state.ambientRequestCounter);
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

function startBgmElementPlayback(player, expectedTrackKey, reqId) {
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
                    if (err && err.name === 'AbortError') return;

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

    if (state.currentTrackKey === trackKey && player) {
        player.loop = shouldLoop;
        state.bgmLoop = shouldLoop;
        player.muted = state.isMuted;
        player.volume = getEffectiveMusicVolume();

        if (!player.paused) {
            return;
        }

        state.bgmPaused = false;
        if (!player.src) {
            player.src = url;
        }
        startBgmElementPlayback(player, trackKey, state.requestCounter);
        return;
    }

    const reqId = ++state.requestCounter;

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
    player.src = url;

    startBgmElementPlayback(player, trackKey, reqId);
}

export function stopMusic() {
    ++state.requestCounter;
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

    startBgmElementPlayback(engine.bgmPlayer, state.currentTrackKey, state.requestCounter);
}

export function playSoundEffect(effectKey, relativePath) {
    if (!state.initialized) {
        initialize();
    }
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    if (state.pendingBgmPlay || state.pendingAmbientPlay || (engine.bgmPlayer && engine.bgmPlayer.paused && state.currentTrackKey && !state.bgmPaused)) {
        handleUserInteractionUnlock();
    }

    if (state.isMuted) return;

    if (activeSfx.size >= MAX_CONCURRENT_SFX) {
        const oldest = activeSfx.values().next().value;
        if (oldest) {
            activeSfx.delete(oldest);
            try {
                if (oldest.audio) {
                    oldest.audio.pause();
                    oldest.audio.removeAttribute('src');
                }
            } catch {
                // Ignore cleanup error
            }
        }
    }

    if (typeof Audio === 'undefined') return;
    try {
        const sfxAudio = new Audio(url);
        sfxAudio.preload = 'auto';
        sfxAudio.loop = false;
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
}

export function stopSoundEffect(effectKey) {
    for (const entry of Array.from(activeSfx)) {
        if (entry.key === effectKey) {
            activeSfx.delete(entry);
            try {
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

function startAmbientElementPlayback(player, expectedAmbientKey, reqId) {
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
                    if (err && err.name === 'AbortError') return;

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

        if (!player.paused) {
            return;
        }
        if (!player.src) {
            player.src = url;
        }
        startAmbientElementPlayback(player, ambientKey, state.ambientRequestCounter);
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
    player.src = url;

    startAmbientElementPlayback(player, ambientKey, reqId);
}

export function stopAmbient() {
    ++state.ambientRequestCounter;
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

// PymSpaceOrbit Cross-Platform Audio Engine (MAUI Blazor Hybrid + Blazor Web / WebAssembly + GitHub Pages)

const STORAGE_KEY = 'PymSpaceOrbit.AudioSettings.v1';
const MAX_CONCURRENT_SFX = 12;
const AMBIENT_VOLUME_SCALE = 0.42;

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
    currentAmbientKey: null,
    currentAmbientUrl: null,
    ambientLoop: true,
    pendingAmbientPlay: false,
    wasBgmPlayingBeforeHide: false,
    wasAmbientPlayingBeforeHide: false,
    unlockListenersAttached: false,
    visibilityListenerAttached: false
};

let bgmPlayer = null;
let ambientPlayer = null;
const activeSfx = new Set();

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
    const cleanPath = relativePath.replace(/^\/+/, '');
    try {
        const baseUri = (typeof document !== 'undefined' && document.baseURI)
            ? document.baseURI
            : (typeof window !== 'undefined' ? window.location.href : '/');
        return new URL(cleanPath, baseUri).href;
    } catch {
        return cleanPath;
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
    }
    if (ambientPlayer) {
        ambientPlayer.volume = getEffectiveAmbientVolume();
    }
    const sfxVol = getEffectiveSfxVolume();
    for (const entry of activeSfx) {
        try {
            entry.audio.volume = sfxVol;
        } catch {
            // Ignore element state error
        }
    }
}

function ensurePlayers() {
    if (typeof Audio === 'undefined') return false;

    if (!bgmPlayer) {
        bgmPlayer = new Audio();
        bgmPlayer.preload = 'auto';
        bgmPlayer.loop = true;
        bgmPlayer.volume = getEffectiveMusicVolume();
    }

    if (!ambientPlayer) {
        ambientPlayer = new Audio();
        ambientPlayer.preload = 'auto';
        ambientPlayer.loop = true;
        ambientPlayer.volume = getEffectiveAmbientVolume();
    }

    return true;
}

function checkAndDetachUnlockListeners() {
    if (!state.pendingBgmPlay && !state.pendingAmbientPlay) {
        detachUnlockListeners();
    }
}

function handleUserInteractionUnlock() {
    if (state.pendingBgmPlay && bgmPlayer && state.currentTrackKey && !state.bgmPaused) {
        bgmPlayer.volume = getEffectiveMusicVolume();
        try {
            const playPromise = bgmPlayer.play();
            if (playPromise && typeof playPromise.then === 'function') {
                playPromise
                    .then(() => {
                        state.pendingBgmPlay = false;
                        checkAndDetachUnlockListeners();
                    })
                    .catch(() => {
                        state.pendingBgmPlay = true;
                    });
            } else {
                state.pendingBgmPlay = false;
            }
        } catch {
            state.pendingBgmPlay = true;
        }
    }

    if (state.pendingAmbientPlay && ambientPlayer && state.currentAmbientKey) {
        ambientPlayer.volume = getEffectiveAmbientVolume();
        try {
            const ambPromise = ambientPlayer.play();
            if (ambPromise && typeof ambPromise.then === 'function') {
                ambPromise
                    .then(() => {
                        state.pendingAmbientPlay = false;
                        checkAndDetachUnlockListeners();
                    })
                    .catch(() => {
                        state.pendingAmbientPlay = true;
                    });
            } else {
                state.pendingAmbientPlay = false;
            }
        } catch {
            state.pendingAmbientPlay = true;
        }
    }

    checkAndDetachUnlockListeners();
}

function attachUnlockListeners() {
    if (state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = true;
    const opts = { capture: true, passive: true };
    document.addEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.addEventListener('keydown', handleUserInteractionUnlock, opts);
    document.addEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.addEventListener('click', handleUserInteractionUnlock, opts);
}

function detachUnlockListeners() {
    if (!state.unlockListenersAttached || typeof document === 'undefined') return;
    state.unlockListenersAttached = false;
    const opts = { capture: true, passive: true };
    document.removeEventListener('pointerdown', handleUserInteractionUnlock, opts);
    document.removeEventListener('keydown', handleUserInteractionUnlock, opts);
    document.removeEventListener('touchstart', handleUserInteractionUnlock, opts);
    document.removeEventListener('click', handleUserInteractionUnlock, opts);
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
    if (!ensurePlayers()) return;

    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    const shouldLoop = loop !== false;

    // Do not restart the same track if already playing or queued for autoplay
    if (state.currentTrackKey === trackKey && bgmPlayer) {
        bgmPlayer.loop = shouldLoop;
        state.bgmLoop = shouldLoop;
        bgmPlayer.volume = getEffectiveMusicVolume();

        if (!bgmPlayer.paused) {
            return;
        }
        if (state.pendingBgmPlay && !state.bgmPaused) {
            attachUnlockListeners();
            return;
        }
        if (state.bgmPaused) {
            state.bgmPaused = false;
            const resumePromise = bgmPlayer.play();
            if (resumePromise && typeof resumePromise.catch === 'function') {
                resumePromise.catch(() => {
                    state.pendingBgmPlay = true;
                    attachUnlockListeners();
                });
            }
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

    bgmPlayer.src = url;
    bgmPlayer.loop = shouldLoop;
    bgmPlayer.volume = getEffectiveMusicVolume();

    try {
        const playPromise = bgmPlayer.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    state.pendingBgmPlay = false;
                })
                .catch(() => {
                    state.pendingBgmPlay = true;
                    attachUnlockListeners();
                });
        }
    } catch {
        state.pendingBgmPlay = true;
        attachUnlockListeners();
    }
}

export function stopMusic() {
    state.pendingBgmPlay = false;
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

    if (!bgmPlayer.paused) return;

    try {
        const playPromise = bgmPlayer.play();
        if (playPromise && typeof playPromise.catch === 'function') {
            playPromise.catch(() => {
                state.pendingBgmPlay = true;
                attachUnlockListeners();
            });
        }
    } catch {
        state.pendingBgmPlay = true;
        attachUnlockListeners();
    }
}

export function playSoundEffect(effectKey, relativePath) {
    if (typeof Audio === 'undefined') return;
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

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

    try {
        const sfxAudio = new Audio(url);
        sfxAudio.preload = 'auto';
        sfxAudio.loop = false;
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

export function playAmbient(ambientKey, relativePath, loop = true) {
    if (!ensurePlayers()) return;
    const url = resolveAssetUrl(relativePath);
    if (!url) return;

    const shouldLoop = loop !== false;

    if (state.currentAmbientKey === ambientKey && ambientPlayer) {
        ambientPlayer.loop = shouldLoop;
        state.ambientLoop = shouldLoop;
        ambientPlayer.volume = getEffectiveAmbientVolume();
        if (!ambientPlayer.paused) {
            return;
        }
        if (state.pendingAmbientPlay) {
            attachUnlockListeners();
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

    ambientPlayer.src = url;
    ambientPlayer.loop = shouldLoop;
    ambientPlayer.volume = getEffectiveAmbientVolume();

    try {
        const playPromise = ambientPlayer.play();
        if (playPromise && typeof playPromise.then === 'function') {
            playPromise
                .then(() => {
                    state.pendingAmbientPlay = false;
                })
                .catch(() => {
                    state.pendingAmbientPlay = true;
                    attachUnlockListeners();
                });
        }
    } catch {
        state.pendingAmbientPlay = true;
        attachUnlockListeners();
    }
}

export function stopAmbient() {
    state.pendingAmbientPlay = false;
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

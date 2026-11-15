using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Constants;
using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Services;

/// <summary>
/// Native .NET MAUI implementation of <see cref="IAudioService"/> powered by <see cref="Plugin.Maui.Audio"/>
/// using the audio assets packaged inside the MAUI project (<c>Resources/Raw/audio/</c>).
/// </summary>
public sealed class AudioService : IAudioService
{
    private const string PrefKeyMasterVolume = "PymSpaceOrbit.Audio.v2.MasterVolume";
    private const string PrefKeyMusicVolume = "PymSpaceOrbit.Audio.v2.MusicVolume";
    private const string PrefKeySfxVolume = "PymSpaceOrbit.Audio.v2.SfxVolume";
    private const string PrefKeyIsMuted = "PymSpaceOrbit.Audio.v2.IsMuted";
    private const double AmbientMixFactor = 0.42;
    private const int MaxConcurrentSfx = 16;

    private readonly IAudioManager _audioManager;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly List<ActiveSfxEntry> _activeSfx = [];
    private readonly object _sfxLock = new();

    private IAudioPlayer? _bgmPlayer;
    private Stream? _bgmStream;
    private bool _bgmPaused;
    private bool _currentMusicLoop = true;

    private IAudioPlayer? _ambientPlayer;
    private Stream? _ambientStream;
    private bool _currentAmbientLoop = true;

    private bool _disposed;

    public bool IsInitialized { get; private set; }
    public double MasterVolume { get; private set; } = 0.85;
    public double MusicVolume { get; private set; } = 0.70;
    public double SoundEffectsVolume { get; private set; } = 0.85;
    public bool IsMuted { get; private set; }
    public BackgroundMusic? CurrentMusic { get; private set; }
    public SoundEffect? CurrentAmbient { get; private set; }

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager ?? AudioManager.Current;
        LoadPersistedPreferences();
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return Task.CompletedTask;
        }

        if (!IsInitialized)
        {
            LoadPersistedPreferences();
            IsInitialized = true;
        }

        return Task.CompletedTask;
    }

    public async Task PlayMusicAsync(BackgroundMusic track, bool loop = true, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await InitializeAsync(cancellationToken);

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_disposed) return;

            if (CurrentMusic == track && _bgmPlayer is not null)
            {
                _currentMusicLoop = loop;
                await RunOnMainThreadAsync(() =>
                {
                    if (_bgmPlayer is null) return;
                    _bgmPlayer.Loop = loop;
                    _bgmPlayer.Volume = GetEffectiveMusicVolume();
                    if (!_bgmPaused && !IsMuted && !_bgmPlayer.IsPlaying)
                    {
                        _bgmPlayer.Play();
                    }
                });
                return;
            }

            CurrentMusic = track;
            _currentMusicLoop = loop;
            _bgmPaused = false;

            string fileName = PymGameOrbitAssets.GetMusicFileName(track);
            string relativePath = PymGameOrbitAssets.GetMusicPath(track);

            await RunOnMainThreadAsync(async () =>
            {
                DisposeBgmInternal();

                var (player, ownedStream) = await CreatePlatformPlayerAsync(fileName, relativePath, cancellationToken);
                if (player is null)
                {
                    ownedStream?.Dispose();
                    return;
                }

                try
                {
                    player.Loop = loop;
                    player.Volume = GetEffectiveMusicVolume();
                    _bgmPlayer = player;
                    _bgmStream = ownedStream;

                    if (!IsMuted)
                    {
                        player.Play();
                    }
                }
                catch
                {
                    player.Dispose();
                    ownedStream?.Dispose();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch
        {
            // Audio failures must never block gameplay
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task StopMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            CurrentMusic = null;
            _bgmPaused = false;
            await RunOnMainThreadAsync(DisposeBgmInternal);
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task PauseMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            _bgmPaused = true;
            await RunOnMainThreadAsync(() =>
            {
                if (_bgmPlayer is { IsPlaying: true })
                {
                    _bgmPlayer.Pause();
                }
            });
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task ResumeMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            _bgmPaused = false;
            await RunOnMainThreadAsync(() =>
            {
                if (_bgmPlayer is not null)
                {
                    _bgmPlayer.Volume = GetEffectiveMusicVolume();
                    if (!IsMuted && !_bgmPlayer.IsPlaying)
                    {
                        _bgmPlayer.Play();
                    }
                }
            });
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task SetMusicVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        MusicVolume = Math.Clamp(volume, 0.0, 1.0);
        SavePersistedPreferences();

        await RunOnMainThreadAsync(() =>
        {
            ApplyVolumesToActivePlayers();
            EnsureActiveTracksResumedIfUnmuted();
        });
    }

    public async Task PlaySoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await InitializeAsync(cancellationToken);

        double effectiveVolume = GetEffectiveSfxVolume();
        if (IsMuted || effectiveVolume <= 0.0001)
        {
            return;
        }

        try
        {
            string fileName = PymGameOrbitAssets.GetSoundEffectFileName(effect);
            string relativePath = PymGameOrbitAssets.GetSoundEffectPath(effect);

            await RunOnMainThreadAsync(async () =>
            {
                if (_disposed) return;

                var (player, ownedStream) = await CreatePlatformPlayerAsync(fileName, relativePath, cancellationToken);
                if (player is null)
                {
                    ownedStream?.Dispose();
                    return;
                }

                try
                {
                    player.Loop = false;
                    player.Volume = GetEffectiveSfxVolume();

                    ActiveSfxEntry? entry = null;
                    void OnPlaybackEnded(object? sender, EventArgs args)
                    {
                        if (entry is not null)
                        {
                            RemoveAndDisposeSfxEntry(entry);
                        }
                    }

                    entry = new ActiveSfxEntry(effect, player, ownedStream, OnPlaybackEnded);
                    player.PlaybackEnded += OnPlaybackEnded;

                    lock (_sfxLock)
                    {
                        while (_activeSfx.Count >= MaxConcurrentSfx)
                        {
                            var oldest = _activeSfx[0];
                            _activeSfx.RemoveAt(0);
                            DisposeSfxEntrySafe(oldest);
                        }

                        _activeSfx.Add(entry);
                    }

                    player.Play();
                }
                catch
                {
                    player.Dispose();
                    ownedStream?.Dispose();
                }
            });
        }
        catch
        {
            // Never allow SFX errors to interrupt gameplay
        }
    }

    public async Task StopSoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await RunOnMainThreadAsync(() =>
        {
            List<ActiveSfxEntry> toStop;
            lock (_sfxLock)
            {
                toStop = _activeSfx.Where(x => x.Effect == effect).ToList();
                foreach (var item in toStop)
                {
                    _activeSfx.Remove(item);
                }
            }

            foreach (var item in toStop)
            {
                DisposeSfxEntrySafe(item);
            }
        });
    }

    public async Task StopAllSoundEffectsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await RunOnMainThreadAsync(DisposeAllSfxInternal);
    }

    public async Task SetSoundEffectsVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        SoundEffectsVolume = Math.Clamp(volume, 0.0, 1.0);
        SavePersistedPreferences();

        await RunOnMainThreadAsync(() =>
        {
            ApplyVolumesToActivePlayers();
            EnsureActiveTracksResumedIfUnmuted();
        });
    }

    public async Task PlayAmbientAsync(SoundEffect ambientEffect = SoundEffect.BioshipCruisingLoop, bool loop = true, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await InitializeAsync(cancellationToken);

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_disposed) return;

            if (CurrentAmbient == ambientEffect && _ambientPlayer is not null)
            {
                _currentAmbientLoop = loop;
                await RunOnMainThreadAsync(() =>
                {
                    if (_ambientPlayer is null) return;
                    _ambientPlayer.Loop = loop;
                    _ambientPlayer.Volume = GetEffectiveAmbientVolume();
                    if (!IsMuted && !_ambientPlayer.IsPlaying)
                    {
                        _ambientPlayer.Play();
                    }
                });
                return;
            }

            CurrentAmbient = ambientEffect;
            _currentAmbientLoop = loop;

            string fileName = PymGameOrbitAssets.GetSoundEffectFileName(ambientEffect);
            string relativePath = PymGameOrbitAssets.GetSoundEffectPath(ambientEffect);

            await RunOnMainThreadAsync(async () =>
            {
                DisposeAmbientInternal();

                var (player, ownedStream) = await CreatePlatformPlayerAsync(fileName, relativePath, cancellationToken);
                if (player is null)
                {
                    ownedStream?.Dispose();
                    return;
                }

                try
                {
                    player.Loop = loop;
                    player.Volume = GetEffectiveAmbientVolume();
                    _ambientPlayer = player;
                    _ambientStream = ownedStream;

                    if (!IsMuted)
                    {
                        player.Play();
                    }
                }
                catch
                {
                    player.Dispose();
                    ownedStream?.Dispose();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch
        {
            // Never allow ambient audio failures to block gameplay
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task StopAmbientAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            CurrentAmbient = null;
            await RunOnMainThreadAsync(DisposeAmbientInternal);
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task SetMasterVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        MasterVolume = Math.Clamp(volume, 0.0, 1.0);
        SavePersistedPreferences();

        await RunOnMainThreadAsync(() =>
        {
            ApplyVolumesToActivePlayers();
            EnsureActiveTracksResumedIfUnmuted();
        });
    }

    public async Task SetMutedAsync(bool muted, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        IsMuted = muted;
        SavePersistedPreferences();

        await RunOnMainThreadAsync(() =>
        {
            ApplyVolumesToActivePlayers();
            if (IsMuted)
            {
                if (_bgmPlayer is { IsPlaying: true })
                {
                    _bgmPlayer.Pause();
                }

                if (_ambientPlayer is { IsPlaying: true })
                {
                    _ambientPlayer.Pause();
                }
            }
            else
            {
                EnsureActiveTracksResumedIfUnmuted();
            }
        });

        if (!IsMuted)
        {
            if (_bgmPlayer is null && CurrentMusic is { } pendingTrack)
            {
                await PlayMusicAsync(pendingTrack, _currentMusicLoop, cancellationToken);
            }

            if (_ambientPlayer is null && CurrentAmbient is { } pendingAmbient)
            {
                await PlayAmbientAsync(pendingAmbient, _currentAmbientLoop, cancellationToken);
            }
        }
    }

    public async Task SyncScreenMusicAsync(string? routeOrUri, GameSessionState? session = null, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        string normalizedRoute = NormalizeRouteSegment(routeOrUri);

        if (normalizedRoute.Equals("options", StringComparison.OrdinalIgnoreCase))
        {
            if (CurrentAmbient is not null)
            {
                await StopAmbientAsync(cancellationToken);
            }
            await PlayMusicAsync(BackgroundMusic.Options, loop: true, cancellationToken);
            return;
        }

        if (normalizedRoute.Equals("info", StringComparison.OrdinalIgnoreCase))
        {
            if (CurrentAmbient is not null)
            {
                await StopAmbientAsync(cancellationToken);
            }
            await PlayMusicAsync(BackgroundMusic.InfoCardSets, loop: true, cancellationToken);
            return;
        }

        if (normalizedRoute.Equals("play", StringComparison.OrdinalIgnoreCase))
        {
            if (session is not null)
            {
                await SyncGameplayMusicAsync(session, cancellationToken);
            }
            else
            {
                await PlayAmbientAsync(SoundEffect.BioshipCruisingLoop, loop: true, cancellationToken);
                await PlayMusicAsync(BackgroundMusic.GameplayBuild, loop: true, cancellationToken);
            }
            return;
        }

        // Default: Main Menu ("", "/", "menu")
        if (CurrentAmbient is not null)
        {
            await StopAmbientAsync(cancellationToken);
        }
        await PlayMusicAsync(BackgroundMusic.MainMenu, loop: true, cancellationToken);
    }

    public async Task SyncGameplayMusicAsync(GameSessionState session, CancellationToken cancellationToken = default)
    {
        if (_disposed || session is null) return;

        if (session.Phase == TurnPhase.MissionEnded || session.Outcome != MissionEndingOutcome.InProgress)
        {
            if (CurrentAmbient is not null)
            {
                await StopAmbientAsync(cancellationToken);
            }

            var endingTrack = session.Outcome == MissionEndingOutcome.SuccessNominalReturn
                ? BackgroundMusic.VictoryReturn
                : BackgroundMusic.GameOver;

            await PlayMusicAsync(endingTrack, loop: true, cancellationToken);
            return;
        }

        await PlayAmbientAsync(SoundEffect.BioshipCruisingLoop, loop: true, cancellationToken);

        if (session.Phase == TurnPhase.BuildPhase)
        {
            await PlayMusicAsync(BackgroundMusic.GameplayBuild, loop: true, cancellationToken);
            return;
        }

        if (IsDonkiSolarStormActive(session))
        {
            await PlayMusicAsync(BackgroundMusic.DonkiSolarStorm, loop: true, cancellationToken);
            return;
        }

        await PlayMusicAsync(BackgroundMusic.GameplayTurns, loop: true, cancellationToken);
    }

    public async Task TriggerCardEquippedAudioAsync(GameCardModel card, OutpostDestination destination, CancellationToken cancellationToken = default)
    {
        if (_disposed || card is null) return;

        await PlaySoundEffectAsync(SoundEffect.CardEquipLock, cancellationToken);

        if (card.Category == CardSlotCategory.MarshallEclssLoop ||
            card.CardId.StartsWith("ECL", StringComparison.OrdinalIgnoreCase) ||
            card.CardId.Equals("ARC-LSS-02", StringComparison.OrdinalIgnoreCase))
        {
            await PlaySoundEffectAsync(SoundEffect.EclssWaterBioreactor, cancellationToken);
        }
        else if (card.CardId.Equals("BLD-03", StringComparison.OrdinalIgnoreCase) ||
                 card.Category == CardSlotCategory.EnvironmentAndSuits)
        {
            await PlaySoundEffectAsync(SoundEffect.AirlockHatchOpen, cancellationToken);
        }
        else if (card.Category == CardSlotCategory.DonkiSpaceWeatherCounter ||
                 card.CardId.StartsWith("SWX", StringComparison.OrdinalIgnoreCase))
        {
            await PlaySoundEffectAsync(SoundEffect.TelemetryNotification, cancellationToken);
        }
        else if (card.Category == CardSlotCategory.OutpostArchitecture)
        {
            var outpostSfx = destination == OutpostDestination.ISS
                ? SoundEffect.IssMetalFootsteps
                : SoundEffect.EvaRegolithFootsteps;
            await PlaySoundEffectAsync(outpostSfx, cancellationToken);
        }
        else if (destination == OutpostDestination.ISS)
        {
            await PlaySoundEffectAsync(SoundEffect.IssMetalFootsteps, cancellationToken);
        }
    }

    public async Task TriggerExplorationEventAudioAsync(TurnExplorationEvent explorationEvent, GameSessionState session, CancellationToken cancellationToken = default)
    {
        if (_disposed || explorationEvent is null) return;

        await PlaySoundEffectAsync(SoundEffect.TelemetryNotification, cancellationToken);

        if (IsDonkiEvent(explorationEvent))
        {
            await PlaySoundEffectAsync(SoundEffect.SolarStormImpact, cancellationToken);
        }
        else if (IsWindOrCryoEvent(explorationEvent))
        {
            await PlaySoundEffectAsync(SoundEffect.MarsLunarWind, cancellationToken);
        }
        else if (IsAirlockEvent(explorationEvent))
        {
            await PlaySoundEffectAsync(SoundEffect.AirlockHatchOpen, cancellationToken);
        }

        if (session is not null)
        {
            await SyncGameplayMusicAsync(session, cancellationToken);
        }
    }

    public async Task TriggerExplorationDecisionAudioAsync(
        TurnExplorationEvent resolvedEvent,
        ExplorationChoiceOption selectedOption,
        TurnResolutionResult resolutionResult,
        GameSessionState session,
        CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (resolutionResult.Outcome != MissionEndingOutcome.InProgress)
        {
            if (resolutionResult.Outcome == MissionEndingOutcome.SuccessNominalReturn)
            {
                await PlaySoundEffectAsync(SoundEffect.StageTransitionRiser, cancellationToken);
            }
            else
            {
                await PlaySoundEffectAsync(SoundEffect.SolarStormImpact, cancellationToken);
            }

            await SyncGameplayMusicAsync(session, cancellationToken);
            return;
        }

        var actionSfx = ResolveActionSoundEffect(resolvedEvent, selectedOption);
        await PlaySoundEffectAsync(actionSfx, cancellationToken);
        await PlaySoundEffectAsync(SoundEffect.CardDrawFlip, cancellationToken);

        if (resolutionResult.NextEvent is not null)
        {
            if (IsDonkiEvent(resolutionResult.NextEvent))
            {
                await PlaySoundEffectAsync(SoundEffect.TelemetryNotification, cancellationToken);
            }
            else if (IsWindOrCryoEvent(resolutionResult.NextEvent))
            {
                await PlaySoundEffectAsync(SoundEffect.MarsLunarWind, cancellationToken);
            }
        }

        await SyncGameplayMusicAsync(session, cancellationToken);
    }

    private async Task<(IAudioPlayer? Player, Stream? OwnedStream)> CreatePlatformPlayerAsync(
        string fileName,
        string relativePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

#if WINDOWS
        string? diskPath = ResolveLocalFilePath(fileName, relativePath);
        if (!string.IsNullOrEmpty(diskPath))
        {
            try
            {
                var player = _audioManager.CreatePlayer();
                var mediaPlayerField = player.GetType().GetField("player", BindingFlags.Instance | BindingFlags.NonPublic);
                if (mediaPlayerField?.GetValue(player) is Windows.Media.Playback.MediaPlayer nativeMediaPlayer)
                {
                    nativeMediaPlayer.AudioCategory = Windows.Media.Playback.MediaPlayerAudioCategory.Media;
                    nativeMediaPlayer.Source = Windows.Media.Core.MediaSource.CreateFromUri(new Uri(diskPath));
                    return (player, null);
                }
                player.Dispose();
            }
            catch
            {
                // Fall back to stream-based creation below
            }

            try
            {
                FileStream fs = File.OpenRead(diskPath);
                var player = _audioManager.CreatePlayer(fs);
                return (player, fs);
            }
            catch
            {
                // Continue to package stream fallback
            }
        }
#else
        // On Android, iOS, and MacCatalyst, try direct asset filename first
        string[] directNames = [fileName, relativePath];
        foreach (string candidate in directNames)
        {
            try
            {
                var player = _audioManager.CreatePlayer(candidate);
                return (player, null);
            }
            catch
            {
                // Fall back to FileSystem.OpenAppPackageFileAsync below
            }
        }
#endif

        Stream? pkgStream = await OpenPackageAudioStreamAsync(fileName, relativePath);
        if (pkgStream is not null)
        {
            try
            {
                var player = _audioManager.CreatePlayer(pkgStream);
                return (player, pkgStream);
            }
            catch
            {
                await pkgStream.DisposeAsync();
            }
        }

        return (null, null);
    }

    private static string? ResolveLocalFilePath(string fileName, string relativePath)
    {
        string baseDir = AppContext.BaseDirectory;
        string osRelative = relativePath.Replace('/', Path.DirectorySeparatorChar);

        string[] candidates =
        [
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, osRelative),
            Path.Combine(baseDir, "Resources", "Raw", osRelative),
            Path.Combine(baseDir, "Resources", "Raw", fileName),
            Path.Combine(baseDir, "wwwroot", osRelative)
        ];

        foreach (string path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static async Task<Stream?> OpenPackageAudioStreamAsync(string fileName, string relativePath)
    {
        string[] candidates =
        [
            fileName,
            relativePath,
            relativePath.Replace('/', '\\')
        ];

        foreach (string candidate in candidates)
        {
            try
            {
                return await FileSystem.OpenAppPackageFileAsync(candidate);
            }
            catch
            {
                // Try next candidate
            }
        }

        return null;
    }

    private double GetEffectiveMusicVolume() =>
        IsMuted ? 0.0 : Math.Clamp(MasterVolume * MusicVolume, 0.0, 1.0);

    private double GetEffectiveSfxVolume() =>
        IsMuted ? 0.0 : Math.Clamp(MasterVolume * SoundEffectsVolume, 0.0, 1.0);

    private double GetEffectiveAmbientVolume() =>
        IsMuted ? 0.0 : Math.Clamp(MasterVolume * SoundEffectsVolume * AmbientMixFactor, 0.0, 1.0);

    private void ApplyVolumesToActivePlayers()
    {
        if (_bgmPlayer is not null)
        {
            try
            {
                _bgmPlayer.Volume = GetEffectiveMusicVolume();
            }
            catch
            {
                // Ignore platform volume errors
            }
        }

        if (_ambientPlayer is not null)
        {
            try
            {
                _ambientPlayer.Volume = GetEffectiveAmbientVolume();
            }
            catch
            {
                // Ignore platform volume errors
            }
        }

        double sfxVol = GetEffectiveSfxVolume();
        lock (_sfxLock)
        {
            foreach (var entry in _activeSfx)
            {
                try
                {
                    entry.Player.Volume = sfxVol;
                }
                catch
                {
                    // Ignore platform volume errors
                }
            }
        }
    }

    private void EnsureActiveTracksResumedIfUnmuted()
    {
        if (IsMuted)
        {
            return;
        }

        if (_bgmPlayer is not null && !_bgmPaused && GetEffectiveMusicVolume() > 0.0001)
        {
            try
            {
                if (!_bgmPlayer.IsPlaying)
                {
                    _bgmPlayer.Play();
                }
            }
            catch
            {
                // Ignore platform playback errors
            }
        }

        if (_ambientPlayer is not null && GetEffectiveAmbientVolume() > 0.0001)
        {
            try
            {
                if (!_ambientPlayer.IsPlaying)
                {
                    _ambientPlayer.Play();
                }
            }
            catch
            {
                // Ignore platform playback errors
            }
        }
    }

    private void LoadPersistedPreferences()
    {
        try
        {
            MasterVolume = Math.Clamp(Preferences.Default.Get(PrefKeyMasterVolume, 0.85), 0.0, 1.0);
            MusicVolume = Math.Clamp(Preferences.Default.Get(PrefKeyMusicVolume, 0.70), 0.0, 1.0);
            SoundEffectsVolume = Math.Clamp(Preferences.Default.Get(PrefKeySfxVolume, 0.85), 0.0, 1.0);
            IsMuted = Preferences.Default.Get(PrefKeyIsMuted, false);
        }
        catch
        {
            // Fallback to defaults if platform preferences are unavailable
        }
    }

    private void SavePersistedPreferences()
    {
        try
        {
            Preferences.Default.Set(PrefKeyMasterVolume, MasterVolume);
            Preferences.Default.Set(PrefKeyMusicVolume, MusicVolume);
            Preferences.Default.Set(PrefKeySfxVolume, SoundEffectsVolume);
            Preferences.Default.Set(PrefKeyIsMuted, IsMuted);
        }
        catch
        {
            // Ignore preference storage errors
        }
    }

    private static Task RunOnMainThreadAsync(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(action);
    }

    private static Task RunOnMainThreadAsync(Func<Task> asyncAction)
    {
        if (MainThread.IsMainThread)
        {
            return asyncAction();
        }

        return MainThread.InvokeOnMainThreadAsync(asyncAction);
    }

    private void DisposeBgmInternal()
    {
        if (_bgmPlayer is not null)
        {
            try
            {
                if (_bgmPlayer.IsPlaying)
                {
                    _bgmPlayer.Stop();
                }
                _bgmPlayer.Dispose();
            }
            catch
            {
                // Ignore disposal exceptions
            }
            finally
            {
                _bgmPlayer = null;
            }
        }

        if (_bgmStream is not null)
        {
            try
            {
                _bgmStream.Dispose();
            }
            catch
            {
                // Ignore stream disposal exceptions
            }
            finally
            {
                _bgmStream = null;
            }
        }
    }

    private void DisposeAmbientInternal()
    {
        if (_ambientPlayer is not null)
        {
            try
            {
                if (_ambientPlayer.IsPlaying)
                {
                    _ambientPlayer.Stop();
                }
                _ambientPlayer.Dispose();
            }
            catch
            {
                // Ignore disposal exceptions
            }
            finally
            {
                _ambientPlayer = null;
            }
        }

        if (_ambientStream is not null)
        {
            try
            {
                _ambientStream.Dispose();
            }
            catch
            {
                // Ignore stream disposal exceptions
            }
            finally
            {
                _ambientStream = null;
            }
        }
    }

    private void RemoveAndDisposeSfxEntry(ActiveSfxEntry entry)
    {
        lock (_sfxLock)
        {
            _activeSfx.Remove(entry);
        }

        DisposeSfxEntrySafe(entry);
    }

    private static void DisposeSfxEntrySafe(ActiveSfxEntry entry)
    {
        try
        {
            entry.Player.PlaybackEnded -= entry.EndedHandler;
            if (entry.Player.IsPlaying)
            {
                entry.Player.Stop();
            }
            entry.Player.Dispose();
        }
        catch
        {
            // Ignore player disposal errors
        }

        try
        {
            entry.OwnedStream?.Dispose();
        }
        catch
        {
            // Ignore stream disposal errors
        }
    }

    private void DisposeAllSfxInternal()
    {
        List<ActiveSfxEntry> all;
        lock (_sfxLock)
        {
            all = [.. _activeSfx];
            _activeSfx.Clear();
        }

        foreach (var entry in all)
        {
            DisposeSfxEntrySafe(entry);
        }
    }

    private static SoundEffect ResolveActionSoundEffect(TurnExplorationEvent resolvedEvent, ExplorationChoiceOption option)
    {
        string hint = option.RequiredToolOrSubsystemHint ?? string.Empty;
        string label = option.LabelEn ?? string.Empty;

        if (option.RadiationDoseSpike >= 0.20f || IsDonkiEvent(resolvedEvent))
        {
            return SoundEffect.SolarStormImpact;
        }

        if (IsAirlockEvent(resolvedEvent) ||
            hint.Contains("SuitPort", StringComparison.OrdinalIgnoreCase) ||
            hint.Contains("Valve", StringComparison.OrdinalIgnoreCase) ||
            hint.Contains("Curtain", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Airlock", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("SuitPort", StringComparison.OrdinalIgnoreCase))
        {
            return SoundEffect.AirlockHatchOpen;
        }

        if (IsWindOrCryoEvent(resolvedEvent))
        {
            return SoundEffect.MarsLunarWind;
        }

        if (resolvedEvent.TargetScope is OutpostDestination.Moon or OutpostDestination.Mars &&
            (option.SuitWearPctDelta < 0f ||
             hint.Contains("EVA", StringComparison.OrdinalIgnoreCase) ||
             hint.Contains("xEMU", StringComparison.OrdinalIgnoreCase) ||
             hint.Contains("Rappel", StringComparison.OrdinalIgnoreCase) ||
             hint.Contains("Sortie", StringComparison.OrdinalIgnoreCase) ||
             label.Contains("EVA", StringComparison.OrdinalIgnoreCase)))
        {
            return SoundEffect.EvaRegolithFootsteps;
        }

        if (option.WaterLitersDelta > 0f ||
            option.FoodKgDelta > 0f ||
            resolvedEvent.EventId is "EVT-ISS-02" or "EVT-ISS-06" or "EVT-ISS-07" or "EVT-MARS-05" ||
            hint.Contains("BPA", StringComparison.OrdinalIgnoreCase) ||
            hint.Contains("WPA", StringComparison.OrdinalIgnoreCase) ||
            hint.Contains("PBR", StringComparison.OrdinalIgnoreCase) ||
            hint.Contains("Bio", StringComparison.OrdinalIgnoreCase))
        {
            return SoundEffect.EclssWaterBioreactor;
        }

        if (resolvedEvent.TargetScope == OutpostDestination.ISS)
        {
            return SoundEffect.IssMetalFootsteps;
        }

        return SoundEffect.TelemetryNotification;
    }

    private static bool IsDonkiSolarStormActive(GameSessionState session)
    {
        if (session.PendingExplorationEvent is not null && IsDonkiEvent(session.PendingExplorationEvent))
        {
            return true;
        }

        return session.Resources.EffectiveRadiationIndex >= 1.85f;
    }

    private static bool IsDonkiEvent(TurnExplorationEvent evt)
    {
        if (evt.EventId is "EVT-MOON-04" or "EVT-ISS-01" or "EVT-MARS-04" or "EVT-MARS-06")
        {
            return true;
        }

        return evt.NasaApiReference.Contains("DONKI", StringComparison.OrdinalIgnoreCase) ||
               evt.TitleEn.Contains("DONKI", StringComparison.OrdinalIgnoreCase) ||
               evt.TitleEn.Contains("Solar Flare", StringComparison.OrdinalIgnoreCase) ||
               evt.TitleEn.Contains("Proton Storm", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWindOrCryoEvent(TurnExplorationEvent evt)
    {
        return evt.EventId is "EVT-MOON-03" or "EVT-MARS-02" or "EVT-MARS-03" ||
               evt.TitleEn.Contains("Dust Storm", StringComparison.OrdinalIgnoreCase) ||
               evt.TitleEn.Contains("Lunar Night", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAirlockEvent(TurnExplorationEvent evt)
    {
        return evt.EventId is "EVT-MOON-02" or "EVT-MARS-01" ||
               evt.TitleEn.Contains("Airlock", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRouteSegment(string? routeOrUri)
    {
        if (string.IsNullOrWhiteSpace(routeOrUri))
        {
            return string.Empty;
        }

        string path = routeOrUri.Trim();
        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
        {
            path = absoluteUri.AbsolutePath;
        }
        else
        {
            int queryOrHash = path.IndexOfAny(['?', '#']);
            if (queryOrHash >= 0)
            {
                path = path[..queryOrHash];
            }
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return segments.Length == 0 ? string.Empty : segments[^1];
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await RunOnMainThreadAsync(() =>
        {
            DisposeBgmInternal();
            DisposeAmbientInternal();
            DisposeAllSfxInternal();
        });

        _stateLock.Dispose();
    }

    private sealed record ActiveSfxEntry(
        SoundEffect Effect,
        IAudioPlayer Player,
        Stream? OwnedStream,
        EventHandler EndedHandler);
}

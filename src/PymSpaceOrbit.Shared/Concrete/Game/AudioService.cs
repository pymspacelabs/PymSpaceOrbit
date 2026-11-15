using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Constants;
using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Concrete.Game;

public sealed class AudioService : IAudioService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _moduleLock = new(1, 1);

    private IJSObjectReference? _jsModule;
    private bool _useGlobalAudioService;
    private bool _disposed;

    public bool IsInitialized { get; private set; }
    public double MasterVolume { get; private set; } = 0.85;
    public double MusicVolume { get; private set; } = 0.70;
    public double SoundEffectsVolume { get; private set; } = 0.85;
    public bool IsMuted { get; private set; }
    public BackgroundMusic? CurrentMusic { get; private set; }
    public SoundEffect? CurrentAmbient { get; private set; }

    private bool _currentMusicLoop = true;
    private bool _currentAmbientLoop = true;

    public AudioService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || IsInitialized)
        {
            return;
        }

        await EnsureInitializedAsync(cancellationToken);
    }

    public async Task PlayMusicAsync(BackgroundMusic track, bool loop = true, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        CurrentMusic = track;
        _currentMusicLoop = loop;

        if (!await EnsureInitializedAsync(cancellationToken))
        {
            return;
        }

        string path = PymGameOrbitAssets.GetMusicPath(track);
        await InvokeSafeVoidAsync("playMusic", cancellationToken, track.ToString(), path, loop);
    }

    public async Task StopMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        CurrentMusic = null;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("stopMusic", cancellationToken);
    }

    public async Task PauseMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("pauseMusic", cancellationToken);
    }

    public async Task ResumeMusicAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("resumeMusic", cancellationToken);
    }

    public async Task SetMusicVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        MusicVolume = Math.Clamp(volume, 0.0, 1.0);

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("setMusicVolume", cancellationToken, MusicVolume);
    }

    public async Task PlaySoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        string path = PymGameOrbitAssets.GetSoundEffectPath(effect);
        await InvokeSafeVoidAsync("playSoundEffect", cancellationToken, effect.ToString(), path);
    }

    public async Task StopSoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("stopSoundEffect", cancellationToken, effect.ToString());
    }

    public async Task StopAllSoundEffectsAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("stopAllSoundEffects", cancellationToken);
    }

    public async Task SetSoundEffectsVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        SoundEffectsVolume = Math.Clamp(volume, 0.0, 1.0);

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("setSoundEffectsVolume", cancellationToken, SoundEffectsVolume);
    }

    public async Task PlayAmbientAsync(SoundEffect ambientEffect = SoundEffect.BioshipCruisingLoop, bool loop = true, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        CurrentAmbient = ambientEffect;
        _currentAmbientLoop = loop;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        string path = PymGameOrbitAssets.GetSoundEffectPath(ambientEffect);
        await InvokeSafeVoidAsync("playAmbient", cancellationToken, ambientEffect.ToString(), path, loop);
    }

    public async Task StopAmbientAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        CurrentAmbient = null;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("stopAmbient", cancellationToken);
    }

    public async Task SetMasterVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        MasterVolume = Math.Clamp(volume, 0.0, 1.0);

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("setMasterVolume", cancellationToken, MasterVolume);
    }

    public async Task SetMutedAsync(bool muted, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        IsMuted = muted;

        if (!await EnsureInitializedAsync(cancellationToken)) return;

        await InvokeSafeVoidAsync("setMuted", cancellationToken, IsMuted);
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

        // Active gameplay: keep continuous Bio-Ship / Outpost cabin hum running on Ambient channel
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

        // Determine primary SFX for the resolved exploration action
        var actionSfx = ResolveActionSoundEffect(resolvedEvent, selectedOption);
        await PlaySoundEffectAsync(actionSfx, cancellationToken);

        // Advancing a turn draws up to 2 cards into the player's hand and flips the next dilemma
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

    private async Task<bool> EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_disposed) return false;
        if (IsInitialized && (_useGlobalAudioService || _jsModule is not null))
        {
            return true;
        }

        await _moduleLock.WaitAsync(cancellationToken);
        try
        {
            if (_disposed) return false;
            if (IsInitialized && (_useGlobalAudioService || _jsModule is not null))
            {
                return true;
            }

            AudioPersistedSettingsDto? persisted = null;

            try
            {
                persisted = await _jsRuntime.InvokeAsync<AudioPersistedSettingsDto?>(
                    "window.pymAudioService.initialize",
                    cancellationToken);
                _useGlobalAudioService = true;
            }
            catch (JSException)
            {
                _jsModule ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    cancellationToken,
                    PymGameOrbitAssets.AudioJsModulePath);

                persisted = await _jsModule.InvokeAsync<AudioPersistedSettingsDto?>(
                    "initialize",
                    cancellationToken);
                _useGlobalAudioService = false;
            }

            if (persisted is not null)
            {
                MasterVolume = Math.Clamp(persisted.MasterVolume, 0.0, 1.0);
                MusicVolume = Math.Clamp(persisted.MusicVolume, 0.0, 1.0);
                SoundEffectsVolume = Math.Clamp(persisted.SfxVolume, 0.0, 1.0);
                IsMuted = persisted.IsMuted;
            }

            IsInitialized = true;

            // If music or ambient playback was requested before JS became interactive, start it now
            if (CurrentMusic is { } pendingTrack)
            {
                string trackPath = PymGameOrbitAssets.GetMusicPath(pendingTrack);
                await InvokeSafeVoidAsync("playMusic", cancellationToken, pendingTrack.ToString(), trackPath, _currentMusicLoop);
            }

            if (CurrentAmbient is { } pendingAmbient)
            {
                string ambientPath = PymGameOrbitAssets.GetSoundEffectPath(pendingAmbient);
                await InvokeSafeVoidAsync("playAmbient", cancellationToken, pendingAmbient.ToString(), ambientPath, _currentAmbientLoop);
            }

            return true;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or JSException or TaskCanceledException or OperationCanceledException)
        {
            // During static prerendering or disconnection, JS interop may not be available yet.
            return false;
        }
        finally
        {
            _moduleLock.Release();
        }
    }

    private async Task InvokeSafeVoidAsync(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] args)
    {
        try
        {
            if (_useGlobalAudioService)
            {
                await _jsRuntime.InvokeVoidAsync($"window.pymAudioService.{identifier}", cancellationToken, args);
            }
            else if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync(identifier, cancellationToken, args);
            }
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or JSException or TaskCanceledException or OperationCanceledException)
        {
            // Gracefully ignore JS disconnection or browser playback restrictions
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or JSException or TaskCanceledException or OperationCanceledException)
            {
                // Ignore disposal errors when circuit or webview is already torn down
            }
            finally
            {
                _jsModule = null;
            }
        }

        _moduleLock.Dispose();
    }

    public sealed class AudioPersistedSettingsDto
    {
        [JsonPropertyName("masterVolume")]
        public double MasterVolume { get; set; } = 0.85;

        [JsonPropertyName("musicVolume")]
        public double MusicVolume { get; set; } = 0.70;

        [JsonPropertyName("sfxVolume")]
        public double SfxVolume { get; set; } = 0.85;

        [JsonPropertyName("isMuted")]
        public bool IsMuted { get; set; }
    }
}

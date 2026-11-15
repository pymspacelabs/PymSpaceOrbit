using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Abstractions.Game;

/// <summary>
/// Cross-platform audio service abstraction supporting background music (BGM),
/// simultaneous sound effects (SFX), continuous ambient cabin loops, volume persistence,
/// centralized screen/gameplay music selection, and browser autoplay recovery.
/// </summary>
public interface IAudioService : IAsyncDisposable
{
    bool IsInitialized { get; }
    double MasterVolume { get; }
    double MusicVolume { get; }
    double SoundEffectsVolume { get; }
    bool IsMuted { get; }
    BackgroundMusic? CurrentMusic { get; }
    SoundEffect? CurrentAmbient { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    // Background Music (BGM) Channel
    Task PlayMusicAsync(BackgroundMusic track, bool loop = true, CancellationToken cancellationToken = default);
    Task StopMusicAsync(CancellationToken cancellationToken = default);
    Task PauseMusicAsync(CancellationToken cancellationToken = default);
    Task ResumeMusicAsync(CancellationToken cancellationToken = default);
    Task SetMusicVolumeAsync(double volume, CancellationToken cancellationToken = default);

    // Sound Effects (SFX) Channel
    Task PlaySoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default);
    Task StopSoundEffectAsync(SoundEffect effect, CancellationToken cancellationToken = default);
    Task StopAllSoundEffectsAsync(CancellationToken cancellationToken = default);
    Task SetSoundEffectsVolumeAsync(double volume, CancellationToken cancellationToken = default);

    // Ambient Channel
    Task PlayAmbientAsync(SoundEffect ambientEffect = SoundEffect.BioshipCruisingLoop, bool loop = true, CancellationToken cancellationToken = default);
    Task StopAmbientAsync(CancellationToken cancellationToken = default);

    // Global Controls
    Task SetMasterVolumeAsync(double volume, CancellationToken cancellationToken = default);
    Task SetMutedAsync(bool muted, CancellationToken cancellationToken = default);

    // Centralized Screen & Gameplay Audio Selection / Event Triggers
    Task SyncScreenMusicAsync(string? routeOrUri, GameSessionState? session = null, CancellationToken cancellationToken = default);
    Task SyncGameplayMusicAsync(GameSessionState session, CancellationToken cancellationToken = default);
    Task TriggerCardEquippedAudioAsync(GameCardModel card, OutpostDestination destination, CancellationToken cancellationToken = default);
    Task TriggerExplorationEventAudioAsync(TurnExplorationEvent explorationEvent, GameSessionState session, CancellationToken cancellationToken = default);
    Task TriggerExplorationDecisionAudioAsync(
        TurnExplorationEvent resolvedEvent,
        ExplorationChoiceOption selectedOption,
        TurnResolutionResult resolutionResult,
        GameSessionState session,
        CancellationToken cancellationToken = default);
}

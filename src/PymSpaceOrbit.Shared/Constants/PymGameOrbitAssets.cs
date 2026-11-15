using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Constants;

/// <summary>
/// Centralized asset registry for shared visual assets (_content/PymSpaceOrbit.Shared/assets/)
/// and platform-hosted audio assets (audio/bgm/ and audio/sfx/).
/// </summary>
public static class PymGameOrbitAssets
{
    public const string BasePath = "_content/PymSpaceOrbit.Shared/assets/";
    public const string AudioBgmBasePath = "audio/bgm/";
    public const string AudioSfxBasePath = "audio/sfx/";
    public const string AudioJsModulePath = "./js/audioService.js";

    // 16:9 Screen Backgrounds
    public const string ScreenMainMenu = $"{BasePath}ui_screen_main_menu.jpg";
    public const string ScreenOptions = $"{BasePath}ui_screen_options.jpg";
    public const string ScreenInfoCardSets = $"{BasePath}ui_screen_info_cardsets.jpg";
    public const string ScreenGameplayBoard = $"{BasePath}ui_screen_gameplay_board.jpg";
    public const string ScreenGameplayBoardWithDecks = $"{BasePath}ui_screen_gameplay_board_with_decks.jpg";

    // 2:3 Card Templates
    public const string CardBack = $"{BasePath}back_pymgameorbit.jpg";
    public const string Deck1ShipBuild = $"{BasePath}deck1_ship_build.jpg";
    public const string Deck2Eclss = $"{BasePath}deck2_eclss.jpg";
    public const string Deck3Environment = $"{BasePath}deck3_environment.jpg";
    public const string Deck4OutpostDecisions = $"{BasePath}deck4_outpost_decisions.jpg";
    public const string Deck5SpaceWeather = $"{BasePath}deck5_space_weather.jpg";

    // Background Music (BGM) File Names
    public const string BgmMainMenuFile = "bgm_main_menu.mp3";
    public const string BgmOptionsFile = "bgm_options.mp3";
    public const string BgmInfoCardSetsFile = "bgm_info_cardsets.mp3";
    public const string BgmGameplayBuildFile = "bgm_gameplay_build.mp3";
    public const string BgmGameplayTurnsFile = "bgm_gameplay_turns.mp3";
    public const string BgmDonkiSolarStormFile = "bgm_donki_solar_storm.mp3";
    public const string BgmVictoryReturnFile = "bgm_victory_return.mp3";
    public const string BgmGameOverFile = "bgm_gameover.mp3";

    // Background Music (BGM) Relative Paths
    public const string BgmMainMenu = $"{AudioBgmBasePath}{BgmMainMenuFile}";
    public const string BgmOptions = $"{AudioBgmBasePath}{BgmOptionsFile}";
    public const string BgmInfoCardSets = $"{AudioBgmBasePath}{BgmInfoCardSetsFile}";
    public const string BgmGameplayBuild = $"{AudioBgmBasePath}{BgmGameplayBuildFile}";
    public const string BgmGameplayTurns = $"{AudioBgmBasePath}{BgmGameplayTurnsFile}";
    public const string BgmDonkiSolarStorm = $"{AudioBgmBasePath}{BgmDonkiSolarStormFile}";
    public const string BgmVictoryReturn = $"{AudioBgmBasePath}{BgmVictoryReturnFile}";
    public const string BgmGameOver = $"{AudioBgmBasePath}{BgmGameOverFile}";

    // Sound Effects (SFX) & Ambient File Names
    public const string SfxUiButtonTapFile = "sfx_ui_button_tap.mp3";
    public const string SfxBioshipCruisingLoopFile = "sfx_bioship_cruising_loop.mp3";
    public const string SfxCardDrawFlipFile = "sfx_card_draw_flip.mp3";
    public const string SfxCardEquipLockFile = "sfx_card_equip_lock.mp3";
    public const string SfxTelemetryNotificationFile = "sfx_telemetry_notification.mp3";
    public const string SfxStageTransitionRiserFile = "sfx_stage_transition_riser.mp3";
    public const string SfxSolarStormImpactFile = "sfx_solar_storm_impact.mp3";
    public const string SfxMarsLunarWindFile = "sfx_mars_lunar_wind.mp3";
    public const string SfxEvaRegolithFootstepsFile = "sfx_eva_regolith_footsteps.mp3";
    public const string SfxIssMetalFootstepsFile = "sfx_iss_metal_footsteps.mp3";
    public const string SfxAirlockHatchOpenFile = "sfx_airlock_hatch_open.mp3";
    public const string SfxEclssWaterBioreactorFile = "sfx_eclss_water_bioreactor.mp3";

    // Sound Effects (SFX) & Ambient Relative Paths
    public const string SfxUiButtonTap = $"{AudioSfxBasePath}{SfxUiButtonTapFile}";
    public const string SfxBioshipCruisingLoop = $"{AudioSfxBasePath}{SfxBioshipCruisingLoopFile}";
    public const string SfxCardDrawFlip = $"{AudioSfxBasePath}{SfxCardDrawFlipFile}";
    public const string SfxCardEquipLock = $"{AudioSfxBasePath}{SfxCardEquipLockFile}";
    public const string SfxTelemetryNotification = $"{AudioSfxBasePath}{SfxTelemetryNotificationFile}";
    public const string SfxStageTransitionRiser = $"{AudioSfxBasePath}{SfxStageTransitionRiserFile}";
    public const string SfxSolarStormImpact = $"{AudioSfxBasePath}{SfxSolarStormImpactFile}";
    public const string SfxMarsLunarWind = $"{AudioSfxBasePath}{SfxMarsLunarWindFile}";
    public const string SfxEvaRegolithFootsteps = $"{AudioSfxBasePath}{SfxEvaRegolithFootstepsFile}";
    public const string SfxIssMetalFootsteps = $"{AudioSfxBasePath}{SfxIssMetalFootstepsFile}";
    public const string SfxAirlockHatchOpen = $"{AudioSfxBasePath}{SfxAirlockHatchOpenFile}";
    public const string SfxEclssWaterBioreactor = $"{AudioSfxBasePath}{SfxEclssWaterBioreactorFile}";

    public static readonly IReadOnlyDictionary<BackgroundMusic, string> BackgroundMusicPaths =
        new Dictionary<BackgroundMusic, string>
        {
            [BackgroundMusic.MainMenu] = BgmMainMenu,
            [BackgroundMusic.Options] = BgmOptions,
            [BackgroundMusic.InfoCardSets] = BgmInfoCardSets,
            [BackgroundMusic.GameplayBuild] = BgmGameplayBuild,
            [BackgroundMusic.GameplayTurns] = BgmGameplayTurns,
            [BackgroundMusic.DonkiSolarStorm] = BgmDonkiSolarStorm,
            [BackgroundMusic.VictoryReturn] = BgmVictoryReturn,
            [BackgroundMusic.GameOver] = BgmGameOver
        };

    public static readonly IReadOnlyDictionary<BackgroundMusic, string> BackgroundMusicFileNames =
        new Dictionary<BackgroundMusic, string>
        {
            [BackgroundMusic.MainMenu] = BgmMainMenuFile,
            [BackgroundMusic.Options] = BgmOptionsFile,
            [BackgroundMusic.InfoCardSets] = BgmInfoCardSetsFile,
            [BackgroundMusic.GameplayBuild] = BgmGameplayBuildFile,
            [BackgroundMusic.GameplayTurns] = BgmGameplayTurnsFile,
            [BackgroundMusic.DonkiSolarStorm] = BgmDonkiSolarStormFile,
            [BackgroundMusic.VictoryReturn] = BgmVictoryReturnFile,
            [BackgroundMusic.GameOver] = BgmGameOverFile
        };

    public static readonly IReadOnlyDictionary<SoundEffect, string> SoundEffectPaths =
        new Dictionary<SoundEffect, string>
        {
            [SoundEffect.UiButtonTap] = SfxUiButtonTap,
            [SoundEffect.BioshipCruisingLoop] = SfxBioshipCruisingLoop,
            [SoundEffect.CardDrawFlip] = SfxCardDrawFlip,
            [SoundEffect.CardEquipLock] = SfxCardEquipLock,
            [SoundEffect.TelemetryNotification] = SfxTelemetryNotification,
            [SoundEffect.StageTransitionRiser] = SfxStageTransitionRiser,
            [SoundEffect.SolarStormImpact] = SfxSolarStormImpact,
            [SoundEffect.MarsLunarWind] = SfxMarsLunarWind,
            [SoundEffect.EvaRegolithFootsteps] = SfxEvaRegolithFootsteps,
            [SoundEffect.IssMetalFootsteps] = SfxIssMetalFootsteps,
            [SoundEffect.AirlockHatchOpen] = SfxAirlockHatchOpen,
            [SoundEffect.EclssWaterBioreactor] = SfxEclssWaterBioreactor
        };

    public static readonly IReadOnlyDictionary<SoundEffect, string> SoundEffectFileNames =
        new Dictionary<SoundEffect, string>
        {
            [SoundEffect.UiButtonTap] = SfxUiButtonTapFile,
            [SoundEffect.BioshipCruisingLoop] = SfxBioshipCruisingLoopFile,
            [SoundEffect.CardDrawFlip] = SfxCardDrawFlipFile,
            [SoundEffect.CardEquipLock] = SfxCardEquipLockFile,
            [SoundEffect.TelemetryNotification] = SfxTelemetryNotificationFile,
            [SoundEffect.StageTransitionRiser] = SfxStageTransitionRiserFile,
            [SoundEffect.SolarStormImpact] = SfxSolarStormImpactFile,
            [SoundEffect.MarsLunarWind] = SfxMarsLunarWindFile,
            [SoundEffect.EvaRegolithFootsteps] = SfxEvaRegolithFootstepsFile,
            [SoundEffect.IssMetalFootsteps] = SfxIssMetalFootstepsFile,
            [SoundEffect.AirlockHatchOpen] = SfxAirlockHatchOpenFile,
            [SoundEffect.EclssWaterBioreactor] = SfxEclssWaterBioreactorFile
        };

    public static string GetMusicPath(BackgroundMusic music) =>
        BackgroundMusicPaths.TryGetValue(music, out var path) ? path : BgmMainMenu;

    public static string GetMusicFileName(BackgroundMusic music) =>
        BackgroundMusicFileNames.TryGetValue(music, out var file) ? file : BgmMainMenuFile;

    public static string GetSoundEffectPath(SoundEffect effect) =>
        SoundEffectPaths.TryGetValue(effect, out var path) ? path : SfxUiButtonTap;

    public static string GetSoundEffectFileName(SoundEffect effect) =>
        SoundEffectFileNames.TryGetValue(effect, out var file) ? file : SfxUiButtonTapFile;
}

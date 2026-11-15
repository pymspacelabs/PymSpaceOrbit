using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Constants;

/// <summary>
/// Centralized asset paths pointing directly to the Razor Class Library assets
/// served under _content/PymSpaceOrbit.Shared/ without requiring file duplication.
/// </summary>
public static class PymGameOrbitAssets
{
    public const string BasePath = "_content/PymSpaceOrbit.Shared/assets/";
    public const string AudioBgmBasePath = "_content/PymSpaceOrbit.Shared/audio/bgm/";
    public const string AudioSfxBasePath = "_content/PymSpaceOrbit.Shared/audio/sfx/";
    public const string AudioJsModulePath = "./_content/PymSpaceOrbit.Shared/js/audioService.js";

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

    // Background Music (BGM) Assets
    public const string BgmMainMenu = $"{AudioBgmBasePath}bgm_main_menu.mp3";
    public const string BgmOptions = $"{AudioBgmBasePath}bgm_options.mp3";
    public const string BgmInfoCardSets = $"{AudioBgmBasePath}bgm_info_cardsets.mp3";
    public const string BgmGameplayBuild = $"{AudioBgmBasePath}bgm_gameplay_build.mp3";
    public const string BgmGameplayTurns = $"{AudioBgmBasePath}bgm_gameplay_turns.mp3";
    public const string BgmDonkiSolarStorm = $"{AudioBgmBasePath}bgm_donki_solar_storm.mp3";
    public const string BgmVictoryReturn = $"{AudioBgmBasePath}bgm_victory_return.mp3";
    public const string BgmGameOver = $"{AudioBgmBasePath}bgm_gameover.mp3";

    // Sound Effects (SFX) & Ambient Assets
    public const string SfxUiButtonTap = $"{AudioSfxBasePath}sfx_ui_button_tap.mp3";
    public const string SfxBioshipCruisingLoop = $"{AudioSfxBasePath}sfx_bioship_cruising_loop.mp3";
    public const string SfxCardDrawFlip = $"{AudioSfxBasePath}sfx_card_draw_flip.mp3";
    public const string SfxCardEquipLock = $"{AudioSfxBasePath}sfx_card_equip_lock.mp3";
    public const string SfxTelemetryNotification = $"{AudioSfxBasePath}sfx_telemetry_notification.mp3";
    public const string SfxStageTransitionRiser = $"{AudioSfxBasePath}sfx_stage_transition_riser.mp3";
    public const string SfxSolarStormImpact = $"{AudioSfxBasePath}sfx_solar_storm_impact.mp3";
    public const string SfxMarsLunarWind = $"{AudioSfxBasePath}sfx_mars_lunar_wind.mp3";
    public const string SfxEvaRegolithFootsteps = $"{AudioSfxBasePath}sfx_eva_regolith_footsteps.mp3";
    public const string SfxIssMetalFootsteps = $"{AudioSfxBasePath}sfx_iss_metal_footsteps.mp3";
    public const string SfxAirlockHatchOpen = $"{AudioSfxBasePath}sfx_airlock_hatch_open.mp3";
    public const string SfxEclssWaterBioreactor = $"{AudioSfxBasePath}sfx_eclss_water_bioreactor.mp3";

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

    public static string GetMusicPath(BackgroundMusic music) =>
        BackgroundMusicPaths.TryGetValue(music, out var path) ? path : BgmMainMenu;

    public static string GetSoundEffectPath(SoundEffect effect) =>
        SoundEffectPaths.TryGetValue(effect, out var path) ? path : SfxUiButtonTap;
}

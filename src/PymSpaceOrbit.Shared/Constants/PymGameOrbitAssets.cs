namespace PymSpaceOrbit.Shared.Constants;

/// <summary>
/// Centralized asset paths pointing directly to the Razor Class Library assets
/// served under _content/PymSpaceOrbit.Shared/assets/ without requiring file duplication.
/// </summary>
public static class PymGameOrbitAssets
{
    public const string BasePath = "_content/PymSpaceOrbit.Shared/assets/";

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
}

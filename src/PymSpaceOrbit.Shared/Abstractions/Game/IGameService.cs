using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Abstractions.Game;

/// <summary>
/// Turn-based PymGameOrbit Space Exploration & Outpost Card Game Service contract in PymSpaceOrbit.Shared.Abstractions.Game.
/// Players build their unified PymGameOrbit spacecraft & outpost base (minimum 5 cards) and receive initial ML parameter resources,
/// then play cards each turn and resolve educational space-exploration choices powered by IPymGameOrbitMlModelService.
/// </summary>
public interface IGameService
{
    GameSessionState CurrentSession { get; }

    Task<GameSessionState> StartNewCampaignAsync(
        OutpostDestination destination,
        int crewCount = 4,
        int minimumBaseBuildCards = 5,
        string locale = "en-US",
        CancellationToken cancellationToken = default);

    bool PlayCardFromHand(string cardId, out string statusMessage);

    bool CompleteInitialBaseConstruction(out string validationMessage);

    TurnExplorationEvent DrawTurnExplorationEvent();

    TurnResolutionResult ResolveExplorationDecisionAndAdvanceTurn(string selectedOptionId);

    IReadOnlyList<GameCardModel> GetAllUnifiedDeckCards();

    IReadOnlyList<TurnExplorationEvent> GetAllExplorationEvents();
}

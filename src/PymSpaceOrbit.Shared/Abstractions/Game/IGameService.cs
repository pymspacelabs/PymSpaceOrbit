using PymSpaceOrbit.Shared.Models.Game;

namespace PymSpaceOrbit.Shared.Abstractions.Game;

/// <summary>
/// Turn-based Bio-Ship & Outpost Card Game Service contract in PymSpaceOrbit.Shared.Abstractions.Game.
/// Players build their unified Bio-Ship/Outpost base (minimum 5 cards) and receive initial ML parameter resources,
/// then play cards each turn and resolve educational space-exploration choices powered by IPymSpaceOrbitMlModelService.
/// </summary>
public interface IGameService
{
    GameSessionState CurrentSession { get; }

    Task<GameSessionState> StartNewCampaignAsync(
        OutpostDestination destination,
        int crewCount = 4,
        int minimumBaseBuildCards = 5,
        string locale = "pt-BR",
        CancellationToken cancellationToken = default);

    bool PlayCardFromHand(string cardId, out string statusMessage);

    bool CompleteInitialBaseConstruction(out string validationMessage);

    TurnExplorationEvent DrawTurnExplorationEvent();

    TurnResolutionResult ResolveExplorationDecisionAndAdvanceTurn(string selectedOptionId);

    IReadOnlyList<GameCardModel> GetAllUnifiedDeckCards();

    IReadOnlyList<TurnExplorationEvent> GetAllExplorationEvents();
}

using PymSpaceOrbit.Shared.Models.ML;

namespace PymSpaceOrbit.Shared.Models.Game;

public sealed class GameSessionState
{
    public Guid SessionId { get; init; } = Guid.NewGuid();
    public OutpostDestination Destination { get; set; } = OutpostDestination.Moon;
    public string Locale { get; set; } = "pt-BR";
    public int CrewCount { get; set; } = 4;

    public int CurrentTurn { get; set; } = 1;
    public int MaxCampaignTurns { get; set; } = 10;
    public float CurrentMissionDay { get; set; } = 0f;
    public int CurrentFlightStageIndex { get; set; } = 1;
    public TurnPhase Phase { get; set; } = TurnPhase.BuildPhase;
    public MissionEndingOutcome Outcome { get; set; } = MissionEndingOutcome.InProgress;

    public int MinimumCardsToBuildBase { get; set; } = 5;
    public int MaxCardsPlayedPerTurn { get; set; } = 2;
    public int CardsPlayedThisTurn { get; set; } = 0;
    public int ScienceDiscoveryPoints { get; set; } = 0;

    public List<GameCardModel> DrawPile { get; set; } = new();
    public List<GameCardModel> PlayerHand { get; set; } = new();
    public List<GameCardModel> EquippedShipAndBaseCards { get; set; } = new();
    public List<GameCardModel> DiscardPile { get; set; } = new();

    public TurnExplorationEvent? PendingExplorationEvent { get; set; }
    public List<string> MissionJournalLogs { get; set; } = new();

    // 36-Feature ML.NET Resource & Telemetry State
    public PymSpaceOrbitFeatureVector Resources { get; set; } = new();
    public QuadMlEvaluationResult LatestMlPrediction { get; set; } = new(0.90f, 180f, 0.96f, 0.95f, 12, 24.0f, true);
}

public sealed record TurnResolutionResult(
    int TurnNumber,
    TurnPhase CurrentPhase,
    MissionEndingOutcome Outcome,
    string OutcomeTitleEn,
    string OutcomeTitlePtBr,
    string OutcomeSummaryEn,
    string OutcomeSummaryPtBr,
    string ScienceLessonLearnedEn,
    string ScienceLessonLearnedPtBr,
    QuadMlEvaluationResult MlPrediction,
    TurnExplorationEvent? NextEvent
);

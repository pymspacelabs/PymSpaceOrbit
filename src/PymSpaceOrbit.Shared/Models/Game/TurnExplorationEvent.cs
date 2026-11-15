namespace PymSpaceOrbit.Shared.Models.Game;

public sealed record ExplorationChoiceOption(
    string OptionId,
    string LabelEn,
    string LabelPtBr,
    string KidOutcomeLessonEn,
    string KidOutcomeLessonPtBr,
    string RequiredToolOrSubsystemHint,
    float WattCostDelta,
    float OxygenLitersDelta,
    float ShieldingGcm2Delta,
    float RadiationDoseSpike,
    float SuitWearPctDelta,
    float FoodKgDelta,
    float WaterLitersDelta,
    float MedsKgDelta,
    float EntropyDelta,
    int ScienceDiscoveryPointsBonus
);

public sealed record TurnExplorationEvent(
    string EventId,
    OutpostDestination TargetScope,
    int RecommendedMinTurn,
    string TitleEn,
    string TitlePtBr,
    string StoryPromptEn,
    string StoryPromptPtBr,
    string SpaceScienceFunFactEn,
    string SpaceScienceFunFactPtBr,
    string NasaApiReference,
    IReadOnlyList<ExplorationChoiceOption> Choices
);

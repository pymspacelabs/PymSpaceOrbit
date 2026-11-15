namespace PymSpaceOrbit.Shared.Models.Game;

public sealed record TechnicalTermNote(
    string Term,
    string KidFriendlyLabelEn,
    string KidFriendlyLabelPtBr,
    string ExplanationEn,
    string ExplanationPtBr
);

public sealed record GameCardModel(
    string CardId,
    CardSlotCategory Category,
    OutpostDestination? TargetAffinity,
    string NameEn,
    string NamePtBr,
    string GameUiLabelEn,
    string GameUiLabelPtBr,
    string KidSummaryEn,
    string KidSummaryPtBr,
    string NasaImageAssetUrl,
    float DryMassTonsDelta,
    float WattBudgetDelta,
    float OxygenLitersPerDayDelta,
    float ShieldingGcm2Delta,
    float CommDelaySecDelta,
    float WaterLoopEffBonus,
    float EclssSynergyBonus,
    float SuitDurabilityBonusPct,
    float CrewHealthBuffBonus,
    float BiomeShieldingBuffBonus,
    Dictionary<string, TechnicalTermNote> TechnicalGlossary,
    string? NasaImageApiUrl = null
);

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
    string DisplayNameEn,
    string DisplayNamePtBr,
    string OriginalNameEn,
    string OriginalNamePtBr,
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
)
{
    public string DisplayName => DisplayNameEn;
    public string OriginalName => OriginalNameEn;
    public string NameEn => DisplayNameEn;
    public string NamePtBr => DisplayNamePtBr;

    public string GetDisplayName(bool isPortuguese) =>
        isPortuguese
            ? (string.IsNullOrWhiteSpace(DisplayNamePtBr) ? DisplayNameEn : DisplayNamePtBr)
            : DisplayNameEn;

    public string GetOriginalName(bool isPortuguese) =>
        isPortuguese
            ? (string.IsNullOrWhiteSpace(OriginalNamePtBr) ? OriginalNameEn : OriginalNamePtBr)
            : OriginalNameEn;
}

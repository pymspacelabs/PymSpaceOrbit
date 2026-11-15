namespace PymSpaceOrbit.Shared.Models.ML;

/// <summary>
/// Represents the 36 input parameters fed into the 240,000-sample Quad-Ensemble FastTree model.
/// </summary>
public sealed class PymSpaceOrbitFeatureVector
{
    public float CrewCount { get; set; } = 4f;
    public float BioreactorVolumeLiters { get; set; } = 3200f;
    public float WaterRecoveryEfficiency { get; set; } = 0.920f;
    public float BrineRecoveryEfficiency { get; set; } = 0.860f;
    public float OrganicBiomass { get; set; } = 92.0f;
    public float TechPower { get; set; } = 90.0f;
    public float EntropyAccumulated { get; set; } = 0.04f;
    public float EffectiveRadiationIndex { get; set; } = 1.15f;
    public float BioreactorThrottle { get; set; } = 0.80f;
    public float TechToBioAllocation { get; set; } = 0.82f;
    public float TargetLog10DistanceKm { get; set; } = 5.5848f;
    public float TargetSolarIrradianceFactor { get; set; } = 1.000f;
    public float TargetMissionDeltaVms { get; set; } = 13850f;
    public float TargetSurfaceGravityMs2 { get; set; } = 1.62f;
    public float CrewProtectionBuff { get; set; } = 0.35f;
    public float BiomeShieldingBuff { get; set; } = 0.35f;
    public float TotalShipMassTons { get; set; } = 58.0f;
    public float FoodAndProvisionsKg { get; set; } = 1400f;
    public float PotableWaterReserveLiters { get; set; } = 1200f;
    public float MedicalSuppliesKg { get; set; } = 140f;
    public float ElectricalPowerKw { get; set; } = 85.0f;
    public float BatteryStorageKwh { get; set; } = 240.0f;
    public float ThermalRejectionCapacityKw { get; set; } = 110.0f;
    public float RadShieldingGcm2 { get; set; } = 14.0f;
    public float MicrometeoroidWhippleLayers { get; set; } = 3f;
    public float ArtificialGravityG { get; set; } = 0.0f;
    public float EvaSuitDurabilityPct { get; set; } = 92.0f;
    public float ToolsAndSparesKg { get; set; } = 260.0f;
    public float CommsBandwidthGbps { get; set; } = 2.5f;
    public float OgaCapacityKgPerDay { get; set; } = 4.2f;
    public float CdraScrubbingCapacityKgPerDay { get; set; } = 4.8f;
    public float EclssSynergyIndex { get; set; } = 0.45f;
    public float OutpostWattBudgetDelta { get; set; } = 4000f;
    public float OutpostOxygenLitersPerDayDelta { get; set; } = 1200f;
    public float OutpostShieldingGcm2Delta { get; set; } = 10.0f;
    public float CommunicationDelaySec { get; set; } = 1.28f;

    public float[] ToFloatArray() =>
    [
        CrewCount, BioreactorVolumeLiters, WaterRecoveryEfficiency, BrineRecoveryEfficiency,
        OrganicBiomass, TechPower, EntropyAccumulated, EffectiveRadiationIndex,
        BioreactorThrottle, TechToBioAllocation, TargetLog10DistanceKm, TargetSolarIrradianceFactor,
        TargetMissionDeltaVms, TargetSurfaceGravityMs2, CrewProtectionBuff, BiomeShieldingBuff,
        TotalShipMassTons, FoodAndProvisionsKg, PotableWaterReserveLiters, MedicalSuppliesKg,
        ElectricalPowerKw, BatteryStorageKwh, ThermalRejectionCapacityKw, RadShieldingGcm2,
        MicrometeoroidWhippleLayers, ArtificialGravityG, EvaSuitDurabilityPct, ToolsAndSparesKg,
        CommsBandwidthGbps, OgaCapacityKgPerDay, CdraScrubbingCapacityKgPerDay, EclssSynergyIndex,
        OutpostWattBudgetDelta, OutpostOxygenLitersPerDayDelta, OutpostShieldingGcm2Delta, CommunicationDelaySec
    ];
}

public sealed record QuadMlEvaluationResult(
    float FuelYieldRate,
    float MaxExplorationStayDays,
    float CrewHealthSurvivalProb,
    float SystemReliabilityIndex,
    int MaxSupportedCrew,
    float SafeReturnFuelReservePct,
    bool IsLoadedFromEnsembleJson
);

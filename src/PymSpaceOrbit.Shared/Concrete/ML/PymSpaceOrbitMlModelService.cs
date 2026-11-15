using System.Net.Http.Json;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Models.Game;
using PymSpaceOrbit.Shared.Models.ML;

namespace PymSpaceOrbit.Shared.Concrete.ML;

public sealed class PymSpaceOrbitMlModelService : IPymSpaceOrbitMlModelService
{
    private readonly HttpClient _httpClient;
    private QuadFastTreeArtifact? _ensemble;

    public bool IsInitialized { get; private set; }
    public string ModelStatusDescription { get; private set; } = "Surrogate Biophysical Engine (36 features)";
    public string LatestDonkiSolarFlareClass { get; private set; } = "M1.4";
    public float SolarWeatherRadiationMultiplier { get; private set; } = 1.15f;

    public PymSpaceOrbitMlModelService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync(
        string? customEnsembleUrl = null,
        string? customSnapshotUrl = null,
        CancellationToken cancellationToken = default)
    {
        string[] candidateEnsembleUrls = customEnsembleUrl is not null
            ? [customEnsembleUrl]
            : ["ml/PymSpaceOrbit_fasttree_ensemble.json", "MLArtifacts/PymSpaceOrbit_fasttree_ensemble.json"];

        foreach (var url in candidateEnsembleUrls)
        {
            try
            {
                var resp = await _httpClient.GetAsync(url, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var artifact = await resp.Content.ReadFromJsonAsync<QuadFastTreeArtifact>(cancellationToken: cancellationToken);
                    if (artifact is not null && (artifact.YieldEnsemble?.Trees.Count > 0 || artifact.Trees.Count > 0))
                    {
                        _ensemble = artifact;
                        IsInitialized = true;
                        int trees = (artifact.YieldEnsemble?.Trees.Count ?? artifact.Trees.Count) +
                                    (artifact.DaysEnsemble?.Trees.Count ?? 0) +
                                    (artifact.HealthEnsemble?.Trees.Count ?? 0) +
                                    (artifact.ReliabilityEnsemble?.Trees.Count ?? 0);
                        ModelStatusDescription = $"240k Quad-FastTree JSON ({trees} trees / 36 features)";
                        break;
                    }
                }
            }
            catch { }
        }

        string[] candidateSnapshotUrls = customSnapshotUrl is not null
            ? [customSnapshotUrl]
            : ["ml/PymSpaceOrbit_nasa_snapshot.json", "MLArtifacts/PymSpaceOrbit_nasa_snapshot.json"];

        foreach (var url in candidateSnapshotUrls)
        {
            try
            {
                var resp = await _httpClient.GetAsync(url, cancellationToken);
                if (resp.IsSuccessStatusCode)
                {
                    var snap = await resp.Content.ReadFromJsonAsync<NasaSnapshotArtifactDto>(cancellationToken: cancellationToken);
                    if (snap is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(snap.LatestSolarFlareClass))
                            LatestDonkiSolarFlareClass = snap.LatestSolarFlareClass;
                        if (snap.BaseSolarWeatherMultiplier > 0)
                            SolarWeatherRadiationMultiplier = snap.BaseSolarWeatherMultiplier;
                        break;
                    }
                }
            }
            catch { }
        }

        IsInitialized = true;
    }

    public PymSpaceOrbitFeatureVector CreateInitialResourceSlice(OutpostDestination destination, int crewCount)
    {
        return destination switch
        {
            OutpostDestination.ISS => new PymSpaceOrbitFeatureVector
            {
                CrewCount = crewCount,
                BioreactorVolumeLiters = 2600f,
                WaterRecoveryEfficiency = 0.935f,
                BrineRecoveryEfficiency = 0.880f,
                OrganicBiomass = 88.0f,
                TechPower = 88.0f,
                EntropyAccumulated = 0.04f,
                EffectiveRadiationIndex = 0.85f * SolarWeatherRadiationMultiplier,
                TargetLog10DistanceKm = MathF.Log10(414.2f),
                TargetSolarIrradianceFactor = 1.000f,
                TargetMissionDeltaVms = 9800f,
                TargetSurfaceGravityMs2 = 8.69f,
                TotalShipMassTons = 52.0f,
                FoodAndProvisionsKg = 1100f,
                PotableWaterReserveLiters = 1000f,
                MedicalSuppliesKg = 110f,
                ElectricalPowerKw = 78.0f,
                BatteryStorageKwh = 210.0f,
                ThermalRejectionCapacityKw = 105.0f,
                RadShieldingGcm2 = 12.0f,
                MicrometeoroidWhippleLayers = 3f,
                EvaSuitDurabilityPct = 90.0f,
                ToolsAndSparesKg = 220.0f,
                CommsBandwidthGbps = 4.0f,
                OgaCapacityKgPerDay = 4.5f,
                CdraScrubbingCapacityKgPerDay = 5.0f,
                EclssSynergyIndex = 0.48f,
                OutpostWattBudgetDelta = 4000f,
                OutpostOxygenLitersPerDayDelta = 900f,
                OutpostShieldingGcm2Delta = 8.0f,
                CommunicationDelaySec = 0.08f
            },
            OutpostDestination.Moon => new PymSpaceOrbitFeatureVector
            {
                CrewCount = crewCount,
                BioreactorVolumeLiters = 3000f,
                WaterRecoveryEfficiency = 0.925f,
                BrineRecoveryEfficiency = 0.865f,
                OrganicBiomass = 86.0f,
                TechPower = 86.0f,
                EntropyAccumulated = 0.05f,
                EffectiveRadiationIndex = 1.15f * SolarWeatherRadiationMultiplier,
                TargetLog10DistanceKm = MathF.Log10(384400f),
                TargetSolarIrradianceFactor = 1.000f,
                TargetMissionDeltaVms = 13850f,
                TargetSurfaceGravityMs2 = 1.62f,
                TotalShipMassTons = 62.0f,
                FoodAndProvisionsKg = 1400f,
                PotableWaterReserveLiters = 1200f,
                MedicalSuppliesKg = 140f,
                ElectricalPowerKw = 84.0f,
                BatteryStorageKwh = 240.0f,
                ThermalRejectionCapacityKw = 115.0f,
                RadShieldingGcm2 = 14.0f,
                MicrometeoroidWhippleLayers = 3f,
                EvaSuitDurabilityPct = 88.0f,
                ToolsAndSparesKg = 260.0f,
                CommsBandwidthGbps = 2.5f,
                OgaCapacityKgPerDay = 4.2f,
                CdraScrubbingCapacityKgPerDay = 4.8f,
                EclssSynergyIndex = 0.45f,
                OutpostWattBudgetDelta = 3000f,
                OutpostOxygenLitersPerDayDelta = 1100f,
                OutpostShieldingGcm2Delta = 10.0f,
                CommunicationDelaySec = 1.28f
            },
            _ => new PymSpaceOrbitFeatureVector
            {
                CrewCount = crewCount,
                BioreactorVolumeLiters = 3600f,
                WaterRecoveryEfficiency = 0.930f,
                BrineRecoveryEfficiency = 0.870f,
                OrganicBiomass = 85.0f,
                TechPower = 85.0f,
                EntropyAccumulated = 0.06f,
                EffectiveRadiationIndex = 1.55f * SolarWeatherRadiationMultiplier,
                TargetLog10DistanceKm = MathF.Log10(78340000f),
                TargetSolarIrradianceFactor = 0.431f,
                TargetMissionDeltaVms = 17900f,
                TargetSurfaceGravityMs2 = 3.71f,
                TotalShipMassTons = 74.0f,
                FoodAndProvisionsKg = 1900f,
                PotableWaterReserveLiters = 1500f,
                MedicalSuppliesKg = 180f,
                ElectricalPowerKw = 92.0f,
                BatteryStorageKwh = 280.0f,
                ThermalRejectionCapacityKw = 125.0f,
                RadShieldingGcm2 = 16.0f,
                MicrometeoroidWhippleLayers = 4f,
                EvaSuitDurabilityPct = 86.0f,
                ToolsAndSparesKg = 310.0f,
                CommsBandwidthGbps = 1.8f,
                OgaCapacityKgPerDay = 4.6f,
                CdraScrubbingCapacityKgPerDay = 5.2f,
                EclssSynergyIndex = 0.46f,
                OutpostWattBudgetDelta = 2500f,
                OutpostOxygenLitersPerDayDelta = 1200f,
                OutpostShieldingGcm2Delta = 12.0f,
                CommunicationDelaySec = 420.0f
            }
        };
    }

    public QuadMlEvaluationResult EvaluateState(PymSpaceOrbitFeatureVector features, OutpostDestination destination)
    {
        float[] vec = features.ToFloatArray();
        var phys = ComputeReferenceQuad(vec);

        float yield = phys.Yield;
        float stayDays = phys.StayDays;
        float health = phys.HealthProb;
        float reliability = phys.Reliability;
        bool fromJson = false;

        if (_ensemble?.YieldEnsemble is { Trees.Count: > 0 } yTrees &&
            _ensemble?.DaysEnsemble is { Trees.Count: > 0 } dTrees)
        {
            fromJson = true;
            yield = Math.Clamp(yTrees.Predict(vec), 0.14f, 0.99f);
            stayDays = Math.Clamp(dTrees.Predict(vec), 12f, 720f);
            if (_ensemble.HealthEnsemble is { Trees.Count: > 0 } hTrees)
                health = Math.Clamp(hTrees.Predict(vec), 0.18f, 0.996f);
            if (_ensemble.ReliabilityEnsemble is { Trees.Count: > 0 } rTrees)
                reliability = Math.Clamp(rTrees.Predict(vec), 0.20f, 0.995f);
        }

        float totalO2KgDay = features.OgaCapacityKgPerDay +
                             (features.OutpostOxygenLitersPerDayDelta / 700f) +
                             ((features.BioreactorVolumeLiters / 420f) * 0.84f * (features.OrganicBiomass / 100f));
        int maxCrew = Math.Clamp((int)Math.Floor(totalO2KgDay / (0.84f * (1.0f - features.CrewProtectionBuff * 0.20f))), 1, 24);

        float floorPct = destination == OutpostDestination.ISS ? 16.0f : (destination == OutpostDestination.Moon ? 24.0f : 30.0f);
        float safeReturnFuel = Math.Clamp(floorPct + (features.TargetMissionDeltaVms / 2200f) + (features.TotalShipMassTons / 25f), 18.0f, 52.0f);

        return new QuadMlEvaluationResult(yield, stayDays, health, reliability, maxCrew, safeReturnFuel, fromJson);
    }

    public MissionEndingOutcome EvaluateMissionOutcome(
        PymSpaceOrbitFeatureVector f,
        QuadMlEvaluationResult pred,
        OutpostDestination destination,
        int currentTurn,
        int maxTurns)
    {
        float safeYieldFloor = destination == OutpostDestination.ISS ? 0.30f : (destination == OutpostDestination.Moon ? 0.34f : 0.36f);

        // Game Over #1: ECLSS / Water / Brine / xEMU Suit Collapse
        if (pred.SystemReliabilityIndex < 0.36f || f.EvaSuitDurabilityPct < 20.0f || f.WaterRecoveryEfficiency < 0.75f || f.EntropyAccumulated > 0.86f)
        {
            return MissionEndingOutcome.GameOverEclssAndSuitCollapse;
        }

        // Game Over #2: NASA DONKI Solar Storm & Radiation / Shielding Failure
        float totalShieldGcm2 = f.RadShieldingGcm2 + f.OutpostShieldingGcm2Delta;
        if (pred.CrewHealthSurvivalProb < 0.38f || (f.EffectiveRadiationIndex > 1.85f && totalShieldGcm2 < 24.0f) || f.MedicalSuppliesKg <= 5.0f)
        {
            return MissionEndingOutcome.GameOverDonkiSolarStormRadiation;
        }

        // Game Over #3: CO2 Scrubbing (CDRA) Acidosis & Caloric/Oxygen Starvation
        float totalO2KgDay = f.OgaCapacityKgPerDay + (f.OutpostOxygenLitersPerDayDelta / 700f) + ((f.BioreactorVolumeLiters / 420f) * 0.84f);
        if (f.CrewCount > pred.MaxSupportedCrew + 2 || totalO2KgDay < f.CrewCount * 0.65f || f.FoodAndProvisionsKg <= 40.0f || f.PotableWaterReserveLiters <= 40.0f)
        {
            return MissionEndingOutcome.GameOverCo2AndCaloricOverload;
        }

        // Nominal Success: Completed all campaign turns or triggered Safe Rate-Limit return with healthy crew & systems
        if (currentTurn >= maxTurns || pred.FuelYieldRate <= safeYieldFloor + 0.04f)
        {
            return MissionEndingOutcome.SuccessNominalReturn;
        }

        return MissionEndingOutcome.InProgress;
    }

    private static (float Yield, float StayDays, float HealthProb, float Reliability) ComputeReferenceQuad(float[] f)
    {
        float safeCrew = Math.Max(1f, f[0]);
        float bioLiters = f[1], waterEff = f[2], brineEff = f[3], biomass = f[4], tech = f[5], entropy = f[6], rad = f[7];
        float throttle = f[8], alloc = f[9], log10Dist = f[10], solarIrr = f[11], deltaV = f[12], surfG = f[13];
        float crewBuff = f[14], biomeBuff = f[15], massTons = f[16], foodKg = f[17], waterL = f[18], medsKg = f[19];
        float elecKw = f[20], battKwh = f[21], thermKw = f[22], radShield = f[23], whipple = f[24], artG = f[25];
        float suitPct = f[26], toolsKg = f[27], commsGbps = f[28], ogaKg = f[29], cdraKg = f[30], eclssSyn = f[31];
        float outpostWatt = f[32], outpostO2L = f[33], outpostShield = f[34], commDelaySec = f[35];

        float commDelayPenalty = Math.Clamp(commDelaySec / 2400f, 0f, 0.28f);
        float effectiveCrewStress = Math.Clamp(1.0f - (crewBuff * 0.40f) + (commDelayPenalty * 0.12f), 0.48f, 1.05f);
        float o2Ratio = Math.Clamp((ogaKg + (outpostO2L / 700f) + (bioLiters / 420f) * 0.84f) / (safeCrew * 0.84f), 0.35f, 3.40f);
        float co2Ratio = Math.Clamp((cdraKg + (bioLiters / 400f) * 1.04f) / (safeCrew * 1.04f), 0.35f, 3.20f);
        float gasLoopRatio = MathF.Sqrt(o2Ratio * co2Ratio);

        float requiredKw = (bioLiters / 1000f) * 14.5f + (safeCrew * 3.4f) + (eclssSyn * 16.0f);
        float electricalRatio = Math.Clamp((elecKw + (outpostWatt / 1000f) + battKwh * 0.04f) / Math.Max(15f, requiredKw), 0.40f, 1.45f);
        float thermalRatio = Math.Clamp(thermKw / Math.Max(40f, requiredKw * 0.85f), 0.55f, 1.25f);
        float massEff = Math.Clamp(112.0f / Math.Max(45.0f, massTons), 0.65f, 1.25f);

        float naturalLight = Math.Clamp(MathF.Sqrt(Math.Min(1.5f, solarIrr)) * 0.10f, 0.02f, 0.14f);
        float bnnAtten = Math.Clamp((radShield + outpostShield) / 48.0f, 0.05f, 0.78f);
        float radPenalty = Math.Max(0f, (rad - 0.80f) * 0.075f * (1.0f - biomeBuff * 0.60f) * (1.0f - bnnAtten * 0.58f));

        float totalWaterLoop = Math.Clamp((waterEff * 0.75f) + (brineEff * 0.25f), 0.80f, 0.994f);
        float loopClosure = Math.Clamp((totalWaterLoop * 0.48f) + (eclssSyn * 0.26f) + ((1.0f - entropy) * 0.22f) + (biomeBuff * 0.10f) + naturalLight - radPenalty, 0.16f, 0.995f);
        float synergy = MathF.Sqrt((biomass / 100f) * (tech / 100f)) * (0.62f + 0.38f * MathF.Min(1.25f, throttle * alloc)) * MathF.Sqrt(electricalRatio * thermalRatio) * Math.Clamp(massEff, 0.78f, 1.15f);
        float yieldRate = Math.Clamp(synergy * loopClosure * Math.Clamp(gasLoopRatio, 0.55f, 1.32f), 0.14f, 0.99f);

        float envWear = Math.Clamp(1.0f + ((log10Dist - 2.6f) * 0.08f) + ((deltaV - 9800f) / 35000f), 0.80f, 1.60f);
        float toolFactor = Math.Clamp(1.0f - (toolsKg / 2500f), 0.58f, 1.0f);
        float filterProt = Math.Clamp(1.0f - (eclssSyn * 0.36f) - (biomeBuff * 0.36f), 0.35f, 1.0f);
        float crewPenalty = Math.Clamp((safeCrew * effectiveCrewStress) / Math.Max(0.5f, bioLiters / 360f), 0.14f, 2.50f);

        float dailyEntropy = ((0.0015f * crewPenalty) + (0.0019f * (1.0f - totalWaterLoop) * 10f) + (0.0010f * MathF.Max(0f, throttle - 0.75f)) + (0.0006f * rad * (1.0f - biomeBuff * 0.60f)) - (0.0008f * (alloc - 0.70f))) * envWear * filterProt * toolFactor;
        dailyEntropy = Math.Clamp(dailyEntropy, 0.0009f, 0.035f);
        float safeCeiling = Math.Clamp(0.85f - ((log10Dist - 2.6f) * 0.020f) + (crewBuff * 0.06f), 0.64f, 0.89f);
        float entropyDays = ((safeCeiling - entropy) / dailyEntropy) * Math.Clamp(gasLoopRatio, 0.50f, 2.05f);

        float algalFoodSup = Math.Clamp((bioLiters / (safeCrew * 1050f)) * 0.48f, 0.10f, 0.60f);
        float foodDays = foodKg / Math.Max(0.20f, safeCrew * 0.62f * (1.0f - algalFoodSup));
        float waterDays = waterL / Math.Max(0.10f, safeCrew * 3.54f * Math.Max(0.008f, 1.0f - totalWaterLoop));
        float medDays = (medsKg / Math.Max(0.018f, safeCrew * 0.040f * Math.Max(0.50f, rad * (1.0f - crewBuff * 0.55f)))) * (1.0f + Math.Min(0.25f, commsGbps * 0.012f) - commDelayPenalty * 0.35f);
        float suitDays = Math.Max(15f, (suitPct - 25.0f) / Math.Max(0.065f, (0.15f + log10Dist * 0.032f) * (1.0f - crewBuff * 0.45f) * toolFactor));

        float logDays = Math.Min(Math.Min(foodDays, waterDays), Math.Min(medDays, suitDays));
        float stayDays = Math.Clamp(((entropyDays * 0.55f) + (Math.Min(entropyDays * 1.35f, logDays) * 0.45f)) * massEff, 12f, 720f);

        float gravHealth = Math.Clamp((surfG / 9.81f) + artG + 0.35f, 0.35f, 1.0f);
        float medRatio = Math.Clamp(medsKg / (safeCrew * 35f), 0.30f, 1.25f);
        float healthProb = Math.Clamp(0.52f + (crewBuff * 0.22f) + (bnnAtten * 0.14f) + (gravHealth * 0.10f) + (Math.Min(1.0f, medRatio) * 0.08f) - (radPenalty * 0.65f) - (entropy * 0.12f), 0.18f, 0.996f);

        float whippleFactor = Math.Clamp(whipple / 6.0f, 0.15f, 1.0f);
        float reliability = Math.Clamp(0.45f + (eclssSyn * 0.20f) + ((suitPct / 100f) * 0.16f) + ((1.0f - toolFactor) * 0.22f) + (whippleFactor * 0.08f) + (Math.Min(1.0f, thermalRatio) * 0.08f) - (commDelayPenalty * 0.16f) - (entropy * 0.18f), 0.20f, 0.995f);

        return (yieldRate, stayDays, healthProb, reliability);
    }
}

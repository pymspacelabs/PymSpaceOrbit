using PymSpaceOrbit.Shared.Models.Game;
using PymSpaceOrbit.Shared.Models.ML;

namespace PymSpaceOrbit.Shared.Abstractions.ML;

/// <summary>
/// Service contract for loading and querying the 240,000-sample / 36-feature Quad-Ensemble ML.NET FastTree model
/// across both .NET MAUI Blazor Hybrid (PymSpaceOrbit) and Blazor WebAssembly (PymSpaceOrbit.Web).
/// </summary>
public interface IPymSpaceOrbitMlModelService
{
    bool IsInitialized { get; }
    string ModelStatusDescription { get; }
    string LatestDonkiSolarFlareClass { get; }
    float SolarWeatherRadiationMultiplier { get; }

    Task InitializeAsync(string? customEnsembleUrl = null, string? customSnapshotUrl = null, CancellationToken cancellationToken = default);

    PymSpaceOrbitFeatureVector CreateInitialResourceSlice(OutpostDestination destination, int crewCount);

    QuadMlEvaluationResult EvaluateState(PymSpaceOrbitFeatureVector features, OutpostDestination destination);

    MissionEndingOutcome EvaluateMissionOutcome(
        PymSpaceOrbitFeatureVector features,
        QuadMlEvaluationResult prediction,
        OutpostDestination destination,
        int currentTurn,
        int maxTurns);
}

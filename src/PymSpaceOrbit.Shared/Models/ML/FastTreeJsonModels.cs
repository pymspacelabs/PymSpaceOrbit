namespace PymSpaceOrbit.Shared.Models.ML;

public sealed class FastTreeNodeData
{
    public int[] LeftChild { get; set; } = [];
    public int[] RightChild { get; set; } = [];
    public int[] SplitFeatures { get; set; } = [];
    public double[] Thresholds { get; set; } = [];
    public double[] LeafValues { get; set; } = [];

    public double Evaluate(float[] features)
    {
        int node = 0;
        while (node >= 0)
        {
            int featIdx = SplitFeatures[node];
            double val = featIdx < features.Length ? features[featIdx] : 0.0;
            node = val <= Thresholds[node] ? LeftChild[node] : RightChild[node];
        }
        return LeafValues[~node];
    }
}

public sealed class FastTreeEnsembleModel
{
    public double Bias { get; set; }
    public List<double> TreeWeights { get; set; } = new();
    public List<FastTreeNodeData> Trees { get; set; } = new();

    public float Predict(float[] features)
    {
        double sum = Bias;
        for (int i = 0; i < Trees.Count; i++)
        {
            double weight = i < TreeWeights.Count ? TreeWeights[i] : 1.0;
            sum += weight * Trees[i].Evaluate(features);
        }
        return (float)sum;
    }
}

public sealed class QuadFastTreeArtifact
{
    public FastTreeEnsembleModel? YieldEnsemble { get; set; }
    public FastTreeEnsembleModel? DaysEnsemble { get; set; }
    public FastTreeEnsembleModel? HealthEnsemble { get; set; }
    public FastTreeEnsembleModel? ReliabilityEnsemble { get; set; }
    public double Bias { get; set; }
    public List<double> TreeWeights { get; set; } = new();
    public List<FastTreeNodeData> Trees { get; set; } = new();
}

public sealed class NasaSnapshotArtifactDto
{
    public string LatestSolarFlareClass { get; set; } = "M1.4";
    public float BaseSolarWeatherMultiplier { get; set; } = 1.15f;
    public Dictionary<string, double> LiveDistancesFromEarthKm { get; set; } = new();
}

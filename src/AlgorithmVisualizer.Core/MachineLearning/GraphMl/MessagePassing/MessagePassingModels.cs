namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.MessagePassing;

public enum MessageAggregation
{
    Mean,
    Sum
}

public enum MessagePassingPhase
{
    Ready,
    Gather,
    Aggregate,
    Transform,
    Activate,
    CommitLayer,
    Complete
}

public sealed record MessagePassingConfiguration(
    double[][] Adjacency,
    double[][] Features,
    double[][] SelfWeights,
    double[][] NeighborWeights,
    double[] Bias,
    MessageAggregation Aggregation = MessageAggregation.Mean,
    int Layers = 1);

public sealed record MessagePassingSnapshot(
    int NodeCount,
    int FeatureDimension,
    double[] AdjacencyValues,
    double[][] Features,
    double[][] NextFeatures,
    double[] Aggregate,
    double[] SelfContribution,
    double[] NeighborContribution,
    double[] PreActivation,
    int[] NeighborCounts,
    MessageAggregation Aggregation,
    MessagePassingPhase Phase,
    int Layer,
    int CurrentNode,
    string FocusText);

public sealed record MessagePassingRunResult(
    double[][] InitialFeatures,
    double[][] FinalFeatures,
    int[] NeighborCounts,
    MessageAggregation Aggregation,
    int Layers,
    string Summary)
{
    public string LayerComplexity => "O(E·d + V·d²)";
    public string SpaceComplexity => "O(V·d + E)";
}

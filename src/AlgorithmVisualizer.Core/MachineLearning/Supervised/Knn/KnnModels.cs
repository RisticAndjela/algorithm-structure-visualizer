namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.Knn;

public enum KnnDistanceMetric
{
    Euclidean,
    Manhattan
}

public enum KnnPhase
{
    Ready,
    MeasuringDistance,
    UpdatingNeighbors,
    Voting,
    Complete
}

public enum KnnExampleState
{
    Stored,
    Comparing,
    Scanned,
    Neighbor,
    Voted
}

public sealed record KnnConfiguration(
    double[][] Features,
    int[] Labels,
    double[] Query,
    int K,
    KnnDistanceMetric DistanceMetric);

public sealed record KnnSnapshot(
    double[][] Features,
    int[] Labels,
    double[] Query,
    double[] Distances,
    KnnExampleState[] States,
    int[] NeighborIndices,
    double[] NeighborDistances,
    int[] NeighborRanks,
    int K,
    KnnDistanceMetric DistanceMetric,
    KnnPhase Phase,
    int CurrentIndex,
    double CurrentDistance,
    int VoteClass0,
    int VoteClass1,
    int? PredictedClass,
    string FocusText)
{
    public int Count => Features.Length;
    public int Dimension => Query.Length;
}

public sealed record KnnRunResult(
    double[][] Features,
    int[] Labels,
    double[] Query,
    int K,
    KnnDistanceMetric DistanceMetric,
    double[] Distances,
    int[] NeighborIndices,
    double[] NeighborDistances,
    int VoteClass0,
    int VoteClass1,
    int PredictedClass,
    int DistanceEvaluations,
    string Summary)
{
    public int Dimension => Query.Length;
    public string PredictionTimeComplexity => "Θ(n·(d + k))";
    public string DistanceScanComplexity => "Θ(n·d)";
    public string NeighborBookkeepingComplexity => "O(n·k)";
    public string WorkingSpaceComplexity => "O(n + k)";
}

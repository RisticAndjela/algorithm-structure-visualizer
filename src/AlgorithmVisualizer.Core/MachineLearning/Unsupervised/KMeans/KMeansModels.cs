namespace AlgorithmVisualizer.Core.MachineLearning.Unsupervised.KMeans;

public enum KMeansPhase
{
    Ready,
    Initializing,
    Assigning,
    Updating,
    Complete
}

public enum KMeansPointState
{
    Unassigned,
    Active,
    Assigned
}

public enum KMeansCentroidState
{
    Stored,
    Active,
    Stable
}

public sealed record KMeansConfiguration(
    double[][] Features,
    int ClusterCount,
    int[] InitialCentroidIndexes,
    int MaxIterations = 12,
    double Tolerance = 0.001d);

public sealed record KMeansSnapshot(
    double[][] Features,
    double[][] Centroids,
    int[] Assignments,
    double[] AssignedDistances,
    int[] ClusterCounts,
    KMeansPointState[] PointStates,
    KMeansCentroidState[] CentroidStates,
    KMeansPhase Phase,
    int Iteration,
    int CurrentPointIndex,
    int CurrentCentroidIndex,
    int ChangedAssignments,
    double Inertia,
    double MaxCentroidMovement,
    bool Converged,
    string FocusText)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
    public int ClusterCount => Centroids.Length;
}

public sealed record KMeansRunResult(
    double[][] Features,
    double[][] InitialCentroids,
    double[][] Centroids,
    int[] Assignments,
    int[] FirstRoundAssignments,
    int[] ClusterCounts,
    int Iterations,
    double Inertia,
    double MaxCentroidMovement,
    bool Converged,
    string Summary)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
    public int ClusterCount => Centroids.Length;
    public string IterationComplexity => "O(n·k·d)";
    public string TotalComplexity => "O(i·n·k·d)";
    public string SpaceComplexity => "O(n + k·d)";
}

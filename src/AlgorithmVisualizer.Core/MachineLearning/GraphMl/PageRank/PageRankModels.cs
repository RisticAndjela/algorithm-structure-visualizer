namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.PageRank;

public enum PageRankPhase
{
    Ready,
    Teleport,
    DanglingMass,
    Distributing,
    Commit,
    Complete
}

public sealed record PageRankConfiguration(
    double[][] Adjacency,
    double Damping = 0.85d,
    int MaxIterations = 16,
    double Tolerance = 1e-6);

public sealed record PageRankSnapshot(
    int NodeCount,
    double[] AdjacencyValues,
    int[] RowPointers,
    int[] ColumnIndexes,
    double[] Ranks,
    double[] NextRanks,
    double[] CurrentContributions,
    int[] OutDegrees,
    PageRankPhase Phase,
    int Iteration,
    int CurrentSource,
    double DanglingMass,
    double Delta,
    string FocusText);

public sealed record PageRankRunResult(
    double[] Ranks,
    int Iterations,
    double Delta,
    int TopNode,
    double TopRank,
    double RankSum,
    bool Converged,
    string Summary)
{
    public string IterationComplexity => "O(V + E)";
    public string TotalComplexity => "O(iterations·(V + E))";
    public string SpaceComplexity => "O(V + E)";
}

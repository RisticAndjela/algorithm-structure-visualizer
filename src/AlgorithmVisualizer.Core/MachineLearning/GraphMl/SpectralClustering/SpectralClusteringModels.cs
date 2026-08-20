namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.SpectralClustering;

public enum SpectralClusteringPhase
{
    Ready,
    Degree,
    Laplacian,
    EigenSolve,
    Embedding,
    Clustering,
    Complete
}

public sealed record SpectralClusteringConfiguration(
    double[][] Adjacency,
    int ClusterCount = 2,
    int MaxJacobiRotations = 256,
    double EigenTolerance = 1e-10);

public sealed record SpectralClusteringSnapshot(
    int NodeCount,
    double[] AdjacencyValues,
    double[] Degrees,
    double[] LaplacianValues,
    double[] Eigenvalues,
    double[][] Embedding,
    int[] Assignments,
    SpectralClusteringPhase Phase,
    int JacobiRotations,
    string FocusText);

public sealed record SpectralClusteringRunResult(
    double[] Degrees,
    double[] Eigenvalues,
    double[][] Embedding,
    int[] Assignments,
    int ClusterCount,
    int JacobiRotations,
    string Summary)
{
    public string LaplacianComplexity => "O(V²) in the teaching dense Laplacian";
    public string EigenComplexity => "O(rotations·V²) Jacobi rotations";
    public string ClusteringComplexity => "O(iterations·V·k·d) via the existing manual K-Means";
}

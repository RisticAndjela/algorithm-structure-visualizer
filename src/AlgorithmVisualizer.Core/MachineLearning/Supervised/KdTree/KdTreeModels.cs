namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.KdTree;

public enum KdTreePhase
{
    Ready,
    Building,
    Measuring,
    Descending,
    Backtracking,
    Pruning,
    Complete
}

public enum KdTreeNodeState
{
    Stored,
    Building,
    Active,
    Visited,
    Best,
    Pruned
}

public sealed record KdTreeConfiguration(
    double[][] Features,
    int[] Labels,
    double[] Query);

public sealed record KdTreeNodeSnapshot(
    int NodeId,
    int PointIndex,
    double[] Point,
    int Label,
    int Axis,
    int Depth,
    int LeftNodeId,
    int RightNodeId,
    KdTreeNodeState State);

public sealed record KdTreeSnapshot(
    double[][] Features,
    int[] Labels,
    double[] Query,
    KdTreeNodeSnapshot[] Nodes,
    int RootNodeId,
    KdTreePhase Phase,
    int CurrentNodeId,
    int BestNodeId,
    double CurrentDistance,
    double BestDistance,
    double SplitPlaneDistance,
    int VisitedNodes,
    int PrunedNodes,
    string FocusText)
{
    public int Count => Features.Length;
    public int Dimension => Query.Length;
}

public sealed record KdTreeRunResult(
    double[][] Features,
    int[] Labels,
    double[] Query,
    KdTreeNodeSnapshot[] Nodes,
    int RootNodeId,
    int NearestNodeId,
    int NearestPointIndex,
    double[] NearestPoint,
    int NearestLabel,
    double NearestDistance,
    int VisitedNodes,
    int PrunedNodes,
    int MaxDepth,
    string Summary)
{
    public int Dimension => Query.Length;
    public string AverageQueryComplexity => "O(log n)";
    public string WorstQueryComplexity => "O(n)";
    public string TeachingBuildComplexity => "O(n log² n)";
    public string TreeSpaceComplexity => "O(n)";
}

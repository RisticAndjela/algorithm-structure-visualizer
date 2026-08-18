using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Core.Algorithms.GraphTraversal;

public enum GraphTraversalVertexState
{
    Unvisited,
    Frontier,
    Current,
    InspectingNeighbor,
    Visited
}

public enum GraphTraversalPhase
{
    Ready,
    Starting,
    TakingFromFrontier,
    InspectingEdge,
    Discovering,
    SkippingVisited,
    Backtracking,
    Complete
}

public enum DepthFirstTraversalVariant
{
    Recursive,
    Iterative
}

public sealed record GraphTraversalVertexSnapshot(
    int Index,
    Guid Id,
    string Label,
    GraphTraversalVertexState State,
    bool Visited,
    int ParentIndex,
    int Level);

public sealed record GraphTraversalSnapshot(
    GraphSnapshot Graph,
    GraphTraversalVertexSnapshot[] Vertices,
    int[] FrontierIndices,
    int[] TraversalOrderIndices,
    int CurrentIndex,
    int NeighborIndex,
    Guid? CurrentEdgeId,
    int EdgeChecks,
    int DiscoveredCount,
    GraphTraversalPhase Phase,
    string FrontierLabel,
    string LevelLabel)
{
    public int VertexCount => Graph.VertexCount;
    public int FrontierCount => FrontierIndices.Length;
}

public sealed record BreadthFirstSearchResult(
    GraphSnapshot Graph,
    int StartIndex,
    int[] TraversalOrderIndices,
    int[] ParentIndices,
    int[] Distances,
    int EdgeChecks,
    int ReachableCount,
    int MaxQueueSize)
{
    public string TimeComplexity => "O(V + E)";
    public string ExtraSpaceComplexity => "O(V)";
    public bool RequiresRestartAfterMutation => true;
}

public sealed record DepthFirstSearchResult(
    GraphSnapshot Graph,
    int StartIndex,
    DepthFirstTraversalVariant Variant,
    int[] TraversalOrderIndices,
    int[] ParentIndices,
    int[] Depths,
    int EdgeChecks,
    int ReachableCount,
    int BacktrackCount,
    int MaxFrontierDepth)
{
    public string TimeComplexity => "O(V + E)";
    public string ExtraSpaceComplexity => "O(V)";
    public bool RequiresRestartAfterMutation => true;
}

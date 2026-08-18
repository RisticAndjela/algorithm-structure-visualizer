using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Core.Algorithms.GraphOrdering.Topological;

public enum TopologicalSortVariant
{
    KahnQueue,
    DfsPostorder
}

public enum TopologicalVertexState
{
    Unprocessed,
    Ready,
    Current,
    InspectingNeighbor,
    Visiting,
    Finished,
    Ordered,
    Cycle
}

public enum TopologicalSortPhase
{
    Ready,
    Initializing,
    TakingReadyVertex,
    InspectingEdge,
    DecrementingInDegree,
    EnteringVertex,
    FinishingVertex,
    Backtracking,
    DetectingCycle,
    ReversingPostorder,
    Complete
}

public sealed record TopologicalVertexSnapshot(
    int Index,
    Guid Id,
    string Label,
    TopologicalVertexState State,
    int InDegree,
    int VisitColor,
    bool Ordered);

public sealed record TopologicalSortSnapshot(
    GraphSnapshot Graph,
    TopologicalVertexSnapshot[] Vertices,
    int[] FrontierIndices,
    int[] OutputIndices,
    int[] PostorderIndices,
    int CurrentIndex,
    int NeighborIndex,
    Guid? CurrentEdgeId,
    int EdgeChecks,
    int InDegreeUpdates,
    int QueueEnqueues,
    int BacktrackCount,
    int InitialReadyCount,
    bool CycleDetected,
    TopologicalSortPhase Phase,
    TopologicalSortVariant Variant)
{
    public int VertexCount => Graph.VertexCount;
}

public sealed record TopologicalSortResult(
    GraphSnapshot Graph,
    TopologicalSortVariant Variant,
    int[] OrderIndices,
    int EdgeChecks,
    int InDegreeUpdates,
    int QueueEnqueues,
    int BacktrackCount,
    int InitialReadyCount,
    bool CycleDetected,
    int ProcessedCount)
{
    public bool IsDag => !CycleDetected && ProcessedCount == Graph.VertexCount;
    public string TimeComplexity => "O(V + E)";
    public string ExtraSpaceComplexity => "O(V)";
    public bool RequiresDirectedGraph => true;
    public bool RequiresRestartAfterMutation => true;
}

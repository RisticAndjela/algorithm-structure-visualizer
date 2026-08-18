using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra;

public enum DijkstraVariant
{
    LinearScan,
    MinHeap
}

public enum DijkstraVertexState
{
    Unreached,
    Frontier,
    Current,
    InspectingNeighbor,
    Settled
}

public enum DijkstraPhase
{
    Ready,
    Starting,
    SelectingMinimum,
    SkippingStaleEntry,
    InspectingEdge,
    Relaxing,
    KeepingDistance,
    Settling,
    Complete
}

public sealed record DijkstraFrontierEntrySnapshot(
    int VertexIndex,
    double Priority,
    bool Stale);

public sealed record DijkstraVertexSnapshot(
    int Index,
    Guid Id,
    string Label,
    DijkstraVertexState State,
    bool Settled,
    int ParentIndex,
    double Distance);

public sealed record DijkstraSnapshot(
    GraphSnapshot Graph,
    DijkstraVertexSnapshot[] Vertices,
    DijkstraFrontierEntrySnapshot[] FrontierEntries,
    int[] SettledOrderIndices,
    int CurrentIndex,
    int NeighborIndex,
    Guid? CurrentEdgeId,
    int EdgeChecks,
    int RelaxationAttempts,
    int DistanceUpdates,
    int SelectionComparisons,
    int FrontierPushes,
    int StalePops,
    DijkstraPhase Phase,
    DijkstraVariant Variant)
{
    public int VertexCount => Graph.VertexCount;
}

public sealed record DijkstraResult(
    GraphSnapshot Graph,
    int StartIndex,
    DijkstraVariant Variant,
    double[] Distances,
    int[] ParentIndices,
    int[] SettledOrderIndices,
    int EdgeChecks,
    int RelaxationAttempts,
    int DistanceUpdates,
    int SelectionComparisons,
    int FrontierPushes,
    int StalePops,
    int ReachableCount)
{
    public string TimeComplexity => Variant == DijkstraVariant.LinearScan
        ? "O(V² + E)"
        : "O((V + E) log V)";

    public string ExtraSpaceComplexity => "O(V + frontier)";
    public bool RequiresRestartAfterMutation => true;
}

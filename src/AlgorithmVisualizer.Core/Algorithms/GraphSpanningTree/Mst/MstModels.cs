using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Core.Algorithms.GraphSpanningTree.Mst;

public enum MstVariant
{
    Prim,
    Kruskal
}

public enum MstVertexState
{
    Normal,
    InForest,
    Current,
    Candidate
}

public enum MstEdgeState
{
    Normal,
    Candidate,
    Selected,
    Rejected
}

public enum MstPhase
{
    Ready,
    StartingComponent,
    PushingFrontier,
    InspectingEdge,
    SelectingEdge,
    RejectingCycle,
    SortingEdges,
    FindingRoots,
    UnionComponents,
    Complete
}

public sealed record MstVertexSnapshot(
    int Index,
    Guid Id,
    string Label,
    MstVertexState State,
    int Component,
    int DsuParent,
    int DsuRank,
    bool InForest);

public sealed record MstFrontierEntrySnapshot(
    int EdgeIndex,
    int FromIndex,
    int ToIndex,
    double Weight,
    bool Stale);

public sealed record MstSnapshot(
    GraphSnapshot Graph,
    MstVertexSnapshot[] Vertices,
    int[] SelectedEdgeIndices,
    MstFrontierEntrySnapshot[] Frontier,
    int[] SortedEdgeIndices,
    int CurrentVertexIndex,
    int CandidateVertexIndex,
    int CandidateEdgeIndex,
    int RejectedEdgeIndex,
    double TotalWeight,
    int ComponentCount,
    int EdgeChecks,
    int FrontierPushes,
    int FrontierPops,
    int CycleSkips,
    int SortComparisons,
    int FindOperations,
    int UnionOperations,
    MstPhase Phase,
    MstVariant Variant,
    int StartIndex)
{
    public int VertexCount => Graph.VertexCount;
}

public sealed record MstResult(
    GraphSnapshot Graph,
    MstVariant Variant,
    int StartIndex,
    int[] SelectedEdgeIndices,
    double TotalWeight,
    int ComponentCount,
    int EdgeChecks,
    int FrontierPushes,
    int FrontierPops,
    int CycleSkips,
    int SortComparisons,
    int FindOperations,
    int UnionOperations)
{
    public bool IsConnected => Graph.VertexCount <= 1 || ComponentCount == 1;
    public bool IsMinimumSpanningTree => Graph.VertexCount > 0 && IsConnected && SelectedEdgeIndices.Length == Math.Max(0, Graph.VertexCount - 1);
    public bool IsMinimumSpanningForest => Graph.VertexCount > 0 && SelectedEdgeIndices.Length == Math.Max(0, Graph.VertexCount - ComponentCount);
    public string TimeComplexity => Variant == MstVariant.Prim ? "O(E log E)" : "O(E log E)";
    public string ExtraSpaceComplexity => "O(V + E)";
    public bool RequiresUndirectedGraph => true;
    public bool AllowsNegativeWeights => true;
    public bool RequiresRestartAfterMutation => true;
}

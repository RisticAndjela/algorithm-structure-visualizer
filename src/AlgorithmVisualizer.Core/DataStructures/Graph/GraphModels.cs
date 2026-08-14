namespace AlgorithmVisualizer.Core.DataStructures.Graph;

public enum GraphOperationKind
{
    AddVertex,
    RenameVertex,
    RemoveVertex,
    SearchVertex,
    AddEdge,
    UpdateEdgeWeight,
    RemoveEdge,
    SearchEdge,
    InspectNeighbors,
    Clear
}

public enum GraphVertexVisualState
{
    Normal,
    Checking,
    Source,
    Target,
    Added,
    Matched,
    Neighbor,
    Removing
}

public enum GraphEdgeVisualState
{
    Normal,
    Checking,
    Added,
    Matched,
    Removing
}

public sealed record GraphNeighborSnapshot(
    Guid VertexId,
    string Label,
    Guid EdgeId,
    double Weight);

public sealed record GraphVertexSnapshot(
    int Index,
    Guid Id,
    string Label,
    GraphVertexVisualState VisualState,
    GraphNeighborSnapshot[] Neighbors,
    int InDegree,
    int OutDegree)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
}

public sealed record GraphEdgeSnapshot(
    Guid Id,
    int FromIndex,
    int ToIndex,
    Guid FromId,
    Guid ToId,
    string FromLabel,
    string ToLabel,
    double Weight,
    bool Directed,
    GraphEdgeVisualState VisualState)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
}

public sealed record GraphMatrixCellSnapshot(
    int Row,
    int Column,
    bool HasEdge,
    double Weight);

public sealed record GraphSnapshot(
    bool Directed,
    bool Weighted,
    int VertexCount,
    int EdgeCount,
    GraphVertexSnapshot[] Vertices,
    GraphEdgeSnapshot[] Edges,
    GraphMatrixCellSnapshot[] MatrixCells)
{
    public bool IsEmpty => VertexCount == 0;
}

public sealed record GraphOperationResult(
    GraphOperationKind Operation,
    bool Succeeded,
    string? FirstLabel,
    string? SecondLabel,
    double? Weight,
    int Comparisons,
    int InitialVertexCount,
    int FinalVertexCount,
    int InitialEdgeCount,
    int FinalEdgeCount,
    Guid? AffectedVertexId = null,
    Guid? AffectedEdgeId = null,
    int NeighborCount = 0)
{
    public string WorstCaseComplexity => Operation switch
    {
        GraphOperationKind.SearchVertex => "O(V)",
        GraphOperationKind.SearchEdge => "O(V + deg(v))",
        GraphOperationKind.InspectNeighbors => "O(V + deg(v))",
        GraphOperationKind.RenameVertex => "O(V)",
        GraphOperationKind.RemoveVertex => "O(V + E)",
        GraphOperationKind.UpdateEdgeWeight => "O(V + deg(v))",
        GraphOperationKind.RemoveEdge => "O(V + E)",
        GraphOperationKind.Clear => "O(V + E)",
        GraphOperationKind.AddVertex => "O(V)",
        GraphOperationKind.AddEdge => "O(V + deg(v))",
        _ => "O(V + E)"
    };

    public string CurrentRunComplexity => Comparisons <= 1 ? "Θ(1)" : "Θ(k)";
}

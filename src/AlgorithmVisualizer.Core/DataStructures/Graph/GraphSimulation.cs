using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Graph;

/// <summary>
/// Teaching-oriented graph implemented from scratch with canonical vertex/edge objects,
/// manual adjacency lists, and the existing ManualMatrix for synchronized adjacency-matrix storage.
/// </summary>
public sealed class GraphSimulation : SimulationAlgorithmBase
{
    private readonly ManualDynamicArray<GraphVertex> _vertices = new();
    private readonly ManualDynamicArray<GraphEdge> _edges = new();
    private readonly ManualMatrix _matrix = new(1, 1);
    private bool[] _matrixPresence = Array.Empty<bool>();

    public GraphSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public bool Directed { get; private set; }
    public bool Weighted { get; private set; }
    public int VertexCount => _vertices.Count;
    public int EdgeCount => _edges.Count;

    public bool TrySetDirected(bool directed)
    {
        if (_edges.Count > 0)
        {
            return false;
        }
        Directed = directed;
        RebuildRepresentations();
        return true;
    }

    public bool TrySetWeighted(bool weighted)
    {
        if (_edges.Count > 0)
        {
            return false;
        }
        Weighted = weighted;
        RebuildRepresentations();
        return true;
    }

    public async Task<GraphOperationResult> AddVertexAsync(string label, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeLabel(label);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();

        if (normalized.Length == 0 || normalized.Length > 12)
        {
            return Result(GraphOperationKind.AddVertex, false, normalized, null, null, 0, beforeV, beforeE);
        }

        var comparisons = 0;
        for (var index = 0; index < _vertices.Count; index++)
        {
            var vertex = _vertices[index];
            vertex.VisualState = GraphVertexVisualState.Checking;
            comparisons++;
            await NextStepAsync($"Check whether label {normalized} already belongs to vertex {vertex.Label}.", cancellationToken);
            if (LabelsEqual(vertex.Label, normalized))
            {
                vertex.VisualState = GraphVertexVisualState.Matched;
                await NextStepAsync($"Vertex labels are unique in this lab, so {normalized} is rejected as a duplicate.", cancellationToken);
                return Result(GraphOperationKind.AddVertex, false, normalized, null, null, comparisons, beforeV, beforeE, vertex.Id);
            }
            vertex.VisualState = GraphVertexVisualState.Normal;
        }

        var added = new GraphVertex(normalized) { VisualState = GraphVertexVisualState.Added };
        _vertices.Add(added);
        RebuildRepresentations();
        await NextStepAsync($"Add vertex {normalized}. It now owns an empty adjacency list and a row/column in the adjacency matrix.", cancellationToken);
        return Result(GraphOperationKind.AddVertex, true, normalized, null, null, comparisons, beforeV, beforeE, added.Id);
    }

    public async Task<GraphOperationResult> SearchVertexAsync(string label, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeLabel(label);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;

        for (var index = 0; index < _vertices.Count; index++)
        {
            var vertex = _vertices[index];
            vertex.VisualState = GraphVertexVisualState.Checking;
            comparisons++;
            await NextStepAsync($"Compare target {normalized} with vertex {vertex.Label} at vertex slot {index}.", cancellationToken);
            if (LabelsEqual(vertex.Label, normalized))
            {
                vertex.VisualState = GraphVertexVisualState.Matched;
                await NextStepAsync($"Found vertex {vertex.Label}. Its adjacency list contains {vertex.Neighbors.Count} outgoing neighbor entr{(vertex.Neighbors.Count == 1 ? "y" : "ies")}.", cancellationToken);
                return Result(GraphOperationKind.SearchVertex, true, normalized, null, null, comparisons, beforeV, beforeE, vertex.Id);
            }
            vertex.VisualState = GraphVertexVisualState.Normal;
        }

        return Result(GraphOperationKind.SearchVertex, false, normalized, null, null, comparisons, beforeV, beforeE);
    }


    public async Task<GraphOperationResult> RenameVertexAsync(string currentLabel, string newLabel, CancellationToken cancellationToken = default)
    {
        var currentName = NormalizeLabel(currentLabel);
        var replacementName = NormalizeLabel(newLabel);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;

        if (replacementName.Length == 0 || replacementName.Length > 12)
        {
            return Result(GraphOperationKind.RenameVertex, false, currentName, replacementName, null, comparisons, beforeV, beforeE);
        }

        var vertex = FindVertex(currentName, ref comparisons);
        if (vertex is null)
        {
            return Result(GraphOperationKind.RenameVertex, false, currentName, replacementName, null, comparisons, beforeV, beforeE);
        }

        vertex.VisualState = GraphVertexVisualState.Source;
        await NextStepAsync($"Found vertex {vertex.Label}. Keep its object ID and edge references while checking whether {replacementName} is available.", cancellationToken);

        for (var index = 0; index < _vertices.Count; index++)
        {
            var candidate = _vertices[index];
            if (candidate.Id == vertex.Id) continue;
            comparisons++;
            candidate.VisualState = GraphVertexVisualState.Checking;
            await NextStepAsync($"Compare new label {replacementName} with existing vertex {candidate.Label}.", cancellationToken);
            if (LabelsEqual(candidate.Label, replacementName))
            {
                candidate.VisualState = GraphVertexVisualState.Matched;
                return Result(GraphOperationKind.RenameVertex, false, currentName, replacementName, null, comparisons, beforeV, beforeE, vertex.Id);
            }
            candidate.VisualState = GraphVertexVisualState.Normal;
        }

        vertex.Rename(replacementName);
        vertex.VisualState = GraphVertexVisualState.Matched;
        RebuildRepresentations();
        vertex.VisualState = GraphVertexVisualState.Matched;
        await NextStepAsync($"Rename the same vertex object to {replacementName}. Its ID and every incident edge object stay unchanged; only representation labels refresh.", cancellationToken);
        return Result(GraphOperationKind.RenameVertex, true, currentName, replacementName, null, comparisons, beforeV, beforeE, vertex.Id);
    }

    public async Task<GraphOperationResult> AddEdgeAsync(string fromLabel, string toLabel, double weight = 1d, CancellationToken cancellationToken = default)
    {
        var fromName = NormalizeLabel(fromLabel);
        var toName = NormalizeLabel(toLabel);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();

        if (!double.IsFinite(weight))
        {
            return Result(GraphOperationKind.AddEdge, false, fromName, toName, weight, 0, beforeV, beforeE);
        }

        var comparisons = 0;
        var from = FindVertex(fromName, ref comparisons);
        var to = FindVertex(toName, ref comparisons);
        if (from is null || to is null)
        {
            return Result(GraphOperationKind.AddEdge, false, fromName, toName, weight, comparisons, beforeV, beforeE);
        }

        from.VisualState = GraphVertexVisualState.Source;
        to.VisualState = GraphVertexVisualState.Target;
        await NextStepAsync($"Use {from.Label} as the source and {to.Label} as the target. Now check whether that edge already exists.", cancellationToken);

        var existing = FindEdge(from, to, ref comparisons);
        if (existing is not null)
        {
            existing.VisualState = GraphEdgeVisualState.Matched;
            await NextStepAsync("That logical edge already exists, so the graph is left unchanged.", cancellationToken);
            return Result(GraphOperationKind.AddEdge, false, fromName, toName, weight, comparisons, beforeV, beforeE, from.Id, existing.Id);
        }

        var storedWeight = Weighted ? weight : 1d;
        var edge = new GraphEdge(from, to, storedWeight) { VisualState = GraphEdgeVisualState.Added };
        _edges.Add(edge);
        RebuildRepresentations();
        edge.VisualState = GraphEdgeVisualState.Added;
        from.VisualState = GraphVertexVisualState.Source;
        to.VisualState = GraphVertexVisualState.Target;
        await NextStepAsync(Directed
            ? $"Add {from.Label} → {to.Label}. Only the source adjacency list receives this outgoing edge."
            : $"Add {from.Label} — {to.Label}. Both adjacency lists receive a neighbor entry for the same logical edge.", cancellationToken);

        await NextStepAsync(Weighted
            ? $"Store weight {FormatWeight(storedWeight)} in the shared adjacency matrix cell{(Directed ? string.Empty : "s")} and keep the matrix synchronized with the adjacency list."
            : "Store 1 for edge presence in the shared adjacency matrix and keep both representations synchronized.", cancellationToken);

        return Result(GraphOperationKind.AddEdge, true, fromName, toName, storedWeight, comparisons, beforeV, beforeE, from.Id, edge.Id);
    }


    public async Task<GraphOperationResult> UpdateEdgeWeightAsync(string fromLabel, string toLabel, double newWeight, CancellationToken cancellationToken = default)
    {
        var fromName = NormalizeLabel(fromLabel);
        var toName = NormalizeLabel(toLabel);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;

        if (!Weighted || !double.IsFinite(newWeight))
        {
            return Result(GraphOperationKind.UpdateEdgeWeight, false, fromName, toName, newWeight, comparisons, beforeV, beforeE);
        }

        var from = FindVertex(fromName, ref comparisons);
        var to = FindVertex(toName, ref comparisons);
        if (from is null || to is null)
        {
            return Result(GraphOperationKind.UpdateEdgeWeight, false, fromName, toName, newWeight, comparisons, beforeV, beforeE);
        }

        from.VisualState = GraphVertexVisualState.Source;
        to.VisualState = GraphVertexVisualState.Target;
        await NextStepAsync($"Locate the existing weighted edge from {from.Label} to {to.Label} before changing any numeric data.", cancellationToken);
        var edge = FindEdge(from, to, ref comparisons);
        if (edge is null)
        {
            return Result(GraphOperationKind.UpdateEdgeWeight, false, fromName, toName, newWeight, comparisons, beforeV, beforeE);
        }

        edge.VisualState = GraphEdgeVisualState.Checking;
        await NextStepAsync($"Keep edge #{edge.Id.ToString("N")[..6].ToUpperInvariant()} and replace only its weight {FormatWeight(edge.Weight)} → {FormatWeight(newWeight)}.", cancellationToken);
        edge.SetWeight(newWeight);
        RebuildRepresentations();
        edge.VisualState = GraphEdgeVisualState.Matched;
        from.VisualState = GraphVertexVisualState.Source;
        to.VisualState = GraphVertexVisualState.Target;
        await NextStepAsync("Refresh the adjacency-list weight and the same Matrix-backed cell without recreating the edge object.", cancellationToken);
        return Result(GraphOperationKind.UpdateEdgeWeight, true, fromName, toName, newWeight, comparisons, beforeV, beforeE, null, edge.Id);
    }

    public async Task<GraphOperationResult> SearchEdgeAsync(string fromLabel, string toLabel, CancellationToken cancellationToken = default)
    {
        var fromName = NormalizeLabel(fromLabel);
        var toName = NormalizeLabel(toLabel);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;
        var from = FindVertex(fromName, ref comparisons);
        var to = FindVertex(toName, ref comparisons);
        if (from is null || to is null)
        {
            return Result(GraphOperationKind.SearchEdge, false, fromName, toName, null, comparisons, beforeV, beforeE);
        }

        from.VisualState = GraphVertexVisualState.Source;
        to.VisualState = GraphVertexVisualState.Target;
        await NextStepAsync($"Open {from.Label}'s adjacency list. Edge lookup scans only this neighborhood, not every possible matrix cell.", cancellationToken);

        for (var index = 0; index < from.Neighbors.Count; index++)
        {
            var neighbor = from.Neighbors[index];
            neighbor.Edge.VisualState = GraphEdgeVisualState.Checking;
            neighbor.Vertex.VisualState = GraphVertexVisualState.Checking;
            comparisons++;
            await NextStepAsync($"Check adjacency entry {index}: {from.Label} connects to {neighbor.Vertex.Label}.", cancellationToken);
            if (neighbor.Vertex.Id == to.Id)
            {
                neighbor.Edge.VisualState = GraphEdgeVisualState.Matched;
                neighbor.Vertex.VisualState = GraphVertexVisualState.Matched;
                await NextStepAsync($"Found the edge. The adjacency matrix answers the same structural question at cell [{IndexOf(from)},{IndexOf(to)}].", cancellationToken);
                return Result(GraphOperationKind.SearchEdge, true, fromName, toName, neighbor.Edge.Weight, comparisons, beforeV, beforeE, to.Id, neighbor.Edge.Id);
            }
            neighbor.Edge.VisualState = GraphEdgeVisualState.Normal;
            if (neighbor.Vertex.Id != from.Id && neighbor.Vertex.Id != to.Id)
            {
                neighbor.Vertex.VisualState = GraphVertexVisualState.Normal;
            }
        }

        await NextStepAsync($"No adjacency entry from {from.Label} reaches {to.Label}, so that edge is missing.", cancellationToken);
        return Result(GraphOperationKind.SearchEdge, false, fromName, toName, null, comparisons, beforeV, beforeE);
    }

    public async Task<GraphOperationResult> InspectNeighborsAsync(string label, CancellationToken cancellationToken = default)
    {
        var name = NormalizeLabel(label);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;
        var vertex = FindVertex(name, ref comparisons);
        if (vertex is null)
        {
            return Result(GraphOperationKind.InspectNeighbors, false, name, null, null, comparisons, beforeV, beforeE);
        }

        vertex.VisualState = GraphVertexVisualState.Source;
        await NextStepAsync($"Open {vertex.Label}'s adjacency list. Each entry is a directly reachable neighbor{(Directed ? " through an outgoing edge" : string.Empty)}.", cancellationToken);
        for (var index = 0; index < vertex.Neighbors.Count; index++)
        {
            var neighbor = vertex.Neighbors[index];
            neighbor.Vertex.VisualState = GraphVertexVisualState.Neighbor;
            neighbor.Edge.VisualState = GraphEdgeVisualState.Matched;
            await NextStepAsync($"Neighbor {index + 1}: {neighbor.Vertex.Label}{(Weighted ? $" with edge weight {FormatWeight(neighbor.Edge.Weight)}" : string.Empty)}.", cancellationToken);
        }

        return Result(GraphOperationKind.InspectNeighbors, true, name, null, null, comparisons, beforeV, beforeE, vertex.Id, null, vertex.Neighbors.Count);
    }

    public async Task<GraphOperationResult> RemoveEdgeAsync(string fromLabel, string toLabel, CancellationToken cancellationToken = default)
    {
        var fromName = NormalizeLabel(fromLabel);
        var toName = NormalizeLabel(toLabel);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;
        var from = FindVertex(fromName, ref comparisons);
        var to = FindVertex(toName, ref comparisons);
        if (from is null || to is null)
        {
            return Result(GraphOperationKind.RemoveEdge, false, fromName, toName, null, comparisons, beforeV, beforeE);
        }

        for (var index = 0; index < _edges.Count; index++)
        {
            var edge = _edges[index];
            edge.VisualState = GraphEdgeVisualState.Checking;
            comparisons++;
            await NextStepAsync($"Check logical edge {EdgeLabel(edge)} against the requested edge.", cancellationToken);
            if (EdgeMatches(edge, from, to))
            {
                edge.VisualState = GraphEdgeVisualState.Removing;
                from.VisualState = GraphVertexVisualState.Source;
                to.VisualState = GraphVertexVisualState.Target;
                await NextStepAsync("Remove the logical edge, then rebuild both adjacency representations so no stale neighbor entry remains.", cancellationToken);
                var edgeId = edge.Id;
                _edges.RemoveAt(index);
                RebuildRepresentations();
                return Result(GraphOperationKind.RemoveEdge, true, fromName, toName, edge.Weight, comparisons, beforeV, beforeE, null, edgeId);
            }
            edge.VisualState = GraphEdgeVisualState.Normal;
        }

        return Result(GraphOperationKind.RemoveEdge, false, fromName, toName, null, comparisons, beforeV, beforeE);
    }

    public async Task<GraphOperationResult> RemoveVertexAsync(string label, CancellationToken cancellationToken = default)
    {
        var name = NormalizeLabel(label);
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        var comparisons = 0;
        var vertexIndex = FindVertexIndex(name, ref comparisons);
        if (vertexIndex < 0)
        {
            return Result(GraphOperationKind.RemoveVertex, false, name, null, null, comparisons, beforeV, beforeE);
        }

        var vertex = _vertices[vertexIndex];
        vertex.VisualState = GraphVertexVisualState.Removing;
        await NextStepAsync($"Remove vertex {vertex.Label}. Every incident edge must disappear too or the graph would contain dangling references.", cancellationToken);

        var edgeIndex = 0;
        while (edgeIndex < _edges.Count)
        {
            var edge = _edges[edgeIndex];
            if (edge.From.Id == vertex.Id || edge.To.Id == vertex.Id)
            {
                edge.VisualState = GraphEdgeVisualState.Removing;
                await NextStepAsync($"Remove incident edge {EdgeLabel(edge)} before releasing the vertex slot.", cancellationToken);
                _edges.RemoveAt(edgeIndex);
                continue;
            }
            edgeIndex++;
        }

        var removedId = vertex.Id;
        _vertices.RemoveAt(vertexIndex);
        RebuildRepresentations();
        await NextStepAsync("Compact the vertex array and rebuild adjacency-list indexes plus the Matrix-backed adjacency table for the new vertex order.", cancellationToken);
        return Result(GraphOperationKind.RemoveVertex, true, name, null, null, comparisons, beforeV, beforeE, removedId);
    }

    public async Task<GraphOperationResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        var beforeV = VertexCount;
        var beforeE = EdgeCount;
        ClearVisualStates();
        if (beforeV > 0 || beforeE > 0)
        {
            await NextStepAsync("Clear logical edges first so adjacency references no longer point at graph vertices.", cancellationToken);
        }
        _edges.Clear();
        _vertices.Clear();
        RebuildRepresentations();
        await NextStepAsync("Graph is empty. Directed/weighted mode can now be changed safely without silently rewriting existing edges.", cancellationToken);
        return Result(GraphOperationKind.Clear, true, null, null, null, beforeV + beforeE, beforeV, beforeE);
    }

    private int FindVertexIndexForSnapshot(GraphVertex target)
    {
        for (var index = 0; index < _vertices.Count; index++)
        {
            if (ReferenceEquals(_vertices[index], target)) return index;
        }

        return -1;
    }


    private int FindEdgeIndexForSnapshot(GraphEdge edge)
    {
        for (var index = 0; index < _edges.Count; index++)
        {
            if (_edges[index].Id == edge.Id) return index;
        }
        return -1;
    }

    public GraphSnapshot CreateSnapshot()
    {
        var vertexSnapshots = new GraphVertexSnapshot[_vertices.Count];
        for (var index = 0; index < _vertices.Count; index++)
        {
            var vertex = _vertices[index];
            var neighbors = new GraphNeighborSnapshot[vertex.Neighbors.Count];
            for (var neighborIndex = 0; neighborIndex < vertex.Neighbors.Count; neighborIndex++)
            {
                var neighbor = vertex.Neighbors[neighborIndex];
                neighbors[neighborIndex] = new GraphNeighborSnapshot(
                    neighbor.Vertex.Id,
                    neighbor.Vertex.Label,
                    neighbor.Edge.Id,
                    neighbor.Edge.Weight,
                    FindVertexIndexForSnapshot(neighbor.Vertex),
                    FindEdgeIndexForSnapshot(neighbor.Edge));
            }

            var inDegree = 0;
            var outDegree = 0;
            for (var edgeIndex = 0; edgeIndex < _edges.Count; edgeIndex++)
            {
                var edge = _edges[edgeIndex];
                if (Directed)
                {
                    if (edge.From.Id == vertex.Id) outDegree++;
                    if (edge.To.Id == vertex.Id) inDegree++;
                }
                else if (edge.From.Id == vertex.Id || edge.To.Id == vertex.Id)
                {
                    var contribution = edge.From.Id == vertex.Id && edge.To.Id == vertex.Id ? 2 : 1;
                    inDegree += contribution;
                    outDegree += contribution;
                }
            }

            vertexSnapshots[index] = new GraphVertexSnapshot(index, vertex.Id, vertex.Label, vertex.VisualState, neighbors, inDegree, outDegree);
        }

        var edgeSnapshots = new GraphEdgeSnapshot[_edges.Count];
        for (var index = 0; index < _edges.Count; index++)
        {
            var edge = _edges[index];
            edgeSnapshots[index] = new GraphEdgeSnapshot(
                edge.Id,
                IndexOf(edge.From),
                IndexOf(edge.To),
                edge.From.Id,
                edge.To.Id,
                edge.From.Label,
                edge.To.Label,
                edge.Weight,
                Directed,
                edge.VisualState);
        }

        var cells = new GraphMatrixCellSnapshot[checked(_vertices.Count * _vertices.Count)];
        var cellIndex = 0;
        for (var row = 0; row < _vertices.Count; row++)
        {
            for (var column = 0; column < _vertices.Count; column++)
            {
                var flat = (row * _vertices.Count) + column;
                cells[cellIndex++] = new GraphMatrixCellSnapshot(row, column, _matrixPresence.Length > flat && _matrixPresence[flat], _vertices.Count == 0 ? 0d : _matrix[row, column]);
            }
        }

        return new GraphSnapshot(Directed, Weighted, _vertices.Count, _edges.Count, vertexSnapshots, edgeSnapshots, cells);
    }

    private void RebuildRepresentations()
    {
        for (var index = 0; index < _vertices.Count; index++)
        {
            _vertices[index].Neighbors.Clear();
        }

        var dimension = Math.Max(1, _vertices.Count);
        _matrix.Resize(dimension, dimension, preserve: false);
        _matrix.Clear();
        _matrixPresence = new bool[checked(_vertices.Count * _vertices.Count)];

        for (var index = 0; index < _edges.Count; index++)
        {
            var edge = _edges[index];
            var fromIndex = IndexOf(edge.From);
            var toIndex = IndexOf(edge.To);
            if (fromIndex < 0 || toIndex < 0)
            {
                continue;
            }

            edge.From.Neighbors.Add(new GraphNeighbor(edge.To, edge));
            SetMatrixCell(fromIndex, toIndex, edge.Weight);

            if (!Directed && edge.From.Id != edge.To.Id)
            {
                edge.To.Neighbors.Add(new GraphNeighbor(edge.From, edge));
                SetMatrixCell(toIndex, fromIndex, edge.Weight);
            }
        }
    }

    private void SetMatrixCell(int row, int column, double weight)
    {
        _matrix[row, column] = Weighted ? weight : 1d;
        _matrixPresence[(row * _vertices.Count) + column] = true;
    }

    private GraphVertex? FindVertex(string label, ref int comparisons)
    {
        var index = FindVertexIndex(label, ref comparisons);
        return index < 0 ? null : _vertices[index];
    }

    private int FindVertexIndex(string label, ref int comparisons)
    {
        for (var index = 0; index < _vertices.Count; index++)
        {
            comparisons++;
            if (LabelsEqual(_vertices[index].Label, label))
            {
                return index;
            }
        }
        return -1;
    }

    private GraphEdge? FindEdge(GraphVertex from, GraphVertex to, ref int comparisons)
    {
        for (var index = 0; index < from.Neighbors.Count; index++)
        {
            comparisons++;
            if (from.Neighbors[index].Vertex.Id == to.Id)
            {
                return from.Neighbors[index].Edge;
            }
        }
        return null;
    }

    private bool EdgeMatches(GraphEdge edge, GraphVertex from, GraphVertex to)
    {
        if (Directed)
        {
            return edge.From.Id == from.Id && edge.To.Id == to.Id;
        }
        return (edge.From.Id == from.Id && edge.To.Id == to.Id) ||
               (edge.From.Id == to.Id && edge.To.Id == from.Id);
    }

    private int IndexOf(GraphVertex vertex)
    {
        for (var index = 0; index < _vertices.Count; index++)
        {
            if (_vertices[index].Id == vertex.Id)
            {
                return index;
            }
        }
        return -1;
    }

    private void ClearVisualStates()
    {
        for (var index = 0; index < _vertices.Count; index++)
        {
            _vertices[index].VisualState = GraphVertexVisualState.Normal;
        }
        for (var index = 0; index < _edges.Count; index++)
        {
            _edges[index].VisualState = GraphEdgeVisualState.Normal;
        }
    }

    private GraphOperationResult Result(
        GraphOperationKind operation,
        bool succeeded,
        string? first,
        string? second,
        double? weight,
        int comparisons,
        int initialVertices,
        int initialEdges,
        Guid? vertexId = null,
        Guid? edgeId = null,
        int neighborCount = 0) =>
        new(operation, succeeded, first, second, weight, comparisons,
            initialVertices, VertexCount, initialEdges, EdgeCount, vertexId, edgeId, neighborCount);

    private string EdgeLabel(GraphEdge edge) => Directed
        ? $"{edge.From.Label} → {edge.To.Label}"
        : $"{edge.From.Label} — {edge.To.Label}";

    private static string NormalizeLabel(string? label) => (label ?? string.Empty).Trim();
    private static bool LabelsEqual(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    private static string FormatWeight(double weight) => weight.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}

using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.GraphSpanningTree.Mst;

/// <summary>
/// Minimum spanning tree/forest simulation over the existing Graph snapshot.
/// Prim uses a project-owned min-edge heap. Kruskal manually merge-sorts edges and
/// uses a hand-written disjoint-set union with path compression + union by rank.
/// </summary>
public sealed class MstSimulation : SimulationAlgorithmBase
{
    private GraphSnapshot _graph = EmptyGraph();
    private MstVariant _variant = MstVariant.Prim;
    private int _startIndex;
    private bool[] _inForest = Array.Empty<bool>();
    private int[] _component = Array.Empty<int>();
    private int[] _dsuParent = Array.Empty<int>();
    private int[] _dsuRank = Array.Empty<int>();
    private readonly ManualDynamicArray<int> _selectedEdges = new();
    private readonly ManualMinEdgeFrontier _frontier = new();
    private int[] _sortedEdges = Array.Empty<int>();
    private int _currentVertex = -1;
    private int _candidateVertex = -1;
    private int _candidateEdge = -1;
    private int _rejectedEdge = -1;
    private double _totalWeight;
    private int _componentCount;
    private int _edgeChecks;
    private int _frontierPushes;
    private int _frontierPops;
    private int _cycleSkips;
    private int _sortComparisons;
    private int _findOperations;
    private int _unionOperations;
    private MstPhase _phase = MstPhase.Ready;

    public MstSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public void LoadGraph(GraphSnapshot graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        ResetRunState();
    }

    public void Configure(MstVariant variant, int startIndex = 0)
    {
        _variant = variant;
        _startIndex = _graph.VertexCount == 0 ? 0 : Math.Clamp(startIndex, 0, _graph.VertexCount - 1);
        ResetRunState();
    }

    public static bool TryValidateGraph(GraphSnapshot graph, out string? error)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.Directed)
        {
            error = "Minimum Spanning Tree requires an undirected graph. A directed edge does not represent the symmetric connection used by Prim or Kruskal.";
            return false;
        }
        error = null;
        return true;
    }

    public MstSnapshot CreateSnapshot()
    {
        var vertices = new MstVertexSnapshot[_graph.VertexCount];
        for (var index = 0; index < vertices.Length; index++)
        {
            var inForest = _inForest.Length == vertices.Length && _inForest[index];
            var state = inForest ? MstVertexState.InForest : MstVertexState.Normal;
            if (index == _currentVertex) state = MstVertexState.Current;
            if (index == _candidateVertex) state = MstVertexState.Candidate;
            vertices[index] = new MstVertexSnapshot(
                index,
                _graph.Vertices[index].Id,
                _graph.Vertices[index].Label,
                state,
                _component.Length == vertices.Length ? _component[index] : -1,
                _dsuParent.Length == vertices.Length ? _dsuParent[index] : index,
                _dsuRank.Length == vertices.Length ? _dsuRank[index] : 0,
                inForest);
        }

        return new MstSnapshot(
            _graph,
            vertices,
            Copy(_selectedEdges),
            _variant == MstVariant.Prim ? _frontier.Snapshot(_inForest) : [],
            Copy(_sortedEdges),
            _currentVertex,
            _candidateVertex,
            _candidateEdge,
            _rejectedEdge,
            _totalWeight,
            _componentCount,
            _edgeChecks,
            _frontierPushes,
            _frontierPops,
            _cycleSkips,
            _sortComparisons,
            _findOperations,
            _unionOperations,
            _phase,
            _variant,
            _startIndex);
    }

    public async Task<MstResult> BuildAsync(
        GraphSnapshot graph,
        MstVariant variant,
        int startIndex = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryValidateGraph(graph, out var error)) throw new InvalidOperationException(error);
        _graph = graph;
        _variant = variant;
        _startIndex = graph.VertexCount == 0 ? 0 : Math.Clamp(startIndex, 0, graph.VertexCount - 1);
        ResetRunState();
        return variant == MstVariant.Prim
            ? await RunPrimAsync(cancellationToken)
            : await RunKruskalAsync(cancellationToken);
    }

    private async Task<MstResult> RunPrimAsync(CancellationToken cancellationToken)
    {
        if (_graph.VertexCount == 0)
        {
            _phase = MstPhase.Complete;
            await NextStepAsync("The graph is empty, so there is no spanning tree to build.", cancellationToken);
            return BuildResult();
        }

        var nextRoot = _startIndex;
        while (true)
        {
            if (_inForest[nextRoot])
            {
                nextRoot = FirstOutsideForest();
                if (nextRoot < 0) break;
            }

            _componentCount++;
            _currentVertex = nextRoot;
            _inForest[nextRoot] = true;
            _component[nextRoot] = _componentCount - 1;
            _phase = MstPhase.StartingComponent;
            await NextStepAsync(
                _componentCount == 1
                    ? $"Start Prim at {Label(nextRoot)}. The tree initially contains this one vertex and no edges."
                    : $"The frontier is empty but unvisited vertices remain. Start forest component {_componentCount} at {Label(nextRoot)}; the graph is disconnected.",
                cancellationToken);

            await PushOutgoingAsync(nextRoot, cancellationToken);

            while (_frontier.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = _frontier.PopMin();
                _frontierPops++;
                _edgeChecks++;
                _candidateEdge = entry.EdgeIndex;
                _currentVertex = entry.FromIndex;
                _candidateVertex = entry.ToIndex;
                _rejectedEdge = -1;
                _phase = MstPhase.InspectingEdge;
                await NextStepAsync(
                    $"Take the lightest frontier edge {EdgeText(entry.EdgeIndex)}. It connects the current forest to candidate vertex {Label(entry.ToIndex)}.",
                    cancellationToken);

                if (_inForest[entry.ToIndex])
                {
                    _cycleSkips++;
                    _rejectedEdge = entry.EdgeIndex;
                    _phase = MstPhase.RejectingCycle;
                    await NextStepAsync(
                        $"Reject {EdgeText(entry.EdgeIndex)}. Both endpoints are already inside the forest, so adding it would create a cycle.",
                        cancellationToken);
                    continue;
                }

                _selectedEdges.Add(entry.EdgeIndex);
                _totalWeight += entry.Weight;
                _inForest[entry.ToIndex] = true;
                _component[entry.ToIndex] = _component[entry.FromIndex];
                _currentVertex = entry.ToIndex;
                _phase = MstPhase.SelectingEdge;
                await NextStepAsync(
                    $"Select {EdgeText(entry.EdgeIndex)}. {Label(entry.ToIndex)} joins the forest and total weight becomes {FormatWeight(_totalWeight)}.",
                    cancellationToken);

                await PushOutgoingAsync(entry.ToIndex, cancellationToken);
            }

            nextRoot = FirstOutsideForest();
            if (nextRoot < 0) break;
        }

        _candidateEdge = -1;
        _candidateVertex = -1;
        _currentVertex = -1;
        _phase = MstPhase.Complete;
        await NextStepAsync(
            _componentCount <= 1
                ? $"Prim complete. {_selectedEdges.Count} edges connect all {_graph.VertexCount} vertices with total weight {FormatWeight(_totalWeight)} and no cycle."
                : $"Prim complete as a minimum spanning forest with {_componentCount} components. A single spanning tree cannot exist because the graph is disconnected.",
            cancellationToken);
        return BuildResult();
    }

    private async Task PushOutgoingAsync(int vertexIndex, CancellationToken cancellationToken)
    {
        var neighbors = _graph.Vertices[vertexIndex].Neighbors;
        var pushed = 0;
        for (var slot = 0; slot < neighbors.Length; slot++)
        {
            var neighbor = neighbors[slot];
            var to = NeighborIndex(neighbor);
            if (to < 0 || to == vertexIndex || _inForest[to]) continue;
            var edgeIndex = EdgeIndex(neighbor);
            if (edgeIndex < 0) continue;
            _frontier.Push(edgeIndex, vertexIndex, to, EffectiveWeight(_graph.Edges[edgeIndex]));
            _frontierPushes++;
            pushed++;
        }
        _phase = MstPhase.PushingFrontier;
        await NextStepAsync(
            pushed == 0
                ? $"{Label(vertexIndex)} adds no new cut edge to the frontier."
                : $"Push {pushed} edge candidate(s) leaving {Label(vertexIndex)} into the manual min-heap frontier.",
            cancellationToken);
    }

    private async Task<MstResult> RunKruskalAsync(CancellationToken cancellationToken)
    {
        if (_graph.VertexCount == 0)
        {
            _phase = MstPhase.Complete;
            await NextStepAsync("The graph is empty, so there is no spanning tree to build.", cancellationToken);
            return BuildResult();
        }

        for (var index = 0; index < _graph.VertexCount; index++)
        {
            _dsuParent[index] = index;
            _component[index] = index;
        }
        _componentCount = _graph.VertexCount;
        _sortedEdges = BuildSortedEdgeOrder();
        _phase = MstPhase.SortingEdges;
        await NextStepAsync(
            $"Manually merge-sort all {_sortedEdges.Length} edges by ascending weight. Kruskal will inspect them in that order and use DSU to reject cycles.",
            cancellationToken);

        for (var order = 0; order < _sortedEdges.Length; order++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var edgeIndex = _sortedEdges[order];
            var edge = _graph.Edges[edgeIndex];
            _candidateEdge = edgeIndex;
            _currentVertex = edge.FromIndex;
            _candidateVertex = edge.ToIndex;
            _rejectedEdge = -1;
            _edgeChecks++;
            _phase = MstPhase.InspectingEdge;
            await NextStepAsync(
                $"Inspect next lightest edge {EdgeText(edgeIndex)}. Ask whether its endpoints already belong to the same DSU component.",
                cancellationToken);

            var leftRoot = Find(edge.FromIndex);
            var rightRoot = Find(edge.ToIndex);
            _phase = MstPhase.FindingRoots;
            await NextStepAsync(
                $"find({Label(edge.FromIndex)}) = {Label(leftRoot)} and find({Label(edge.ToIndex)}) = {Label(rightRoot)}.",
                cancellationToken);

            if (leftRoot == rightRoot)
            {
                _cycleSkips++;
                _rejectedEdge = edgeIndex;
                _phase = MstPhase.RejectingCycle;
                await NextStepAsync(
                    $"Reject {EdgeText(edgeIndex)}. Both endpoints are already connected, so this edge would close a cycle.",
                    cancellationToken);
                continue;
            }

            UnionRoots(leftRoot, rightRoot);
            _selectedEdges.Add(edgeIndex);
            _totalWeight += EffectiveWeight(edge);
            _unionOperations++;
            _componentCount--;
            RefreshComponentSnapshot();
            _phase = MstPhase.UnionComponents;
            await NextStepAsync(
                $"Select {EdgeText(edgeIndex)} and union the two DSU components. Total weight is now {FormatWeight(_totalWeight)}.",
                cancellationToken);
        }

        _candidateEdge = -1;
        _candidateVertex = -1;
        _currentVertex = -1;
        _phase = MstPhase.Complete;
        await NextStepAsync(
            _componentCount <= 1
                ? $"Kruskal complete. {_selectedEdges.Count} selected edges form a minimum spanning tree with total weight {FormatWeight(_totalWeight)}."
                : $"Kruskal complete as a minimum spanning forest with {_componentCount} components. The graph is disconnected, so one spanning tree cannot cover every vertex.",
            cancellationToken);
        return BuildResult();
    }

    private int[] BuildSortedEdgeOrder()
    {
        var indices = new int[_graph.Edges.Length];
        for (var index = 0; index < indices.Length; index++) indices[index] = index;
        if (indices.Length <= 1) return indices;
        var buffer = new int[indices.Length];
        MergeSort(indices, buffer, 0, indices.Length);
        return indices;
    }

    private void MergeSort(int[] values, int[] buffer, int start, int end)
    {
        if (end - start <= 1) return;
        var middle = start + ((end - start) / 2);
        MergeSort(values, buffer, start, middle);
        MergeSort(values, buffer, middle, end);
        var left = start;
        var right = middle;
        var write = start;
        while (left < middle && right < end)
        {
            _sortComparisons++;
            if (EdgeLessOrEqual(values[left], values[right])) buffer[write++] = values[left++];
            else buffer[write++] = values[right++];
        }
        while (left < middle) buffer[write++] = values[left++];
        while (right < end) buffer[write++] = values[right++];
        for (var index = start; index < end; index++) values[index] = buffer[index];
    }

    private bool EdgeLessOrEqual(int leftIndex, int rightIndex)
    {
        var left = EffectiveWeight(_graph.Edges[leftIndex]);
        var right = EffectiveWeight(_graph.Edges[rightIndex]);
        return left < right || (Math.Abs(left - right) <= 1e-9 && leftIndex <= rightIndex);
    }

    private int Find(int vertex)
    {
        _findOperations++;
        var root = vertex;
        while (_dsuParent[root] != root)
        {
            _findOperations++;
            root = _dsuParent[root];
        }
        while (_dsuParent[vertex] != vertex)
        {
            var next = _dsuParent[vertex];
            _dsuParent[vertex] = root;
            vertex = next;
        }
        return root;
    }

    private void UnionRoots(int leftRoot, int rightRoot)
    {
        if (_dsuRank[leftRoot] < _dsuRank[rightRoot]) _dsuParent[leftRoot] = rightRoot;
        else if (_dsuRank[leftRoot] > _dsuRank[rightRoot]) _dsuParent[rightRoot] = leftRoot;
        else
        {
            _dsuParent[rightRoot] = leftRoot;
            _dsuRank[leftRoot]++;
        }
    }

    private void RefreshComponentSnapshot()
    {
        for (var index = 0; index < _component.Length; index++) _component[index] = RootReadonly(index);
    }

    private int RootReadonly(int vertex)
    {
        var guard = 0;
        while (_dsuParent.Length > vertex && _dsuParent[vertex] != vertex && guard++ <= _dsuParent.Length) vertex = _dsuParent[vertex];
        return vertex;
    }

    private MstResult BuildResult() => new(
        _graph,
        _variant,
        _startIndex,
        Copy(_selectedEdges),
        _totalWeight,
        _graph.VertexCount == 0 ? 0 : Math.Max(1, _componentCount),
        _edgeChecks,
        _frontierPushes,
        _frontierPops,
        _cycleSkips,
        _sortComparisons,
        _findOperations,
        _unionOperations);

    private void ResetRunState()
    {
        _inForest = new bool[_graph.VertexCount];
        _component = new int[_graph.VertexCount];
        _dsuParent = new int[_graph.VertexCount];
        _dsuRank = new int[_graph.VertexCount];
        for (var index = 0; index < _component.Length; index++)
        {
            _component[index] = -1;
            _dsuParent[index] = index;
        }
        _selectedEdges.Clear();
        _frontier.Clear();
        _sortedEdges = Array.Empty<int>();
        _currentVertex = -1;
        _candidateVertex = -1;
        _candidateEdge = -1;
        _rejectedEdge = -1;
        _totalWeight = 0d;
        _componentCount = 0;
        _edgeChecks = 0;
        _frontierPushes = 0;
        _frontierPops = 0;
        _cycleSkips = 0;
        _sortComparisons = 0;
        _findOperations = 0;
        _unionOperations = 0;
        _phase = MstPhase.Ready;
    }

    private int FirstOutsideForest()
    {
        for (var index = 0; index < _inForest.Length; index++) if (!_inForest[index]) return index;
        return -1;
    }

    private int NeighborIndex(GraphNeighborSnapshot neighbor)
    {
        if (neighbor.VertexIndex >= 0 && neighbor.VertexIndex < _graph.VertexCount && _graph.Vertices[neighbor.VertexIndex].Id == neighbor.VertexId) return neighbor.VertexIndex;
        for (var index = 0; index < _graph.VertexCount; index++) if (_graph.Vertices[index].Id == neighbor.VertexId) return index;
        return -1;
    }

    private int EdgeIndex(GraphNeighborSnapshot neighbor)
    {
        if (neighbor.EdgeIndex >= 0 && neighbor.EdgeIndex < _graph.Edges.Length && _graph.Edges[neighbor.EdgeIndex].Id == neighbor.EdgeId) return neighbor.EdgeIndex;
        for (var index = 0; index < _graph.Edges.Length; index++) if (_graph.Edges[index].Id == neighbor.EdgeId) return index;
        return -1;
    }

    private string Label(int index) => index >= 0 && index < _graph.VertexCount ? _graph.Vertices[index].Label : "?";
    private string EdgeText(int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex >= _graph.Edges.Length) return "?";
        var edge = _graph.Edges[edgeIndex];
        return $"{edge.FromLabel}—{edge.ToLabel} (w={FormatWeight(EffectiveWeight(edge))})";
    }

    private static double EffectiveWeight(GraphEdgeSnapshot edge) => edge.Weight;
    private static string FormatWeight(double value) => value.ToString("0.###");
    private static int[] Copy(ManualDynamicArray<int> source)
    {
        var result = new int[source.Count];
        for (var index = 0; index < source.Count; index++) result[index] = source[index];
        return result;
    }
    private static int[] Copy(int[] source)
    {
        var result = new int[source.Length];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        return result;
    }
    private static GraphSnapshot EmptyGraph() => new(false, true, 0, 0, [], [], []);
}

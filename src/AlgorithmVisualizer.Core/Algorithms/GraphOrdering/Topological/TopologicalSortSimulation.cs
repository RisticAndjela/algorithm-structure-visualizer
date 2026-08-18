using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.GraphOrdering.Topological;

/// <summary>
/// Topological ordering over the existing Graph snapshot. Kahn mode uses the project's
/// ManualDynamicArray as a head-index FIFO queue; DFS mode uses recursive color-state
/// traversal plus a manual postorder buffer. No framework Queue/Stack/topological helper is used.
/// </summary>
public sealed class TopologicalSortSimulation : SimulationAlgorithmBase
{
    private GraphSnapshot _graph = EmptyGraph();
    private TopologicalSortVariant _variant = TopologicalSortVariant.KahnQueue;
    private int[] _inDegree = Array.Empty<int>();
    private int[] _color = Array.Empty<int>();
    private bool[] _ordered = Array.Empty<bool>();
    private readonly ManualDynamicArray<int> _frontier = new();
    private readonly ManualDynamicArray<int> _output = new();
    private readonly ManualDynamicArray<int> _postorder = new();
    private int _frontierHead;
    private int _currentIndex = -1;
    private int _neighborIndex = -1;
    private Guid? _currentEdgeId;
    private int _edgeChecks;
    private int _inDegreeUpdates;
    private int _queueEnqueues;
    private int _backtrackCount;
    private int _initialReadyCount;
    private bool _cycleDetected;
    private TopologicalSortPhase _phase = TopologicalSortPhase.Ready;

    public TopologicalSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public void LoadGraph(GraphSnapshot graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        ResetRunState();
    }

    public void Configure(TopologicalSortVariant variant)
    {
        _variant = variant;
        ResetRunState();
    }

    public static bool TryValidateGraph(GraphSnapshot graph, out string? error)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!graph.Directed)
        {
            error = "Topological Sort requires a directed graph. Edge direction expresses prerequisite/dependency order.";
            return false;
        }

        error = null;
        return true;
    }

    public TopologicalSortSnapshot CreateSnapshot()
    {
        var vertices = new TopologicalVertexSnapshot[_graph.VertexCount];
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            var state = TopologicalVertexState.Unprocessed;
            var ordered = _ordered.Length == _graph.VertexCount && _ordered[index];
            var color = _color.Length == _graph.VertexCount ? _color[index] : 0;
            var inDegree = _inDegree.Length == _graph.VertexCount ? _inDegree[index] : _graph.Vertices[index].InDegree;

            if (_variant == TopologicalSortVariant.KahnQueue && IsInActiveFrontier(index)) state = TopologicalVertexState.Ready;
            if (_variant == TopologicalSortVariant.DfsPostorder)
            {
                if (color == 1) state = TopologicalVertexState.Visiting;
                else if (color == 2) state = TopologicalVertexState.Finished;
            }
            if (ordered) state = TopologicalVertexState.Ordered;
            if (index == _currentIndex) state = TopologicalVertexState.Current;
            if (index == _neighborIndex) state = _cycleDetected ? TopologicalVertexState.Cycle : TopologicalVertexState.InspectingNeighbor;

            vertices[index] = new TopologicalVertexSnapshot(
                index,
                _graph.Vertices[index].Id,
                _graph.Vertices[index].Label,
                state,
                inDegree,
                color,
                ordered);
        }

        return new TopologicalSortSnapshot(
            _graph,
            vertices,
            CopyActiveFrontier(),
            Copy(_output),
            Copy(_postorder),
            _currentIndex,
            _neighborIndex,
            _currentEdgeId,
            _edgeChecks,
            _inDegreeUpdates,
            _queueEnqueues,
            _backtrackCount,
            _initialReadyCount,
            _cycleDetected,
            _phase,
            _variant);
    }

    public async Task<TopologicalSortResult> SortAsync(
        GraphSnapshot graph,
        TopologicalSortVariant variant,
        CancellationToken cancellationToken = default)
    {
        if (!TryValidateGraph(graph, out var error))
        {
            throw new InvalidOperationException(error);
        }

        _graph = graph;
        _variant = variant;
        ResetRunState();

        return variant == TopologicalSortVariant.KahnQueue
            ? await RunKahnAsync(cancellationToken)
            : await RunDfsAsync(cancellationToken);
    }

    private async Task<TopologicalSortResult> RunKahnAsync(CancellationToken cancellationToken)
    {
        _ordered = new bool[_graph.VertexCount];

        for (var index = 0; index < _graph.VertexCount; index++)
        {
            if (_inDegree[index] != 0) continue;
            _frontier.Add(index);
            _queueEnqueues++;
        }
        _initialReadyCount = _frontier.Count;
        _phase = TopologicalSortPhase.Initializing;

        await NextStepAsync(
            _initialReadyCount == 0 && _graph.VertexCount > 0
                ? "Kahn initialization found no zero-in-degree vertex. A directed cycle must block every possible first dependency-free choice."
                : $"Compute indegree[] and enqueue every zero-in-degree vertex. {_initialReadyCount} vertex/vertices are ready to appear next.",
            cancellationToken);

        while (_frontierHead < _frontier.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentIndex = _frontier[_frontierHead++];
            _output.Add(_currentIndex);
            _ordered[_currentIndex] = true;
            _phase = TopologicalSortPhase.TakingReadyVertex;

            await NextStepAsync(
                $"Dequeue {Label(_currentIndex)} and append it to the topological order. Its remaining indegree is 0, so every prerequisite that points to it has already been placed.",
                cancellationToken);

            var neighbors = _graph.Vertices[_currentIndex].Neighbors;
            for (var slot = 0; slot < neighbors.Length; slot++)
            {
                var neighbor = neighbors[slot];
                var candidateIndex = NeighborIndex(neighbor);
                if (candidateIndex < 0) continue;

                _neighborIndex = candidateIndex;
                _currentEdgeId = neighbor.EdgeId;
                _edgeChecks++;
                _phase = TopologicalSortPhase.InspectingEdge;
                await NextStepAsync(
                    $"Remove dependency effect {Label(_currentIndex)} → {Label(candidateIndex)} from the remaining graph. Inspect indegree[{Label(candidateIndex)}].",
                    cancellationToken);

                _inDegree[candidateIndex]--;
                _inDegreeUpdates++;
                _phase = TopologicalSortPhase.DecrementingInDegree;
                await NextStepAsync(
                    $"indegree[{Label(candidateIndex)}] becomes {_inDegree[candidateIndex]}." + (_inDegree[candidateIndex] == 0 ? " It is now dependency-free and can enter the FIFO ready queue." : " It still has an unprocessed prerequisite."),
                    cancellationToken);

                if (_inDegree[candidateIndex] == 0)
                {
                    _frontier.Add(candidateIndex);
                    _queueEnqueues++;
                }
            }

            _neighborIndex = -1;
            _currentEdgeId = null;
        }

        if (_output.Count != _graph.VertexCount)
        {
            _cycleDetected = true;
            _phase = TopologicalSortPhase.DetectingCycle;
            await NextStepAsync(
                $"The ready queue is empty after ordering {_output.Count} of {_graph.VertexCount} vertices. The remaining vertices all depend on one another through a directed cycle, so no topological ordering exists.",
                cancellationToken);
        }
        else
        {
            _phase = TopologicalSortPhase.Complete;
            await NextStepAsync(
                $"Topological Sort is complete. All {_graph.VertexCount} vertices were emitted while every directed prerequisite stayed before its dependent vertex.",
                cancellationToken);
        }

        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        return BuildResult();
    }

    private async Task<TopologicalSortResult> RunDfsAsync(CancellationToken cancellationToken)
    {
        _color = new int[_graph.VertexCount];
        _ordered = new bool[_graph.VertexCount];
        _phase = TopologicalSortPhase.Initializing;
        await NextStepAsync(
            "Initialize DFS color state: white = unvisited, gray = currently on the recursion path, black = completely finished. A gray → gray edge proves a directed cycle.",
            cancellationToken);

        for (var index = 0; index < _graph.VertexCount && !_cycleDetected; index++)
        {
            if (_color[index] != 0) continue;
            await VisitDfsAsync(index, cancellationToken);
        }

        if (_cycleDetected)
        {
            _phase = TopologicalSortPhase.DetectingCycle;
            await NextStepAsync(
                "DFS found a back edge to a gray vertex. That edge closes a directed cycle, so reversing finish times cannot produce a valid topological order.",
                cancellationToken);
        }
        else
        {
            _phase = TopologicalSortPhase.ReversingPostorder;
            await NextStepAsync(
                "Every vertex is black. Reverse the DFS finish/postorder sequence so each prerequisite appears before vertices that depend on it.",
                cancellationToken);

            for (var index = _postorder.Count - 1; index >= 0; index--)
            {
                var vertex = _postorder[index];
                _output.Add(vertex);
                _ordered[vertex] = true;
            }

            _phase = TopologicalSortPhase.Complete;
            await NextStepAsync(
                $"Topological Sort is complete. Reversed postorder contains all {_graph.VertexCount} vertices and respects every directed edge.",
                cancellationToken);
        }

        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        return BuildResult();
    }

    private async Task VisitDfsAsync(int vertexIndex, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _color[vertexIndex] = 1;
        _frontier.Add(vertexIndex);
        _currentIndex = vertexIndex;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = TopologicalSortPhase.EnteringVertex;

        await NextStepAsync(
            $"Enter {Label(vertexIndex)}. Mark it gray and push it onto the visible recursion path before exploring outgoing dependencies.",
            cancellationToken);

        var neighbors = _graph.Vertices[vertexIndex].Neighbors;
        for (var slot = 0; slot < neighbors.Length; slot++)
        {
            if (_cycleDetected) return;
            var neighbor = neighbors[slot];
            var candidateIndex = NeighborIndex(neighbor);
            if (candidateIndex < 0) continue;

            _currentIndex = vertexIndex;
            _neighborIndex = candidateIndex;
            _currentEdgeId = neighbor.EdgeId;
            _edgeChecks++;
            _phase = TopologicalSortPhase.InspectingEdge;
            await NextStepAsync(
                $"Inspect {Label(vertexIndex)} → {Label(candidateIndex)}. The color of {Label(candidateIndex)} decides whether to recurse, skip a finished subtree, or report a cycle.",
                cancellationToken);

            if (_color[candidateIndex] == 1)
            {
                _cycleDetected = true;
                _phase = TopologicalSortPhase.DetectingCycle;
                await NextStepAsync(
                    $"{Label(candidateIndex)} is gray, so it is already on the current recursion path. Edge {Label(vertexIndex)} → {Label(candidateIndex)} is a back edge and proves a directed cycle.",
                    cancellationToken);
                return;
            }

            if (_color[candidateIndex] == 0)
            {
                await VisitDfsAsync(candidateIndex, cancellationToken);
                if (_cycleDetected) return;
                _currentIndex = vertexIndex;
            }
        }

        _color[vertexIndex] = 2;
        _postorder.Add(vertexIndex);
        if (_frontier.Count > 0) _frontier.RemoveAt(_frontier.Count - 1);
        _backtrackCount++;
        _currentIndex = vertexIndex;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = TopologicalSortPhase.FinishingVertex;
        await NextStepAsync(
            $"Finish {Label(vertexIndex)}. Mark it black, append it to postorder, and backtrack. It is safe to place only after every outgoing dependency subtree has finished.",
            cancellationToken);
    }

    private TopologicalSortResult BuildResult() => new(
        _graph,
        _variant,
        Copy(_output),
        _edgeChecks,
        _inDegreeUpdates,
        _queueEnqueues,
        _backtrackCount,
        _initialReadyCount,
        _cycleDetected,
        _variant == TopologicalSortVariant.KahnQueue ? _output.Count : (_cycleDetected ? _postorder.Count : _output.Count));

    private void ResetRunState()
    {
        _inDegree = BuildInDegrees(_graph);
        _color = new int[_graph.VertexCount];
        _ordered = new bool[_graph.VertexCount];
        _frontier.Clear();
        _output.Clear();
        _postorder.Clear();
        _frontierHead = 0;
        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _edgeChecks = 0;
        _inDegreeUpdates = 0;
        _queueEnqueues = 0;
        _backtrackCount = 0;
        _initialReadyCount = 0;
        _cycleDetected = false;
        _phase = TopologicalSortPhase.Ready;
    }

    private static int[] BuildInDegrees(GraphSnapshot graph)
    {
        var result = new int[graph.VertexCount];
        for (var edgeIndex = 0; edgeIndex < graph.Edges.Length; edgeIndex++)
        {
            var target = graph.Edges[edgeIndex].ToIndex;
            if (target >= 0 && target < result.Length) result[target]++;
        }
        return result;
    }

    private int NeighborIndex(GraphNeighborSnapshot neighbor)
    {
        if (neighbor.VertexIndex >= 0 && neighbor.VertexIndex < _graph.VertexCount && _graph.Vertices[neighbor.VertexIndex].Id == neighbor.VertexId)
            return neighbor.VertexIndex;
        for (var index = 0; index < _graph.VertexCount; index++)
            if (_graph.Vertices[index].Id == neighbor.VertexId) return index;
        return -1;
    }

    private bool IsInActiveFrontier(int vertexIndex)
    {
        for (var index = _frontierHead; index < _frontier.Count; index++)
            if (_frontier[index] == vertexIndex) return true;
        return false;
    }

    private int[] CopyActiveFrontier()
    {
        if (_variant == TopologicalSortVariant.DfsPostorder) return Copy(_frontier);
        var length = Math.Max(0, _frontier.Count - _frontierHead);
        var result = new int[length];
        for (var index = 0; index < length; index++) result[index] = _frontier[_frontierHead + index];
        return result;
    }

    private static int[] Copy(ManualDynamicArray<int> source)
    {
        var result = new int[source.Count];
        for (var index = 0; index < source.Count; index++) result[index] = source[index];
        return result;
    }

    private string Label(int index) => index >= 0 && index < _graph.VertexCount ? _graph.Vertices[index].Label : "?";
    private static GraphSnapshot EmptyGraph() => new(true, false, 0, 0, [], [], []);
}

using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.GraphTraversal;

/// <summary>
/// DFS over the existing Graph snapshot. Recursive mode exposes call-stack/backtracking behavior;
/// iterative mode uses the project's manual dynamic-array storage as an explicit LIFO stack.
/// </summary>
public sealed class DepthFirstSearchSimulation : SimulationAlgorithmBase
{
    private GraphSnapshot _graph = EmptyGraph();
    private bool[] _visited = Array.Empty<bool>();
    private bool[] _scheduled = Array.Empty<bool>();
    private int[] _parents = Array.Empty<int>();
    private int[] _depths = Array.Empty<int>();
    private readonly ManualDynamicArray<int> _frontier = new();
    private readonly ManualDynamicArray<int> _order = new();
    private int _currentIndex = -1;
    private int _neighborIndex = -1;
    private Guid? _currentEdgeId;
    private int _edgeChecks;
    private int _discoveredCount;
    private int _backtracks;
    private int _maxFrontierDepth;
    private GraphTraversalPhase _phase = GraphTraversalPhase.Ready;
    private DepthFirstTraversalVariant _variant = DepthFirstTraversalVariant.Recursive;

    public DepthFirstSearchSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public void LoadGraph(GraphSnapshot graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        ResetRunState();
    }

    public void Configure(DepthFirstTraversalVariant variant)
    {
        _variant = variant;
        ResetRunState();
    }

    public GraphTraversalSnapshot CreateSnapshot()
    {
        var vertices = new GraphTraversalVertexSnapshot[_graph.VertexCount];
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            var state = GraphTraversalVertexState.Unvisited;
            if (_visited.Length == _graph.VertexCount && _visited[index]) state = GraphTraversalVertexState.Visited;
            if (Contains(_frontier, index) && !_visited[index]) state = GraphTraversalVertexState.Frontier;
            if (index == _currentIndex) state = GraphTraversalVertexState.Current;
            if (index == _neighborIndex) state = GraphTraversalVertexState.InspectingNeighbor;

            vertices[index] = new GraphTraversalVertexSnapshot(
                index,
                _graph.Vertices[index].Id,
                _graph.Vertices[index].Label,
                state,
                _visited.Length == _graph.VertexCount && _visited[index],
                _parents.Length == _graph.VertexCount ? _parents[index] : -1,
                _depths.Length == _graph.VertexCount ? _depths[index] : -1);
        }

        return new GraphTraversalSnapshot(
            _graph,
            vertices,
            GraphTraversalSupport.Copy(_frontier),
            GraphTraversalSupport.Copy(_order),
            _currentIndex,
            _neighborIndex,
            _currentEdgeId,
            _edgeChecks,
            _discoveredCount,
            _phase,
            _variant == DepthFirstTraversalVariant.Recursive ? "Call stack" : "Explicit stack",
            "Depth");
    }

    public async Task<DepthFirstSearchResult> TraverseAsync(
        GraphSnapshot graph,
        int startIndex,
        DepthFirstTraversalVariant variant,
        CancellationToken cancellationToken = default)
    {
        GraphTraversalSupport.ValidateStart(graph, startIndex);
        _graph = graph;
        _variant = variant;
        ResetRunState();
        _visited = new bool[graph.VertexCount];
        _scheduled = new bool[graph.VertexCount];
        _parents = GraphTraversalSupport.CreateFilled(graph.VertexCount, -1);
        _depths = GraphTraversalSupport.CreateFilled(graph.VertexCount, -1);

        if (variant == DepthFirstTraversalVariant.Recursive)
        {
            _depths[startIndex] = 0;
            await VisitRecursiveAsync(startIndex, 0, cancellationToken);
        }
        else
        {
            await TraverseIterativeAsync(startIndex, cancellationToken);
        }

        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = GraphTraversalPhase.Complete;
        await NextStepAsync(
            $"DFS is complete. It reached {_discoveredCount} of {_graph.VertexCount} vertices from the chosen start. Unreachable components stay unvisited.",
            cancellationToken);

        return new DepthFirstSearchResult(
            _graph,
            startIndex,
            variant,
            GraphTraversalSupport.Copy(_order),
            GraphTraversalSupport.Copy(_parents),
            GraphTraversalSupport.Copy(_depths),
            _edgeChecks,
            _discoveredCount,
            _backtracks,
            _maxFrontierDepth);
    }

    private async Task VisitRecursiveAsync(int vertexIndex, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _visited[vertexIndex] = true;
        _scheduled[vertexIndex] = true;
        _depths[vertexIndex] = depth;
        _frontier.Add(vertexIndex);
        _order.Add(vertexIndex);
        _discoveredCount++;
        if (_frontier.Count > _maxFrontierDepth) _maxFrontierDepth = _frontier.Count;
        _currentIndex = vertexIndex;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = GraphTraversalPhase.Starting;

        await NextStepAsync(
            $"Enter {Label(vertexIndex)} at depth {depth}. Mark it visited and push a recursive call frame. DFS now tries one outgoing neighbor before any sibling branch.",
            cancellationToken);

        var neighbors = _graph.Vertices[vertexIndex].Neighbors;
        for (var slot = 0; slot < neighbors.Length; slot++)
        {
            var neighbor = neighbors[slot];
            var candidateIndex = GraphTraversalSupport.GetNeighborVertexIndex(_graph, neighbor);
            if (candidateIndex < 0) continue;

            _currentIndex = vertexIndex;
            _neighborIndex = candidateIndex;
            _currentEdgeId = neighbor.EdgeId;
            _edgeChecks++;
            _phase = GraphTraversalPhase.InspectingEdge;

            await NextStepAsync(
                $"From {Label(vertexIndex)}, inspect {Label(candidateIndex)}. DFS follows this edge immediately only if the neighbor is still unvisited.",
                cancellationToken);

            if (_visited[candidateIndex])
            {
                _phase = GraphTraversalPhase.SkippingVisited;
                await NextStepAsync(
                    $"{Label(candidateIndex)} is already visited. Skip this edge; the visited set prevents cycles from causing infinite recursion.",
                    cancellationToken);
                continue;
            }

            _parents[candidateIndex] = vertexIndex;
            _depths[candidateIndex] = depth + 1;
            _phase = GraphTraversalPhase.Discovering;
            await NextStepAsync(
                $"Descend from {Label(vertexIndex)} to {Label(candidateIndex)}. Parent[{Label(candidateIndex)}] = {Label(vertexIndex)}.",
                cancellationToken);

            await VisitRecursiveAsync(candidateIndex, depth + 1, cancellationToken);
            _currentIndex = vertexIndex;
            _neighborIndex = -1;
            _currentEdgeId = null;
        }

        if (_frontier.Count > 0) _frontier.RemoveAt(_frontier.Count - 1);
        if (_parents[vertexIndex] >= 0) _backtracks++;
        _currentIndex = _parents[vertexIndex];
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = GraphTraversalPhase.Backtracking;

        await NextStepAsync(
            _parents[vertexIndex] >= 0
                ? $"{Label(vertexIndex)} has no unvisited outgoing neighbor left. Return to {Label(_parents[vertexIndex])}: this is DFS backtracking."
                : $"{Label(vertexIndex)} has no unvisited outgoing neighbor left. Its root call returns, so the reachable DFS component is finished.",
            cancellationToken);
    }

    private async Task TraverseIterativeAsync(int startIndex, CancellationToken cancellationToken)
    {
        _scheduled[startIndex] = true;
        _depths[startIndex] = 0;
        _frontier.Add(startIndex);
        _maxFrontierDepth = 1;
        _phase = GraphTraversalPhase.Starting;

        await NextStepAsync(
            $"Push start vertex {Label(startIndex)} onto the explicit stack. Iterative DFS uses LIFO instead of the language call stack.",
            cancellationToken);

        while (_frontier.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var top = _frontier.Count - 1;
            _currentIndex = _frontier[top];
            _frontier.RemoveAt(top);
            _neighborIndex = -1;
            _currentEdgeId = null;
            _phase = GraphTraversalPhase.TakingFromFrontier;

            if (!_visited[_currentIndex])
            {
                _visited[_currentIndex] = true;
                _order.Add(_currentIndex);
                _discoveredCount++;
            }

            await NextStepAsync(
                $"Pop {Label(_currentIndex)} from the stack and visit it. LIFO makes the most recently scheduled branch run next.",
                cancellationToken);

            var neighbors = _graph.Vertices[_currentIndex].Neighbors;
            for (var slot = neighbors.Length - 1; slot >= 0; slot--)
            {
                var neighbor = neighbors[slot];
                var candidateIndex = GraphTraversalSupport.GetNeighborVertexIndex(_graph, neighbor);
                if (candidateIndex < 0) continue;

                _neighborIndex = candidateIndex;
                _currentEdgeId = neighbor.EdgeId;
                _edgeChecks++;
                _phase = GraphTraversalPhase.InspectingEdge;

                await NextStepAsync(
                    $"Inspect {Label(candidateIndex)} while scanning neighbors in reverse order. Reverse push preserves the graph's first-neighbor-first DFS behavior when the stack later pops.",
                    cancellationToken);

                if (_scheduled[candidateIndex] || _visited[candidateIndex])
                {
                    _phase = GraphTraversalPhase.SkippingVisited;
                    await NextStepAsync(
                        $"{Label(candidateIndex)} is already visited or already scheduled, so do not push a duplicate stack entry.",
                        cancellationToken);
                    continue;
                }

                _scheduled[candidateIndex] = true;
                _parents[candidateIndex] = _currentIndex;
                _depths[candidateIndex] = _depths[_currentIndex] + 1;
                _frontier.Add(candidateIndex);
                if (_frontier.Count > _maxFrontierDepth) _maxFrontierDepth = _frontier.Count;
                _phase = GraphTraversalPhase.Discovering;

                await NextStepAsync(
                    $"Push {Label(candidateIndex)}. Its parent is {Label(_currentIndex)} and its DFS depth is {_depths[candidateIndex]}.",
                    cancellationToken);
            }

            _neighborIndex = -1;
            _currentEdgeId = null;
            _phase = GraphTraversalPhase.Backtracking;
            _backtracks++;
            await NextStepAsync(
                $"Finish {Label(_currentIndex)} for this iteration. Continue from the new top of the explicit stack.",
                cancellationToken);
        }
    }

    private void ResetRunState()
    {
        _visited = new bool[_graph.VertexCount];
        _scheduled = new bool[_graph.VertexCount];
        _parents = GraphTraversalSupport.CreateFilled(_graph.VertexCount, -1);
        _depths = GraphTraversalSupport.CreateFilled(_graph.VertexCount, -1);
        _frontier.Clear();
        _order.Clear();
        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _edgeChecks = 0;
        _discoveredCount = 0;
        _backtracks = 0;
        _maxFrontierDepth = 0;
        _phase = GraphTraversalPhase.Ready;
    }

    private string Label(int index) => _graph.Vertices[index].Label;

    private static bool Contains(ManualDynamicArray<int> values, int target)
    {
        for (var index = 0; index < values.Count; index++) if (values[index] == target) return true;
        return false;
    }

    private static GraphSnapshot EmptyGraph() => new(false, false, 0, 0, [], [], []);
}

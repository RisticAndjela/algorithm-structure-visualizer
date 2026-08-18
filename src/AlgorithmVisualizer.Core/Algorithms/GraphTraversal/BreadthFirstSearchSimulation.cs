using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.GraphTraversal;

/// <summary>
/// Canonical BFS over the existing Graph snapshot. The queue is built from the project's own
/// ManualDynamicArray so no framework Queue implementation hides enqueue/dequeue behavior.
/// </summary>
public sealed class BreadthFirstSearchSimulation : SimulationAlgorithmBase
{
    private GraphSnapshot _graph = EmptyGraph();
    private bool[] _visited = Array.Empty<bool>();
    private bool[] _inQueue = Array.Empty<bool>();
    private int[] _parents = Array.Empty<int>();
    private int[] _distances = Array.Empty<int>();
    private readonly ManualDynamicArray<int> _queue = new();
    private readonly ManualDynamicArray<int> _order = new();
    private int _queueHead;
    private int _currentIndex = -1;
    private int _neighborIndex = -1;
    private Guid? _currentEdgeId;
    private int _edgeChecks;
    private int _discoveredCount;
    private int _maxQueueSize;
    private GraphTraversalPhase _phase = GraphTraversalPhase.Ready;

    public BreadthFirstSearchSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public void LoadGraph(GraphSnapshot graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        ResetRunState();
    }

    public GraphTraversalSnapshot CreateSnapshot()
    {
        var vertices = new GraphTraversalVertexSnapshot[_graph.VertexCount];
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            var state = GraphTraversalVertexState.Unvisited;
            if (_visited.Length == _graph.VertexCount && _visited[index]) state = GraphTraversalVertexState.Visited;
            if (_inQueue.Length == _graph.VertexCount && _inQueue[index]) state = GraphTraversalVertexState.Frontier;
            if (index == _currentIndex) state = GraphTraversalVertexState.Current;
            if (index == _neighborIndex) state = GraphTraversalVertexState.InspectingNeighbor;

            vertices[index] = new GraphTraversalVertexSnapshot(
                index,
                _graph.Vertices[index].Id,
                _graph.Vertices[index].Label,
                state,
                _visited.Length == _graph.VertexCount && _visited[index],
                _parents.Length == _graph.VertexCount ? _parents[index] : -1,
                _distances.Length == _graph.VertexCount ? _distances[index] : -1);
        }

        return new GraphTraversalSnapshot(
            _graph,
            vertices,
            GraphTraversalSupport.CopyRange(_queue, _queueHead),
            GraphTraversalSupport.Copy(_order),
            _currentIndex,
            _neighborIndex,
            _currentEdgeId,
            _edgeChecks,
            _discoveredCount,
            _phase,
            "Queue",
            "Distance");
    }

    public async Task<BreadthFirstSearchResult> TraverseAsync(GraphSnapshot graph, int startIndex, CancellationToken cancellationToken = default)
    {
        GraphTraversalSupport.ValidateStart(graph, startIndex);
        _graph = graph;
        ResetRunState();

        _visited = new bool[graph.VertexCount];
        _inQueue = new bool[graph.VertexCount];
        _parents = GraphTraversalSupport.CreateFilled(graph.VertexCount, -1);
        _distances = GraphTraversalSupport.CreateFilled(graph.VertexCount, -1);

        _visited[startIndex] = true;
        _distances[startIndex] = 0;
        _queue.Add(startIndex);
        _inQueue[startIndex] = true;
        _discoveredCount = 1;
        _maxQueueSize = 1;
        _phase = GraphTraversalPhase.Starting;

        await NextStepAsync(
            $"Start BFS at {Label(startIndex)}. Mark it discovered at distance 0 and enqueue it. Mark-on-enqueue prevents the same vertex from entering the queue twice.",
            cancellationToken);

        while (_queueHead < _queue.Count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _currentIndex = _queue[_queueHead];
            _queueHead++;
            _inQueue[_currentIndex] = false;
            _neighborIndex = -1;
            _currentEdgeId = null;
            _order.Add(_currentIndex);
            _phase = GraphTraversalPhase.TakingFromFrontier;

            await NextStepAsync(
                $"Dequeue {Label(_currentIndex)}. Visit it next because BFS processes the oldest discovered vertex first (FIFO).",
                cancellationToken);

            var neighbors = _graph.Vertices[_currentIndex].Neighbors;
            for (var neighborSlot = 0; neighborSlot < neighbors.Length; neighborSlot++)
            {
                var neighbor = neighbors[neighborSlot];
                var candidateIndex = GraphTraversalSupport.GetNeighborVertexIndex(_graph, neighbor);
                if (candidateIndex < 0) continue;

                _neighborIndex = candidateIndex;
                _currentEdgeId = neighbor.EdgeId;
                _edgeChecks++;
                _phase = GraphTraversalPhase.InspectingEdge;

                await NextStepAsync(
                    $"Inspect edge {Label(_currentIndex)} {Arrow()} {Label(candidateIndex)}. Check whether {Label(candidateIndex)} has already been discovered.",
                    cancellationToken);

                if (_visited[candidateIndex])
                {
                    _phase = GraphTraversalPhase.SkippingVisited;
                    await NextStepAsync(
                        $"{Label(candidateIndex)} is already discovered, so do not enqueue it again. This is what keeps BFS finite on cycles and self-loops.",
                        cancellationToken);
                    continue;
                }

                _visited[candidateIndex] = true;
                _parents[candidateIndex] = _currentIndex;
                _distances[candidateIndex] = _distances[_currentIndex] + 1;
                _queue.Add(candidateIndex);
                _inQueue[candidateIndex] = true;
                _discoveredCount++;
                var activeQueueCount = _queue.Count - _queueHead;
                if (activeQueueCount > _maxQueueSize) _maxQueueSize = activeQueueCount;
                _phase = GraphTraversalPhase.Discovering;

                await NextStepAsync(
                    $"Discover {Label(candidateIndex)} at distance {_distances[candidateIndex]}. Set parent = {Label(_currentIndex)} and enqueue it at the rear.",
                    cancellationToken);
            }

            _neighborIndex = -1;
            _currentEdgeId = null;
            _phase = GraphTraversalPhase.Backtracking;
            await NextStepAsync(
                $"All outgoing neighbors of {Label(_currentIndex)} are handled. BFS now continues with the next vertex at the front of the queue.",
                cancellationToken);
        }

        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = GraphTraversalPhase.Complete;
        await NextStepAsync(
            $"BFS is complete. It reached {_discoveredCount} of {_graph.VertexCount} vertices from the chosen start and the queue is empty.",
            cancellationToken);

        return new BreadthFirstSearchResult(
            _graph,
            startIndex,
            GraphTraversalSupport.Copy(_order),
            GraphTraversalSupport.Copy(_parents),
            GraphTraversalSupport.Copy(_distances),
            _edgeChecks,
            _discoveredCount,
            _maxQueueSize);
    }

    private void ResetRunState()
    {
        _visited = new bool[_graph.VertexCount];
        _inQueue = new bool[_graph.VertexCount];
        _parents = GraphTraversalSupport.CreateFilled(_graph.VertexCount, -1);
        _distances = GraphTraversalSupport.CreateFilled(_graph.VertexCount, -1);
        _queue.Clear();
        _queueHead = 0;
        _order.Clear();
        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _edgeChecks = 0;
        _discoveredCount = 0;
        _maxQueueSize = 0;
        _phase = GraphTraversalPhase.Ready;
    }

    private string Label(int index) => _graph.Vertices[index].Label;
    private string Arrow() => _graph.Directed ? "→" : "—";

    private static GraphSnapshot EmptyGraph() => new(false, false, 0, 0, [], [], []);
}

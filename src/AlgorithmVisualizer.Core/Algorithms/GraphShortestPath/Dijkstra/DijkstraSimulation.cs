using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra;

/// <summary>
/// Dijkstra shortest paths over the existing Graph snapshot. The basic mode finds the next
/// unsettled minimum with a manual O(V) scan. The advanced mode uses a Dijkstra-specific binary min-heap built on the existing
/// ManualHeapArray storage, with lazy duplicate entries instead of PriorityQueue.
/// </summary>
public sealed class DijkstraSimulation : SimulationAlgorithmBase
{
    private GraphSnapshot _graph = EmptyGraph();
    private double[] _distances = Array.Empty<double>();
    private int[] _parents = Array.Empty<int>();
    private bool[] _settled = Array.Empty<bool>();
    private readonly ManualDynamicArray<int> _settledOrder = new();
    private readonly ManualMinPriorityFrontier _frontier = new();
    private DijkstraVariant _variant = DijkstraVariant.LinearScan;
    private int _currentIndex = -1;
    private int _neighborIndex = -1;
    private Guid? _currentEdgeId;
    private int _edgeChecks;
    private int _relaxationAttempts;
    private int _distanceUpdates;
    private int _selectionComparisons;
    private int _frontierPushes;
    private int _stalePops;
    private DijkstraPhase _phase = DijkstraPhase.Ready;

    public DijkstraSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public void Configure(DijkstraVariant variant) => _variant = variant;

    public void LoadGraph(GraphSnapshot graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        _graph = graph;
        ResetRunState();
    }

    public DijkstraSnapshot CreateSnapshot()
    {
        var vertices = new DijkstraVertexSnapshot[_graph.VertexCount];
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            var state = DijkstraVertexState.Unreached;
            if (_distances.Length == _graph.VertexCount && double.IsFinite(_distances[index])) state = DijkstraVertexState.Frontier;
            if (_settled.Length == _graph.VertexCount && _settled[index]) state = DijkstraVertexState.Settled;
            if (index == _currentIndex) state = DijkstraVertexState.Current;
            if (index == _neighborIndex) state = DijkstraVertexState.InspectingNeighbor;

            vertices[index] = new DijkstraVertexSnapshot(
                index,
                _graph.Vertices[index].Id,
                _graph.Vertices[index].Label,
                state,
                _settled.Length == _graph.VertexCount && _settled[index],
                _parents.Length == _graph.VertexCount ? _parents[index] : -1,
                _distances.Length == _graph.VertexCount ? _distances[index] : double.PositiveInfinity);
        }

        return new DijkstraSnapshot(
            _graph,
            vertices,
            CreateFrontierSnapshot(),
            CopySettledOrder(),
            _currentIndex,
            _neighborIndex,
            _currentEdgeId,
            _edgeChecks,
            _relaxationAttempts,
            _distanceUpdates,
            _selectionComparisons,
            _frontierPushes,
            _stalePops,
            _phase,
            _variant);
    }

    public static bool TryValidateGraph(GraphSnapshot graph, out string? error)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.VertexCount == 0)
        {
            error = "Dijkstra requires at least one graph vertex.";
            return false;
        }

        for (var edgeIndex = 0; edgeIndex < graph.Edges.Length; edgeIndex++)
        {
            var edge = graph.Edges[edgeIndex];
            if (!double.IsFinite(edge.Weight))
            {
                error = $"Edge {edge.FromLabel} → {edge.ToLabel} has a non-finite weight.";
                return false;
            }

            if (edge.Weight < 0)
            {
                error = $"Dijkstra cannot run with negative edge weights. Edge {edge.FromLabel} → {edge.ToLabel} has weight {Format(edge.Weight)}.";
                return false;
            }
        }

        error = null;
        return true;
    }

    public async Task<DijkstraResult> TraverseAsync(
        GraphSnapshot graph,
        int startIndex,
        DijkstraVariant variant,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (!TryValidateGraph(graph, out var validationError))
        {
            throw new InvalidOperationException(validationError);
        }

        if (startIndex < 0 || startIndex >= graph.VertexCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        _graph = graph;
        _variant = variant;
        ResetRunState();
        _distances = CreateInfinityArray(graph.VertexCount);
        _parents = CreateFilled(graph.VertexCount, -1);
        _settled = new bool[graph.VertexCount];
        _distances[startIndex] = 0d;
        _phase = DijkstraPhase.Starting;

        if (_variant == DijkstraVariant.MinHeap)
        {
            _frontier.Push(startIndex, 0d);
            _frontierPushes = 1;
        }

        await NextStepAsync(
            $"Start Dijkstra at {Label(startIndex)}. Set dist[{Label(startIndex)}] = 0 and every other tentative distance = ∞.",
            cancellationToken);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = _variant == DijkstraVariant.LinearScan
                ? await SelectByLinearScanAsync(cancellationToken)
                : await SelectByHeapAsync(cancellationToken);

            if (next < 0) break;

            _currentIndex = next;
            _neighborIndex = -1;
            _currentEdgeId = null;
            _settled[next] = true;
            _settledOrder.Add(next);
            _phase = DijkstraPhase.Settling;

            await NextStepAsync(
                $"Settle {Label(next)} with final distance {Format(_distances[next])}. No later non-negative path can make this distance smaller.",
                cancellationToken);

            var neighbors = graph.Vertices[next].Neighbors;
            for (var slot = 0; slot < neighbors.Length; slot++)
            {
                var neighbor = neighbors[slot];
                var candidateIndex = NeighborIndex(graph, neighbor);
                if (candidateIndex < 0) continue;

                _neighborIndex = candidateIndex;
                _currentEdgeId = neighbor.EdgeId;
                _edgeChecks++;
                _relaxationAttempts++;
                _phase = DijkstraPhase.InspectingEdge;

                await NextStepAsync(
                    $"Inspect {Label(next)} {Arrow()} {Label(candidateIndex)} with weight {Format(neighbor.Weight)}. Candidate = {Format(_distances[next])} + {Format(neighbor.Weight)}.",
                    cancellationToken);

                if (_settled[candidateIndex])
                {
                    _phase = DijkstraPhase.KeepingDistance;
                    await NextStepAsync(
                        $"{Label(candidateIndex)} is already settled at {Format(_distances[candidateIndex])}, so this outgoing check cannot change it.",
                        cancellationToken);
                    continue;
                }

                var candidateDistance = _distances[next] + neighbor.Weight;
                if (candidateDistance + 1e-9 < _distances[candidateIndex])
                {
                    var previous = _distances[candidateIndex];
                    _distances[candidateIndex] = candidateDistance;
                    _parents[candidateIndex] = next;
                    _distanceUpdates++;
                    _phase = DijkstraPhase.Relaxing;

                    if (_variant == DijkstraVariant.MinHeap)
                    {
                        _frontier.Push(candidateIndex, candidateDistance);
                        _frontierPushes++;
                    }

                    await NextStepAsync(
                        $"Relax {Label(candidateIndex)}: {Format(previous)} → {Format(candidateDistance)}. Parent becomes {Label(next)}{(_variant == DijkstraVariant.MinHeap ? " and a new priority entry is pushed." : ".")}",
                        cancellationToken);
                }
                else
                {
                    _phase = DijkstraPhase.KeepingDistance;
                    await NextStepAsync(
                        $"Keep dist[{Label(candidateIndex)}] = {Format(_distances[candidateIndex])}; candidate {Format(candidateDistance)} is not better.",
                        cancellationToken);
                }
            }

            _neighborIndex = -1;
            _currentEdgeId = null;
        }

        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _phase = DijkstraPhase.Complete;
        var reachable = 0;
        for (var index = 0; index < _distances.Length; index++)
        {
            if (double.IsFinite(_distances[index])) reachable++;
        }

        await NextStepAsync(
            $"Dijkstra is complete. {reachable} of {graph.VertexCount} vertices are reachable from {Label(startIndex)}; unreachable distances remain ∞.",
            cancellationToken);

        return new DijkstraResult(
            graph,
            startIndex,
            variant,
            Copy(_distances),
            Copy(_parents),
            CopySettledOrder(),
            _edgeChecks,
            _relaxationAttempts,
            _distanceUpdates,
            _selectionComparisons,
            _frontierPushes,
            _stalePops,
            reachable);
    }

    private async Task<int> SelectByLinearScanAsync(CancellationToken cancellationToken)
    {
        var bestIndex = -1;
        var bestDistance = double.PositiveInfinity;
        _phase = DijkstraPhase.SelectingMinimum;

        for (var index = 0; index < _graph.VertexCount; index++)
        {
            if (_settled[index] || !double.IsFinite(_distances[index])) continue;
            _selectionComparisons++;
            await NextStepAsync(
                bestIndex < 0
                    ? $"Linear minimum scan considers {Label(index)} at {Format(_distances[index])} as the first candidate."
                    : $"Compare tentative distance of {Label(index)} ({Format(_distances[index])}) with current minimum {Label(bestIndex)} ({Format(bestDistance)}).",
                cancellationToken);

            if (bestIndex < 0 || _distances[index] < bestDistance - 1e-9 ||
                (SameDistance(_distances[index], bestDistance) && index < bestIndex))
            {
                bestIndex = index;
                bestDistance = _distances[index];
            }
        }

        return bestIndex;
    }

    private async Task<int> SelectByHeapAsync(CancellationToken cancellationToken)
    {
        _phase = DijkstraPhase.SelectingMinimum;
        while (_frontier.Count > 0)
        {
            var comparisonsBefore = _frontier.ComparisonCount;
            var entry = _frontier.PopMin();
            _selectionComparisons += _frontier.ComparisonCount - comparisonsBefore;

            if (_settled[entry.VertexIndex] || !SameDistance(entry.Priority, _distances[entry.VertexIndex]))
            {
                _stalePops++;
                _phase = DijkstraPhase.SkippingStaleEntry;
                await NextStepAsync(
                    $"Pop stale priority entry {Label(entry.VertexIndex)} @ {Format(entry.Priority)}. Its current distance is {Format(_distances[entry.VertexIndex])}, so discard this old entry.",
                    cancellationToken);
                continue;
            }

            await NextStepAsync(
                $"Pop minimum priority {Label(entry.VertexIndex)} @ {Format(entry.Priority)} from the manual binary min-heap frontier.",
                cancellationToken);
            return entry.VertexIndex;
        }

        return -1;
    }

    private DijkstraFrontierEntrySnapshot[] CreateFrontierSnapshot()
    {
        if (_graph.VertexCount == 0) return [];
        if (_variant == DijkstraVariant.MinHeap && _distances.Length == _graph.VertexCount)
        {
            return _frontier.Snapshot(_distances, _settled);
        }

        if (_distances.Length != _graph.VertexCount) return [];
        var count = 0;
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            if (!_settled[index] && double.IsFinite(_distances[index])) count++;
        }

        var result = new DijkstraFrontierEntrySnapshot[count];
        var cursor = 0;
        for (var index = 0; index < _graph.VertexCount; index++)
        {
            if (_settled[index] || !double.IsFinite(_distances[index])) continue;
            result[cursor++] = new DijkstraFrontierEntrySnapshot(index, _distances[index], false);
        }
        return result;
    }

    private void ResetRunState()
    {
        _distances = CreateInfinityArray(_graph.VertexCount);
        _parents = CreateFilled(_graph.VertexCount, -1);
        _settled = new bool[_graph.VertexCount];
        _settledOrder.Clear();
        _frontier.Clear();
        _currentIndex = -1;
        _neighborIndex = -1;
        _currentEdgeId = null;
        _edgeChecks = 0;
        _relaxationAttempts = 0;
        _distanceUpdates = 0;
        _selectionComparisons = 0;
        _frontierPushes = 0;
        _stalePops = 0;
        _phase = DijkstraPhase.Ready;
    }

    private int[] CopySettledOrder()
    {
        var result = new int[_settledOrder.Count];
        for (var index = 0; index < _settledOrder.Count; index++) result[index] = _settledOrder[index];
        return result;
    }

    private static double[] CreateInfinityArray(int length)
    {
        var result = new double[length];
        for (var index = 0; index < length; index++) result[index] = double.PositiveInfinity;
        return result;
    }

    private static int[] CreateFilled(int length, int value)
    {
        var result = new int[length];
        for (var index = 0; index < length; index++) result[index] = value;
        return result;
    }

    private static double[] Copy(double[] source)
    {
        var result = new double[source.Length];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        return result;
    }

    private static int[] Copy(int[] source)
    {
        var result = new int[source.Length];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        return result;
    }

    private static int NeighborIndex(GraphSnapshot graph, GraphNeighborSnapshot neighbor)
    {
        if (neighbor.VertexIndex >= 0 && neighbor.VertexIndex < graph.VertexCount &&
            graph.Vertices[neighbor.VertexIndex].Id == neighbor.VertexId)
        {
            return neighbor.VertexIndex;
        }

        for (var index = 0; index < graph.VertexCount; index++)
        {
            if (graph.Vertices[index].Id == neighbor.VertexId) return index;
        }
        return -1;
    }

    private string Label(int index) => index >= 0 && index < _graph.VertexCount ? _graph.Vertices[index].Label : "?";
    private string Arrow() => _graph.Directed ? "→" : "—";
    private static string Format(double value) => double.IsPositiveInfinity(value) ? "∞" : value.ToString("0.###");
    private static bool SameDistance(double left, double right) => Math.Abs(left - right) <= 1e-9;
    private static GraphSnapshot EmptyGraph() => new(false, true, 0, 0, [], [], []);
}

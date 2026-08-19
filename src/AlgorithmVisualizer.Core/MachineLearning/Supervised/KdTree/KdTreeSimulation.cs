using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.KdTree;

/// <summary>
/// From-scratch KD-Tree build + nearest-neighbor search.
/// Points/query use the project-owned ManualVector and VectorSimulation supplies Euclidean distance.
/// KD-Tree itself owns median splitting, explicit node links, branch selection, backtracking and pruning.
/// </summary>
public sealed class KdTreeSimulation : SimulationAlgorithmBase
{
    private readonly VectorSimulation _vectorMath;
    private ManualVector[] _features = [];
    private int[] _labels = [];
    private ManualVector _query = new(2);
    private KdNode?[] _nodes = [];
    private KdTreeNodeState[] _states = [];
    private int _nodeCount;
    private int _rootNodeId = -1;
    private int _currentNodeId = -1;
    private int _bestNodeId = -1;
    private double _currentDistance = double.NaN;
    private double _bestDistance = double.PositiveInfinity;
    private double _splitPlaneDistance = double.NaN;
    private int _visitedNodes;
    private int _prunedNodes;
    private KdTreePhase _phase = KdTreePhase.Ready;
    private string _focusText = "Ready.";

    public KdTreeSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        _vectorMath = new VectorSimulation(new ImmediateSimulationRuntime());
        Configure(new KdTreeConfiguration(
            [
                [-4d, -1d], [-3d, 2d], [-2d, -3d], [-1d, 1d],
                [1d, -2d], [2d, 2d], [3d, -1d], [4d, 3d], [5d, 0d]
            ],
            [0, 0, 0, 0, 1, 1, 1, 1, 1],
            [2.4d, 1.5d]));
    }

    public void Configure(KdTreeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        _features = new ManualVector[configuration.Features.Length];
        for (var index = 0; index < configuration.Features.Length; index++)
        {
            var vector = new ManualVector(configuration.Features[index].Length);
            vector.CopyFrom(configuration.Features[index]);
            _features[index] = vector;
        }

        _labels = Copy(configuration.Labels);
        _query = new ManualVector(configuration.Query.Length);
        _query.CopyFrom(configuration.Query);
        ResetRunState();
    }

    public KdTreeSnapshot CreateSnapshot() => new(
        CopyFeatures(),
        Copy(_labels),
        _query.CopyValues(),
        CopyNodes(),
        _rootNodeId,
        _phase,
        _currentNodeId,
        _bestNodeId,
        _currentDistance,
        _bestDistance,
        _splitPlaneDistance,
        _visitedNodes,
        _prunedNodes,
        _focusText);

    public async Task<KdTreeRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        _phase = KdTreePhase.Building;
        _focusText = "Build a balanced spatial tree by alternating coordinate axes and choosing the median point on each split.";
        await NextStepAsync(_focusText, cancellationToken);

        var pointIndexes = new int[_features.Length];
        for (var index = 0; index < pointIndexes.Length; index++) pointIndexes[index] = index;
        var scratch = new int[pointIndexes.Length];
        _rootNodeId = await BuildAsync(pointIndexes, scratch, 0, pointIndexes.Length - 1, 0, cancellationToken);

        for (var nodeId = 0; nodeId < _nodeCount; nodeId++) _states[nodeId] = KdTreeNodeState.Stored;
        _phase = KdTreePhase.Descending;
        _focusText = "Tree built. Follow the query side first, then backtrack only when the opposite region can still beat the best distance.";
        await NextStepAsync(_focusText, cancellationToken);

        await SearchAsync(_rootNodeId, cancellationToken);

        _phase = KdTreePhase.Complete;
        _currentNodeId = -1;
        _currentDistance = double.NaN;
        _splitPlaneDistance = double.NaN;
        if (_bestNodeId >= 0) _states[_bestNodeId] = KdTreeNodeState.Best;
        var best = _nodes[_bestNodeId]!;
        _focusText = $"Nearest example is {best.PointIndex} with distance {Format(_bestDistance)}. Search visited {_visitedNodes} of {_features.Length} nodes and pruned {_prunedNodes}.";
        await NextStepAsync(_focusText, cancellationToken);

        return new KdTreeRunResult(
            CopyFeatures(),
            Copy(_labels),
            _query.CopyValues(),
            CopyNodes(),
            _rootNodeId,
            _bestNodeId,
            best.PointIndex,
            _features[best.PointIndex].CopyValues(),
            _labels[best.PointIndex],
            _bestDistance,
            _visitedNodes,
            _prunedNodes,
            MaxDepth(),
            _focusText);
    }

    private async Task<int> BuildAsync(int[] indexes, int[] scratch, int start, int end, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (start > end) return -1;

        var axis = depth % _query.Dimension;
        MergeSortRange(indexes, scratch, start, end, axis);
        var median = start + ((end - start) / 2);
        var pointIndex = indexes[median];
        var nodeId = _nodeCount++;
        _nodes[nodeId] = new KdNode(nodeId, pointIndex, axis, depth);
        _states[nodeId] = KdTreeNodeState.Building;
        _currentNodeId = nodeId;
        _focusText = $"Depth {depth}: split on feature {axis + 1}. Median example {pointIndex} becomes node {nodeId}.";
        await NextStepAsync(_focusText, cancellationToken);

        var left = await BuildAsync(indexes, scratch, start, median - 1, depth + 1, cancellationToken);
        var right = await BuildAsync(indexes, scratch, median + 1, end, depth + 1, cancellationToken);
        _nodes[nodeId]!.LeftNodeId = left;
        _nodes[nodeId]!.RightNodeId = right;
        _states[nodeId] = KdTreeNodeState.Stored;
        return nodeId;
    }

    private async Task SearchAsync(int nodeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (nodeId < 0) return;

        var node = _nodes[nodeId]!;
        _phase = KdTreePhase.Measuring;
        _currentNodeId = nodeId;
        _states[nodeId] = KdTreeNodeState.Active;
        _visitedNodes++;
        _currentDistance = await MeasureDistanceAsync(_features[node.PointIndex], cancellationToken);
        _focusText = $"Visit node {nodeId}: example {node.PointIndex} is {Format(_currentDistance)} from the query.";
        await NextStepAsync(_focusText, cancellationToken);

        if (_bestNodeId < 0 || _currentDistance < _bestDistance - 1e-12d ||
            (Math.Abs(_currentDistance - _bestDistance) <= 1e-12d && node.PointIndex < _nodes[_bestNodeId]!.PointIndex))
        {
            if (_bestNodeId >= 0 && _states[_bestNodeId] == KdTreeNodeState.Best) _states[_bestNodeId] = KdTreeNodeState.Visited;
            _bestNodeId = nodeId;
            _bestDistance = _currentDistance;
            _states[nodeId] = KdTreeNodeState.Best;
            _focusText = $"New best: example {node.PointIndex} at distance {Format(_bestDistance)}.";
            await NextStepAsync(_focusText, cancellationToken);
        }
        else if (_states[nodeId] != KdTreeNodeState.Best)
        {
            _states[nodeId] = KdTreeNodeState.Visited;
        }

        var splitValue = _features[node.PointIndex][node.Axis];
        var delta = _query[node.Axis] - splitValue;
        var nearNodeId = delta < 0d ? node.LeftNodeId : node.RightNodeId;
        var farNodeId = delta < 0d ? node.RightNodeId : node.LeftNodeId;

        if (nearNodeId >= 0)
        {
            _phase = KdTreePhase.Descending;
            _focusText = $"Query feature {node.Axis + 1} is {(delta < 0d ? "below" : "at/above")} split {Format(splitValue)}. Search that side first.";
            await NextStepAsync(_focusText, cancellationToken);
            await SearchAsync(nearNodeId, cancellationToken);
        }

        if (farNodeId >= 0)
        {
            _phase = KdTreePhase.Backtracking;
            _currentNodeId = nodeId;
            _splitPlaneDistance = Math.Abs(delta);
            _focusText = $"Backtrack to node {nodeId}. Split plane is {Format(_splitPlaneDistance)} away; current best is {Format(_bestDistance)}.";
            await NextStepAsync(_focusText, cancellationToken);

            if (_splitPlaneDistance <= _bestDistance + 1e-12d)
            {
                _focusText = "The opposite region can still contain a closer point, so search it too.";
                await NextStepAsync(_focusText, cancellationToken);
                await SearchAsync(farNodeId, cancellationToken);
            }
            else
            {
                _phase = KdTreePhase.Pruning;
                var pruned = MarkSubtreePruned(farNodeId);
                _prunedNodes += pruned;
                _focusText = $"Prune {pruned} node{(pruned == 1 ? string.Empty : "s")}: the split plane is already farther than the best candidate.";
                await NextStepAsync(_focusText, cancellationToken);
            }
        }

        if (_states[nodeId] == KdTreeNodeState.Active) _states[nodeId] = KdTreeNodeState.Visited;
    }

    private int MarkSubtreePruned(int nodeId)
    {
        if (nodeId < 0) return 0;
        var node = _nodes[nodeId]!;
        if (_states[nodeId] != KdTreeNodeState.Best) _states[nodeId] = KdTreeNodeState.Pruned;
        return 1 + MarkSubtreePruned(node.LeftNodeId) + MarkSubtreePruned(node.RightNodeId);
    }

    private async Task<double> MeasureDistanceAsync(ManualVector point, CancellationToken cancellationToken)
    {
        _vectorMath.LoadVectors(_query.CopyValues(), point.CopyValues());
        var result = await _vectorMath.ExecuteAsync(VectorOperationKind.EuclideanDistance, 1d, cancellationToken);
        return result.ScalarResult ?? double.PositiveInfinity;
    }

    private void MergeSortRange(int[] indexes, int[] scratch, int start, int end, int axis)
    {
        if (start >= end) return;
        var mid = start + ((end - start) / 2);
        MergeSortRange(indexes, scratch, start, mid, axis);
        MergeSortRange(indexes, scratch, mid + 1, end, axis);
        Merge(indexes, scratch, start, mid, end, axis);
    }

    private void Merge(int[] indexes, int[] scratch, int start, int mid, int end, int axis)
    {
        var left = start;
        var right = mid + 1;
        var write = start;
        while (left <= mid && right <= end)
        {
            if (ComparePointIndexes(indexes[left], indexes[right], axis) <= 0) scratch[write++] = indexes[left++];
            else scratch[write++] = indexes[right++];
        }
        while (left <= mid) scratch[write++] = indexes[left++];
        while (right <= end) scratch[write++] = indexes[right++];
        for (var index = start; index <= end; index++) indexes[index] = scratch[index];
    }

    private int ComparePointIndexes(int leftPointIndex, int rightPointIndex, int axis)
    {
        var left = _features[leftPointIndex][axis];
        var right = _features[rightPointIndex][axis];
        if (left < right) return -1;
        if (left > right) return 1;
        return leftPointIndex.CompareTo(rightPointIndex);
    }

    private void ResetRunState()
    {
        _nodes = new KdNode?[_features.Length];
        _states = new KdTreeNodeState[_features.Length];
        _nodeCount = 0;
        _rootNodeId = -1;
        _currentNodeId = -1;
        _bestNodeId = -1;
        _currentDistance = double.NaN;
        _bestDistance = double.PositiveInfinity;
        _splitPlaneDistance = double.NaN;
        _visitedNodes = 0;
        _prunedNodes = 0;
        _phase = KdTreePhase.Ready;
        _focusText = "Ready.";
    }

    private static void ValidateConfiguration(KdTreeConfiguration configuration)
    {
        if (configuration.Features is null || configuration.Labels is null || configuration.Query is null)
            throw new ArgumentException("Features, labels, and query are required.", nameof(configuration));
        if (configuration.Features.Length is < 3 or > 31)
            throw new ArgumentException("Use between 3 and 31 points.", nameof(configuration));
        if (configuration.Features.Length != configuration.Labels.Length)
            throw new ArgumentException("Every point needs exactly one label.", nameof(configuration));
        if (configuration.Query.Length is < 1 or > 12)
            throw new ArgumentException("Use between 1 and 12 dimensions.", nameof(configuration));

        for (var index = 0; index < configuration.Features.Length; index++)
        {
            var point = configuration.Features[index] ?? throw new ArgumentException($"Point {index} is missing.", nameof(configuration));
            if (point.Length != configuration.Query.Length)
                throw new ArgumentException("Every point must have the same dimension as the query.", nameof(configuration));
            for (var component = 0; component < point.Length; component++)
                if (!double.IsFinite(point[component])) throw new ArgumentException("Point coordinates must be finite numbers.", nameof(configuration));
            if (configuration.Labels[index] is not 0 and not 1)
                throw new ArgumentException("Teaching labels must be 0 or 1.", nameof(configuration));
        }
        for (var component = 0; component < configuration.Query.Length; component++)
            if (!double.IsFinite(configuration.Query[component])) throw new ArgumentException("Query coordinates must be finite numbers.", nameof(configuration));
    }

    private KdTreeNodeSnapshot[] CopyNodes()
    {
        var copy = new KdTreeNodeSnapshot[_nodeCount];
        for (var nodeId = 0; nodeId < _nodeCount; nodeId++)
        {
            var node = _nodes[nodeId]!;
            copy[nodeId] = new KdTreeNodeSnapshot(
                node.Id,
                node.PointIndex,
                _features[node.PointIndex].CopyValues(),
                _labels[node.PointIndex],
                node.Axis,
                node.Depth,
                node.LeftNodeId,
                node.RightNodeId,
                _states[nodeId]);
        }
        return copy;
    }

    private double[][] CopyFeatures()
    {
        var copy = new double[_features.Length][];
        for (var index = 0; index < _features.Length; index++) copy[index] = _features[index].CopyValues();
        return copy;
    }

    private static int[] Copy(IReadOnlyList<int> values)
    {
        var copy = new int[values.Count];
        for (var index = 0; index < values.Count; index++) copy[index] = values[index];
        return copy;
    }

    private int MaxDepth()
    {
        var max = 0;
        for (var nodeId = 0; nodeId < _nodeCount; nodeId++) if (_nodes[nodeId]!.Depth > max) max = _nodes[nodeId]!.Depth;
        return max;
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12d
        ? "0"
        : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class KdNode(int id, int pointIndex, int axis, int depth)
    {
        public int Id { get; } = id;
        public int PointIndex { get; } = pointIndex;
        public int Axis { get; } = axis;
        public int Depth { get; } = depth;
        public int LeftNodeId { get; set; } = -1;
        public int RightNodeId { get; set; } = -1;
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

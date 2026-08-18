using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.Knn;

/// <summary>
/// From-scratch K-Nearest Neighbors classification.
/// Each training example and the query are stored as project-owned ManualVector instances.
/// VectorSimulation performs the Euclidean/Manhattan distance primitive; KNN itself performs
/// the full scan, deterministic top-k insertion, and majority vote with explicit loops.
/// </summary>
public sealed class KnnSimulation : SimulationAlgorithmBase
{
    private readonly VectorSimulation _vectorMath;
    private ManualVector[] _features = [];
    private int[] _labels = [];
    private ManualVector _query = new(2);
    private double[] _distances = [];
    private KnnExampleState[] _states = [];
    private int[] _neighborIndices = [];
    private double[] _neighborDistances = [];
    private int[] _neighborRanks = [];
    private int _k = 3;
    private KnnDistanceMetric _distanceMetric = KnnDistanceMetric.Euclidean;
    private KnnPhase _phase = KnnPhase.Ready;
    private int _currentIndex = -1;
    private double _currentDistance = double.NaN;
    private int _voteClass0;
    private int _voteClass1;
    private int? _predictedClass;
    private string _focusText = "Ready.";

    public KnnSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        _vectorMath = new VectorSimulation(new ImmediateSimulationRuntime());
        Configure(new KnnConfiguration(
            [
                [-3d, -2d], [-2d, -3d], [-2d, -1d], [-1d, -2d],
                [1d, 2d], [2d, 1d], [2d, 3d], [3d, 2d]
            ],
            [0, 0, 0, 0, 1, 1, 1, 1],
            [1.7d, 1.6d],
            3,
            KnnDistanceMetric.Euclidean));
    }

    public void Configure(KnnConfiguration configuration)
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
        _k = configuration.K;
        _distanceMetric = configuration.DistanceMetric;
        ResetRunState();
    }

    public KnnSnapshot CreateSnapshot() => new(
        CopyFeatures(),
        Copy(_labels),
        _query.CopyValues(),
        Copy(_distances),
        Copy(_states),
        Copy(_neighborIndices),
        Copy(_neighborDistances),
        Copy(_neighborRanks),
        _k,
        _distanceMetric,
        _phase,
        _currentIndex,
        _currentDistance,
        _voteClass0,
        _voteClass1,
        _predictedClass,
        _focusText);

    public async Task<KnnRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        _focusText = $"KNN stores the examples. For this prediction it will measure {_features.Length} distances, keep the {_k} closest, then vote.";
        await NextStepAsync(_focusText, cancellationToken);

        for (var index = 0; index < _features.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _phase = KnnPhase.MeasuringDistance;
            _currentIndex = index;
            _states[index] = KnnExampleState.Comparing;
            _currentDistance = await MeasureDistanceAsync(_features[index], cancellationToken);
            _distances[index] = _currentDistance;
            _focusText = $"Example {index}: distance from query = {Format(_currentDistance)}. Its label is class {_labels[index]}.";
            await NextStepAsync(_focusText, cancellationToken);

            _phase = KnnPhase.UpdatingNeighbors;
            var inserted = InsertNeighbor(index, _currentDistance, out var displacedIndex, out var rank);
            if (displacedIndex >= 0 && displacedIndex != index)
            {
                _states[displacedIndex] = KnnExampleState.Scanned;
            }

            _states[index] = inserted ? KnnExampleState.Neighbor : KnnExampleState.Scanned;
            RefreshNeighborStates();
            _focusText = inserted
                ? $"Keep example {index} as neighbor #{rank + 1}. The top-{_k} list stays ordered from closest to farthest."
                : $"Example {index} is farther than the current top-{_k}, so it is not kept as a neighbor.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = KnnPhase.Voting;
        _currentIndex = -1;
        _currentDistance = double.NaN;
        _voteClass0 = 0;
        _voteClass1 = 0;
        for (var rank = 0; rank < _k; rank++)
        {
            var index = _neighborIndices[rank];
            var label = _labels[index];
            if (label == 0) _voteClass0++;
            else _voteClass1++;
            _states[index] = KnnExampleState.Voted;
            _focusText = $"Neighbor #{rank + 1} votes for class {label}. Vote is now class 0: {_voteClass0}, class 1: {_voteClass1}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _predictedClass = _voteClass1 > _voteClass0 ? 1 : 0;
        _phase = KnnPhase.Complete;
        _focusText = $"Prediction = class {_predictedClass}. The {_k} nearest labels voted {_voteClass0} to {_voteClass1}.";
        await NextStepAsync(_focusText, cancellationToken);

        return new KnnRunResult(
            CopyFeatures(),
            Copy(_labels),
            _query.CopyValues(),
            _k,
            _distanceMetric,
            Copy(_distances),
            Copy(_neighborIndices),
            Copy(_neighborDistances),
            _voteClass0,
            _voteClass1,
            _predictedClass.Value,
            _features.Length,
            _focusText);
    }

    private async Task<double> MeasureDistanceAsync(ManualVector point, CancellationToken cancellationToken)
    {
        _vectorMath.LoadVectors(_query.CopyValues(), point.CopyValues());
        var operation = _distanceMetric == KnnDistanceMetric.Euclidean
            ? VectorOperationKind.EuclideanDistance
            : VectorOperationKind.ManhattanDistance;
        var result = await _vectorMath.ExecuteAsync(operation, 1d, cancellationToken);
        return result.ScalarResult ?? double.PositiveInfinity;
    }

    private bool InsertNeighbor(int index, double distance, out int displacedIndex, out int rank)
    {
        displacedIndex = -1;
        rank = -1;
        var insertAt = -1;

        for (var position = 0; position < _k; position++)
        {
            var currentIndex = _neighborIndices[position];
            var currentDistance = _neighborDistances[position];
            if (currentIndex < 0 || distance < currentDistance - 1e-12d ||
                (Math.Abs(distance - currentDistance) <= 1e-12d && index < currentIndex))
            {
                insertAt = position;
                break;
            }
        }

        if (insertAt < 0) return false;

        displacedIndex = _neighborIndices[_k - 1];
        for (var position = _k - 1; position > insertAt; position--)
        {
            _neighborIndices[position] = _neighborIndices[position - 1];
            _neighborDistances[position] = _neighborDistances[position - 1];
        }

        _neighborIndices[insertAt] = index;
        _neighborDistances[insertAt] = distance;
        rank = insertAt;
        RefreshNeighborRanks();
        return true;
    }

    private void RefreshNeighborStates()
    {
        for (var index = 0; index < _features.Length; index++)
        {
            if (_states[index] == KnnExampleState.Comparing) continue;
            if (_states[index] == KnnExampleState.Neighbor) _states[index] = KnnExampleState.Scanned;
        }

        for (var rank = 0; rank < _k; rank++)
        {
            var index = _neighborIndices[rank];
            if (index >= 0) _states[index] = KnnExampleState.Neighbor;
        }
    }

    private void RefreshNeighborRanks()
    {
        for (var index = 0; index < _neighborRanks.Length; index++) _neighborRanks[index] = 0;
        for (var rank = 0; rank < _k; rank++)
        {
            var index = _neighborIndices[rank];
            if (index >= 0) _neighborRanks[index] = rank + 1;
        }
    }

    private void ResetRunState()
    {
        _distances = new double[_features.Length];
        for (var index = 0; index < _distances.Length; index++) _distances[index] = double.NaN;
        _states = new KnnExampleState[_features.Length];
        for (var index = 0; index < _states.Length; index++) _states[index] = KnnExampleState.Stored;
        _neighborIndices = new int[_k];
        _neighborDistances = new double[_k];
        for (var index = 0; index < _k; index++)
        {
            _neighborIndices[index] = -1;
            _neighborDistances[index] = double.PositiveInfinity;
        }
        _neighborRanks = new int[_features.Length];
        _phase = KnnPhase.Ready;
        _currentIndex = -1;
        _currentDistance = double.NaN;
        _voteClass0 = 0;
        _voteClass1 = 0;
        _predictedClass = null;
        _focusText = "Ready.";
    }

    private static void ValidateConfiguration(KnnConfiguration configuration)
    {
        if (configuration.Features is null || configuration.Labels is null || configuration.Query is null)
            throw new ArgumentException("Features, labels, and query are required.", nameof(configuration));
        if (configuration.Features.Length is < 2 or > 30)
            throw new ArgumentException("Use between 2 and 30 training examples.", nameof(configuration));
        if (configuration.Features.Length != configuration.Labels.Length)
            throw new ArgumentException("Every training example needs exactly one label.", nameof(configuration));
        if (configuration.Query.Length is < 1 or > 12)
            throw new ArgumentException("Use between 1 and 12 features per example.", nameof(configuration));
        if (configuration.K < 1 || configuration.K > configuration.Features.Length || configuration.K % 2 == 0)
            throw new ArgumentException("k must be an odd number between 1 and the number of training examples.", nameof(configuration));

        var sawZero = false;
        var sawOne = false;
        for (var index = 0; index < configuration.Features.Length; index++)
        {
            var point = configuration.Features[index] ?? throw new ArgumentException($"Training example {index} is missing.", nameof(configuration));
            if (point.Length != configuration.Query.Length)
                throw new ArgumentException("Every example must have the same dimension as the query.", nameof(configuration));
            for (var component = 0; component < point.Length; component++)
            {
                if (!double.IsFinite(point[component])) throw new ArgumentException("Training features must be finite numbers.", nameof(configuration));
            }
            if (configuration.Labels[index] == 0) sawZero = true;
            else if (configuration.Labels[index] == 1) sawOne = true;
            else throw new ArgumentException("KNN classification labels must be exactly 0 or 1.", nameof(configuration));
        }

        for (var component = 0; component < configuration.Query.Length; component++)
        {
            if (!double.IsFinite(configuration.Query[component])) throw new ArgumentException("Query features must be finite numbers.", nameof(configuration));
        }

        if (!sawZero || !sawOne) throw new ArgumentException("Include at least one class 0 example and one class 1 example.", nameof(configuration));
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

    private static double[] Copy(IReadOnlyList<double> values)
    {
        var copy = new double[values.Count];
        for (var index = 0; index < values.Count; index++) copy[index] = values[index];
        return copy;
    }

    private static KnnExampleState[] Copy(IReadOnlyList<KnnExampleState> values)
    {
        var copy = new KnnExampleState[values.Count];
        for (var index = 0; index < values.Count; index++) copy[index] = values[index];
        return copy;
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12d
        ? "0"
        : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

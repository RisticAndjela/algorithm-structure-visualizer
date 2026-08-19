using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Unsupervised.KMeans;

/// <summary>
/// From-scratch K-Means clustering over project-owned ManualVector values.
/// Assignment scans every centroid explicitly; centroid updates use explicit sums/counts.
/// Euclidean distance is reused from the existing Vector simulation rather than a numerical/ML library.
/// </summary>
public sealed class KMeansSimulation : SimulationAlgorithmBase
{
    private readonly VectorSimulation _vectorMath;
    private ManualVector[] _features = [];
    private ManualVector[] _centroids = [];
    private ManualVector[] _initialCentroids = [];
    private int[] _assignments = [];
    private int[] _firstRoundAssignments = [];
    private double[] _assignedDistances = [];
    private int[] _clusterCounts = [];
    private KMeansPointState[] _pointStates = [];
    private KMeansCentroidState[] _centroidStates = [];
    private int _maxIterations = 12;
    private double _tolerance = 0.001d;
    private KMeansPhase _phase = KMeansPhase.Ready;
    private int _iteration;
    private int _currentPointIndex = -1;
    private int _currentCentroidIndex = -1;
    private int _changedAssignments;
    private double _inertia;
    private double _maxCentroidMovement;
    private bool _converged;
    private string _focusText = "Ready.";

    public KMeansSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        _vectorMath = new VectorSimulation(new ImmediateSimulationRuntime());
        Configure(new KMeansConfiguration(
            [
                [-4d, -2d], [-3.4d, -1d], [-2.6d, -2.4d],
                [.3d, 3.2d], [1.1d, 2.2d], [1.8d, 3.5d],
                [3.6d, -1.7d], [4.4d, -.5d], [5d, -2.2d]
            ],
            3,
            [0, 3, 6]));
    }

    public void Configure(KMeansConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        _features = new ManualVector[configuration.Features.Length];
        for (var pointIndex = 0; pointIndex < configuration.Features.Length; pointIndex++)
        {
            var point = new ManualVector(configuration.Features[pointIndex].Length);
            point.CopyFrom(configuration.Features[pointIndex]);
            _features[pointIndex] = point;
        }

        _centroids = new ManualVector[configuration.ClusterCount];
        _initialCentroids = new ManualVector[configuration.ClusterCount];
        for (var cluster = 0; cluster < configuration.ClusterCount; cluster++)
        {
            var source = _features[configuration.InitialCentroidIndexes[cluster]];
            _centroids[cluster] = CopyVector(source);
            _initialCentroids[cluster] = CopyVector(source);
        }

        _maxIterations = configuration.MaxIterations;
        _tolerance = configuration.Tolerance;
        ResetRunState();
    }

    public KMeansSnapshot CreateSnapshot() => new(
        CopyFeatures(),
        CopyCentroids(_centroids),
        Copy(_assignments),
        Copy(_assignedDistances),
        Copy(_clusterCounts),
        Copy(_pointStates),
        Copy(_centroidStates),
        _phase,
        _iteration,
        _currentPointIndex,
        _currentCentroidIndex,
        _changedAssignments,
        _inertia,
        _maxCentroidMovement,
        _converged,
        _focusText);

    public async Task<KMeansRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState(resetCentroids: true);
        _phase = KMeansPhase.Initializing;
        _focusText = $"Start with {_centroids.Length} visible centroid seeds. K-Means will alternate assignment and centroid movement.";
        await NextStepAsync(_focusText, cancellationToken);

        for (var iteration = 1; iteration <= _maxIterations; iteration++)
        {
            _iteration = iteration;
            _changedAssignments = 0;
            _inertia = 0d;
            ClearStatesForIteration();

            _phase = KMeansPhase.Assigning;
            _focusText = $"Round {iteration}: assign every point to its nearest centroid.";
            await NextStepAsync(_focusText, cancellationToken);

            for (var pointIndex = 0; pointIndex < _features.Length; pointIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentPointIndex = pointIndex;
                _pointStates[pointIndex] = KMeansPointState.Active;

                var nearestCluster = 0;
                var nearestDistance = double.PositiveInfinity;
                for (var cluster = 0; cluster < _centroids.Length; cluster++)
                {
                    _currentCentroidIndex = cluster;
                    _centroidStates[cluster] = KMeansCentroidState.Active;
                    var distance = await EuclideanDistanceAsync(_features[pointIndex], _centroids[cluster], cancellationToken);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCluster = cluster;
                    }
                    _centroidStates[cluster] = KMeansCentroidState.Stored;
                }

                if (_assignments[pointIndex] != nearestCluster)
                {
                    _assignments[pointIndex] = nearestCluster;
                    _changedAssignments++;
                }

                _assignedDistances[pointIndex] = nearestDistance;
                _clusterCounts[nearestCluster]++;
                _inertia += nearestDistance * nearestDistance;
                _pointStates[pointIndex] = KMeansPointState.Assigned;
                _currentCentroidIndex = nearestCluster;
                _centroidStates[nearestCluster] = KMeansCentroidState.Active;
                _focusText = $"Point {pointIndex} joins cluster {nearestCluster + 1}: its nearest centroid is {Format(nearestDistance)} away.";
                await NextStepAsync(_focusText, cancellationToken);
                _centroidStates[nearestCluster] = KMeansCentroidState.Stored;
            }

            if (iteration == 1) _firstRoundAssignments = Copy(_assignments);

            _phase = KMeansPhase.Updating;
            _currentPointIndex = -1;
            _maxCentroidMovement = 0d;
            _focusText = $"Round {iteration}: move each centroid to the mean of the points currently assigned to it.";
            await NextStepAsync(_focusText, cancellationToken);

            var sums = CreateSums(_centroids.Length, _features[0].Dimension);
            var counts = new int[_centroids.Length];
            AccumulateAssignedPoints(sums, counts);

            for (var cluster = 0; cluster < _centroids.Length; cluster++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentCentroidIndex = cluster;
                _centroidStates[cluster] = KMeansCentroidState.Active;

                if (counts[cluster] == 0)
                {
                    _focusText = $"Cluster {cluster + 1} received no points, so its centroid stays where it is for this teaching run.";
                    await NextStepAsync(_focusText, cancellationToken);
                    _centroidStates[cluster] = KMeansCentroidState.Stored;
                    continue;
                }

                var next = new ManualVector(_features[0].Dimension);
                for (var dimension = 0; dimension < next.Dimension; dimension++)
                {
                    next[dimension] = sums[cluster][dimension] / counts[cluster];
                }

                var movement = await EuclideanDistanceAsync(_centroids[cluster], next, cancellationToken);
                if (movement > _maxCentroidMovement) _maxCentroidMovement = movement;
                _centroids[cluster] = next;
                _focusText = $"Centroid {cluster + 1} moves {Format(movement)} to the mean of {counts[cluster]} assigned point{(counts[cluster] == 1 ? string.Empty : "s")}.";
                await NextStepAsync(_focusText, cancellationToken);
                _centroidStates[cluster] = KMeansCentroidState.Stored;
            }

            _clusterCounts = Copy(counts);
            _converged = _changedAssignments == 0 || _maxCentroidMovement <= _tolerance;
            if (_converged)
            {
                for (var cluster = 0; cluster < _centroidStates.Length; cluster++) _centroidStates[cluster] = KMeansCentroidState.Stable;
                break;
            }
        }

        _phase = KMeansPhase.Complete;
        _currentPointIndex = -1;
        _currentCentroidIndex = -1;
        _focusText = _converged
            ? $"Clusters stabilized after {_iteration} rounds. Final inertia = {Format(_inertia)}."
            : $"Stopped after {_iteration} rounds. Final inertia = {Format(_inertia)}; more rounds may still move the centroids.";
        await NextStepAsync(_focusText, cancellationToken);

        return new KMeansRunResult(
            CopyFeatures(),
            CopyCentroids(_initialCentroids),
            CopyCentroids(_centroids),
            Copy(_assignments),
            Copy(_firstRoundAssignments),
            Copy(_clusterCounts),
            _iteration,
            _inertia,
            _maxCentroidMovement,
            _converged,
            _focusText);
    }

    private async Task<double> EuclideanDistanceAsync(ManualVector left, ManualVector right, CancellationToken cancellationToken)
    {
        _vectorMath.LoadVectors(left.CopyValues(), right.CopyValues());
        var result = await _vectorMath.ExecuteAsync(VectorOperationKind.EuclideanDistance, 1d, cancellationToken);
        if (!result.Succeeded || result.ScalarResult is not double distance)
        {
            throw new InvalidOperationException("Vector distance calculation failed.");
        }
        return distance;
    }

    private void AccumulateAssignedPoints(double[][] sums, int[] counts)
    {
        for (var pointIndex = 0; pointIndex < _features.Length; pointIndex++)
        {
            var cluster = _assignments[pointIndex];
            if (cluster < 0) continue;
            counts[cluster]++;
            for (var dimension = 0; dimension < _features[pointIndex].Dimension; dimension++)
            {
                sums[cluster][dimension] += _features[pointIndex][dimension];
            }
        }
    }

    private static double[][] CreateSums(int clusters, int dimension)
    {
        var sums = new double[clusters][];
        for (var cluster = 0; cluster < clusters; cluster++) sums[cluster] = new double[dimension];
        return sums;
    }

    private void ClearStatesForIteration()
    {
        for (var point = 0; point < _pointStates.Length; point++) _pointStates[point] = KMeansPointState.Unassigned;
        for (var cluster = 0; cluster < _clusterCounts.Length; cluster++)
        {
            _clusterCounts[cluster] = 0;
            _centroidStates[cluster] = KMeansCentroidState.Stored;
        }
    }

    private void ResetRunState(bool resetCentroids = false)
    {
        if (resetCentroids)
        {
            for (var cluster = 0; cluster < _centroids.Length; cluster++) _centroids[cluster] = CopyVector(_initialCentroids[cluster]);
        }
        _assignments = new int[_features.Length];
        _firstRoundAssignments = new int[_features.Length];
        _assignedDistances = new double[_features.Length];
        _pointStates = new KMeansPointState[_features.Length];
        _clusterCounts = new int[_centroids.Length];
        _centroidStates = new KMeansCentroidState[_centroids.Length];
        for (var point = 0; point < _assignments.Length; point++)
        {
            _assignments[point] = -1;
            _firstRoundAssignments[point] = -1;
            _assignedDistances[point] = double.NaN;
        }
        _phase = KMeansPhase.Ready;
        _iteration = 0;
        _currentPointIndex = -1;
        _currentCentroidIndex = -1;
        _changedAssignments = 0;
        _inertia = 0d;
        _maxCentroidMovement = 0d;
        _converged = false;
        _focusText = "Ready.";
    }

    private static void ValidateConfiguration(KMeansConfiguration configuration)
    {
        if (configuration.Features is null || configuration.Features.Length < 2)
            throw new ArgumentException("K-Means needs at least two data points.");
        if (configuration.ClusterCount < 2 || configuration.ClusterCount > 5)
            throw new ArgumentException("Use between 2 and 5 clusters in this learning module.");
        if (configuration.ClusterCount > configuration.Features.Length)
            throw new ArgumentException("Cluster count cannot exceed the number of data points.");
        if (configuration.InitialCentroidIndexes is null || configuration.InitialCentroidIndexes.Length != configuration.ClusterCount)
            throw new ArgumentException("Provide exactly one initial centroid index per cluster.");
        if (configuration.MaxIterations < 1 || configuration.MaxIterations > 100)
            throw new ArgumentException("Max iterations must be between 1 and 100.");
        if (!double.IsFinite(configuration.Tolerance) || configuration.Tolerance < 0d)
            throw new ArgumentException("Tolerance must be a finite non-negative value.");

        var dimension = configuration.Features[0]?.Length ?? 0;
        if (dimension < 1) throw new ArgumentException("Every point needs at least one feature.");
        for (var pointIndex = 0; pointIndex < configuration.Features.Length; pointIndex++)
        {
            var point = configuration.Features[pointIndex];
            if (point is null || point.Length != dimension)
                throw new ArgumentException("All points must have the same dimension.");
            for (var axis = 0; axis < point.Length; axis++)
            {
                if (!double.IsFinite(point[axis])) throw new ArgumentException("All point coordinates must be finite.");
            }
        }

        var used = new bool[configuration.Features.Length];
        for (var cluster = 0; cluster < configuration.InitialCentroidIndexes.Length; cluster++)
        {
            var index = configuration.InitialCentroidIndexes[cluster];
            if (index < 0 || index >= configuration.Features.Length)
                throw new ArgumentException("Initial centroid indexes must refer to existing points.");
            if (used[index]) throw new ArgumentException("Initial centroid indexes must be distinct.");
            used[index] = true;
        }
    }

    private double[][] CopyFeatures()
    {
        var copy = new double[_features.Length][];
        for (var index = 0; index < _features.Length; index++) copy[index] = _features[index].CopyValues();
        return copy;
    }

    private static double[][] CopyCentroids(ManualVector[] vectors)
    {
        var copy = new double[vectors.Length][];
        for (var index = 0; index < vectors.Length; index++) copy[index] = vectors[index].CopyValues();
        return copy;
    }

    private static ManualVector CopyVector(ManualVector source)
    {
        var copy = new ManualVector(source.Dimension);
        copy.CopyFrom(source.CopyValues());
        return copy;
    }

    private static int[] Copy(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static double[] Copy(double[] source)
    {
        var copy = new double[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static KMeansPointState[] Copy(KMeansPointState[] source)
    {
        var copy = new KMeansPointState[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static KMeansCentroidState[] Copy(KMeansCentroidState[] source)
    {
        var copy = new KMeansCentroidState[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12d ? "0" : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.MachineLearning.Unsupervised.KMeans;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.SpectralClustering;

/// <summary>
/// Two/three-way spectral clustering for small undirected teaching graphs.
/// Builds L_sym manually, eigendecomposes it with project-owned Jacobi rotations,
/// row-normalizes the first k eigenvectors, then reuses the existing manual K-Means implementation.
/// </summary>
public sealed class SpectralClusteringSimulation : SimulationAlgorithmBase
{
    private double[][] _adjacency = [];
    private int _clusterCount = 2;
    private int _maxRotations = 256;
    private double _eigenTolerance = 1e-10;
    private double[] _degrees = [];
    private ManualMatrix _laplacian = new(1, 1);
    private double[] _eigenvalues = [];
    private double[][] _embedding = [];
    private int[] _assignments = [];
    private SpectralClusteringPhase _phase = SpectralClusteringPhase.Ready;
    private int _rotations;
    private string _focusText = "Ready.";

    public SpectralClusteringSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new SpectralClusteringConfiguration(
            [
                [0d,1d,1d,0d,0d,0d],
                [1d,0d,1d,0d,0d,0d],
                [1d,1d,0d,1d,0d,0d],
                [0d,0d,1d,0d,1d,1d],
                [0d,0d,0d,1d,0d,1d],
                [0d,0d,0d,1d,1d,0d]
            ], 2));
    }

    public void Configure(SpectralClusteringConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _adjacency = Copy(configuration.Adjacency);
        _clusterCount = configuration.ClusterCount;
        _maxRotations = configuration.MaxJacobiRotations;
        _eigenTolerance = configuration.EigenTolerance;
        ResetRunState();
    }

    public SpectralClusteringSnapshot CreateSnapshot() => new(
        _adjacency.Length,
        Flatten(_adjacency),
        Copy(_degrees),
        _laplacian.CopyRawValues(),
        Copy(_eigenvalues),
        Copy(_embedding),
        Copy(_assignments),
        _phase,
        _rotations,
        _focusText);

    public async Task<SpectralClusteringRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var n = _adjacency.Length;

        _phase = SpectralClusteringPhase.Degree;
        for (var node = 0; node < n; node++)
        {
            var degree = 0d;
            for (var neighbor = 0; neighbor < n; neighbor++) degree += _adjacency[node][neighbor];
            _degrees[node] = degree;
        }
        _focusText = "Compute each node degree. Spectral clustering uses degree to scale raw adjacency before comparing graph directions.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = SpectralClusteringPhase.Laplacian;
        _laplacian = new ManualMatrix(n, n);
        for (var row = 0; row < n; row++)
        {
            for (var column = 0; column < n; column++)
            {
                if (row == column) _laplacian[row, column] = 1d;
                if (_adjacency[row][column] == 0d) continue;
                _laplacian[row, column] -= _adjacency[row][column] / Math.Sqrt(_degrees[row] * _degrees[column]);
            }
        }
        _focusText = "Build the normalized graph Laplacian L = I − D⁻¹ᐟ² A D⁻¹ᐟ². Strong within-group connections shape its smallest eigenvectors.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = SpectralClusteringPhase.EigenSolve;
        var eigen = ManualSymmetricEigenSolver.Solve(_laplacian, _maxRotations, _eigenTolerance);
        _eigenvalues = Copy(eigen.Eigenvalues);
        _rotations = eigen.Rotations;
        _focusText = $"Jacobi rotations diagonalized the symmetric Laplacian in {_rotations} rotation{(_rotations == 1 ? string.Empty : "s")}. Keep the {_clusterCount} eigenvectors with the smallest eigenvalues.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = SpectralClusteringPhase.Embedding;
        _embedding = new double[n][];
        for (var node = 0; node < n; node++)
        {
            _embedding[node] = new double[_clusterCount];
            var normSquared = 0d;
            for (var dimension = 0; dimension < _clusterCount; dimension++)
            {
                var value = eigen.Eigenvectors[dimension][node];
                _embedding[node][dimension] = value;
                normSquared += value * value;
            }
            var norm = Math.Sqrt(normSquared);
            if (norm > 1e-12)
                for (var dimension = 0; dimension < _clusterCount; dimension++) _embedding[node][dimension] /= norm;
        }
        _focusText = "Each graph node is now a row in spectral space. Nodes with similar connectivity patterns move near each other even when the original graph has no geometric coordinates.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = SpectralClusteringPhase.Clustering;
        var seeds = ChooseSeparatedSeeds(_embedding, _clusterCount);
        var kMeans = new KMeansSimulation(new ImmediateSimulationRuntime());
        kMeans.Configure(new KMeansConfiguration(Copy(_embedding), _clusterCount, seeds, MaxIterations: 20, Tolerance: 1e-6));
        var clustered = await kMeans.ExecuteAsync(cancellationToken);
        _assignments = Copy(clustered.Assignments);
        _focusText = $"Reuse the existing from-scratch K-Means on the {_clusterCount}D spectral embedding. Connectivity becomes geometry; K-Means turns that geometry into {_clusterCount} cluster labels.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = SpectralClusteringPhase.Complete;
        _focusText = $"Spectral clustering complete: normalized Laplacian → smallest eigenvectors → row-normalized embedding → {_clusterCount}-means. The cluster split follows graph connectivity rather than raw feature distance.";
        await NextStepAsync(_focusText, cancellationToken);
        return new SpectralClusteringRunResult(Copy(_degrees), Copy(_eigenvalues), Copy(_embedding), Copy(_assignments), _clusterCount, _rotations, _focusText);
    }

    private void ResetRunState()
    {
        var n = _adjacency.Length;
        _degrees = new double[n];
        _laplacian = new ManualMatrix(n, n);
        _eigenvalues = [];
        _embedding = [];
        _assignments = new int[n];
        for (var index = 0; index < n; index++) _assignments[index] = -1;
        _phase = SpectralClusteringPhase.Ready;
        _rotations = 0;
        _focusText = "Ready.";
    }

    private static int[] ChooseSeparatedSeeds(double[][] points, int count)
    {
        var seeds = new int[count];
        seeds[0] = 0;
        for (var seed = 1; seed < count; seed++)
        {
            var bestIndex = 0;
            var bestMinDistance = double.NegativeInfinity;
            for (var point = 0; point < points.Length; point++)
            {
                if (Contains(seeds, seed, point)) continue;
                var minDistance = double.PositiveInfinity;
                for (var existing = 0; existing < seed; existing++)
                {
                    var distance = DistanceSquared(points[point], points[seeds[existing]]);
                    if (distance < minDistance) minDistance = distance;
                }
                if (minDistance > bestMinDistance)
                {
                    bestMinDistance = minDistance;
                    bestIndex = point;
                }
            }
            seeds[seed] = bestIndex;
        }
        return seeds;
    }

    private static bool Contains(int[] values, int count, int target)
    {
        for (var index = 0; index < count; index++) if (values[index] == target) return true;
        return false;
    }

    private static double DistanceSquared(double[] left, double[] right)
    {
        var sum = 0d;
        for (var index = 0; index < left.Length; index++)
        {
            var delta = left[index] - right[index];
            sum += delta * delta;
        }
        return sum;
    }

    private static void Validate(SpectralClusteringConfiguration configuration)
    {
        if (configuration.Adjacency is null || configuration.Adjacency.Length < 4 || configuration.Adjacency.Length > 10) throw new ArgumentException("Use 4–10 nodes for the spectral teaching lab.", nameof(configuration));
        var n = configuration.Adjacency.Length;
        if (configuration.ClusterCount < 2 || configuration.ClusterCount > 3 || configuration.ClusterCount >= n) throw new ArgumentOutOfRangeException(nameof(configuration.ClusterCount), "Use 2 or 3 clusters and fewer clusters than nodes.");
        for (var row = 0; row < n; row++)
        {
            if (configuration.Adjacency[row] is null || configuration.Adjacency[row].Length != n) throw new ArgumentException("Adjacency must be square.", nameof(configuration));
        }
        for (var row = 0; row < n; row++)
        {
            var degree = 0d;
            for (var column = 0; column < n; column++)
            {
                var value = configuration.Adjacency[row][column];
                if (!double.IsFinite(value) || value < 0d) throw new ArgumentException("Adjacency values must be finite and non-negative.", nameof(configuration));
                if (Math.Abs(value - configuration.Adjacency[column][row]) > 1e-10) throw new ArgumentException("Spectral teaching presets require an undirected symmetric adjacency matrix.", nameof(configuration));
                if (row == column && Math.Abs(value) > 1e-12) throw new ArgumentException("Self-loops are omitted from this spectral teaching lab.", nameof(configuration));
                degree += value;
            }
            if (degree <= 0d) throw new ArgumentException("Every node needs at least one edge for normalized spectral clustering.", nameof(configuration));
        }
        if (configuration.MaxJacobiRotations < 1 || configuration.MaxJacobiRotations > 2048) throw new ArgumentOutOfRangeException(nameof(configuration.MaxJacobiRotations));
        if (!double.IsFinite(configuration.EigenTolerance) || configuration.EigenTolerance <= 0d) throw new ArgumentOutOfRangeException(nameof(configuration.EigenTolerance));
    }

    private static double[][] Copy(double[][] source) { var result = new double[source.Length][]; for (var row = 0; row < source.Length; row++) result[row] = Copy(source[row]); return result; }
    private static double[] Copy(double[] source) { var result = new double[source.Length]; for (var index = 0; index < source.Length; index++) result[index] = source[index]; return result; }
    private static int[] Copy(int[] source) { var result = new int[source.Length]; for (var index = 0; index < source.Length; index++) result[index] = source[index]; return result; }
    private static double[] Flatten(double[][] source) { var n = source.Length; var result = new double[n * n]; for (var row = 0; row < n; row++) for (var column = 0; column < n; column++) result[(row * n) + column] = source[row][column]; return result; }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

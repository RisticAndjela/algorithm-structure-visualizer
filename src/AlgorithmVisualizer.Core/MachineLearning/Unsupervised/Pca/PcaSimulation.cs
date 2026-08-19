using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Unsupervised.Pca;

/// <summary>
/// From-scratch first-component PCA. Data, mean, principal direction and projected
/// points use project-owned ManualVector storage. Centering, covariance, power
/// iteration and projection are explicit loops; no numerical/ML package is used.
/// </summary>
public sealed class PcaSimulation : SimulationAlgorithmBase
{
    private ManualVector[] _features = [];
    private ManualVector[] _centered = [];
    private ManualVector[] _projected = [];
    private ManualVector _mean = new(2);
    private ManualVector _component = new(2);
    private double[][] _covariance = [];
    private double[] _projections = [];
    private int _powerIterations = 12;
    private double _directionTolerance = 0.00001d;
    private PcaPhase _phase = PcaPhase.Ready;
    private int _currentPointIndex = -1;
    private int _directionIteration;
    private double _eigenvalue;
    private double _explainedVarianceRatio;
    private string _focusText = "Ready.";

    public PcaSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new PcaConfiguration(
        [
            [-4d, -3.2d], [-3d, -2.1d], [-2d, -1.6d], [-1d, -.4d],
            [1d, .7d], [2d, 1.8d], [3d, 2.3d], [4d, 3.4d]
        ]));
    }

    public void Configure(PcaConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);

        var dimension = configuration.Features[0].Length;
        _features = new ManualVector[configuration.Features.Length];
        _centered = new ManualVector[configuration.Features.Length];
        _projected = new ManualVector[configuration.Features.Length];
        for (var point = 0; point < configuration.Features.Length; point++)
        {
            _features[point] = CopyVector(configuration.Features[point]);
            _centered[point] = new ManualVector(dimension);
            _projected[point] = new ManualVector(dimension);
        }

        _mean = new ManualVector(dimension);
        _component = new ManualVector(dimension);
        _covariance = CreateMatrix(dimension);
        _projections = new double[configuration.Features.Length];
        _powerIterations = configuration.PowerIterations;
        _directionTolerance = configuration.DirectionTolerance;
        ResetRunState();
    }

    public PcaSnapshot CreateSnapshot() => new(
        CopyVectors(_features),
        CopyVectors(_centered),
        CopyVectors(_projected),
        _mean.CopyValues(),
        CopyMatrix(_covariance),
        _component.CopyValues(),
        Copy(_projections),
        _phase,
        _currentPointIndex,
        _directionIteration,
        _eigenvalue,
        _explainedVarianceRatio,
        _focusText);

    public async Task<PcaRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var dimension = _features[0].Dimension;

        _phase = PcaPhase.Centering;
        for (var feature = 0; feature < dimension; feature++)
        {
            var sum = 0d;
            for (var point = 0; point < _features.Length; point++) sum += _features[point][feature];
            _mean[feature] = sum / _features.Length;
        }
        _focusText = $"Compute the mean vector {FormatVector(_mean)}. PCA centers every feature before measuring variance.";
        await NextStepAsync(_focusText, cancellationToken);

        for (var point = 0; point < _features.Length; point++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentPointIndex = point;
            for (var feature = 0; feature < dimension; feature++)
            {
                _centered[point][feature] = _features[point][feature] - _mean[feature];
            }
            _focusText = $"Center point {point}: subtract the same mean vector, giving {FormatVector(_centered[point])}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = PcaPhase.Covariance;
        _currentPointIndex = -1;
        var denominator = _features.Length - 1d;
        for (var row = 0; row < dimension; row++)
        {
            for (var column = 0; column < dimension; column++)
            {
                var sum = 0d;
                for (var point = 0; point < _centered.Length; point++)
                {
                    sum += _centered[point][row] * _centered[point][column];
                }
                _covariance[row][column] = sum / denominator;
            }
        }
        _focusText = "Build covariance from centered coordinates. Diagonal cells measure feature variance; off-diagonal cells measure how features vary together.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = PcaPhase.Direction;
        InitializeDirection(_component);
        _focusText = $"Start power iteration from unit direction {FormatVector(_component)}.";
        await NextStepAsync(_focusText, cancellationToken);

        for (var iteration = 1; iteration <= _powerIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _directionIteration = iteration;
            var next = Multiply(_covariance, _component);
            var norm = Norm(next);
            if (norm <= 1e-12d)
            {
                SetAxisDirection(_component, 0);
                _focusText = "Covariance has no usable direction from this start, so the teaching implementation falls back to the first axis.";
                await NextStepAsync(_focusText, cancellationToken);
                break;
            }

            for (var feature = 0; feature < dimension; feature++) next[feature] /= norm;
            if (Dot(_component, next) < 0d)
            {
                for (var feature = 0; feature < dimension; feature++) next[feature] = -next[feature];
            }

            var change = Distance(_component, next);
            _component = next;
            _focusText = $"Direction pass {iteration}: normalize covariance × direction. Change = {Format(change)}; direction = {FormatVector(_component)}.";
            await NextStepAsync(_focusText, cancellationToken);
            if (iteration >= 2 && change <= _directionTolerance) break;
        }

        var covarianceTimesComponent = Multiply(_covariance, _component);
        _eigenvalue = Dot(_component, covarianceTimesComponent);
        var totalVariance = 0d;
        for (var feature = 0; feature < dimension; feature++) totalVariance += _covariance[feature][feature];
        _explainedVarianceRatio = totalVariance <= 1e-12d ? 0d : Math.Clamp(_eigenvalue / totalVariance, 0d, 1d);
        _focusText = $"The first principal component captures {FormatPercent(_explainedVarianceRatio)} of total variance in this dataset.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = PcaPhase.Projection;
        for (var point = 0; point < _centered.Length; point++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentPointIndex = point;
            var coordinate = Dot(_centered[point], _component);
            _projections[point] = coordinate;
            for (var feature = 0; feature < dimension; feature++)
            {
                _projected[point][feature] = _mean[feature] + coordinate * _component[feature];
            }
            _focusText = $"Project point {point}: one PCA coordinate = {Format(coordinate)}. The reconstructed 1D position lies on the principal axis.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = PcaPhase.Complete;
        _currentPointIndex = -1;
        _focusText = $"PCA complete: {_features.Length} points became one principal coordinate each, preserving {FormatPercent(_explainedVarianceRatio)} of the original variance.";
        await NextStepAsync(_focusText, cancellationToken);

        return new PcaRunResult(
            CopyVectors(_features),
            CopyVectors(_centered),
            CopyVectors(_projected),
            _mean.CopyValues(),
            CopyMatrix(_covariance),
            _component.CopyValues(),
            Copy(_projections),
            _directionIteration,
            _eigenvalue,
            _explainedVarianceRatio,
            _focusText);
    }

    private void ResetRunState()
    {
        var dimension = _features.Length == 0 ? 2 : _features[0].Dimension;
        _mean = new ManualVector(dimension);
        _component = new ManualVector(dimension);
        _covariance = CreateMatrix(dimension);
        _projections = new double[_features.Length];
        _centered = new ManualVector[_features.Length];
        _projected = new ManualVector[_features.Length];
        for (var point = 0; point < _features.Length; point++)
        {
            _centered[point] = new ManualVector(dimension);
            _projected[point] = new ManualVector(dimension);
        }
        _phase = PcaPhase.Ready;
        _currentPointIndex = -1;
        _directionIteration = 0;
        _eigenvalue = 0d;
        _explainedVarianceRatio = 0d;
        _focusText = "Ready.";
    }

    private static void Validate(PcaConfiguration configuration)
    {
        if (configuration.Features is null || configuration.Features.Length < 2)
            throw new ArgumentException("PCA needs at least two points.", nameof(configuration));
        if (configuration.Features[0] is null || configuration.Features[0].Length < 2)
            throw new ArgumentException("PCA needs at least two numeric features.", nameof(configuration));

        var dimension = configuration.Features[0].Length;
        for (var point = 0; point < configuration.Features.Length; point++)
        {
            var values = configuration.Features[point];
            if (values is null || values.Length != dimension)
                throw new ArgumentException("Every PCA point must have the same dimension.", nameof(configuration));
            for (var feature = 0; feature < dimension; feature++)
            {
                if (!double.IsFinite(values[feature]))
                    throw new ArgumentException("PCA features must be finite numbers.", nameof(configuration));
            }
        }

        if (configuration.PowerIterations < 1 || configuration.PowerIterations > 50)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Use between 1 and 50 direction iterations.");
        if (!double.IsFinite(configuration.DirectionTolerance) || configuration.DirectionTolerance <= 0d)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Direction tolerance must be positive and finite.");
    }

    private static ManualVector CopyVector(IReadOnlyList<double> values)
    {
        var vector = new ManualVector(values.Count);
        vector.CopyFrom(values);
        return vector;
    }

    private static double[][] CreateMatrix(int dimension)
    {
        var matrix = new double[dimension][];
        for (var row = 0; row < dimension; row++) matrix[row] = new double[dimension];
        return matrix;
    }

    private static double[][] CopyMatrix(double[][] source)
    {
        var copy = new double[source.Length][];
        for (var row = 0; row < source.Length; row++)
        {
            copy[row] = new double[source[row].Length];
            for (var column = 0; column < source[row].Length; column++) copy[row][column] = source[row][column];
        }
        return copy;
    }

    private static double[][] CopyVectors(ManualVector[] source)
    {
        var copy = new double[source.Length][];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index].CopyValues();
        return copy;
    }

    private static double[] Copy(double[] source)
    {
        var copy = new double[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static ManualVector Multiply(double[][] matrix, ManualVector vector)
    {
        var result = new ManualVector(vector.Dimension);
        for (var row = 0; row < matrix.Length; row++)
        {
            var sum = 0d;
            for (var column = 0; column < vector.Dimension; column++) sum += matrix[row][column] * vector[column];
            result[row] = sum;
        }
        return result;
    }

    private static double Dot(ManualVector left, ManualVector right)
    {
        var sum = 0d;
        for (var index = 0; index < left.Dimension; index++) sum += left[index] * right[index];
        return sum;
    }

    private static double Norm(ManualVector vector)
    {
        var sum = 0d;
        for (var index = 0; index < vector.Dimension; index++) sum += vector[index] * vector[index];
        return Math.Sqrt(sum);
    }

    private static double Distance(ManualVector left, ManualVector right)
    {
        var sum = 0d;
        for (var index = 0; index < left.Dimension; index++)
        {
            var difference = left[index] - right[index];
            sum += difference * difference;
        }
        return Math.Sqrt(sum);
    }

    private static void InitializeDirection(ManualVector vector)
    {
        var normSquared = 0d;
        for (var index = 0; index < vector.Dimension; index++)
        {
            var value = 1d / (index + 1d);
            vector[index] = value;
            normSquared += value * value;
        }
        var norm = Math.Sqrt(normSquared);
        for (var index = 0; index < vector.Dimension; index++) vector[index] /= norm;
    }

    private static void SetAxisDirection(ManualVector vector, int axis)
    {
        for (var index = 0; index < vector.Dimension; index++) vector[index] = index == axis ? 1d : 0d;
    }

    private static string FormatVector(ManualVector vector) => FormatVector(vector.CopyValues());
    private static string FormatVector(IReadOnlyList<double> values)
    {
        var parts = new string[values.Count];
        for (var index = 0; index < values.Count; index++) parts[index] = Format(values[index]);
        return $"[{string.Join(", ", parts)}]";
    }
    private static string Format(double value) => Math.Abs(value) < 1e-12d ? "0" : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    private static string FormatPercent(double value) => (value * 100d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";
}

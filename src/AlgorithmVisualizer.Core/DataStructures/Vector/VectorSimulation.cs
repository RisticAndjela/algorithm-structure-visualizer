using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Vector;

/// <summary>
/// Step-by-step mathematical vector operations over project-owned raw-array storage.
/// All reductions and component-wise operations use explicit loops so the learning
/// UI can expose each read, contribution, write, and running accumulator value.
/// </summary>
public sealed class VectorSimulation : SimulationAlgorithmBase
{
    private ManualVector _a = new(3);
    private ManualVector _b = new(3);
    private ManualVector _result = new(3);
    private VectorComponentVisualState[] _aStates = new VectorComponentVisualState[3];
    private VectorComponentVisualState[] _bStates = new VectorComponentVisualState[3];
    private VectorComponentVisualState[] _resultStates = new VectorComponentVisualState[3];
    private VectorOperationKind _operation = VectorOperationKind.Add;
    private VectorPhase _phase = VectorPhase.Ready;
    private int _currentIndex = -1;
    private double _runningValue;
    private double? _scalarResult;
    private double? _normA;
    private double? _normB;
    private string _focusText = "Ready.";

    public VectorSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        LoadVectors([2d, -1d, 3d], [4d, 5d, -2d]);
    }

    public void LoadVectors(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Count < 1 || b.Count < 1)
        {
            throw new ArgumentException("Both vectors need at least one component.");
        }

        _a.CopyFrom(a);
        _b.CopyFrom(b);
        _result = new ManualVector(Math.Max(1, a.Count));
        _aStates = new VectorComponentVisualState[a.Count];
        _bStates = new VectorComponentVisualState[b.Count];
        _resultStates = new VectorComponentVisualState[Math.Max(1, a.Count)];
        ResetVisualState();
    }

    public VectorSnapshot CreateSnapshot() => new(
        _a.CopyValues(),
        _b.CopyValues(),
        SnapshotResultValues(),
        CloneStates(_aStates),
        CloneStates(_bStates),
        SnapshotResultStates(),
        _operation,
        _phase,
        _currentIndex,
        _runningValue,
        _scalarResult,
        _normA,
        _normB,
        _focusText);

    public async Task<VectorOperationResult> ExecuteAsync(
        VectorOperationKind operation,
        double scalar = 1d,
        CancellationToken cancellationToken = default)
    {
        _operation = operation;
        ResetVisualState(keepOperation: true);

        return operation switch
        {
            VectorOperationKind.Add => await ComponentWiseBinaryAsync(operation, (left, right) => left + right, "+", scalar, cancellationToken),
            VectorOperationKind.Subtract => await ComponentWiseBinaryAsync(operation, (left, right) => left - right, "−", scalar, cancellationToken),
            VectorOperationKind.Hadamard => await ComponentWiseBinaryAsync(operation, (left, right) => left * right, "×", scalar, cancellationToken),
            VectorOperationKind.ScalarMultiply => await ScalarMultiplyAsync(scalar, cancellationToken),
            VectorOperationKind.DotProduct => await DotProductAsync(scalar, cancellationToken),
            VectorOperationKind.L1Norm => await NormAsync(operation, scalar, cancellationToken),
            VectorOperationKind.L2Norm => await NormAsync(operation, scalar, cancellationToken),
            VectorOperationKind.NormalizeL2 => await NormalizeAsync(scalar, cancellationToken),
            VectorOperationKind.EuclideanDistance => await DistanceAsync(operation, scalar, cancellationToken),
            VectorOperationKind.ManhattanDistance => await DistanceAsync(operation, scalar, cancellationToken),
            VectorOperationKind.CosineSimilarity => await CosineSimilarityAsync(scalar, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    private async Task<VectorOperationResult> ComponentWiseBinaryAsync(
        VectorOperationKind operation,
        Func<double, double, double> calculate,
        string symbol,
        double scalar,
        CancellationToken cancellationToken)
    {
        if (!TryRequireSameDimension(out var failure))
        {
            return await InvalidAsync(operation, scalar, failure, cancellationToken);
        }

        PrepareResult(_a.Dimension);
        var arithmetic = 0;
        await PublishAsync($"{OperationName(operation)} works component by component. Match index 0 with index 0, then move right.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: true, includeResult: true, VectorPhase.Reading);
            var value = calculate(_a[index], _b[index]);
            arithmetic++;
            _focusText = $"index {index}: {Format(_a[index])} {symbol} {Format(_b[index])} = {Format(value)}";
            await PublishAsync(_focusText, cancellationToken);

            _result[index] = value;
            _resultStates[index] = VectorComponentVisualState.Written;
            _aStates[index] = VectorComponentVisualState.Read;
            _bStates[index] = VectorComponentVisualState.Read;
            _phase = VectorPhase.Writing;
            await PublishAsync($"Write {Format(value)} into result[{index}]. The indexes stay aligned.", cancellationToken);
        }

        return Complete(operation, scalar, _result.CopyValues(), null, _a.Dimension, arithmetic,
            $"{OperationName(operation)} produced a {_a.Dimension}-component result.");
    }

    private async Task<VectorOperationResult> ScalarMultiplyAsync(double scalar, CancellationToken cancellationToken)
    {
        PrepareResult(_a.Dimension);
        var arithmetic = 0;
        await PublishAsync($"Scale vector A by {Format(scalar)}. Every component is multiplied independently by the same scalar.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: false, includeResult: true, VectorPhase.Reading);
            var value = _a[index] * scalar;
            arithmetic++;
            _focusText = $"index {index}: {Format(_a[index])} × {Format(scalar)} = {Format(value)}";
            await PublishAsync(_focusText, cancellationToken);
            _result[index] = value;
            _resultStates[index] = VectorComponentVisualState.Written;
            _aStates[index] = VectorComponentVisualState.Read;
        }

        return Complete(VectorOperationKind.ScalarMultiply, scalar, _result.CopyValues(), null, _a.Dimension, arithmetic,
            $"Scalar multiplication produced {FormatVector(_result.CopyValues())}.");
    }

    private async Task<VectorOperationResult> DotProductAsync(double scalar, CancellationToken cancellationToken)
    {
        if (!TryRequireSameDimension(out var failure))
        {
            return await InvalidAsync(VectorOperationKind.DotProduct, scalar, failure, cancellationToken);
        }

        PrepareScalarReduction();
        var arithmetic = 0;
        await PublishAsync("Dot product multiplies matching components, then adds every product into one scalar accumulator.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: true, includeResult: false, VectorPhase.Reducing);
            var product = _a[index] * _b[index];
            _runningValue += product;
            arithmetic += 2;
            _aStates[index] = VectorComponentVisualState.Contributing;
            _bStates[index] = VectorComponentVisualState.Contributing;
            _focusText = $"index {index}: {Format(_a[index])} × {Format(_b[index])} = {Format(product)}; running sum = {Format(_runningValue)}";
            await PublishAsync(_focusText, cancellationToken);
        }

        _scalarResult = _runningValue;
        return Complete(VectorOperationKind.DotProduct, scalar, [], _scalarResult, _a.Dimension, arithmetic,
            $"Dot product = {Format(_runningValue)}.");
    }

    private async Task<VectorOperationResult> NormAsync(VectorOperationKind operation, double scalar, CancellationToken cancellationToken)
    {
        PrepareScalarReduction();
        var arithmetic = 0;
        var l1 = operation == VectorOperationKind.L1Norm;
        await PublishAsync(l1
            ? "L1 norm adds the absolute value of every component."
            : "L2 norm adds squared components, then takes the square root of the final sum.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: false, includeResult: false, VectorPhase.Reducing);
            var contribution = l1 ? Math.Abs(_a[index]) : _a[index] * _a[index];
            _runningValue += contribution;
            arithmetic += l1 ? 1 : 2;
            _aStates[index] = VectorComponentVisualState.Contributing;
            _focusText = l1
                ? $"index {index}: |{Format(_a[index])}| = {Format(contribution)}; running sum = {Format(_runningValue)}"
                : $"index {index}: {Format(_a[index])}² = {Format(contribution)}; running sum = {Format(_runningValue)}";
            await PublishAsync(_focusText, cancellationToken);
        }

        if (!l1)
        {
            _runningValue = Math.Sqrt(_runningValue);
            arithmetic++;
            await PublishAsync($"Take √(sum of squares). L2 norm = {Format(_runningValue)}.", cancellationToken);
        }

        _scalarResult = _runningValue;
        _normA = operation == VectorOperationKind.L2Norm ? _runningValue : null;
        return Complete(operation, scalar, [], _scalarResult, _a.Dimension, arithmetic,
            $"{OperationName(operation)} = {Format(_runningValue)}.");
    }

    private async Task<VectorOperationResult> NormalizeAsync(double scalar, CancellationToken cancellationToken)
    {
        PrepareResult(_a.Dimension);
        var arithmetic = 0;
        var sumSquares = 0d;
        await PublishAsync("Normalization first measures A with the L2 norm. Only then can each component be divided by that length.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: false, includeResult: false, VectorPhase.Reducing);
            var contribution = _a[index] * _a[index];
            sumSquares += contribution;
            arithmetic += 2;
            _aStates[index] = VectorComponentVisualState.Contributing;
            _runningValue = sumSquares;
            await PublishAsync($"Norm pass index {index}: {Format(_a[index])}² = {Format(contribution)}; sum = {Format(sumSquares)}.", cancellationToken);
        }

        var norm = Math.Sqrt(sumSquares);
        arithmetic++;
        _normA = norm;
        _runningValue = norm;
        await PublishAsync($"L2 norm of A = {Format(norm)}.", cancellationToken);

        if (Math.Abs(norm) <= 1e-12)
        {
            return await InvalidAsync(VectorOperationKind.NormalizeL2, scalar,
                "The zero vector cannot be normalized because dividing by its L2 norm would divide by zero.", cancellationToken,
                preserveVisited: true);
        }

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: false, includeResult: true, VectorPhase.Writing);
            var normalized = _a[index] / norm;
            arithmetic++;
            _result[index] = normalized;
            _resultStates[index] = VectorComponentVisualState.Written;
            await PublishAsync($"Normalize index {index}: {Format(_a[index])} ÷ {Format(norm)} = {Format(normalized)}.", cancellationToken);
        }

        return Complete(VectorOperationKind.NormalizeL2, scalar, _result.CopyValues(), null, _a.Dimension * 2, arithmetic,
            "A was normalized to unit L2 length.");
    }

    private async Task<VectorOperationResult> DistanceAsync(VectorOperationKind operation, double scalar, CancellationToken cancellationToken)
    {
        if (!TryRequireSameDimension(out var failure))
        {
            return await InvalidAsync(operation, scalar, failure, cancellationToken);
        }

        PrepareScalarReduction();
        var euclidean = operation == VectorOperationKind.EuclideanDistance;
        var arithmetic = 0;
        await PublishAsync(euclidean
            ? "Euclidean distance accumulates squared component differences, then takes the square root."
            : "Manhattan distance accumulates the absolute component differences.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: true, includeResult: false, VectorPhase.Reducing);
            var difference = _a[index] - _b[index];
            var contribution = euclidean ? difference * difference : Math.Abs(difference);
            _runningValue += contribution;
            arithmetic += euclidean ? 3 : 2;
            _aStates[index] = VectorComponentVisualState.Contributing;
            _bStates[index] = VectorComponentVisualState.Contributing;
            await PublishAsync($"index {index}: difference = {Format(difference)}, contribution = {Format(contribution)}, running total = {Format(_runningValue)}.", cancellationToken);
        }

        if (euclidean)
        {
            _runningValue = Math.Sqrt(_runningValue);
            arithmetic++;
            await PublishAsync($"Take the square root. Euclidean distance = {Format(_runningValue)}.", cancellationToken);
        }

        _scalarResult = _runningValue;
        return Complete(operation, scalar, [], _scalarResult, _a.Dimension, arithmetic,
            $"{OperationName(operation)} = {Format(_runningValue)}.");
    }

    private async Task<VectorOperationResult> CosineSimilarityAsync(double scalar, CancellationToken cancellationToken)
    {
        if (!TryRequireSameDimension(out var failure))
        {
            return await InvalidAsync(VectorOperationKind.CosineSimilarity, scalar, failure, cancellationToken);
        }

        PrepareScalarReduction();
        var dot = 0d;
        var sumA = 0d;
        var sumB = 0d;
        var arithmetic = 0;
        await PublishAsync("Cosine similarity compares direction: accumulate A·B, ||A||² and ||B||² over the same component pairs.", cancellationToken);

        for (var index = 0; index < _a.Dimension; index++)
        {
            SetCurrent(index, includeB: true, includeResult: false, VectorPhase.Reducing);
            dot += _a[index] * _b[index];
            sumA += _a[index] * _a[index];
            sumB += _b[index] * _b[index];
            arithmetic += 6;
            _runningValue = dot;
            _aStates[index] = VectorComponentVisualState.Contributing;
            _bStates[index] = VectorComponentVisualState.Contributing;
            await PublishAsync($"index {index}: dot = {Format(dot)}, A squares = {Format(sumA)}, B squares = {Format(sumB)}.", cancellationToken);
        }

        _normA = Math.Sqrt(sumA);
        _normB = Math.Sqrt(sumB);
        arithmetic += 2;
        await PublishAsync($"Lengths: ||A||₂ = {Format(_normA.Value)}, ||B||₂ = {Format(_normB.Value)}.", cancellationToken);

        if (_normA.Value <= 1e-12 || _normB.Value <= 1e-12)
        {
            return await InvalidAsync(VectorOperationKind.CosineSimilarity, scalar,
                "Cosine similarity is undefined when either vector has zero L2 length.", cancellationToken,
                preserveVisited: true);
        }

        _scalarResult = dot / (_normA.Value * _normB.Value);
        arithmetic += 2;
        _runningValue = _scalarResult.Value;
        await PublishAsync($"cos(θ) = (A·B) / (||A||₂ ||B||₂) = {Format(_scalarResult.Value)}.", cancellationToken);

        return Complete(VectorOperationKind.CosineSimilarity, scalar, [], _scalarResult, _a.Dimension, arithmetic,
            $"Cosine similarity = {Format(_scalarResult.Value)}.");
    }

    private bool TryRequireSameDimension(out string failure)
    {
        if (_a.Dimension != _b.Dimension)
        {
            failure = $"This operation pairs components by index, so dimensions must match. A has {_a.Dimension} component(s) and B has {_b.Dimension}.";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    private async Task<VectorOperationResult> InvalidAsync(
        VectorOperationKind operation,
        double scalar,
        string reason,
        CancellationToken cancellationToken,
        bool preserveVisited = false)
    {
        if (!preserveVisited)
        {
            ClearStates();
        }

        _phase = VectorPhase.Invalid;
        _currentIndex = -1;
        _focusText = reason;
        await PublishAsync(reason, cancellationToken);
        return new VectorOperationResult(operation, false, _a.CopyValues(), _b.CopyValues(), [], null, scalar, 0, 0, "Operation rejected.", reason);
    }

    private VectorOperationResult Complete(
        VectorOperationKind operation,
        double scalar,
        double[] resultVector,
        double? scalarResult,
        int componentsVisited,
        int arithmeticOperations,
        string summary)
    {
        _phase = VectorPhase.Complete;
        _currentIndex = -1;
        _focusText = summary;
        return new VectorOperationResult(operation, true, _a.CopyValues(), _b.CopyValues(), resultVector, scalarResult, scalar, componentsVisited, arithmeticOperations, summary, string.Empty);
    }

    private void PrepareResult(int dimension)
    {
        _result = new ManualVector(dimension);
        _resultStates = new VectorComponentVisualState[dimension];
        _scalarResult = null;
        _runningValue = 0d;
        _normA = null;
        _normB = null;
        ClearStates();
    }

    private void PrepareScalarReduction()
    {
        _result = new ManualVector(Math.Max(1, _a.Dimension));
        _resultStates = new VectorComponentVisualState[Math.Max(1, _a.Dimension)];
        _scalarResult = null;
        _runningValue = 0d;
        _normA = null;
        _normB = null;
        ClearStates();
    }

    private void SetCurrent(int index, bool includeB, bool includeResult, VectorPhase phase)
    {
        _currentIndex = index;
        _phase = phase;
        if (index >= 0 && index < _aStates.Length) _aStates[index] = VectorComponentVisualState.Current;
        if (includeB && index >= 0 && index < _bStates.Length) _bStates[index] = VectorComponentVisualState.Current;
        if (includeResult && index >= 0 && index < _resultStates.Length) _resultStates[index] = VectorComponentVisualState.Current;
    }

    private async Task PublishAsync(string text, CancellationToken cancellationToken)
    {
        _focusText = text;
        await NextStepAsync(text, cancellationToken);
    }

    private void ResetVisualState(bool keepOperation = false)
    {
        if (!keepOperation) _operation = VectorOperationKind.Add;
        _phase = VectorPhase.Ready;
        _currentIndex = -1;
        _runningValue = 0d;
        _scalarResult = null;
        _normA = null;
        _normB = null;
        _focusText = "Ready.";
        _result = new ManualVector(Math.Max(1, _a.Dimension));
        _resultStates = new VectorComponentVisualState[Math.Max(1, _a.Dimension)];
        ClearStates();
    }

    private void ClearStates()
    {
        for (var index = 0; index < _aStates.Length; index++) _aStates[index] = VectorComponentVisualState.Normal;
        for (var index = 0; index < _bStates.Length; index++) _bStates[index] = VectorComponentVisualState.Normal;
        for (var index = 0; index < _resultStates.Length; index++) _resultStates[index] = VectorComponentVisualState.Normal;
    }

    private static VectorComponentVisualState[] CloneStates(VectorComponentVisualState[] source)
    {
        var copy = new VectorComponentVisualState[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private double[] SnapshotResultValues() => ProducesVector(_operation) ? _result.CopyValues() : [];

    private VectorComponentVisualState[] SnapshotResultStates() => ProducesVector(_operation) ? CloneStates(_resultStates) : [];

    private static bool ProducesVector(VectorOperationKind operation) => operation is
        VectorOperationKind.Add or
        VectorOperationKind.Subtract or
        VectorOperationKind.ScalarMultiply or
        VectorOperationKind.Hadamard or
        VectorOperationKind.NormalizeL2;

    private static string OperationName(VectorOperationKind operation) => operation switch
    {
        VectorOperationKind.Add => "Vector addition",
        VectorOperationKind.Subtract => "Vector subtraction",
        VectorOperationKind.ScalarMultiply => "Scalar multiplication",
        VectorOperationKind.Hadamard => "Hadamard product",
        VectorOperationKind.DotProduct => "Dot product",
        VectorOperationKind.L1Norm => "L1 norm",
        VectorOperationKind.L2Norm => "L2 norm",
        VectorOperationKind.NormalizeL2 => "L2 normalization",
        VectorOperationKind.EuclideanDistance => "Euclidean distance",
        VectorOperationKind.ManhattanDistance => "Manhattan distance",
        VectorOperationKind.CosineSimilarity => "Cosine similarity",
        _ => operation.ToString()
    };

    private static string Format(double value) => Math.Abs(value) < 1e-12 ? "0" : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    private static string FormatVector(IReadOnlyList<double> values)
    {
        var parts = new string[values.Count];
        for (var index = 0; index < values.Count; index++) parts[index] = Format(values[index]);
        return $"[{string.Join(", ", parts)}]";
    }
}

using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Optimization.GradientDescent;

/// <summary>
/// Renderer-neutral Gradient Descent simulation over the convex teaching objective
/// J(theta) = 1/2 * sum(curvature[i] * (theta[i] - target[i])^2).
///
/// The objective-specific loss and analytical gradient are intentionally explicit here.
/// Vector magnitude, scalar multiplication and subtraction are delegated to the existing
/// VectorSimulation so later ML lessons reuse earlier project-owned numerical primitives.
/// </summary>
public sealed class GradientDescentSimulation : SimulationAlgorithmBase
{
    private readonly VectorSimulation _vectorMath;

    private ManualVector _parameters = new(2);
    private ManualVector _initialParameters = new(2);
    private ManualVector _target = new(2);
    private ManualVector _curvature = new(2);
    private ManualVector _gradient = new(2);
    private ManualVector _scaledGradient = new(2);
    private ManualVector _nextParameters = new(2);

    private GradientDescentVariant _variant = GradientDescentVariant.FixedLearningRate;
    private GradientDescentPhase _phase = GradientDescentPhase.Ready;
    private GradientDescentHistoryPoint[] _history = new GradientDescentHistoryPoint[1];
    private int _historyCount;
    private int _iteration;
    private int _currentIndex = -1;
    private int _maximumIterations = 30;
    private double _learningRate = 0.2d;
    private double _gradientTolerance = 0.01d;
    private double _decayRate = 0.15d;
    private double _loss;
    private double _previousLoss;
    private double _effectiveLearningRate = 0.2d;
    private double _gradientNorm;
    private string _focusText = "Ready.";

    public GradientDescentSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        _vectorMath = new VectorSimulation(new ImmediateSimulationRuntime());
        Configure(new GradientDescentConfiguration(
            [5d, -3d],
            [0d, 0d],
            [1d, 2d],
            0.2d,
            30,
            0.01d,
            0.15d));
    }

    public void Configure(GradientDescentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        _parameters.CopyFrom(configuration.InitialParameters);
        _initialParameters.CopyFrom(configuration.InitialParameters);
        _target.CopyFrom(configuration.Target);
        _curvature.CopyFrom(configuration.Curvature);
        _gradient = new ManualVector(configuration.InitialParameters.Length);
        _scaledGradient = new ManualVector(configuration.InitialParameters.Length);
        _nextParameters = new ManualVector(configuration.InitialParameters.Length);
        _learningRate = configuration.LearningRate;
        _maximumIterations = configuration.MaximumIterations;
        _gradientTolerance = configuration.GradientTolerance;
        _decayRate = configuration.DecayRate;
        _history = new GradientDescentHistoryPoint[_maximumIterations + 1];
        ResetRunState();
    }

    public GradientDescentSnapshot CreateSnapshot() => new(
        _parameters.CopyValues(),
        _target.CopyValues(),
        _curvature.CopyValues(),
        _gradient.CopyValues(),
        _scaledGradient.CopyValues(),
        _nextParameters.CopyValues(),
        _variant,
        _phase,
        _iteration,
        _currentIndex,
        _loss,
        _previousLoss,
        _effectiveLearningRate,
        _gradientNorm,
        _focusText,
        CopyHistory());

    public async Task<GradientDescentRunResult> ExecuteAsync(
        GradientDescentVariant variant,
        CancellationToken cancellationToken = default)
    {
        _variant = variant;
        ResetRunState(keepVariant: true);

        _phase = GradientDescentPhase.EvaluatingLoss;
        _loss = EvaluateLoss(_parameters);
        _previousLoss = _loss;
        _effectiveLearningRate = EffectiveLearningRate(0);
        _gradientNorm = await ComputeGradientAndNormAsync(cancellationToken, publishComponents: false);
        AddHistoryPoint(0);

        await PublishAsync(
            $"Start at J(θ) = {Format(_loss)}. ||∇J||₂ = {Format(_gradientNorm)} and η = {Format(_learningRate)}.",
            cancellationToken);

        if (_gradientNorm <= _gradientTolerance)
        {
            return await FinishConvergedAsync(0, cancellationToken);
        }

        var lossGrowthStreak = 0;

        for (var update = 0; update < _maximumIterations; update++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _iteration = update + 1;
            _effectiveLearningRate = EffectiveLearningRate(update);
            _previousLoss = _loss;

            _phase = GradientDescentPhase.ComputingGradient;
            _gradientNorm = await ComputeGradientAndNormAsync(cancellationToken, publishComponents: true);

            _phase = GradientDescentPhase.ScalingGradient;
            await ScaleGradientWithVectorCoreAsync(cancellationToken);
            await PublishAsync(
                $"Vector Core scales ∇J by η={Format(_effectiveLearningRate)}: η∇J = {FormatVector(_scaledGradient)}.",
                cancellationToken);

            _phase = GradientDescentPhase.UpdatingParameters;
            await SubtractWithVectorCoreAsync(cancellationToken);

            for (var index = 0; index < _parameters.Dimension; index++)
            {
                _currentIndex = index;
                await PublishAsync(
                    $"Update θ[{index}]: {Format(_parameters[index])} − {Format(_scaledGradient[index])} = {Format(_nextParameters[index])}.",
                    cancellationToken);
            }

            _parameters.CopyFrom(_nextParameters.CopyValues());
            _currentIndex = -1;
            _phase = GradientDescentPhase.EvaluatingLoss;
            _loss = EvaluateLoss(_parameters);
            _gradientNorm = await ComputeGradientAndNormAsync(cancellationToken, publishComponents: false);
            AddHistoryPoint(_iteration);

            await PublishAsync(
                $"Iteration {_iteration}: J(θ) {Format(_previousLoss)} → {Format(_loss)}; ||∇J||₂ = {Format(_gradientNorm)}.",
                cancellationToken);

            if (!double.IsFinite(_loss) || _loss > 1e12d)
            {
                return await FinishDivergedAsync(
                    "Loss became non-finite or exceeded the safe teaching range. The learning rate is unstable for this objective.",
                    cancellationToken);
            }

            if (_loss > _previousLoss * (1d + 1e-10d))
            {
                lossGrowthStreak++;
            }
            else
            {
                lossGrowthStreak = 0;
            }

            if (lossGrowthStreak >= 3)
            {
                return await FinishDivergedAsync(
                    "Loss increased for three consecutive updates. The learning rate is too aggressive for the current curvature.",
                    cancellationToken);
            }

            if (_gradientNorm <= _gradientTolerance)
            {
                return await FinishConvergedAsync(_iteration, cancellationToken);
            }
        }

        _phase = GradientDescentPhase.Complete;
        _focusText = $"Stopped after {_maximumIterations} updates. Loss is {Format(_loss)} and ||∇J||₂ is {Format(_gradientNorm)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(false, false, GradientDescentStopReason.MaximumIterations, _focusText);
    }

    private async Task<double> ComputeGradientAndNormAsync(
        CancellationToken cancellationToken,
        bool publishComponents)
    {
        for (var index = 0; index < _parameters.Dimension; index++)
        {
            _currentIndex = index;
            var difference = _parameters[index] - _target[index];
            _gradient[index] = _curvature[index] * difference;

            if (publishComponents)
            {
                await PublishAsync(
                    $"Gradient[{index}] = curvature[{index}] × (θ[{index}] − target[{index}]) = {Format(_curvature[index])} × {Format(difference)} = {Format(_gradient[index])}.",
                    cancellationToken);
            }
        }

        _currentIndex = -1;
        _vectorMath.LoadVectors(_gradient.CopyValues(), _gradient.CopyValues());
        var normResult = await _vectorMath.ExecuteAsync(VectorOperationKind.L2Norm, 1d, cancellationToken);
        return normResult.ScalarResult ?? 0d;
    }

    private async Task ScaleGradientWithVectorCoreAsync(CancellationToken cancellationToken)
    {
        _vectorMath.LoadVectors(_gradient.CopyValues(), _gradient.CopyValues());
        var result = await _vectorMath.ExecuteAsync(
            VectorOperationKind.ScalarMultiply,
            _effectiveLearningRate,
            cancellationToken);
        _scaledGradient.CopyFrom(result.ResultVector);
    }

    private async Task SubtractWithVectorCoreAsync(CancellationToken cancellationToken)
    {
        _vectorMath.LoadVectors(_parameters.CopyValues(), _scaledGradient.CopyValues());
        var result = await _vectorMath.ExecuteAsync(VectorOperationKind.Subtract, 1d, cancellationToken);
        _nextParameters.CopyFrom(result.ResultVector);
    }

    private double EvaluateLoss(ManualVector parameters)
    {
        var sum = 0d;
        for (var index = 0; index < parameters.Dimension; index++)
        {
            var difference = parameters[index] - _target[index];
            sum += 0.5d * _curvature[index] * difference * difference;
        }

        return sum;
    }

    private double EffectiveLearningRate(int zeroBasedUpdate)
    {
        if (_variant == GradientDescentVariant.FixedLearningRate)
        {
            return _learningRate;
        }

        return _learningRate / (1d + (_decayRate * zeroBasedUpdate));
    }

    private async Task<GradientDescentRunResult> FinishConvergedAsync(
        int iterationsCompleted,
        CancellationToken cancellationToken)
    {
        _phase = GradientDescentPhase.Complete;
        _iteration = iterationsCompleted;
        _focusText = $"Converged: ||∇J||₂ = {Format(_gradientNorm)} is within tolerance {Format(_gradientTolerance)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(true, false, GradientDescentStopReason.GradientTolerance, _focusText);
    }

    private async Task<GradientDescentRunResult> FinishDivergedAsync(
        string reason,
        CancellationToken cancellationToken)
    {
        _phase = GradientDescentPhase.Diverged;
        _focusText = reason;
        await NextStepAsync(reason, cancellationToken);
        return BuildResult(false, true, GradientDescentStopReason.Diverged, reason);
    }

    private GradientDescentRunResult BuildResult(
        bool converged,
        bool diverged,
        GradientDescentStopReason stopReason,
        string summary) => new(
        _variant,
        converged,
        diverged,
        stopReason,
        _initialParameters.CopyValues(),
        _parameters.CopyValues(),
        _target.CopyValues(),
        _curvature.CopyValues(),
        _historyCount > 0 ? _history[0].Loss : _loss,
        _loss,
        _learningRate,
        _effectiveLearningRate,
        _iteration,
        _gradientNorm,
        CopyHistory(),
        summary);

    private void AddHistoryPoint(int iteration)
    {
        if (_historyCount >= _history.Length)
        {
            return;
        }

        _history[_historyCount++] = new GradientDescentHistoryPoint(
            iteration,
            _loss,
            _effectiveLearningRate,
            _gradientNorm,
            _parameters.CopyValues());
    }

    private GradientDescentHistoryPoint[] CopyHistory()
    {
        var copy = new GradientDescentHistoryPoint[_historyCount];
        for (var index = 0; index < _historyCount; index++)
        {
            var item = _history[index];
            var parameters = new double[item.Parameters.Length];
            for (var component = 0; component < item.Parameters.Length; component++)
            {
                parameters[component] = item.Parameters[component];
            }

            copy[index] = item with { Parameters = parameters };
        }

        return copy;
    }

    private void ResetRunState(bool keepVariant = false)
    {
        if (!keepVariant)
        {
            _variant = GradientDescentVariant.FixedLearningRate;
        }

        _parameters.CopyFrom(_initialParameters.CopyValues());
        _gradient = new ManualVector(_parameters.Dimension);
        _scaledGradient = new ManualVector(_parameters.Dimension);
        _nextParameters = new ManualVector(_parameters.Dimension);
        _nextParameters.CopyFrom(_parameters.CopyValues());
        _phase = GradientDescentPhase.Ready;
        _historyCount = 0;
        _iteration = 0;
        _currentIndex = -1;
        _loss = EvaluateLoss(_parameters);
        _previousLoss = _loss;
        _effectiveLearningRate = _learningRate;
        _gradientNorm = 0d;
        _focusText = "Ready.";
    }

    private static void ValidateConfiguration(GradientDescentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration.InitialParameters);
        ArgumentNullException.ThrowIfNull(configuration.Target);
        ArgumentNullException.ThrowIfNull(configuration.Curvature);

        var dimension = configuration.InitialParameters.Length;
        if (dimension < 1)
        {
            throw new ArgumentException("Gradient Descent needs at least one parameter.", nameof(configuration));
        }

        if (configuration.Target.Length != dimension || configuration.Curvature.Length != dimension)
        {
            throw new ArgumentException("Initial parameters, target and curvature must have the same dimension.", nameof(configuration));
        }

        for (var index = 0; index < dimension; index++)
        {
            if (!double.IsFinite(configuration.InitialParameters[index]) ||
                !double.IsFinite(configuration.Target[index]) ||
                !double.IsFinite(configuration.Curvature[index]))
            {
                throw new ArgumentException("All components must be finite real numbers.", nameof(configuration));
            }

            if (configuration.Curvature[index] <= 0d)
            {
                throw new ArgumentException("Every curvature component must be greater than zero so the teaching objective stays convex.", nameof(configuration));
            }
        }

        if (!double.IsFinite(configuration.LearningRate) || configuration.LearningRate <= 0d)
        {
            throw new ArgumentException("Learning rate must be a finite number greater than zero.", nameof(configuration));
        }

        if (configuration.MaximumIterations is < 1 or > 200)
        {
            throw new ArgumentException("Maximum iterations must be between 1 and 200.", nameof(configuration));
        }

        if (!double.IsFinite(configuration.GradientTolerance) || configuration.GradientTolerance <= 0d)
        {
            throw new ArgumentException("Gradient tolerance must be a finite number greater than zero.", nameof(configuration));
        }

        if (!double.IsFinite(configuration.DecayRate) || configuration.DecayRate < 0d)
        {
            throw new ArgumentException("Decay rate must be a finite non-negative number.", nameof(configuration));
        }
    }

    private async Task PublishAsync(string text, CancellationToken cancellationToken)
    {
        _focusText = text;
        await NextStepAsync(text, cancellationToken);
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12d
        ? "0"
        : value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private static string FormatVector(ManualVector values)
    {
        var parts = new string[values.Dimension];
        for (var index = 0; index < values.Dimension; index++)
        {
            parts[index] = Format(values[index]);
        }

        return $"[{string.Join(", ", parts)}]";
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.LinearRegression;

/// <summary>
/// From-scratch univariate Linear Regression trained with batch Gradient Descent.
/// Dataset, predictions, and residuals use the project's ManualVector storage.
/// The taught prediction, MSE, derivative, and parameter-update loops are explicit.
/// </summary>
public sealed class LinearRegressionSimulation : SimulationAlgorithmBase
{
    private ManualVector _x = new(5);
    private ManualVector _y = new(5);
    private ManualVector _predictions = new(5);
    private ManualVector _residuals = new(5);
    private LinearRegressionHistoryPoint[] _history = new LinearRegressionHistoryPoint[31];
    private int _historyCount;
    private double _weight;
    private double _bias;
    private double _initialWeight;
    private double _initialBias;
    private double _nextWeight;
    private double _nextBias;
    private double _weightGradient;
    private double _biasGradient;
    private double _loss;
    private double _previousLoss;
    private double _gradientNorm;
    private double _learningRate = 0.05d;
    private int _maximumIterations = 30;
    private double _gradientTolerance = 0.01d;
    private int _iteration;
    private int _currentPointIndex = -1;
    private LinearRegressionPhase _phase = LinearRegressionPhase.Ready;
    private string _focusText = "Ready.";

    public LinearRegressionSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new LinearRegressionConfiguration(
            [0d, 1d, 2d, 3d, 4d],
            [1d, 3d, 5d, 7d, 9d],
            0d,
            0d,
            0.05d,
            30,
            0.01d));
    }

    public void Configure(LinearRegressionConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        _x.CopyFrom(configuration.X);
        _y.CopyFrom(configuration.Y);
        _predictions = new ManualVector(configuration.X.Length);
        _residuals = new ManualVector(configuration.X.Length);
        _initialWeight = configuration.InitialWeight;
        _initialBias = configuration.InitialBias;
        _learningRate = configuration.LearningRate;
        _maximumIterations = configuration.MaximumIterations;
        _gradientTolerance = configuration.GradientTolerance;
        _history = new LinearRegressionHistoryPoint[configuration.MaximumIterations + 1];
        ResetRunState();
    }

    public LinearRegressionSnapshot CreateSnapshot() => new(
        _x.CopyValues(),
        _y.CopyValues(),
        _predictions.CopyValues(),
        _residuals.CopyValues(),
        _weight,
        _bias,
        _nextWeight,
        _nextBias,
        _weightGradient,
        _biasGradient,
        _loss,
        _previousLoss,
        _gradientNorm,
        _learningRate,
        _iteration,
        _currentPointIndex,
        _phase,
        _focusText,
        CopyHistory());

    public async Task<LinearRegressionRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        await EvaluateModelAsync("Start by predicting every training point with ŷ = w·x + b.", cancellationToken);
        AddHistoryPoint(0);

        if (_gradientNorm <= _gradientTolerance)
        {
            return await FinishConvergedAsync(0, cancellationToken);
        }

        var initialLoss = _loss;
        for (var update = 1; update <= _maximumIterations; update++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _iteration = update;

            _phase = LinearRegressionPhase.ComputingGradient;
            ComputeGradients();
            _focusText = $"Gradient: dw={Format(_weightGradient)}, db={Format(_biasGradient)}. These values say how the loss changes if weight or bias increases.";
            await NextStepAsync(_focusText, cancellationToken);

            _phase = LinearRegressionPhase.UpdatingModel;
            _nextWeight = _weight - (_learningRate * _weightGradient);
            _nextBias = _bias - (_learningRate * _biasGradient);
            _focusText = $"Update the line: w={Format(_weight)} → {Format(_nextWeight)}, b={Format(_bias)} → {Format(_nextBias)}.";
            await NextStepAsync(_focusText, cancellationToken);

            _weight = _nextWeight;
            _bias = _nextBias;
            _previousLoss = _loss;
            await EvaluateModelAsync($"Iteration {update}: use the new line and measure its error again.", cancellationToken);
            AddHistoryPoint(update);

            if (!double.IsFinite(_loss) || !double.IsFinite(_weight) || !double.IsFinite(_bias) || _loss > Math.Max(1_000_000d, initialLoss * 1_000d))
            {
                return await FinishDivergedAsync("The loss grew far beyond the starting error. This learning rate is too large for this dataset.", cancellationToken);
            }

            if (_gradientNorm <= _gradientTolerance)
            {
                return await FinishConvergedAsync(update, cancellationToken);
            }
        }

        _phase = LinearRegressionPhase.Complete;
        _focusText = $"Stopped after {_maximumIterations} updates. Loss is {Format(_loss)} and the gradient norm is {Format(_gradientNorm)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(false, false, LinearRegressionStopReason.MaximumIterations, _focusText);
    }

    private async Task EvaluateModelAsync(string intro, CancellationToken cancellationToken)
    {
        _phase = LinearRegressionPhase.Predicting;
        _currentPointIndex = -1;
        _focusText = intro;
        for (var index = 0; index < _x.Dimension; index++)
        {
            _predictions[index] = (_weight * _x[index]) + _bias;
        }
        await NextStepAsync(_focusText, cancellationToken);

        _phase = LinearRegressionPhase.MeasuringError;
        var squaredErrorSum = 0d;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var residual = _predictions[index] - _y[index];
            _residuals[index] = residual;
            squaredErrorSum += residual * residual;
        }

        _loss = squaredErrorSum / _x.Dimension;
        ComputeGradients();
        _focusText = $"Measure residuals (prediction − actual), square them, and average. MSE = {Format(_loss)}.";
        await NextStepAsync(_focusText, cancellationToken);
    }

    private void ComputeGradients()
    {
        var weightSum = 0d;
        var biasSum = 0d;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var residual = _predictions[index] - _y[index];
            weightSum += residual * _x[index];
            biasSum += residual;
        }

        var scale = 2d / _x.Dimension;
        _weightGradient = scale * weightSum;
        _biasGradient = scale * biasSum;
        _gradientNorm = Math.Sqrt((_weightGradient * _weightGradient) + (_biasGradient * _biasGradient));
        _nextWeight = _weight - (_learningRate * _weightGradient);
        _nextBias = _bias - (_learningRate * _biasGradient);
    }

    private void ResetRunState()
    {
        _weight = _initialWeight;
        _bias = _initialBias;
        _nextWeight = _weight;
        _nextBias = _bias;
        _weightGradient = 0d;
        _biasGradient = 0d;
        _loss = 0d;
        _previousLoss = 0d;
        _gradientNorm = 0d;
        _iteration = 0;
        _currentPointIndex = -1;
        _phase = LinearRegressionPhase.Ready;
        _focusText = "Ready.";
        _historyCount = 0;
        for (var index = 0; index < _predictions.Dimension; index++)
        {
            _predictions[index] = (_weight * _x[index]) + _bias;
            _residuals[index] = _predictions[index] - _y[index];
        }
        _loss = EvaluateLoss();
        ComputeGradients();
    }

    private double EvaluateLoss()
    {
        var sum = 0d;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var residual = ((_weight * _x[index]) + _bias) - _y[index];
            sum += residual * residual;
        }
        return sum / _x.Dimension;
    }

    private void AddHistoryPoint(int iteration)
    {
        if (_historyCount >= _history.Length)
        {
            return;
        }
        _history[_historyCount++] = new LinearRegressionHistoryPoint(iteration, _weight, _bias, _loss, _gradientNorm);
    }

    private LinearRegressionHistoryPoint[] CopyHistory()
    {
        var copy = new LinearRegressionHistoryPoint[_historyCount];
        for (var index = 0; index < _historyCount; index++)
        {
            copy[index] = _history[index];
        }
        return copy;
    }

    private async Task<LinearRegressionRunResult> FinishConvergedAsync(int iterationsCompleted, CancellationToken cancellationToken)
    {
        _phase = LinearRegressionPhase.Complete;
        _iteration = iterationsCompleted;
        _focusText = $"Training stopped because the gradient norm {Format(_gradientNorm)} is within tolerance {Format(_gradientTolerance)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(true, false, LinearRegressionStopReason.GradientTolerance, _focusText);
    }

    private async Task<LinearRegressionRunResult> FinishDivergedAsync(string reason, CancellationToken cancellationToken)
    {
        _phase = LinearRegressionPhase.Diverged;
        _focusText = reason;
        await NextStepAsync(reason, cancellationToken);
        return BuildResult(false, true, LinearRegressionStopReason.Diverged, reason);
    }

    private LinearRegressionRunResult BuildResult(bool converged, bool diverged, LinearRegressionStopReason stopReason, string summary) => new(
        converged,
        diverged,
        stopReason,
        _initialWeight,
        _initialBias,
        _weight,
        _bias,
        _historyCount > 0 ? _history[0].Loss : _loss,
        _loss,
        _learningRate,
        _iteration,
        _gradientNorm,
        _x.CopyValues(),
        _y.CopyValues(),
        _predictions.CopyValues(),
        _residuals.CopyValues(),
        CopyHistory(),
        summary);

    private static void ValidateConfiguration(LinearRegressionConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration.X);
        ArgumentNullException.ThrowIfNull(configuration.Y);
        if (configuration.X.Length < 2)
        {
            throw new ArgumentException("Linear Regression needs at least two training points.");
        }
        if (configuration.X.Length != configuration.Y.Length)
        {
            throw new ArgumentException("X and Y must contain the same number of training points.");
        }
        if (!double.IsFinite(configuration.InitialWeight) || !double.IsFinite(configuration.InitialBias))
        {
            throw new ArgumentException("Starting weight and bias must be finite.");
        }
        if (!double.IsFinite(configuration.LearningRate) || configuration.LearningRate <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration.LearningRate), "Learning rate must be finite and greater than 0.");
        }
        if (configuration.MaximumIterations is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration.MaximumIterations), "Use between 1 and 200 training updates.");
        }
        if (!double.IsFinite(configuration.GradientTolerance) || configuration.GradientTolerance <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration.GradientTolerance), "Gradient tolerance must be finite and greater than 0.");
        }
        for (var index = 0; index < configuration.X.Length; index++)
        {
            if (!double.IsFinite(configuration.X[index]) || !double.IsFinite(configuration.Y[index]))
            {
                throw new ArgumentException("Training points cannot contain NaN or infinity.");
            }
        }
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12 ? "0" : value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
}

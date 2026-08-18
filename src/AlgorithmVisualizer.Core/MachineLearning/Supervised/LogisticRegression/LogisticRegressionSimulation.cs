using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.LogisticRegression;

/// <summary>
/// From-scratch univariate binary Logistic Regression trained with full-batch Gradient Descent.
/// X, labels, scores, probabilities, and probability errors use the project's ManualVector storage.
/// Score, stable sigmoid, binary cross-entropy, derivative, threshold, and parameter-update loops are explicit.
/// </summary>
public sealed class LogisticRegressionSimulation : SimulationAlgorithmBase
{
    private ManualVector _x = new(6);
    private ManualVector _labels = new(6);
    private ManualVector _scores = new(6);
    private ManualVector _probabilities = new(6);
    private ManualVector _errors = new(6);
    private int[] _predictedClasses = new int[6];
    private LogisticRegressionHistoryPoint[] _history = new LogisticRegressionHistoryPoint[41];
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
    private double _accuracy;
    private double _initialAccuracy;
    private double _learningRate = 0.6d;
    private int _maximumIterations = 12;
    private double _gradientTolerance = 0.03d;
    private int _iteration;
    private LogisticRegressionPhase _phase = LogisticRegressionPhase.Ready;
    private string _focusText = "Ready.";

    public LogisticRegressionSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new LogisticRegressionConfiguration(
            [-3d, -2d, -1d, 1d, 2d, 3d],
            [0d, 0d, 0d, 1d, 1d, 1d],
            0d,
            0d,
            0.6d,
            12,
            0.03d));
    }

    public void Configure(LogisticRegressionConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        _x.CopyFrom(configuration.X);
        _labels.CopyFrom(configuration.Labels);
        _scores = new ManualVector(configuration.X.Length);
        _probabilities = new ManualVector(configuration.X.Length);
        _errors = new ManualVector(configuration.X.Length);
        _predictedClasses = new int[configuration.X.Length];
        _initialWeight = configuration.InitialWeight;
        _initialBias = configuration.InitialBias;
        _learningRate = configuration.LearningRate;
        _maximumIterations = configuration.MaximumIterations;
        _gradientTolerance = configuration.GradientTolerance;
        _history = new LogisticRegressionHistoryPoint[configuration.MaximumIterations + 1];
        ResetRunState();
    }

    public LogisticRegressionSnapshot CreateSnapshot() => new(
        _x.CopyValues(),
        _labels.CopyValues(),
        _scores.CopyValues(),
        _probabilities.CopyValues(),
        _errors.CopyValues(),
        CopyPredictedClasses(),
        _weight,
        _bias,
        _nextWeight,
        _nextBias,
        _weightGradient,
        _biasGradient,
        _loss,
        _previousLoss,
        _gradientNorm,
        _accuracy,
        _learningRate,
        _iteration,
        _phase,
        _focusText,
        CopyHistory());

    public async Task<LogisticRegressionRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var initialLoss = _loss;
        _initialAccuracy = _accuracy;

        await EvaluateModelAsync("Start by turning each x into a linear score z = w·x + b.", cancellationToken);
        await PublishGradientAsync(cancellationToken);
        AddHistoryPoint(0);

        if (_gradientNorm <= _gradientTolerance)
        {
            return await FinishConvergedAsync(0, cancellationToken);
        }

        for (var update = 1; update <= _maximumIterations; update++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _iteration = update;

            _phase = LogisticRegressionPhase.UpdatingModel;
            _nextWeight = _weight - (_learningRate * _weightGradient);
            _nextBias = _bias - (_learningRate * _biasGradient);
            _focusText = $"Update the boundary: w={Format(_weight)} → {Format(_nextWeight)}, b={Format(_bias)} → {Format(_nextBias)}.";
            await NextStepAsync(_focusText, cancellationToken);

            _weight = _nextWeight;
            _bias = _nextBias;
            _previousLoss = _loss;

            await EvaluateModelAsync($"Update {update}: score the same training examples with the new weight and bias.", cancellationToken);
            await PublishGradientAsync(cancellationToken);
            AddHistoryPoint(update);

            if (HasDiverged(initialLoss))
            {
                return await FinishDivergedAsync("The parameter updates became numerically unstable. Use a smaller learning rate.", cancellationToken);
            }

            if (_gradientNorm <= _gradientTolerance)
            {
                return await FinishConvergedAsync(update, cancellationToken);
            }
        }

        _phase = LogisticRegressionPhase.Complete;
        _focusText = $"Stopped after {_maximumIterations} updates. Cross-entropy is {Format(_loss)} and accuracy is {FormatPercent(_accuracy)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(false, false, LogisticRegressionStopReason.MaximumIterations, _focusText);
    }

    private async Task EvaluateModelAsync(string intro, CancellationToken cancellationToken)
    {
        _phase = LogisticRegressionPhase.ForwardPass;
        var lossSum = 0d;
        var correct = 0;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var score = (_weight * _x[index]) + _bias;
            var probability = Sigmoid(score);
            var label = _labels[index];
            _scores[index] = score;
            _probabilities[index] = probability;
            _errors[index] = probability - label;
            _predictedClasses[index] = probability >= 0.5d ? 1 : 0;
            if (_predictedClasses[index] == (int)label) correct++;
            lossSum += BinaryCrossEntropyFromScore(score, label);
        }

        _loss = lossSum / _x.Dimension;
        _accuracy = (double)correct / _x.Dimension;
        ComputeGradients();
        _focusText = $"{intro} Forward pass: score → sigmoid probability → 0.5 class. Cross-entropy = {Format(_loss)}; accuracy = {FormatPercent(_accuracy)}.";
        await NextStepAsync(_focusText, cancellationToken);
    }

    private async Task PublishGradientAsync(CancellationToken cancellationToken)
    {
        _phase = LogisticRegressionPhase.ComputingGradient;
        ComputeGradients();
        _focusText = $"Gradient from probability − label: dw={Format(_weightGradient)}, db={Format(_biasGradient)}. Gradient norm = {Format(_gradientNorm)}.";
        await NextStepAsync(_focusText, cancellationToken);
    }

    private void ComputeGradients()
    {
        var weightSum = 0d;
        var biasSum = 0d;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var error = _probabilities[index] - _labels[index];
            weightSum += error * _x[index];
            biasSum += error;
        }

        _weightGradient = weightSum / _x.Dimension;
        _biasGradient = biasSum / _x.Dimension;
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
        _accuracy = 0d;
        _initialAccuracy = 0d;
        _iteration = 0;
        _phase = LogisticRegressionPhase.Ready;
        _focusText = "Ready.";
        _historyCount = 0;
        EvaluateModelWithoutSteps();
        _previousLoss = _loss;
        _initialAccuracy = _accuracy;
    }

    private void EvaluateModelWithoutSteps()
    {
        var lossSum = 0d;
        var correct = 0;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var score = (_weight * _x[index]) + _bias;
            var probability = Sigmoid(score);
            var label = _labels[index];
            _scores[index] = score;
            _probabilities[index] = probability;
            _errors[index] = probability - label;
            _predictedClasses[index] = probability >= 0.5d ? 1 : 0;
            if (_predictedClasses[index] == (int)label) correct++;
            lossSum += BinaryCrossEntropyFromScore(score, label);
        }

        _loss = lossSum / _x.Dimension;
        _accuracy = (double)correct / _x.Dimension;
        ComputeGradients();
    }

    private async Task<LogisticRegressionRunResult> FinishConvergedAsync(int iterations, CancellationToken cancellationToken)
    {
        _phase = LogisticRegressionPhase.Complete;
        _focusText = iterations == 0
            ? $"The starting model already has a small gradient ({Format(_gradientNorm)}). No update is needed."
            : $"Gradient norm is {Format(_gradientNorm)}, inside the tolerance. Training stops with {FormatPercent(_accuracy)} accuracy.";
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(true, false, LogisticRegressionStopReason.GradientTolerance, _focusText);
    }

    private async Task<LogisticRegressionRunResult> FinishDivergedAsync(string reason, CancellationToken cancellationToken)
    {
        _phase = LogisticRegressionPhase.Diverged;
        _focusText = reason;
        await NextStepAsync(_focusText, cancellationToken);
        return BuildResult(false, true, LogisticRegressionStopReason.Diverged, reason);
    }

    private LogisticRegressionRunResult BuildResult(bool converged, bool diverged, LogisticRegressionStopReason stopReason, string summary) => new(
        converged,
        diverged,
        stopReason,
        _initialWeight,
        _initialBias,
        _weight,
        _bias,
        _historyCount > 0 ? _history[0].Loss : _loss,
        _loss,
        _historyCount > 0 ? _history[0].Accuracy : _initialAccuracy,
        _accuracy,
        _learningRate,
        _iteration,
        _gradientNorm,
        _x.CopyValues(),
        _labels.CopyValues(),
        _scores.CopyValues(),
        _probabilities.CopyValues(),
        CopyPredictedClasses(),
        CopyHistory(),
        summary);

    private void AddHistoryPoint(int iteration)
    {
        if (_historyCount >= _history.Length) return;
        _history[_historyCount++] = new LogisticRegressionHistoryPoint(iteration, _weight, _bias, _loss, _gradientNorm, _accuracy);
    }

    private LogisticRegressionHistoryPoint[] CopyHistory()
    {
        var copy = new LogisticRegressionHistoryPoint[_historyCount];
        for (var index = 0; index < _historyCount; index++) copy[index] = _history[index];
        return copy;
    }

    private int[] CopyPredictedClasses()
    {
        var copy = new int[_predictedClasses.Length];
        for (var index = 0; index < _predictedClasses.Length; index++) copy[index] = _predictedClasses[index];
        return copy;
    }

    private bool HasDiverged(double initialLoss) =>
        !double.IsFinite(_loss) ||
        !double.IsFinite(_weight) ||
        !double.IsFinite(_bias) ||
        Math.Abs(_weight) > 1_000_000d ||
        Math.Abs(_bias) > 1_000_000d ||
        _loss > Math.Max(50d, initialLoss * 100d);

    private static double Sigmoid(double score)
    {
        if (score >= 0d)
        {
            return 1d / (1d + Math.Exp(-score));
        }

        var exp = Math.Exp(score);
        return exp / (1d + exp);
    }

    private static double BinaryCrossEntropyFromScore(double score, double label) =>
        Math.Max(score, 0d) - (score * label) + Math.Log(1d + Math.Exp(-Math.Abs(score)));

    private static void ValidateConfiguration(LogisticRegressionConfiguration configuration)
    {
        if (configuration.X is null || configuration.Labels is null) throw new ArgumentException("Training vectors are required.", nameof(configuration));
        if (configuration.X.Length != configuration.Labels.Length) throw new ArgumentException("X and label vectors must contain the same number of examples.", nameof(configuration));
        if (configuration.X.Length is < 2 or > 10) throw new ArgumentException("Use between 2 and 10 examples in this visual lesson.", nameof(configuration));

        var sawZero = false;
        var sawOne = false;
        for (var index = 0; index < configuration.X.Length; index++)
        {
            if (!double.IsFinite(configuration.X[index])) throw new ArgumentException("Every x value must be finite.", nameof(configuration));
            if (configuration.Labels[index] == 0d) sawZero = true;
            else if (configuration.Labels[index] == 1d) sawOne = true;
            else throw new ArgumentException("Logistic Regression labels must be exactly 0 or 1.", nameof(configuration));
        }

        if (!sawZero || !sawOne) throw new ArgumentException("Include at least one example from class 0 and one from class 1.", nameof(configuration));
        if (!double.IsFinite(configuration.InitialWeight) || !double.IsFinite(configuration.InitialBias)) throw new ArgumentException("Starting weight and bias must be finite.", nameof(configuration));
        if (!double.IsFinite(configuration.LearningRate) || configuration.LearningRate <= 0d) throw new ArgumentException("Learning rate must be a finite number greater than 0.", nameof(configuration));
        if (configuration.MaximumIterations is < 1 or > 200) throw new ArgumentException("Maximum iterations must be between 1 and 200.", nameof(configuration));
        if (!double.IsFinite(configuration.GradientTolerance) || configuration.GradientTolerance <= 0d) throw new ArgumentException("Gradient tolerance must be a finite number greater than 0.", nameof(configuration));
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12 ? "0" : value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
    private static string FormatPercent(double value) => (value * 100d).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";
}

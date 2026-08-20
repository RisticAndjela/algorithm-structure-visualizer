using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Optimizers;

/// <summary>
/// Deterministic teaching comparison of SGD, Momentum, and Adam on the same tiny
/// linear-regression stream. Each step consumes exactly one sample in cyclic order,
/// computes that sample gradient explicitly, updates optimizer state, then updates w,b.
/// </summary>
public sealed class OptimizerSimulation : SimulationAlgorithmBase
{
    private ManualVector _x = new(4);
    private ManualVector _y = new(4);
    private OptimizerConfiguration _configuration = DefaultConfiguration();
    private double _weight;
    private double _bias;
    private double _gradientWeight;
    private double _gradientBias;
    private double _mWeight;
    private double _mBias;
    private double _vWeight;
    private double _vBias;
    private double _currentX;
    private double _currentY;
    private double _prediction;
    private double _sampleLoss;
    private double _datasetMse;
    private int _stepIndex;
    private int _sampleIndex = -1;
    private double[] _weightPath = [];
    private double[] _biasPath = [];
    private int _pathCount;
    private OptimizerPhase _phase = OptimizerPhase.Ready;
    private string _focusText = "Ready.";

    public OptimizerSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(DefaultConfiguration());
    }

    public void Configure(OptimizerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _configuration = configuration;
        _x.CopyFrom(configuration.X);
        _y.CopyFrom(configuration.Y);
        ResetRunState();
    }

    public OptimizerSnapshot CreateSnapshot() => new(
        _configuration.Kind, _weight, _bias, _gradientWeight, _gradientBias,
        _mWeight, _mBias, _vWeight, _vBias, _currentX, _currentY, _prediction,
        _sampleLoss, _datasetMse, _stepIndex, _sampleIndex, CopyPath(_weightPath, _pathCount),
        CopyPath(_biasPath, _pathCount), _phase, _focusText);

    public async Task<OptimizerRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var initialMse = _datasetMse;

        for (var step = 1; step <= _configuration.Steps; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _stepIndex = step;
            _sampleIndex = (step - 1) % _x.Dimension;
            _currentX = _x[_sampleIndex];
            _currentY = _y[_sampleIndex];
            _prediction = (_weight * _currentX) + _bias;
            var error = _prediction - _currentY;
            _sampleLoss = 0.5d * error * error;

            _phase = OptimizerPhase.Sample;
            _focusText = $"Step {step}: use sample {_sampleIndex + 1}, x={_currentX:0.###}, y={_currentY:0.###}. Prediction = {_prediction:0.###}.";
            await NextStepAsync(_focusText, cancellationToken);

            _gradientWeight = error * _currentX;
            _gradientBias = error;
            _phase = OptimizerPhase.Gradient;
            _focusText = $"Sample gradient: dw = error·x = {_gradientWeight:0.###}, db = error = {_gradientBias:0.###}. The selected optimizer starts from this raw gradient; its stored state decides how that signal becomes an update.";
            await NextStepAsync(_focusText, cancellationToken);

            _phase = OptimizerPhase.StateUpdate;
            var updateWeight = _gradientWeight;
            var updateBias = _gradientBias;
            if (_configuration.Kind == OptimizerKind.Momentum)
            {
                _mWeight = (_configuration.MomentumBeta * _mWeight) + _gradientWeight;
                _mBias = (_configuration.MomentumBeta * _mBias) + _gradientBias;
                updateWeight = _mWeight;
                updateBias = _mBias;
                _focusText = $"Momentum accumulates a velocity v = βv + g: v_w={_mWeight:0.###}, v_b={_mBias:0.###}.";
            }
            else if (_configuration.Kind == OptimizerKind.Adam)
            {
                _mWeight = (_configuration.AdamBeta1 * _mWeight) + ((1d - _configuration.AdamBeta1) * _gradientWeight);
                _mBias = (_configuration.AdamBeta1 * _mBias) + ((1d - _configuration.AdamBeta1) * _gradientBias);
                _vWeight = (_configuration.AdamBeta2 * _vWeight) + ((1d - _configuration.AdamBeta2) * _gradientWeight * _gradientWeight);
                _vBias = (_configuration.AdamBeta2 * _vBias) + ((1d - _configuration.AdamBeta2) * _gradientBias * _gradientBias);
                var correction1 = 1d - Math.Pow(_configuration.AdamBeta1, step);
                var correction2 = 1d - Math.Pow(_configuration.AdamBeta2, step);
                var mHatWeight = _mWeight / correction1;
                var mHatBias = _mBias / correction1;
                var vHatWeight = _vWeight / correction2;
                var vHatBias = _vBias / correction2;
                updateWeight = mHatWeight / (Math.Sqrt(vHatWeight) + _configuration.AdamEpsilon);
                updateBias = mHatBias / (Math.Sqrt(vHatBias) + _configuration.AdamEpsilon);
                _focusText = $"Adam updates first and second moments, bias-corrects them, then scales each parameter by its own recent gradient magnitude.";
            }
            else
            {
                _focusText = "SGD keeps no running optimizer state: the raw sample gradient is the update direction.";
            }
            await NextStepAsync(_focusText, cancellationToken);

            _phase = OptimizerPhase.ParameterUpdate;
            _weight -= _configuration.LearningRate * updateWeight;
            _bias -= _configuration.LearningRate * updateBias;
            _datasetMse = ComputeMse(_weight, _bias);
            AppendPath(_weight, _bias);
            _focusText = $"Update parameters with η={_configuration.LearningRate:0.###}: w={_weight:0.###}, b={_bias:0.###}. Dataset MSE is now {_datasetMse:0.####}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = OptimizerPhase.Complete;
        _focusText = $"{_configuration.Kind} complete after {_configuration.Steps} stochastic updates. MSE changed {initialMse:0.####} → {_datasetMse:0.####}.";
        await NextStepAsync(_focusText, cancellationToken);

        return new OptimizerRunResult(_configuration.Kind, initialMse, _datasetMse, _weight, _bias,
            CopyPath(_weightPath, _pathCount), CopyPath(_biasPath, _pathCount), _configuration.Steps, _focusText);
    }

    private void ResetRunState()
    {
        _weight = _configuration.InitialWeight;
        _bias = _configuration.InitialBias;
        _gradientWeight = _gradientBias = _mWeight = _mBias = _vWeight = _vBias = 0d;
        _currentX = _currentY = _prediction = _sampleLoss = 0d;
        _stepIndex = 0; _sampleIndex = -1;
        _datasetMse = ComputeMse(_weight, _bias);
        _weightPath = new double[_configuration.Steps + 1];
        _biasPath = new double[_configuration.Steps + 1];
        _pathCount = 0;
        AppendPath(_weight, _bias);
        _phase = OptimizerPhase.Ready; _focusText = "Ready.";
    }

    private double ComputeMse(double weight, double bias)
    {
        var sum = 0d;
        for (var index = 0; index < _x.Dimension; index++)
        {
            var error = ((weight * _x[index]) + bias) - _y[index];
            sum += error * error;
        }
        return sum / _x.Dimension;
    }

    private void AppendPath(double weight, double bias)
    {
        _weightPath[_pathCount] = weight;
        _biasPath[_pathCount] = bias;
        _pathCount++;
    }

    private static double[] CopyPath(double[] source, int count)
    {
        var copy = new double[count];
        for (var index = 0; index < count; index++) copy[index] = source[index];
        return copy;
    }

    private static void Validate(OptimizerConfiguration configuration)
    {
        if (configuration.X is null || configuration.Y is null || configuration.X.Length < 2 || configuration.X.Length != configuration.Y.Length) throw new ArgumentException("X and Y must contain the same 2+ samples.", nameof(configuration));
        if (configuration.X.Length > 16) throw new ArgumentException("Use at most 16 samples in the teaching optimizer.", nameof(configuration));
        if (configuration.Steps < 1 || configuration.Steps > 80) throw new ArgumentOutOfRangeException(nameof(configuration), "Use 1–80 teaching steps.");
        if (!double.IsFinite(configuration.InitialWeight) || !double.IsFinite(configuration.InitialBias) || !double.IsFinite(configuration.LearningRate) || configuration.LearningRate <= 0d || configuration.LearningRate > 1d) throw new ArgumentException("Initial parameters and learning rate must be finite; learning rate must be in (0,1].", nameof(configuration));
        if (configuration.MomentumBeta < 0d || configuration.MomentumBeta >= 1d || configuration.AdamBeta1 < 0d || configuration.AdamBeta1 >= 1d || configuration.AdamBeta2 < 0d || configuration.AdamBeta2 >= 1d || configuration.AdamEpsilon <= 0d) throw new ArgumentException("Optimizer beta values must be in [0,1) and epsilon must be positive.", nameof(configuration));
        for (var index = 0; index < configuration.X.Length; index++) if (!double.IsFinite(configuration.X[index]) || !double.IsFinite(configuration.Y[index])) throw new ArgumentException("Dataset values must be finite.", nameof(configuration));
    }

    private static OptimizerConfiguration DefaultConfiguration() => new(
        [-1d, 0d, 1d, 2d], [-1d, 1d, 3d, 5d], 0d, 0d, 0.08d, 12, OptimizerKind.Sgd);
}

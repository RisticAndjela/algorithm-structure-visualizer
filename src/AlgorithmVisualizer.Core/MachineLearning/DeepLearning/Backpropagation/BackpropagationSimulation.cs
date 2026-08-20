using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Backpropagation;

/// <summary>
/// Explicit one-hidden-layer backpropagation. Every chain-rule factor and parameter
/// gradient is computed with loops over the same weights used by the forward pass.
/// </summary>
public sealed class BackpropagationSimulation : SimulationAlgorithmBase
{
    private ManualVector _inputs = new(2);
    private ManualMatrix _hiddenWeights = new(2, 2);
    private ManualVector _hiddenBiases = new(2);
    private ManualVector _outputWeights = new(2);
    private ManualVector _hiddenPre = new(2);
    private ManualVector _hiddenAct = new(2);
    private ManualVector _hiddenDeltas = new(2);
    private ManualMatrix _hiddenWeightGradients = new(2, 2);
    private ManualVector _hiddenBiasGradients = new(2);
    private ManualVector _outputWeightGradients = new(2);
    private double _outputBias;
    private ActivationKind _hiddenActivation = ActivationKind.Tanh;
    private ActivationKind _outputActivation = ActivationKind.Sigmoid;
    private double _target = 1d;
    private double _learningRate = 0.2d;
    private double _outputPre;
    private double _output;
    private double _loss;
    private double _outputDelta;
    private double _outputBiasGradient;
    private int _activeHiddenNeuron = -1;
    private BackpropagationPhase _phase = BackpropagationPhase.Ready;
    private string _focusText = "Ready.";

    public BackpropagationSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(DefaultConfiguration());
    }

    public void Configure(BackpropagationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _inputs.CopyFrom(configuration.Inputs);
        _hiddenWeights = new ManualMatrix(configuration.HiddenCount, configuration.Inputs.Length);
        var offset = 0;
        for (var hidden = 0; hidden < configuration.HiddenCount; hidden++)
            for (var input = 0; input < configuration.Inputs.Length; input++)
                _hiddenWeights[hidden, input] = configuration.HiddenWeights[offset++];
        _hiddenBiases = new ManualVector(configuration.HiddenCount); _hiddenBiases.CopyFrom(configuration.HiddenBiases);
        _outputWeights = new ManualVector(configuration.HiddenCount); _outputWeights.CopyFrom(configuration.OutputWeights);
        _outputBias = configuration.OutputBias;
        _hiddenActivation = configuration.HiddenActivation;
        _outputActivation = configuration.OutputActivation;
        _target = configuration.Target;
        _learningRate = configuration.LearningRate;
        ResetTransientState();
    }

    public BackpropagationSnapshot CreateSnapshot() => new(
        _inputs.CopyValues(), _hiddenWeights.Rows, _hiddenWeights.CopyRawValues(), _hiddenBiases.CopyValues(),
        _hiddenPre.CopyValues(), _hiddenAct.CopyValues(), _outputWeights.CopyValues(), _outputBias,
        _outputPre, _output, _target, _loss, _outputDelta, _hiddenDeltas.CopyValues(),
        _outputWeightGradients.CopyValues(), _outputBiasGradient, _hiddenWeightGradients.CopyRawValues(),
        _hiddenBiasGradients.CopyValues(), _activeHiddenNeuron, _phase, _focusText);

    public async Task<BackpropagationRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetTransientState();
        Forward();
        var predictionBefore = _output;
        var lossBefore = _loss;

        _phase = BackpropagationPhase.ForwardHidden;
        _focusText = $"Forward pass cached {_hiddenAct.Dimension} hidden activations. Backprop will reuse these values instead of recomputing the layer.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = BackpropagationPhase.ForwardOutput;
        _focusText = $"Forward output = {_output:0.###} for target {_target:0.###}. The reverse pass starts from this final prediction.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = BackpropagationPhase.Loss;
        _focusText = $"Loss = ½(prediction − target)² = {_loss:0.#####}. This scalar is the starting point for the chain rule.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = BackpropagationPhase.OutputGradient;
        var dLossDOutput = _output - _target;
        _outputDelta = dLossDOutput * ActivationMath.DerivativeFromPreActivation(_outputActivation, _outputPre);
        _outputBiasGradient = _outputDelta;
        for (var hidden = 0; hidden < _hiddenAct.Dimension; hidden++)
            _outputWeightGradients[hidden] = _outputDelta * _hiddenAct[hidden];
        _focusText = $"Output delta = dL/dŷ × activation′(z) = {_outputDelta:0.#####}. It directly gives the output-bias gradient and scales every output weight gradient.";
        await NextStepAsync(_focusText, cancellationToken);

        for (var hidden = 0; hidden < _hiddenAct.Dimension; hidden++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _phase = BackpropagationPhase.HiddenGradient;
            _activeHiddenNeuron = hidden;
            _hiddenDeltas[hidden] = _outputDelta * _outputWeights[hidden] * ActivationMath.DerivativeFromPreActivation(_hiddenActivation, _hiddenPre[hidden]);
            _hiddenBiasGradients[hidden] = _hiddenDeltas[hidden];
            for (var input = 0; input < _inputs.Dimension; input++)
                _hiddenWeightGradients[hidden, input] = _hiddenDeltas[hidden] * _inputs[input];
            _focusText = $"Hidden neuron {hidden + 1}: send the output delta backward through weight {_outputWeights[hidden]:0.###}, then multiply by {ActivationMath.DisplayName(_hiddenActivation)}′(z). Hidden delta = {_hiddenDeltas[hidden]:0.#####}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _activeHiddenNeuron = -1;
        _phase = BackpropagationPhase.WeightGradients;
        _focusText = "Every parameter now has a gradient. Each weight gradient is the upstream delta multiplied by the activation/input that fed that weight.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = BackpropagationPhase.Update;
        ApplyGradientUpdate();
        Forward();
        var predictionAfter = _output;
        var lossAfter = _loss;
        _focusText = $"Apply parameter ← parameter − η·gradient with η = {_learningRate:0.###}. Loss changes {lossBefore:0.#####} → {lossAfter:0.#####}.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = BackpropagationPhase.Complete;
        _focusText = $"Backpropagation complete. Forward values carried information right; gradients carried responsibility left. New prediction = {predictionAfter:0.###}.";
        await NextStepAsync(_focusText, cancellationToken);

        return new BackpropagationRunResult(
            predictionBefore, predictionAfter, lossBefore, lossAfter, _outputDelta,
            _hiddenDeltas.CopyValues(), _hiddenWeightGradients.CopyRawValues(),
            _outputWeightGradients.CopyValues(), _focusText);
    }

    private void Forward()
    {
        for (var hidden = 0; hidden < _hiddenWeights.Rows; hidden++)
        {
            var z = _hiddenBiases[hidden];
            for (var input = 0; input < _hiddenWeights.Columns; input++) z += _hiddenWeights[hidden, input] * _inputs[input];
            _hiddenPre[hidden] = z;
            _hiddenAct[hidden] = ActivationMath.Apply(_hiddenActivation, z);
        }
        _outputPre = _outputBias;
        for (var hidden = 0; hidden < _hiddenAct.Dimension; hidden++) _outputPre += _outputWeights[hidden] * _hiddenAct[hidden];
        _output = ActivationMath.Apply(_outputActivation, _outputPre);
        var error = _output - _target;
        _loss = 0.5d * error * error;
    }

    private void ApplyGradientUpdate()
    {
        for (var hidden = 0; hidden < _hiddenWeights.Rows; hidden++)
        {
            _outputWeights[hidden] -= _learningRate * _outputWeightGradients[hidden];
            _hiddenBiases[hidden] -= _learningRate * _hiddenBiasGradients[hidden];
            for (var input = 0; input < _hiddenWeights.Columns; input++)
                _hiddenWeights[hidden, input] -= _learningRate * _hiddenWeightGradients[hidden, input];
        }
        _outputBias -= _learningRate * _outputBiasGradient;
    }

    private void ResetTransientState()
    {
        _hiddenPre = new ManualVector(_hiddenWeights.Rows);
        _hiddenAct = new ManualVector(_hiddenWeights.Rows);
        _hiddenDeltas = new ManualVector(_hiddenWeights.Rows);
        _hiddenWeightGradients = new ManualMatrix(_hiddenWeights.Rows, _hiddenWeights.Columns);
        _hiddenBiasGradients = new ManualVector(_hiddenWeights.Rows);
        _outputWeightGradients = new ManualVector(_hiddenWeights.Rows);
        _outputPre = 0d; _output = 0d; _loss = 0d; _outputDelta = 0d; _outputBiasGradient = 0d;
        _activeHiddenNeuron = -1; _phase = BackpropagationPhase.Ready; _focusText = "Ready.";
        Forward();
    }

    private static void Validate(BackpropagationConfiguration configuration)
    {
        if (configuration.Inputs is null || configuration.Inputs.Length < 1 || configuration.Inputs.Length > 6) throw new ArgumentException("Use 1–6 inputs.", nameof(configuration));
        if (configuration.HiddenCount < 1 || configuration.HiddenCount > 6) throw new ArgumentOutOfRangeException(nameof(configuration), "Use 1–6 hidden neurons.");
        if (configuration.HiddenWeights is null || configuration.HiddenWeights.Length != configuration.Inputs.Length * configuration.HiddenCount) throw new ArgumentException("HiddenWeights shape is invalid.", nameof(configuration));
        if (configuration.HiddenBiases is null || configuration.HiddenBiases.Length != configuration.HiddenCount) throw new ArgumentException("HiddenBiases shape is invalid.", nameof(configuration));
        if (configuration.OutputWeights is null || configuration.OutputWeights.Length != configuration.HiddenCount) throw new ArgumentException("OutputWeights shape is invalid.", nameof(configuration));
        if (!double.IsFinite(configuration.OutputBias) || !double.IsFinite(configuration.Target) || !double.IsFinite(configuration.LearningRate) || configuration.LearningRate <= 0d || configuration.LearningRate > 1d) throw new ArgumentException("Bias, target, and learning rate must be finite; learning rate must be in (0, 1].", nameof(configuration));
        ValidateFinite(configuration.Inputs); ValidateFinite(configuration.HiddenWeights); ValidateFinite(configuration.HiddenBiases); ValidateFinite(configuration.OutputWeights);
    }

    private static void ValidateFinite(double[] values)
    {
        for (var index = 0; index < values.Length; index++) if (!double.IsFinite(values[index])) throw new ArgumentException("Backpropagation values must be finite.");
    }

    private static BackpropagationConfiguration DefaultConfiguration() => new(
        [1d, 0.5d], 2,
        [0.6d, -0.2d, -0.4d, 0.7d],
        [0.1d, -0.1d],
        [0.8d, -0.6d], 0.05d,
        ActivationKind.Tanh, ActivationKind.Sigmoid,
        1d, 0.2d);
}

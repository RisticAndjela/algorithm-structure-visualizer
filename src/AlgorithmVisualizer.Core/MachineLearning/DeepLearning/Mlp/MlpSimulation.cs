using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Mlp;

/// <summary>
/// One-hidden-layer dense MLP forward pass built from ManualVector and ManualMatrix.
/// The layer/neuron loops, weighted sums, biases, and activations are explicit.
/// </summary>
public sealed class MlpSimulation : SimulationAlgorithmBase
{
    private ManualVector _inputs = new(2);
    private ManualMatrix _hiddenWeights = new(2, 2);
    private ManualVector _hiddenBiases = new(2);
    private ManualVector _hiddenPre = new(2);
    private ManualVector _hiddenAct = new(2);
    private ManualVector _outputWeights = new(2);
    private double _outputBias;
    private double _outputPre;
    private double _output;
    private ActivationKind _hiddenActivation = ActivationKind.ReLU;
    private ActivationKind _outputActivation = ActivationKind.Sigmoid;
    private int _activeHiddenNeuron = -1;
    private MlpPhase _phase = MlpPhase.Ready;
    private string _focusText = "Ready.";

    public MlpSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(DefaultConfiguration());
    }

    public void Configure(MlpConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _inputs.CopyFrom(configuration.Inputs);
        _hiddenWeights = new ManualMatrix(configuration.HiddenCount, configuration.Inputs.Length);
        var offset = 0;
        for (var hidden = 0; hidden < configuration.HiddenCount; hidden++)
        {
            for (var input = 0; input < configuration.Inputs.Length; input++)
                _hiddenWeights[hidden, input] = configuration.HiddenWeights[offset++];
        }
        _hiddenBiases = new ManualVector(configuration.HiddenCount);
        _hiddenBiases.CopyFrom(configuration.HiddenBiases);
        _outputWeights = new ManualVector(configuration.HiddenCount);
        _outputWeights.CopyFrom(configuration.OutputWeights);
        _outputBias = configuration.OutputBias;
        _hiddenActivation = configuration.HiddenActivation;
        _outputActivation = configuration.OutputActivation;
        ResetRunState();
    }

    public MlpSnapshot CreateSnapshot() => new(
        _inputs.CopyValues(),
        _hiddenWeights.Rows,
        _hiddenWeights.CopyRawValues(),
        _hiddenBiases.CopyValues(),
        _hiddenPre.CopyValues(),
        _hiddenAct.CopyValues(),
        _outputWeights.CopyValues(),
        _outputBias,
        _outputPre,
        _output,
        _hiddenActivation,
        _outputActivation,
        _activeHiddenNeuron,
        _phase,
        _focusText);

    public async Task<MlpRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();

        for (var hidden = 0; hidden < _hiddenWeights.Rows; hidden++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _phase = MlpPhase.HiddenNeuron;
            _activeHiddenNeuron = hidden;
            var z = _hiddenBiases[hidden];
            for (var input = 0; input < _hiddenWeights.Columns; input++)
                z += _inputs[input] * _hiddenWeights[hidden, input];
            _hiddenPre[hidden] = z;
            _hiddenAct[hidden] = ActivationMath.Apply(_hiddenActivation, z);
            _focusText = $"Hidden neuron {hidden + 1}: weighted sum z = {z:0.###}, then {ActivationMath.DisplayName(_hiddenActivation)} gives a = {_hiddenAct[hidden]:0.###}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _activeHiddenNeuron = -1;
        _phase = MlpPhase.HiddenLayerComplete;
        _focusText = "The hidden layer is complete. Its activation vector becomes the input to the output neuron.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = MlpPhase.OutputNeuron;
        _outputPre = _outputBias;
        for (var hidden = 0; hidden < _hiddenAct.Dimension; hidden++)
            _outputPre += _hiddenAct[hidden] * _outputWeights[hidden];
        _output = ActivationMath.Apply(_outputActivation, _outputPre);
        _focusText = $"Output neuron: z = {_outputPre:0.###}; {ActivationMath.DisplayName(_outputActivation)} gives prediction {_output:0.###}.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = MlpPhase.Complete;
        _focusText = $"Forward pass complete. Information moved input → hidden layer → output, producing {_output:0.###}.";
        await NextStepAsync(_focusText, cancellationToken);

        return new MlpRunResult(
            _inputs.CopyValues(), _hiddenAct.Dimension, _hiddenPre.CopyValues(), _hiddenAct.CopyValues(),
            _outputPre, _output, _hiddenActivation, _outputActivation, _focusText);
    }

    private void ResetRunState()
    {
        _hiddenPre = new ManualVector(_hiddenWeights.Rows);
        _hiddenAct = new ManualVector(_hiddenWeights.Rows);
        _outputPre = 0d;
        _output = 0d;
        _activeHiddenNeuron = -1;
        _phase = MlpPhase.Ready;
        _focusText = "Ready.";
    }

    private static void Validate(MlpConfiguration configuration)
    {
        if (configuration.Inputs is null || configuration.Inputs.Length < 1 || configuration.Inputs.Length > 6)
            throw new ArgumentException("Use 1–6 inputs in the teaching MLP.", nameof(configuration));
        if (configuration.HiddenCount < 1 || configuration.HiddenCount > 6)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Use 1–6 hidden neurons.");
        if (configuration.HiddenWeights is null || configuration.HiddenWeights.Length != configuration.Inputs.Length * configuration.HiddenCount)
            throw new ArgumentException("HiddenWeights must be row-major hiddenCount × inputCount.", nameof(configuration));
        if (configuration.HiddenBiases is null || configuration.HiddenBiases.Length != configuration.HiddenCount)
            throw new ArgumentException("One hidden bias is required per hidden neuron.", nameof(configuration));
        if (configuration.OutputWeights is null || configuration.OutputWeights.Length != configuration.HiddenCount)
            throw new ArgumentException("The output neuron needs one weight per hidden activation.", nameof(configuration));
        if (!double.IsFinite(configuration.OutputBias)) throw new ArgumentException("Output bias must be finite.", nameof(configuration));
        ValidateFinite(configuration.Inputs, nameof(configuration));
        ValidateFinite(configuration.HiddenWeights, nameof(configuration));
        ValidateFinite(configuration.HiddenBiases, nameof(configuration));
        ValidateFinite(configuration.OutputWeights, nameof(configuration));
    }

    private static void ValidateFinite(double[] values, string parameterName)
    {
        for (var index = 0; index < values.Length; index++)
            if (!double.IsFinite(values[index])) throw new ArgumentException("All MLP values must be finite.", parameterName);
    }

    private static MlpConfiguration DefaultConfiguration() => new(
        [1d, -0.5d],
        2,
        [0.8d, -0.4d, -0.3d, 0.9d],
        [0.1d, -0.2d],
        [1.1d, -0.7d],
        0.05d,
        ActivationKind.ReLU,
        ActivationKind.Sigmoid);
}

using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Neuron;

/// <summary>
/// From-scratch dense neuron simulation. Inputs, weights, and weighted contributions
/// use the project's ManualVector. No ML/tensor/autodiff package is used.
/// </summary>
public sealed class NeuronSimulation : SimulationAlgorithmBase
{
    private ManualVector _inputs = new(2);
    private ManualVector _weights = new(2);
    private ManualVector _contributions = new(2);
    private double _bias;
    private ActivationKind _activation;
    private double _preActivation;
    private double _output;
    private int _activeInputIndex = -1;
    private NeuronPhase _phase = NeuronPhase.Ready;
    private string _focusText = "Ready.";

    public NeuronSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new NeuronConfiguration([1d, 2d], [0.8d, -0.4d], 0.5d, ActivationKind.ReLU));
    }

    public void Configure(NeuronConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _inputs.CopyFrom(configuration.Inputs);
        _weights.CopyFrom(configuration.Weights);
        _contributions = new ManualVector(configuration.Inputs.Length);
        _bias = configuration.Bias;
        _activation = configuration.Activation;
        ResetRunState();
    }

    public NeuronSnapshot CreateSnapshot() => new(
        _inputs.CopyValues(),
        _weights.CopyValues(),
        _contributions.CopyValues(),
        _bias,
        _activation,
        _preActivation,
        _output,
        _activeInputIndex,
        _phase,
        _focusText);

    public async Task<NeuronRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var runningSum = 0d;

        for (var index = 0; index < _inputs.Dimension; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _phase = NeuronPhase.WeightedContribution;
            _activeInputIndex = index;
            _contributions[index] = _inputs[index] * _weights[index];
            runningSum += _contributions[index];
            _focusText = $"Input {index + 1}: {_inputs[index]:0.###} × weight {_weights[index]:0.###} = {_contributions[index]:0.###}. This branch now contributes to z.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _activeInputIndex = -1;
        _phase = NeuronPhase.Summing;
        _preActivation = runningSum + _bias;
        _focusText = $"Add the weighted contributions and bias {_bias:0.###}: z = {_preActivation:0.###}. The activation has not changed z yet.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = NeuronPhase.Activating;
        _output = ActivationMath.Apply(_activation, _preActivation);
        _focusText = $"Apply {ActivationMath.DisplayName(_activation)} to z = {_preActivation:0.###}. The neuron output becomes {_output:0.###}.";
        await NextStepAsync(_focusText, cancellationToken);

        _phase = NeuronPhase.Complete;
        _focusText = $"Neuron complete: z = {_preActivation:0.###}, output = {_output:0.###}. The weighted sum and activation are two separate stages.";
        await NextStepAsync(_focusText, cancellationToken);

        return new NeuronRunResult(
            _inputs.CopyValues(), _weights.CopyValues(), _contributions.CopyValues(), _bias,
            _activation, _preActivation, _output, _focusText);
    }

    private void ResetRunState()
    {
        _contributions = new ManualVector(_inputs.Dimension);
        _preActivation = 0d;
        _output = 0d;
        _activeInputIndex = -1;
        _phase = NeuronPhase.Ready;
        _focusText = "Ready.";
    }

    private static void Validate(NeuronConfiguration configuration)
    {
        if (configuration.Inputs is null || configuration.Weights is null)
            throw new ArgumentException("Inputs and weights are required.", nameof(configuration));
        if (configuration.Inputs.Length < 1 || configuration.Inputs.Length > 8)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Use 1–8 inputs in the teaching neuron.");
        if (configuration.Inputs.Length != configuration.Weights.Length)
            throw new ArgumentException("Inputs and weights must have the same length.", nameof(configuration));
        if (!double.IsFinite(configuration.Bias))
            throw new ArgumentException("Bias must be finite.", nameof(configuration));
        for (var index = 0; index < configuration.Inputs.Length; index++)
        {
            if (!double.IsFinite(configuration.Inputs[index]) || !double.IsFinite(configuration.Weights[index]))
                throw new ArgumentException("Inputs and weights must be finite.", nameof(configuration));
        }
    }
}

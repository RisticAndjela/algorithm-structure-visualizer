using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Neuron;

public enum NeuronPhase
{
    Ready,
    WeightedContribution,
    Summing,
    Activating,
    Complete
}

public sealed record NeuronConfiguration(
    double[] Inputs,
    double[] Weights,
    double Bias,
    ActivationKind Activation);

public sealed record NeuronSnapshot(
    double[] Inputs,
    double[] Weights,
    double[] Contributions,
    double Bias,
    ActivationKind Activation,
    double PreActivation,
    double Output,
    int ActiveInputIndex,
    NeuronPhase Phase,
    string FocusText);

public sealed record NeuronRunResult(
    double[] Inputs,
    double[] Weights,
    double[] Contributions,
    double Bias,
    ActivationKind Activation,
    double PreActivation,
    double Output,
    string Summary)
{
    public int InputCount => Inputs.Length;
    public string TimeComplexity => "Θ(n)";
    public string MemoryComplexity => "O(n) teaching state";
}

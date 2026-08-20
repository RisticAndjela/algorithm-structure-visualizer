using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Mlp;

public enum MlpPhase
{
    Ready,
    HiddenNeuron,
    HiddenLayerComplete,
    OutputNeuron,
    Complete
}

public sealed record MlpConfiguration(
    double[] Inputs,
    int HiddenCount,
    double[] HiddenWeights,
    double[] HiddenBiases,
    double[] OutputWeights,
    double OutputBias,
    ActivationKind HiddenActivation,
    ActivationKind OutputActivation);

public sealed record MlpSnapshot(
    double[] Inputs,
    int HiddenCount,
    double[] HiddenWeights,
    double[] HiddenBiases,
    double[] HiddenPreActivations,
    double[] HiddenActivations,
    double[] OutputWeights,
    double OutputBias,
    double OutputPreActivation,
    double Output,
    ActivationKind HiddenActivation,
    ActivationKind OutputActivation,
    int ActiveHiddenNeuron,
    MlpPhase Phase,
    string FocusText);

public sealed record MlpRunResult(
    double[] Inputs,
    int HiddenCount,
    double[] HiddenPreActivations,
    double[] HiddenActivations,
    double OutputPreActivation,
    double Output,
    ActivationKind HiddenActivation,
    ActivationKind OutputActivation,
    string Summary)
{
    public int InputCount => Inputs.Length;
    public int ConnectionCount => (InputCount * HiddenCount) + HiddenCount;
    public string TimeComplexity => "Θ(inputs × hidden)";
    public string MemoryComplexity => "O(inputs × hidden) weights";
}

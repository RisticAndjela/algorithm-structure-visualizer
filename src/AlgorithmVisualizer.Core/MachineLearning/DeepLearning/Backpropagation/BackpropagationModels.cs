using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Backpropagation;

public enum BackpropagationPhase
{
    Ready,
    ForwardHidden,
    ForwardOutput,
    Loss,
    OutputGradient,
    HiddenGradient,
    WeightGradients,
    Update,
    Complete
}

public sealed record BackpropagationConfiguration(
    double[] Inputs,
    int HiddenCount,
    double[] HiddenWeights,
    double[] HiddenBiases,
    double[] OutputWeights,
    double OutputBias,
    ActivationKind HiddenActivation,
    ActivationKind OutputActivation,
    double Target,
    double LearningRate);

public sealed record BackpropagationSnapshot(
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
    double Target,
    double Loss,
    double OutputDelta,
    double[] HiddenDeltas,
    double[] OutputWeightGradients,
    double OutputBiasGradient,
    double[] HiddenWeightGradients,
    double[] HiddenBiasGradients,
    int ActiveHiddenNeuron,
    BackpropagationPhase Phase,
    string FocusText);

public sealed record BackpropagationRunResult(
    double PredictionBefore,
    double PredictionAfter,
    double LossBefore,
    double LossAfter,
    double OutputDelta,
    double[] HiddenDeltas,
    double[] HiddenWeightGradients,
    double[] OutputWeightGradients,
    string Summary)
{
    public bool Improved => LossAfter <= LossBefore + 1e-12d;
    public string TimeComplexity => "Θ(weights)";
    public string MemoryComplexity => "O(weights) gradients";
}

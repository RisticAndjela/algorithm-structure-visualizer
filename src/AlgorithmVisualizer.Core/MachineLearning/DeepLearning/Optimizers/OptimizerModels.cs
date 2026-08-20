namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Optimizers;

public enum OptimizerKind
{
    Sgd,
    Momentum,
    Adam
}

public enum OptimizerPhase
{
    Ready,
    Sample,
    Gradient,
    StateUpdate,
    ParameterUpdate,
    Complete
}

public sealed record OptimizerConfiguration(
    double[] X,
    double[] Y,
    double InitialWeight,
    double InitialBias,
    double LearningRate,
    int Steps,
    OptimizerKind Kind,
    double MomentumBeta = 0.9d,
    double AdamBeta1 = 0.9d,
    double AdamBeta2 = 0.999d,
    double AdamEpsilon = 1e-8d);

public sealed record OptimizerSnapshot(
    OptimizerKind Kind,
    double Weight,
    double Bias,
    double GradientWeight,
    double GradientBias,
    double FirstMomentWeight,
    double FirstMomentBias,
    double SecondMomentWeight,
    double SecondMomentBias,
    double CurrentX,
    double CurrentY,
    double Prediction,
    double SampleLoss,
    double DatasetMse,
    int StepIndex,
    int SampleIndex,
    double[] WeightPath,
    double[] BiasPath,
    OptimizerPhase Phase,
    string FocusText);

public sealed record OptimizerRunResult(
    OptimizerKind Kind,
    double InitialMse,
    double FinalMse,
    double FinalWeight,
    double FinalBias,
    double[] WeightPath,
    double[] BiasPath,
    int Steps,
    string Summary)
{
    public bool Improved => FinalMse <= InitialMse + 1e-12d;
    public string TimeComplexity => "Θ(steps) stochastic updates";
    public string MemoryComplexity => Kind == OptimizerKind.Adam ? "O(parameters) first + second moments" : Kind == OptimizerKind.Momentum ? "O(parameters) velocity" : "O(1) optimizer state";
}

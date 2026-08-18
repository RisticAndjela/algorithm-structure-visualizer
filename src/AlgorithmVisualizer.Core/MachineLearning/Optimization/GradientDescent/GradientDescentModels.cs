namespace AlgorithmVisualizer.Core.MachineLearning.Optimization.GradientDescent;

public enum GradientDescentVariant
{
    FixedLearningRate,
    LearningRateDecay
}

public enum GradientDescentPhase
{
    Ready,
    EvaluatingLoss,
    ComputingGradient,
    ScalingGradient,
    UpdatingParameters,
    Complete,
    Diverged
}

public enum GradientDescentStopReason
{
    None,
    GradientTolerance,
    MaximumIterations,
    Diverged
}

public sealed record GradientDescentConfiguration(
    double[] InitialParameters,
    double[] Target,
    double[] Curvature,
    double LearningRate,
    int MaximumIterations,
    double GradientTolerance,
    double DecayRate);

public sealed record GradientDescentHistoryPoint(
    int Iteration,
    double Loss,
    double LearningRate,
    double GradientNorm,
    double[] Parameters);

public sealed record GradientDescentSnapshot(
    double[] Parameters,
    double[] Target,
    double[] Curvature,
    double[] Gradient,
    double[] ScaledGradient,
    double[] NextParameters,
    GradientDescentVariant Variant,
    GradientDescentPhase Phase,
    int Iteration,
    int CurrentIndex,
    double Loss,
    double PreviousLoss,
    double EffectiveLearningRate,
    double GradientNorm,
    string FocusText,
    GradientDescentHistoryPoint[] History)
{
    public int Dimension => Parameters.Length;
}

public sealed record GradientDescentRunResult(
    GradientDescentVariant Variant,
    bool Converged,
    bool Diverged,
    GradientDescentStopReason StopReason,
    double[] InitialParameters,
    double[] FinalParameters,
    double[] Target,
    double[] Curvature,
    double InitialLoss,
    double FinalLoss,
    double InitialLearningRate,
    double FinalLearningRate,
    int IterationsCompleted,
    double FinalGradientNorm,
    GradientDescentHistoryPoint[] History,
    string Summary)
{
    public bool Succeeded => !Diverged;
    public string TimeComplexity => "Θ(k · n)";
    public string WorkingSpaceComplexity => "O(n)";
    public string ReviewHistorySpaceComplexity => "O(k · n)";
}

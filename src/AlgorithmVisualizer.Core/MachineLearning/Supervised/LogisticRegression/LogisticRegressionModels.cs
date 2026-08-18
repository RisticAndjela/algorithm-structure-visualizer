namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.LogisticRegression;

public enum LogisticRegressionPhase
{
    Ready,
    ForwardPass,
    ComputingGradient,
    UpdatingModel,
    Complete,
    Diverged
}

public enum LogisticRegressionStopReason
{
    None,
    GradientTolerance,
    MaximumIterations,
    Diverged
}

public sealed record LogisticRegressionConfiguration(
    double[] X,
    double[] Labels,
    double InitialWeight,
    double InitialBias,
    double LearningRate,
    int MaximumIterations,
    double GradientTolerance);

public sealed record LogisticRegressionHistoryPoint(
    int Iteration,
    double Weight,
    double Bias,
    double Loss,
    double GradientNorm,
    double Accuracy);

public sealed record LogisticRegressionSnapshot(
    double[] X,
    double[] Labels,
    double[] Scores,
    double[] Probabilities,
    double[] PredictionErrors,
    int[] PredictedClasses,
    double Weight,
    double Bias,
    double NextWeight,
    double NextBias,
    double WeightGradient,
    double BiasGradient,
    double Loss,
    double PreviousLoss,
    double GradientNorm,
    double Accuracy,
    double LearningRate,
    int Iteration,
    LogisticRegressionPhase Phase,
    string FocusText,
    LogisticRegressionHistoryPoint[] History)
{
    public int Count => X.Length;
}

public sealed record LogisticRegressionRunResult(
    bool Converged,
    bool Diverged,
    LogisticRegressionStopReason StopReason,
    double InitialWeight,
    double InitialBias,
    double FinalWeight,
    double FinalBias,
    double InitialLoss,
    double FinalLoss,
    double InitialAccuracy,
    double FinalAccuracy,
    double LearningRate,
    int IterationsCompleted,
    double FinalGradientNorm,
    double[] X,
    double[] Labels,
    double[] FinalScores,
    double[] FinalProbabilities,
    int[] FinalPredictedClasses,
    LogisticRegressionHistoryPoint[] History,
    string Summary)
{
    public string TrainingTimeComplexity => "Θ(k · n)";
    public string PredictionTimeComplexity => "Θ(n)";
    public string WorkingSpaceComplexity => "O(n)";
    public string ReviewHistorySpaceComplexity => "O(k)";
}

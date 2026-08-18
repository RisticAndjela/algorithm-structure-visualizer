namespace AlgorithmVisualizer.Core.MachineLearning.Supervised.LinearRegression;

public enum LinearRegressionPhase
{
    Ready,
    Predicting,
    MeasuringError,
    ComputingGradient,
    UpdatingModel,
    Complete,
    Diverged
}

public enum LinearRegressionStopReason
{
    None,
    GradientTolerance,
    MaximumIterations,
    Diverged
}

public sealed record LinearRegressionConfiguration(
    double[] X,
    double[] Y,
    double InitialWeight,
    double InitialBias,
    double LearningRate,
    int MaximumIterations,
    double GradientTolerance);

public sealed record LinearRegressionHistoryPoint(
    int Iteration,
    double Weight,
    double Bias,
    double Loss,
    double GradientNorm);

public sealed record LinearRegressionSnapshot(
    double[] X,
    double[] Y,
    double[] Predictions,
    double[] Residuals,
    double Weight,
    double Bias,
    double NextWeight,
    double NextBias,
    double WeightGradient,
    double BiasGradient,
    double Loss,
    double PreviousLoss,
    double GradientNorm,
    double LearningRate,
    int Iteration,
    int CurrentPointIndex,
    LinearRegressionPhase Phase,
    string FocusText,
    LinearRegressionHistoryPoint[] History)
{
    public int Count => X.Length;
}

public sealed record LinearRegressionRunResult(
    bool Converged,
    bool Diverged,
    LinearRegressionStopReason StopReason,
    double InitialWeight,
    double InitialBias,
    double FinalWeight,
    double FinalBias,
    double InitialLoss,
    double FinalLoss,
    double LearningRate,
    int IterationsCompleted,
    double FinalGradientNorm,
    double[] X,
    double[] Y,
    double[] FinalPredictions,
    double[] FinalResiduals,
    LinearRegressionHistoryPoint[] History,
    string Summary)
{
    public string TrainingTimeComplexity => "Θ(k · n)";
    public string PredictionTimeComplexity => "Θ(n)";
    public string WorkingSpaceComplexity => "O(n)";
    public string ReviewHistorySpaceComplexity => "O(k)";
}

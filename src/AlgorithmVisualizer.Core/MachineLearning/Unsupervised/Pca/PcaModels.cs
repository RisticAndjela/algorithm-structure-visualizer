namespace AlgorithmVisualizer.Core.MachineLearning.Unsupervised.Pca;

public enum PcaPhase
{
    Ready,
    Centering,
    Covariance,
    Direction,
    Projection,
    Complete
}

public sealed record PcaConfiguration(
    double[][] Features,
    int PowerIterations = 12,
    double DirectionTolerance = 0.00001d);

public sealed record PcaSnapshot(
    double[][] Features,
    double[][] CenteredFeatures,
    double[][] ProjectedFeatures,
    double[] Mean,
    double[][] Covariance,
    double[] PrincipalComponent,
    double[] Projections,
    PcaPhase Phase,
    int CurrentPointIndex,
    int DirectionIteration,
    double Eigenvalue,
    double ExplainedVarianceRatio,
    string FocusText)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
}

public sealed record PcaRunResult(
    double[][] Features,
    double[][] CenteredFeatures,
    double[][] ProjectedFeatures,
    double[] Mean,
    double[][] Covariance,
    double[] PrincipalComponent,
    double[] Projections,
    int DirectionIterations,
    double Eigenvalue,
    double ExplainedVarianceRatio,
    string Summary)
{
    public int Count => Features.Length;
    public int Dimension => Features.Length == 0 ? 0 : Features[0].Length;
    public string CenteringComplexity => "O(n·d)";
    public string CovarianceComplexity => "O(n·d²)";
    public string DirectionComplexity => "O(p·d²)";
    public string ProjectionComplexity => "O(n·d)";
}

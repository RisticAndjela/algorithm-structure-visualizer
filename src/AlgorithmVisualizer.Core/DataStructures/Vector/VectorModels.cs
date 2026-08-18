namespace AlgorithmVisualizer.Core.DataStructures.Vector;

public enum VectorOperationKind
{
    Add,
    Subtract,
    ScalarMultiply,
    Hadamard,
    DotProduct,
    L1Norm,
    L2Norm,
    NormalizeL2,
    EuclideanDistance,
    ManhattanDistance,
    CosineSimilarity
}

public enum VectorComponentVisualState
{
    Normal,
    Current,
    Read,
    Written,
    Contributing
}

public enum VectorPhase
{
    Ready,
    Reading,
    Reducing,
    Writing,
    Complete,
    Invalid
}

public sealed record VectorSnapshot(
    double[] A,
    double[] B,
    double[] Result,
    VectorComponentVisualState[] AStates,
    VectorComponentVisualState[] BStates,
    VectorComponentVisualState[] ResultStates,
    VectorOperationKind Operation,
    VectorPhase Phase,
    int CurrentIndex,
    double RunningValue,
    double? ScalarResult,
    double? NormA,
    double? NormB,
    string FocusText)
{
    public int DimensionA => A.Length;
    public int DimensionB => B.Length;
    public int ResultDimension => Result.Length;
}

public sealed record VectorOperationResult(
    VectorOperationKind Operation,
    bool Succeeded,
    double[] A,
    double[] B,
    double[] ResultVector,
    double? ScalarResult,
    double Scalar,
    int ComponentsVisited,
    int ArithmeticOperations,
    string Summary,
    string FailureReason)
{
    public bool ProducesVector => ResultVector.Length > 0;
    public bool ProducesScalar => ScalarResult.HasValue;
    public string TimeComplexity => "Θ(n)";
    public string InputSpaceComplexity => "O(n)";
    public string ReductionExtraSpaceComplexity => "O(1)";
    public string VectorResultExtraSpaceComplexity => ProducesVector ? "O(n) result" : "O(1)";
}

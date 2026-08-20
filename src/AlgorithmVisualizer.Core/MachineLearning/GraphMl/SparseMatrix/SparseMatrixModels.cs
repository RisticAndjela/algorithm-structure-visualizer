namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.SparseMatrix;

public enum SparseMatrixPhase
{
    Ready,
    Scanning,
    RowClosed,
    Multiplying,
    Complete
}

public sealed record SparseMatrixConfiguration(
    double[][] Dense,
    double[] MultiplyVector,
    double ZeroTolerance = 1e-12);

public sealed record SparseMatrixSnapshot(
    int Rows,
    int Columns,
    double[] DenseValues,
    double[] Values,
    int[] ColumnIndexes,
    int[] RowPointers,
    double[] MultiplyVector,
    double[] Product,
    SparseMatrixPhase Phase,
    int CurrentRow,
    int CurrentColumn,
    int CurrentNonZeroIndex,
    string FocusText)
{
    public int NonZeroCount => Values.Length;
    public int DenseCellCount => Rows * Columns;
}

public sealed record SparseMatrixRunResult(
    int Rows,
    int Columns,
    double[] Values,
    int[] ColumnIndexes,
    int[] RowPointers,
    double[] Product,
    int NonZeroCount,
    int DenseCellCount,
    int CsrStoredSlots,
    string Summary)
{
    public double Density => DenseCellCount == 0 ? 0d : (double)NonZeroCount / DenseCellCount;
    public string ConversionComplexity => "O(rows·columns)";
    public string MultiplyComplexity => "O(nnz)";
    public string SpaceComplexity => "O(nnz + rows)";
}

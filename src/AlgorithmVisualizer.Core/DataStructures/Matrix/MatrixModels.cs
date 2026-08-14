namespace AlgorithmVisualizer.Core.DataStructures.Matrix;

public enum MatrixSlot
{
    A,
    B,
    Result
}

public enum MatrixCellVisualState
{
    Normal,
    Reading,
    Writing,
    Pivot,
    Candidate,
    Matched,
    Eliminated
}

public enum MatrixOperationKind
{
    Add,
    Subtract,
    Hadamard,
    Multiply,
    ScalarMultiply,
    Transpose,
    Trace,
    Determinant,
    Inverse,
    Ref,
    Rref,
    Rank,
    Power,
    Minor,
    Cofactor,
    Solve,
    SwapRows,
    ScaleRow,
    AddScaledRow
}

public enum MatrixPreset
{
    Zero,
    Identity,
    Sequence,
    Diagonal,
    Symmetric,
    GraphAdjacency,
    RandomSmall
}

public sealed record MatrixGridSnapshot(
    int Rows,
    int Columns,
    double[] Values,
    MatrixCellVisualState[] States)
{
    public double GetValue(int row, int column) => Values[(row * Columns) + column];
    public MatrixCellVisualState GetState(int row, int column) => States[(row * Columns) + column];
}

public sealed record MatrixWorkspaceSnapshot(
    MatrixGridSnapshot A,
    MatrixGridSnapshot B,
    MatrixGridSnapshot Result,
    string ResultLabel,
    string FocusText);

public sealed record MatrixOperationResult(
    MatrixOperationKind Operation,
    bool Succeeded,
    string Summary,
    string Complexity,
    int ArithmeticOperations,
    int RowOperations,
    double? ScalarResult = null,
    int? IntegerResult = null);

public sealed record MatrixProperties(
    bool IsSquare,
    bool IsZero,
    bool IsIdentity,
    bool IsDiagonal,
    bool IsUpperTriangular,
    bool IsLowerTriangular,
    bool IsSymmetric,
    double? Trace);

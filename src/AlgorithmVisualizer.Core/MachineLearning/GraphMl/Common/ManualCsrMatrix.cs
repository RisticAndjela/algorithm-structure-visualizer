using AlgorithmVisualizer.Core.DataStructures.Vector;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;

/// <summary>
/// Minimal compressed-sparse-row storage owned by this project.
/// Values, column indexes, and row pointers are stored in plain arrays;
/// no sparse/numerical framework type is used.
/// </summary>
public sealed class ManualCsrMatrix
{
    private readonly double[] _values;
    private readonly int[] _columnIndexes;
    private readonly int[] _rowPointers;

    public ManualCsrMatrix(int rows, int columns, double[] values, int[] columnIndexes, int[] rowPointers)
    {
        if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows));
        if (columns < 1) throw new ArgumentOutOfRangeException(nameof(columns));
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(columnIndexes);
        ArgumentNullException.ThrowIfNull(rowPointers);
        if (values.Length != columnIndexes.Length) throw new ArgumentException("CSR values and column-index arrays must have the same length.");
        if (rowPointers.Length != rows + 1) throw new ArgumentException("CSR rowPointers must contain rows + 1 entries.", nameof(rowPointers));
        if (rowPointers[0] != 0 || rowPointers[^1] != values.Length) throw new ArgumentException("CSR rowPointers must start at 0 and end at nnz.", nameof(rowPointers));
        for (var row = 0; row < rows; row++)
        {
            if (rowPointers[row] > rowPointers[row + 1]) throw new ArgumentException("CSR row pointers must be nondecreasing.", nameof(rowPointers));
        }
        for (var index = 0; index < columnIndexes.Length; index++)
        {
            if (columnIndexes[index] < 0 || columnIndexes[index] >= columns) throw new ArgumentOutOfRangeException(nameof(columnIndexes), "CSR column index is outside the matrix.");
            if (!double.IsFinite(values[index])) throw new ArgumentException("CSR values must be finite.", nameof(values));
        }
        for (var row = 0; row < rows; row++)
        {
            for (var index = rowPointers[row] + 1; index < rowPointers[row + 1]; index++)
            {
                if (columnIndexes[index - 1] >= columnIndexes[index]) throw new ArgumentException("CSR column indexes must be strictly increasing inside each row.", nameof(columnIndexes));
            }
        }

        Rows = rows;
        Columns = columns;
        _values = Copy(values);
        _columnIndexes = Copy(columnIndexes);
        _rowPointers = Copy(rowPointers);
    }

    public int Rows { get; }
    public int Columns { get; }
    public int NonZeroCount => _values.Length;

    public int RowStart(int row)
    {
        ValidateRow(row);
        return _rowPointers[row];
    }

    public int RowEnd(int row)
    {
        ValidateRow(row);
        return _rowPointers[row + 1];
    }

    public int ColumnAt(int nonZeroIndex)
    {
        ValidateNonZeroIndex(nonZeroIndex);
        return _columnIndexes[nonZeroIndex];
    }

    public double ValueAt(int nonZeroIndex)
    {
        ValidateNonZeroIndex(nonZeroIndex);
        return _values[nonZeroIndex];
    }

    public double Get(int row, int column)
    {
        ValidateRow(row);
        if (column < 0 || column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));
        for (var index = _rowPointers[row]; index < _rowPointers[row + 1]; index++)
        {
            if (_columnIndexes[index] == column) return _values[index];
            if (_columnIndexes[index] > column) break;
        }
        return 0d;
    }

    public ManualVector Multiply(ManualVector vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        if (vector.Dimension != Columns) throw new ArgumentException("Vector dimension must match the sparse matrix column count.", nameof(vector));
        var result = new ManualVector(Rows);
        for (var row = 0; row < Rows; row++)
        {
            var sum = 0d;
            for (var index = _rowPointers[row]; index < _rowPointers[row + 1]; index++)
            {
                sum += _values[index] * vector[_columnIndexes[index]];
            }
            result[row] = sum;
        }
        return result;
    }

    public double[] CopyValues() => Copy(_values);
    public int[] CopyColumnIndexes() => Copy(_columnIndexes);
    public int[] CopyRowPointers() => Copy(_rowPointers);

    public static ManualCsrMatrix FromDense(double[][] dense, double zeroTolerance = 1e-12)
    {
        ValidateDense(dense);
        if (zeroTolerance < 0d || !double.IsFinite(zeroTolerance)) throw new ArgumentOutOfRangeException(nameof(zeroTolerance));
        var rows = dense.Length;
        var columns = dense[0].Length;
        var count = 0;
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
                if (Math.Abs(dense[row][column]) > zeroTolerance) count++;

        var values = new double[count];
        var columnsByValue = new int[count];
        var rowPointers = new int[rows + 1];
        var write = 0;
        for (var row = 0; row < rows; row++)
        {
            rowPointers[row] = write;
            for (var column = 0; column < columns; column++)
            {
                var value = dense[row][column];
                if (Math.Abs(value) <= zeroTolerance) continue;
                values[write] = value;
                columnsByValue[write] = column;
                write++;
            }
        }
        rowPointers[rows] = write;
        return new ManualCsrMatrix(rows, columns, values, columnsByValue, rowPointers);
    }

    private static void ValidateDense(double[][] dense)
    {
        ArgumentNullException.ThrowIfNull(dense);
        if (dense.Length < 1) throw new ArgumentException("Dense source needs at least one row.", nameof(dense));
        if (dense[0] is null || dense[0].Length < 1) throw new ArgumentException("Dense source needs at least one column.", nameof(dense));
        var columns = dense[0].Length;
        for (var row = 0; row < dense.Length; row++)
        {
            if (dense[row] is null || dense[row].Length != columns) throw new ArgumentException("Dense source must be rectangular.", nameof(dense));
            for (var column = 0; column < columns; column++)
                if (!double.IsFinite(dense[row][column])) throw new ArgumentException("Dense source values must be finite.", nameof(dense));
        }
    }

    private void ValidateRow(int row)
    {
        if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
    }

    private void ValidateNonZeroIndex(int index)
    {
        if (index < 0 || index >= _values.Length) throw new ArgumentOutOfRangeException(nameof(index));
    }

    private static double[] Copy(double[] source)
    {
        var copy = new double[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static int[] Copy(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }
}

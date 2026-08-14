namespace AlgorithmVisualizer.Core.DataStructures.Matrix;

/// <summary>
/// Matrix storage implemented directly over a row-major double[]. No List, multidimensional
/// framework matrix type, or numerical library is used for the taught storage/algorithms.
/// </summary>
public sealed class ManualMatrix
{
    private double[] _values;

    public ManualMatrix(int rows, int columns)
    {
        ValidateDimensions(rows, columns);
        Rows = rows;
        Columns = columns;
        _values = new double[checked(rows * columns)];
    }

    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public int Count => _values.Length;

    public double this[int row, int column]
    {
        get => _values[GetIndex(row, column)];
        set => _values[GetIndex(row, column)] = value;
    }

    public int GetFlatIndex(int row, int column) => GetIndex(row, column);

    public void Resize(int rows, int columns, bool preserve = true)
    {
        ValidateDimensions(rows, columns);
        if (rows == Rows && columns == Columns)
        {
            return;
        }

        var replacement = new double[checked(rows * columns)];
        if (preserve)
        {
            var copyRows = Math.Min(rows, Rows);
            var copyColumns = Math.Min(columns, Columns);
            for (var row = 0; row < copyRows; row++)
            {
                for (var column = 0; column < copyColumns; column++)
                {
                    replacement[(row * columns) + column] = _values[(row * Columns) + column];
                }
            }
        }

        Rows = rows;
        Columns = columns;
        _values = replacement;
    }

    public void Clear()
    {
        for (var index = 0; index < _values.Length; index++)
        {
            _values[index] = 0d;
        }
    }

    public ManualMatrix Clone()
    {
        var clone = new ManualMatrix(Rows, Columns);
        for (var index = 0; index < _values.Length; index++)
        {
            clone._values[index] = _values[index];
        }
        return clone;
    }

    public void CopyFrom(ManualMatrix source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Resize(source.Rows, source.Columns, preserve: false);
        for (var index = 0; index < source._values.Length; index++)
        {
            _values[index] = source._values[index];
        }
    }

    public void SwapRows(int firstRow, int secondRow)
    {
        ValidateRow(firstRow);
        ValidateRow(secondRow);
        if (firstRow == secondRow)
        {
            return;
        }

        for (var column = 0; column < Columns; column++)
        {
            var firstIndex = GetIndex(firstRow, column);
            var secondIndex = GetIndex(secondRow, column);
            (_values[firstIndex], _values[secondIndex]) = (_values[secondIndex], _values[firstIndex]);
        }
    }

    public void ScaleRow(int row, double factor)
    {
        ValidateRow(row);
        for (var column = 0; column < Columns; column++)
        {
            this[row, column] *= factor;
        }
    }

    public void AddScaledRow(int targetRow, int sourceRow, double factor)
    {
        ValidateRow(targetRow);
        ValidateRow(sourceRow);
        for (var column = 0; column < Columns; column++)
        {
            this[targetRow, column] += factor * this[sourceRow, column];
        }
    }

    public double[] CopyRawValues()
    {
        var copy = new double[_values.Length];
        for (var index = 0; index < _values.Length; index++)
        {
            copy[index] = _values[index];
        }
        return copy;
    }

    private int GetIndex(int row, int column)
    {
        if (row < 0 || row >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }
        if (column < 0 || column >= Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column));
        }
        return checked((row * Columns) + column);
    }

    private void ValidateRow(int row)
    {
        if (row < 0 || row >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }
    }

    private static void ValidateDimensions(int rows, int columns)
    {
        if (rows < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be at least 1.");
        }
        if (columns < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be at least 1.");
        }

        // The reusable Core storage has no 8×8 teaching cap. Individual learning
        // pages may impose smaller UI limits for readability (Matrix currently does).
        _ = checked(rows * columns);
    }
}

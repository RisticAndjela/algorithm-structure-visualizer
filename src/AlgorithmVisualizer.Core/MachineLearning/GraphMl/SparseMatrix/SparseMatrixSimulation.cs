using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.SparseMatrix;

/// <summary>
/// Teaches dense -> CSR conversion and sparse matrix-vector multiplication.
/// All storage is project-owned arrays/ManualVector; no sparse matrix package is used.
/// </summary>
public sealed class SparseMatrixSimulation : SimulationAlgorithmBase
{
    private double[][] _dense = [];
    private ManualVector _multiplyVector = new(1);
    private double _zeroTolerance = 1e-12;
    private double[] _values = [];
    private int[] _columnIndexes = [];
    private int[] _rowPointers = [];
    private double[] _product = [];
    private SparseMatrixPhase _phase = SparseMatrixPhase.Ready;
    private int _currentRow = -1;
    private int _currentColumn = -1;
    private int _currentNonZeroIndex = -1;
    private string _focusText = "Ready.";

    public SparseMatrixSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new SparseMatrixConfiguration(
            [
                [0d, 1d, 0d, 0d, 2d],
                [0d, 0d, 0d, 3d, 0d],
                [4d, 0d, 0d, 0d, 0d],
                [0d, 0d, 5d, 0d, 0d],
                [0d, 6d, 0d, 0d, 0d]
            ],
            [1d, 2d, 3d, 4d, 5d]));
    }

    public void Configure(SparseMatrixConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _dense = Copy(configuration.Dense);
        _multiplyVector = new ManualVector(configuration.MultiplyVector.Length);
        _multiplyVector.CopyFrom(configuration.MultiplyVector);
        _zeroTolerance = configuration.ZeroTolerance;
        ResetRunState();
    }

    public SparseMatrixSnapshot CreateSnapshot() => new(
        _dense.Length,
        _dense[0].Length,
        Flatten(_dense),
        Copy(_values),
        Copy(_columnIndexes),
        Copy(_rowPointers),
        _multiplyVector.CopyValues(),
        Copy(_product),
        _phase,
        _currentRow,
        _currentColumn,
        _currentNonZeroIndex,
        _focusText);

    public async Task<SparseMatrixRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var rows = _dense.Length;
        var columns = _dense[0].Length;
        var maxNonZeros = rows * columns;
        var valuesBuffer = new double[maxNonZeros];
        var columnBuffer = new int[maxNonZeros];
        var rowPointers = new int[rows + 1];
        var count = 0;

        for (var row = 0; row < rows; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _phase = SparseMatrixPhase.Scanning;
            _currentRow = row;
            _currentColumn = -1;
            rowPointers[row] = count;
            _rowPointers = CopyPrefix(rowPointers, row + 1);
            _focusText = $"Scan row {row}. Zeros cost a dense cell but do not enter CSR; only non-zero values are appended.";
            await NextStepAsync(_focusText, cancellationToken);

            for (var column = 0; column < columns; column++)
            {
                _currentColumn = column;
                var value = _dense[row][column];
                if (Math.Abs(value) <= _zeroTolerance) continue;
                valuesBuffer[count] = value;
                columnBuffer[count] = column;
                _currentNonZeroIndex = count;
                count++;
                _values = CopyPrefix(valuesBuffer, count);
                _columnIndexes = CopyPrefix(columnBuffer, count);
                _focusText = $"Keep A[{row},{column}] = {Format(value)} as values[{count - 1}] and remember column {column}.";
                await NextStepAsync(_focusText, cancellationToken);
            }

            rowPointers[row + 1] = count;
            _rowPointers = CopyPrefix(rowPointers, row + 2);
            _phase = SparseMatrixPhase.RowClosed;
            _focusText = $"Close row {row}: rowPointers[{row + 1}] = {count}. Its CSR slice is [{rowPointers[row]}, {count}).";
            await NextStepAsync(_focusText, cancellationToken);
        }

        var values = CopyPrefix(valuesBuffer, count);
        var columnsByValue = CopyPrefix(columnBuffer, count);
        var csr = new ManualCsrMatrix(rows, columns, values, columnsByValue, rowPointers);
        _values = csr.CopyValues();
        _columnIndexes = csr.CopyColumnIndexes();
        _rowPointers = csr.CopyRowPointers();
        _product = new double[rows];

        _phase = SparseMatrixPhase.Multiplying;
        for (var row = 0; row < rows; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentRow = row;
            _currentColumn = -1;
            var sum = 0d;
            for (var index = csr.RowStart(row); index < csr.RowEnd(row); index++)
            {
                _currentNonZeroIndex = index;
                var column = csr.ColumnAt(index);
                _currentColumn = column;
                sum += csr.ValueAt(index) * _multiplyVector[column];
            }
            _product[row] = sum;
            _focusText = $"Sparse multiply row {row}: visit only {csr.RowEnd(row) - csr.RowStart(row)} stored entr{(csr.RowEnd(row) - csr.RowStart(row) == 1 ? "y" : "ies")} and write y[{row}] = {Format(sum)}.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = SparseMatrixPhase.Complete;
        _currentRow = _currentColumn = _currentNonZeroIndex = -1;
        var denseCells = rows * columns;
        var slots = count + count + rowPointers.Length;
        _focusText = $"CSR complete: {count} non-zero values instead of {denseCells} dense numeric cells. SpMV visited exactly {count} stored values.";
        await NextStepAsync(_focusText, cancellationToken);

        return new SparseMatrixRunResult(rows, columns, csr.CopyValues(), csr.CopyColumnIndexes(), csr.CopyRowPointers(), Copy(_product), count, denseCells, slots, _focusText);
    }

    private void ResetRunState()
    {
        _values = [];
        _columnIndexes = [];
        _rowPointers = [0];
        _product = new double[_dense.Length];
        _phase = SparseMatrixPhase.Ready;
        _currentRow = _currentColumn = _currentNonZeroIndex = -1;
        _focusText = "Ready.";
    }

    private static void Validate(SparseMatrixConfiguration configuration)
    {
        if (configuration.Dense is null || configuration.Dense.Length < 1 || configuration.Dense.Length > 10) throw new ArgumentException("Use 1–10 matrix rows.", nameof(configuration));
        if (configuration.Dense[0] is null || configuration.Dense[0].Length < 1 || configuration.Dense[0].Length > 10) throw new ArgumentException("Use 1–10 matrix columns.", nameof(configuration));
        var columns = configuration.Dense[0].Length;
        for (var row = 0; row < configuration.Dense.Length; row++)
        {
            if (configuration.Dense[row] is null || configuration.Dense[row].Length != columns) throw new ArgumentException("Dense matrix must be rectangular.", nameof(configuration));
            for (var column = 0; column < columns; column++) if (!double.IsFinite(configuration.Dense[row][column])) throw new ArgumentException("Matrix values must be finite.", nameof(configuration));
        }
        if (configuration.MultiplyVector is null || configuration.MultiplyVector.Length != columns) throw new ArgumentException("Multiply vector dimension must match the matrix column count.", nameof(configuration));
        for (var index = 0; index < configuration.MultiplyVector.Length; index++) if (!double.IsFinite(configuration.MultiplyVector[index])) throw new ArgumentException("Multiply vector values must be finite.", nameof(configuration));
        if (!double.IsFinite(configuration.ZeroTolerance) || configuration.ZeroTolerance < 0d) throw new ArgumentOutOfRangeException(nameof(configuration.ZeroTolerance));
    }

    private static double[][] Copy(double[][] source)
    {
        var copy = new double[source.Length][];
        for (var row = 0; row < source.Length; row++) copy[row] = Copy(source[row]);
        return copy;
    }

    private static double[] Flatten(double[][] source)
    {
        var columns = source[0].Length;
        var result = new double[source.Length * columns];
        for (var row = 0; row < source.Length; row++)
            for (var column = 0; column < columns; column++)
                result[(row * columns) + column] = source[row][column];
        return result;
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

    private static double[] CopyPrefix(double[] source, int count)
    {
        var copy = new double[count];
        for (var index = 0; index < count; index++) copy[index] = source[index];
        return copy;
    }

    private static int[] CopyPrefix(int[] source, int count)
    {
        var copy = new int[count];
        for (var index = 0; index < count; index++) copy[index] = source[index];
        return copy;
    }

    private static string Format(double value) => Math.Abs(value) < 1e-12 ? "0" : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}

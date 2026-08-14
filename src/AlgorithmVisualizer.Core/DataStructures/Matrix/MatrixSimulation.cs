using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Matrix;

/// <summary>
/// Comprehensive matrix learning workspace. All matrix storage and core algorithms are implemented
/// from scratch over ManualMatrix / double[]. Numerical operations use elementary loops and row
/// operations rather than a matrix or linear-algebra library.
/// </summary>
public sealed class MatrixSimulation : SimulationAlgorithmBase
{
    private const double Epsilon = 1e-9;
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly ManualMatrix _a = new(3, 3);
    private readonly ManualMatrix _b = new(3, 3);
    private readonly ManualMatrix _result = new(3, 3);
    private MatrixCellVisualState[] _aStates = new MatrixCellVisualState[9];
    private MatrixCellVisualState[] _bStates = new MatrixCellVisualState[9];
    private MatrixCellVisualState[] _resultStates = new MatrixCellVisualState[9];

    public MatrixSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
        LoadPreset(MatrixSlot.A, MatrixPreset.Sequence);
        LoadPreset(MatrixSlot.B, MatrixPreset.Identity);
        _result.Clear();
        ResultLabel = "Result";
        FocusText = "Edit A and B, then choose an operation.";
    }

    public event Action? Changed;

    public string ResultLabel { get; private set; }
    public string FocusText { get; private set; }

    public int RowsA => _a.Rows;
    public int ColumnsA => _a.Columns;
    public int RowsB => _b.Rows;
    public int ColumnsB => _b.Columns;

    public MatrixWorkspaceSnapshot CreateSnapshot() => new(
        CreateGridSnapshot(_a, _aStates),
        CreateGridSnapshot(_b, _bStates),
        CreateGridSnapshot(_result, _resultStates),
        ResultLabel,
        FocusText);

    public MatrixProperties Analyze(MatrixSlot slot)
    {
        var matrix = GetEditableMatrix(slot);
        var isSquare = matrix.Rows == matrix.Columns;
        var isZero = true;
        var isIdentity = isSquare;
        var isDiagonal = isSquare;
        var isUpper = isSquare;
        var isLower = isSquare;
        var isSymmetric = isSquare;
        double trace = 0d;

        for (var row = 0; row < matrix.Rows; row++)
        {
            for (var column = 0; column < matrix.Columns; column++)
            {
                var value = matrix[row, column];
                if (!NearlyZero(value)) isZero = false;
                if (isSquare)
                {
                    if (row == column)
                    {
                        trace += value;
                        if (!NearlyEqual(value, 1d)) isIdentity = false;
                    }
                    else
                    {
                        if (!NearlyZero(value))
                        {
                            isIdentity = false;
                            isDiagonal = false;
                        }
                        if (row > column && !NearlyZero(value)) isUpper = false;
                        if (row < column && !NearlyZero(value)) isLower = false;
                        if (!NearlyEqual(value, matrix[column, row])) isSymmetric = false;
                    }
                }
            }
        }

        return new MatrixProperties(isSquare, isZero, isIdentity, isDiagonal, isUpper, isLower, isSymmetric, isSquare ? trace : null);
    }

    public void SetCell(MatrixSlot slot, int row, int column, double value)
    {
        var matrix = GetEditableMatrix(slot);
        matrix[row, column] = Normalize(value);
        ResetStates();
        FocusText = $"Updated {slot}[{row + 1},{column + 1}]. Row-major slot = {matrix.GetFlatIndex(row, column)}.";
        NotifyChanged();
    }

    public void Resize(MatrixSlot slot, int rows, int columns)
    {
        var matrix = GetEditableMatrix(slot);
        matrix.Resize(rows, columns, preserve: true);
        ResizeStates(slot, rows * columns);
        ResetResult(1, 1, "Result");
        ResetStates();
        FocusText = $"{slot} resized to {rows} × {columns}. Overlapping cells were preserved; new cells start at 0.";
        NotifyChanged();
    }

    public void LoadPreset(MatrixSlot slot, MatrixPreset preset)
    {
        var matrix = GetEditableMatrix(slot);
        matrix.Clear();
        switch (preset)
        {
            case MatrixPreset.Identity:
                for (var index = 0; index < Math.Min(matrix.Rows, matrix.Columns); index++) matrix[index, index] = 1d;
                break;
            case MatrixPreset.Sequence:
                for (var row = 0; row < matrix.Rows; row++)
                    for (var column = 0; column < matrix.Columns; column++)
                        matrix[row, column] = (row * matrix.Columns) + column + 1;
                break;
            case MatrixPreset.Diagonal:
                for (var index = 0; index < Math.Min(matrix.Rows, matrix.Columns); index++) matrix[index, index] = index + 1;
                break;
            case MatrixPreset.Symmetric:
                if (matrix.Rows != matrix.Columns)
                {
                    var size = Math.Min(8, Math.Max(matrix.Rows, matrix.Columns));
                    matrix.Resize(size, size, preserve: false);
                    ResizeStates(slot, size * size);
                }
                for (var row = 0; row < matrix.Rows; row++)
                    for (var column = row; column < matrix.Columns; column++)
                    {
                        var value = row == column ? row + 2 : row + column + 1;
                        matrix[row, column] = value;
                        matrix[column, row] = value;
                    }
                break;
            case MatrixPreset.GraphAdjacency:
                matrix.Resize(4, 4, preserve: false);
                ResizeStates(slot, 16);
                matrix[0, 1] = 1; matrix[1, 0] = 1;
                matrix[0, 2] = 1; matrix[2, 0] = 1;
                matrix[1, 3] = 1; matrix[3, 1] = 1;
                matrix[2, 3] = 1; matrix[3, 2] = 1;
                break;
            case MatrixPreset.RandomSmall:
                for (var row = 0; row < matrix.Rows; row++)
                    for (var column = 0; column < matrix.Columns; column++)
                        matrix[row, column] = Random.Shared.Next(-5, 6);
                break;
            case MatrixPreset.Zero:
            default:
                break;
        }
        ResetStates();
        ResetResult(1, 1, "Result");
        FocusText = $"Loaded the {PresetLabel(preset)} preset into {slot}.";
        NotifyChanged();
    }

    public void CopyAToB()
    {
        _b.CopyFrom(_a);
        _bStates = new MatrixCellVisualState[_b.Count];
        ResetResult(1, 1, "Result");
        FocusText = "Copied every row-major cell from A into B using our own storage loop.";
        NotifyChanged();
    }


    public void CopyResultTo(MatrixSlot slot)
    {
        var target = GetEditableMatrix(slot);
        target.CopyFrom(_result);
        ResizeStates(slot, target.Count);
        ResetStates();
        FocusText = $"Copied the derived result into {slot} so it can become input for the next operation.";
        NotifyChanged();
    }

    public void SwapAB()
    {
        var aCopy = _a.Clone();
        _a.CopyFrom(_b);
        _b.CopyFrom(aCopy);
        _aStates = new MatrixCellVisualState[_a.Count];
        _bStates = new MatrixCellVisualState[_b.Count];
        ResetResult(1, 1, "Result");
        FocusText = "Swapped matrices A and B.";
        NotifyChanged();
    }

    public Task<MatrixOperationResult> AddAsync(CancellationToken token = default) => ElementwiseBinaryAsync(MatrixOperationKind.Add, "+", (left, right) => left + right, token);
    public Task<MatrixOperationResult> SubtractAsync(CancellationToken token = default) => ElementwiseBinaryAsync(MatrixOperationKind.Subtract, "−", (left, right) => left - right, token);
    public Task<MatrixOperationResult> HadamardAsync(CancellationToken token = default) => ElementwiseBinaryAsync(MatrixOperationKind.Hadamard, "× element-wise", (left, right) => left * right, token);

    public Task<MatrixOperationResult> MultiplyAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Columns != _b.Rows)
            return await FailAsync(MatrixOperationKind.Multiply, $"A is {_a.Rows}×{_a.Columns} and B is {_b.Rows}×{_b.Columns}. Matrix multiplication requires A columns = B rows.", token);

        ResetResult(_a.Rows, _b.Columns, "A × B");
        var arithmetic = 0;
        for (var row = 0; row < _a.Rows; row++)
        {
            for (var column = 0; column < _b.Columns; column++)
            {
                double sum = 0d;
                for (var k = 0; k < _a.Columns; k++)
                {
                    ClearTransientStates();
                    SetState(MatrixSlot.A, row, k, MatrixCellVisualState.Reading);
                    SetState(MatrixSlot.B, k, column, MatrixCellVisualState.Reading);
                    _resultStates[(row * _result.Columns) + column] = MatrixCellVisualState.Writing;
                    NotifyChanged();
                    await NextStepAsync($"Result[{row + 1},{column + 1}]: take A[{row + 1},{k + 1}] = {Format(_a[row, k])} and B[{k + 1},{column + 1}] = {Format(_b[k, column])}. Multiply them, then add to this row·column dot product.", token);
                    sum += _a[row, k] * _b[k, column];
                    arithmetic += 2;
                    _result[row, column] = Normalize(sum);
                    NotifyChanged();
                }
                _resultStates[(row * _result.Columns) + column] = MatrixCellVisualState.Matched;
            }
        }
        ClearTransientStates(keepMatchedResult: true);
        FocusText = "Each result cell is one row of A dotted with one column of B.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Multiply, true, $"Computed A × B = {_result.Rows}×{_result.Columns}.", "O(r·c·k)", arithmetic, 0);
    }, token);

    public Task<MatrixOperationResult> ScalarMultiplyAsync(double scalar, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        ResetResult(_a.Rows, _a.Columns, $"{Format(scalar)} × A");
        var operations = 0;
        for (var row = 0; row < _a.Rows; row++)
        {
            for (var column = 0; column < _a.Columns; column++)
            {
                ClearTransientStates();
                SetState(MatrixSlot.A, row, column, MatrixCellVisualState.Reading);
                _resultStates[(row * _result.Columns) + column] = MatrixCellVisualState.Writing;
                NotifyChanged();
                await NextStepAsync($"Multiply A[{row + 1},{column + 1}] = {Format(_a[row, column])} by scalar {Format(scalar)}.", token);
                _result[row, column] = Normalize(_a[row, column] * scalar);
                operations++;
            }
        }
        ClearTransientStates();
        FocusText = "Scalar multiplication changes every cell independently; dimensions stay unchanged.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.ScalarMultiply, true, $"Scaled A by {Format(scalar)}.", "O(r·c)", operations, 0);
    }, token);

    public Task<MatrixOperationResult> TransposeAsync(MatrixSlot slot, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        var source = GetEditableMatrix(slot);
        ResetResult(source.Columns, source.Rows, $"{slot}ᵀ");
        var operations = 0;
        for (var row = 0; row < source.Rows; row++)
        {
            for (var column = 0; column < source.Columns; column++)
            {
                ClearTransientStates();
                SetState(slot, row, column, MatrixCellVisualState.Reading);
                _resultStates[(column * _result.Columns) + row] = MatrixCellVisualState.Writing;
                NotifyChanged();
                await NextStepAsync($"Move {slot}[{row + 1},{column + 1}] to transposed position [{column + 1},{row + 1}]. Rows become columns.", token);
                _result[column, row] = source[row, column];
                operations++;
            }
        }
        ClearTransientStates();
        FocusText = $"Transpose changed {source.Rows}×{source.Columns} into {source.Columns}×{source.Rows}.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Transpose, true, $"Computed {slot}ᵀ.", "O(r·c)", operations, 0);
    }, token);

    public Task<MatrixOperationResult> TraceAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _a.Columns) return await FailAsync(MatrixOperationKind.Trace, "Trace is defined here for a square matrix: sum the main diagonal of A.", token);
        double trace = 0d;
        for (var index = 0; index < _a.Rows; index++)
        {
            ClearTransientStates();
            SetState(MatrixSlot.A, index, index, MatrixCellVisualState.Reading);
            NotifyChanged();
            await NextStepAsync($"Add diagonal A[{index + 1},{index + 1}] = {Format(_a[index, index])} to the trace.", token);
            trace += _a[index, index];
        }
        ResetResult(1, 1, "trace(A)");
        _result[0, 0] = Normalize(trace);
        _resultStates[0] = MatrixCellVisualState.Matched;
        FocusText = $"trace(A) = {Format(trace)}.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Trace, true, $"Trace = {Format(trace)}.", "O(n)", _a.Rows, 0, trace);
    }, token);

    public Task<MatrixOperationResult> DeterminantAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _a.Columns) return await FailAsync(MatrixOperationKind.Determinant, "Determinant requires a square matrix.", token);
        var work = _a.Clone();
        ResetResult(work.Rows, work.Columns, "Elimination workspace for det(A)");
        _result.CopyFrom(work);
        var rowOps = 0;
        var arithmetic = 0;
        var sign = 1d;
        double determinant = 1d;

        for (var pivotColumn = 0; pivotColumn < work.Columns; pivotColumn++)
        {
            var pivotRow = FindPivotRow(work, pivotColumn, pivotColumn);
            if (pivotRow < 0)
            {
                determinant = 0d;
                await NextStepAsync($"Column {pivotColumn + 1} has no non-zero pivot at or below row {pivotColumn + 1}. The matrix is singular, so det(A) = 0.", token);
                break;
            }
            if (pivotRow != pivotColumn)
            {
                work.SwapRows(pivotRow, pivotColumn);
                sign *= -1d;
                rowOps++;
                CopyToResult(work);
                MarkResultRow(pivotColumn, MatrixCellVisualState.Pivot);
                NotifyChanged();
                await NextStepAsync($"Swap rows {pivotColumn + 1} and {pivotRow + 1} to place a non-zero pivot. Every row swap flips the determinant sign.", token);
            }

            var pivot = work[pivotColumn, pivotColumn];
            determinant *= pivot;
            MarkResultCell(pivotColumn, pivotColumn, MatrixCellVisualState.Pivot);
            NotifyChanged();
            await NextStepAsync($"Use pivot {Format(pivot)} at [{pivotColumn + 1},{pivotColumn + 1}]. The determinant accumulates the diagonal pivots, adjusted for row-swap sign.", token);

            for (var row = pivotColumn + 1; row < work.Rows; row++)
            {
                if (NearlyZero(work[row, pivotColumn])) continue;
                var factor = work[row, pivotColumn] / pivot;
                for (var column = pivotColumn; column < work.Columns; column++)
                {
                    work[row, column] = Normalize(work[row, column] - factor * work[pivotColumn, column]);
                    arithmetic += 2;
                }
                rowOps++;
                CopyToResult(work);
                MarkResultRow(row, MatrixCellVisualState.Eliminated);
                NotifyChanged();
                await NextStepAsync($"Eliminate entry [{row + 1},{pivotColumn + 1}] with R{row + 1} ← R{row + 1} − ({Format(factor)})·R{pivotColumn + 1}.", token);
            }
        }

        determinant = Normalize(sign * determinant);
        FocusText = $"det(A) = {Format(determinant)}. The original A was not mutated; elimination ran on a manual copy.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Determinant, true, $"det(A) = {Format(determinant)}.", "O(n³)", arithmetic, rowOps, determinant);
    }, token);

    public Task<MatrixOperationResult> InverseAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _a.Columns) return await FailAsync(MatrixOperationKind.Inverse, "Inverse requires a square matrix.", token);
        var n = _a.Rows;
        if (n * 2 > 8) return await FailAsync(MatrixOperationKind.Inverse, "For the visual augmented [A | I] workspace, inverse is limited to matrices up to 4×4 in this lab.", token);

        var augmented = new ManualMatrix(n, n * 2);
        for (var row = 0; row < n; row++)
            for (var column = 0; column < n; column++)
            {
                augmented[row, column] = _a[row, column];
                augmented[row, n + column] = row == column ? 1d : 0d;
            }
        ResetResult(n, n * 2, "Gauss–Jordan [A | I]");
        CopyToResult(augmented);
        var stats = await ReduceAsync(augmented, reduced: true, token, pivotColumnLimit: n);
        if (stats.Rank < n)
        {
            FocusText = "A cannot be inverted because a pivot is missing: the matrix is singular.";
            return new MatrixOperationResult(MatrixOperationKind.Inverse, false, "A is singular; inverse does not exist.", "O(n³)", stats.Arithmetic, stats.RowOperations);
        }

        var inverse = new ManualMatrix(n, n);
        for (var row = 0; row < n; row++)
            for (var column = 0; column < n; column++)
                inverse[row, column] = augmented[row, n + column];
        ResetResult(n, n, "A⁻¹");
        _result.CopyFrom(inverse);
        FocusText = "Gauss–Jordan transformed [A | I] into [I | A⁻¹]; the right half is the inverse.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Inverse, true, "Computed A⁻¹ with Gauss–Jordan elimination.", "O(n³)", stats.Arithmetic, stats.RowOperations);
    }, token);

    public Task<MatrixOperationResult> RefAsync(CancellationToken token = default) => RowReduceAsync(reduced: false, MatrixOperationKind.Ref, token);
    public Task<MatrixOperationResult> RrefAsync(CancellationToken token = default) => RowReduceAsync(reduced: true, MatrixOperationKind.Rref, token);

    public Task<MatrixOperationResult> RankAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        var work = _a.Clone();
        ResetResult(work.Rows, work.Columns, "RREF(A) for rank");
        CopyToResult(work);
        var stats = await ReduceAsync(work, reduced: true, token);
        FocusText = $"rank(A) = {stats.Rank}: the number of pivot rows in RREF.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Rank, true, $"rank(A) = {stats.Rank}.", "O(r·c·min(r,c))", stats.Arithmetic, stats.RowOperations, IntegerResult: stats.Rank);
    }, token);

    public Task<MatrixOperationResult> PowerAsync(int exponent, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _a.Columns) return await FailAsync(MatrixOperationKind.Power, "A^k requires A to be square.", token);
        if (exponent < 0 || exponent > 8) return await FailAsync(MatrixOperationKind.Power, "Use an integer exponent from 0 to 8. Negative powers would require inversion first.", token);
        var n = _a.Rows;
        var accumulator = Identity(n);
        var arithmetic = 0;
        if (exponent == 0)
        {
            ResetResult(n, n, "A⁰ = I");
            _result.CopyFrom(accumulator);
            FocusText = "By definition, A⁰ is the identity matrix for square A.";
            NotifyChanged();
            return new MatrixOperationResult(MatrixOperationKind.Power, true, "Computed A⁰ = I.", "O(n²)", 0, 0);
        }

        for (var step = 1; step <= exponent; step++)
        {
            await NextStepAsync($"Power step {step}/{exponent}: multiply the current accumulator by A.", token);
            accumulator = MultiplyRaw(accumulator, _a, ref arithmetic);
            ResetResult(n, n, $"A^{step}");
            _result.CopyFrom(accumulator);
            NotifyChanged();
        }
        FocusText = $"Computed A^{exponent} by repeated matrix multiplication.";
        return new MatrixOperationResult(MatrixOperationKind.Power, true, $"Computed A^{exponent}.", $"O({exponent}·n³)", arithmetic, 0);
    }, token);

    public Task<MatrixOperationResult> MinorAsync(int row, int column, CancellationToken token = default) => MinorOrCofactorAsync(row, column, cofactor: false, token);
    public Task<MatrixOperationResult> CofactorAsync(int row, int column, CancellationToken token = default) => MinorOrCofactorAsync(row, column, cofactor: true, token);

    public Task<MatrixOperationResult> SolveAsync(CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _a.Columns) return await FailAsync(MatrixOperationKind.Solve, "Solve Ax=B currently requires square A.", token);
        if (_b.Rows != _a.Rows) return await FailAsync(MatrixOperationKind.Solve, "B must have the same number of rows as A so it can represent one or more right-hand sides.", token);
        if (_a.Columns + _b.Columns > 8) return await FailAsync(MatrixOperationKind.Solve, "The visual augmented [A | B] workspace is limited to 8 columns. Reduce A/B dimensions for this run.", token);

        var augmented = new ManualMatrix(_a.Rows, _a.Columns + _b.Columns);
        for (var row = 0; row < _a.Rows; row++)
        {
            for (var column = 0; column < _a.Columns; column++) augmented[row, column] = _a[row, column];
            for (var column = 0; column < _b.Columns; column++) augmented[row, _a.Columns + column] = _b[row, column];
        }
        ResetResult(augmented.Rows, augmented.Columns, "Gauss–Jordan [A | B]");
        CopyToResult(augmented);
        var stats = await ReduceAsync(augmented, reduced: true, token, pivotColumnLimit: _a.Columns);
        if (stats.Rank < _a.Rows)
        {
            FocusText = "A is singular in this square-system lab, so there is no unique solution X.";
            return new MatrixOperationResult(MatrixOperationKind.Solve, false, "No unique solution because A is singular.", "O(n³)", stats.Arithmetic, stats.RowOperations);
        }

        var solution = new ManualMatrix(_a.Columns, _b.Columns);
        for (var row = 0; row < solution.Rows; row++)
            for (var column = 0; column < solution.Columns; column++)
                solution[row, column] = augmented[row, _a.Columns + column];
        ResetResult(solution.Rows, solution.Columns, "X in A·X = B");
        _result.CopyFrom(solution);
        FocusText = "After Gauss–Jordan reduces the left side to I, the transformed right side is X.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.Solve, true, "Solved A·X = B.", "O(n³)", stats.Arithmetic, stats.RowOperations);
    }, token);

    public Task<MatrixOperationResult> SwapRowsAsync(int firstRow, int secondRow, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (!ValidRow(_a, firstRow) || !ValidRow(_a, secondRow)) return await FailAsync(MatrixOperationKind.SwapRows, "Row indexes are outside matrix A.", token);
        MarkARow(firstRow, MatrixCellVisualState.Candidate);
        MarkARow(secondRow, MatrixCellVisualState.Candidate);
        NotifyChanged();
        await NextStepAsync($"Elementary row operation: swap R{firstRow + 1} ↔ R{secondRow + 1}. This changes row order but not the number of rows/columns.", token);
        _a.SwapRows(firstRow, secondRow);
        ResetStates();
        FocusText = $"Swapped rows {firstRow + 1} and {secondRow + 1} in A.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.SwapRows, true, FocusText, "O(c)", _a.Columns, 1);
    }, token);

    public Task<MatrixOperationResult> ScaleRowAsync(int row, double factor, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (!ValidRow(_a, row)) return await FailAsync(MatrixOperationKind.ScaleRow, "Row index is outside matrix A.", token);
        if (NearlyZero(factor)) return await FailAsync(MatrixOperationKind.ScaleRow, "Use a non-zero factor for an invertible elementary row scaling.", token);
        MarkARow(row, MatrixCellVisualState.Writing);
        NotifyChanged();
        await NextStepAsync($"Scale R{row + 1} by {Format(factor)}. Every cell in that row is multiplied by the same non-zero factor.", token);
        _a.ScaleRow(row, factor);
        ResetStates();
        FocusText = $"R{row + 1} ← {Format(factor)}·R{row + 1}.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.ScaleRow, true, FocusText, "O(c)", _a.Columns, 1);
    }, token);

    public Task<MatrixOperationResult> AddScaledRowAsync(int targetRow, int sourceRow, double factor, CancellationToken token = default) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (!ValidRow(_a, targetRow) || !ValidRow(_a, sourceRow)) return await FailAsync(MatrixOperationKind.AddScaledRow, "Row indexes are outside matrix A.", token);
        MarkARow(sourceRow, MatrixCellVisualState.Reading);
        MarkARow(targetRow, MatrixCellVisualState.Writing);
        NotifyChanged();
        await NextStepAsync($"Elementary row operation: R{targetRow + 1} ← R{targetRow + 1} + ({Format(factor)})·R{sourceRow + 1}. This is the core elimination move used by REF/RREF.", token);
        _a.AddScaledRow(targetRow, sourceRow, factor);
        ResetStates();
        FocusText = $"Updated R{targetRow + 1} using R{sourceRow + 1}.";
        NotifyChanged();
        return new MatrixOperationResult(MatrixOperationKind.AddScaledRow, true, FocusText, "O(c)", _a.Columns * 2, 1);
    }, token);

    private Task<MatrixOperationResult> ElementwiseBinaryAsync(MatrixOperationKind kind, string symbol, Func<double, double, double> combine, CancellationToken token) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        if (_a.Rows != _b.Rows || _a.Columns != _b.Columns)
            return await FailAsync(kind, $"Element-wise {symbol} requires equal dimensions. A is {_a.Rows}×{_a.Columns}; B is {_b.Rows}×{_b.Columns}.", token);
        ResetResult(_a.Rows, _a.Columns, kind == MatrixOperationKind.Add ? "A + B" : kind == MatrixOperationKind.Subtract ? "A − B" : "A ⊙ B");
        var operations = 0;
        for (var row = 0; row < _a.Rows; row++)
        {
            for (var column = 0; column < _a.Columns; column++)
            {
                ClearTransientStates();
                SetState(MatrixSlot.A, row, column, MatrixCellVisualState.Reading);
                SetState(MatrixSlot.B, row, column, MatrixCellVisualState.Reading);
                _resultStates[(row * _result.Columns) + column] = MatrixCellVisualState.Writing;
                NotifyChanged();
                await NextStepAsync($"Cell [{row + 1},{column + 1}]: {Format(_a[row, column])} {symbol} {Format(_b[row, column])}. Element-wise operations pair cells at identical coordinates.", token);
                _result[row, column] = Normalize(combine(_a[row, column], _b[row, column]));
                operations++;
            }
        }
        ClearTransientStates();
        FocusText = $"{ResultLabel} completed cell by cell; dimensions stayed {_result.Rows}×{_result.Columns}.";
        NotifyChanged();
        return new MatrixOperationResult(kind, true, $"Computed {ResultLabel}.", "O(r·c)", operations, 0);
    }, token);

    private Task<MatrixOperationResult> RowReduceAsync(bool reduced, MatrixOperationKind kind, CancellationToken token) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        var work = _a.Clone();
        ResetResult(work.Rows, work.Columns, reduced ? "RREF(A)" : "REF(A)");
        CopyToResult(work);
        var stats = await ReduceAsync(work, reduced, token);
        FocusText = reduced
            ? $"RREF finished with {stats.Rank} pivot row(s): pivots are 1 and are the only non-zero entries in their pivot columns."
            : $"REF finished with {stats.Rank} pivot row(s): each pivot moves strictly to the right as rows descend.";
        NotifyChanged();
        return new MatrixOperationResult(kind, true, FocusText, "O(r·c·min(r,c))", stats.Arithmetic, stats.RowOperations, IntegerResult: stats.Rank);
    }, token);

    private async Task<ReductionStats> ReduceAsync(ManualMatrix work, bool reduced, CancellationToken token, int? pivotColumnLimit = null)
    {
        var pivotRow = 0;
        var arithmetic = 0;
        var rowOperations = 0;
        var columnLimit = Math.Min(work.Columns, pivotColumnLimit ?? work.Columns);
        for (var pivotColumn = 0; pivotColumn < columnLimit && pivotRow < work.Rows; pivotColumn++)
        {
            var candidate = FindPivotRow(work, pivotRow, pivotColumn);
            if (candidate < 0) continue;
            if (candidate != pivotRow)
            {
                work.SwapRows(candidate, pivotRow);
                rowOperations++;
                CopyToResult(work);
                MarkResultRow(pivotRow, MatrixCellVisualState.Candidate);
                NotifyChanged();
                await NextStepAsync($"Pivot column {pivotColumn + 1}: swap R{pivotRow + 1} with R{candidate + 1} so a non-zero pivot moves into position.", token);
            }

            var pivot = work[pivotRow, pivotColumn];
            if (reduced && !NearlyEqual(pivot, 1d))
            {
                var scale = 1d / pivot;
                work.ScaleRow(pivotRow, scale);
                arithmetic += work.Columns;
                rowOperations++;
                CopyToResult(work);
                MarkResultRow(pivotRow, MatrixCellVisualState.Pivot);
                NotifyChanged();
                await NextStepAsync($"Normalize pivot row R{pivotRow + 1} by multiplying by 1/{Format(pivot)} so the pivot becomes 1.", token);
                pivot = 1d;
            }
            else
            {
                MarkResultCell(pivotRow, pivotColumn, MatrixCellVisualState.Pivot);
                NotifyChanged();
                await NextStepAsync($"Pivot at [{pivotRow + 1},{pivotColumn + 1}] is {Format(pivot)}. Use it to eliminate entries {(reduced ? "above and below" : "below")} this position.", token);
            }

            var startRow = reduced ? 0 : pivotRow + 1;
            for (var row = startRow; row < work.Rows; row++)
            {
                if (row == pivotRow || NearlyZero(work[row, pivotColumn])) continue;
                var factor = -work[row, pivotColumn] / pivot;
                work.AddScaledRow(row, pivotRow, factor);
                arithmetic += work.Columns * 2;
                rowOperations++;
                CopyToResult(work);
                MarkResultRow(row, MatrixCellVisualState.Eliminated);
                MarkResultCell(pivotRow, pivotColumn, MatrixCellVisualState.Pivot);
                NotifyChanged();
                await NextStepAsync($"Eliminate column {pivotColumn + 1} in R{row + 1}: R{row + 1} ← R{row + 1} + ({Format(factor)})·R{pivotRow + 1}.", token);
            }
            pivotRow++;
        }
        CopyToResult(work);
        return new ReductionStats(pivotRow, arithmetic, rowOperations);
    }

    private Task<MatrixOperationResult> MinorOrCofactorAsync(int row, int column, bool cofactor, CancellationToken token) => ExecuteExclusiveAsync(async () =>
    {
        ResetStates();
        var kind = cofactor ? MatrixOperationKind.Cofactor : MatrixOperationKind.Minor;
        if (_a.Rows != _a.Columns || _a.Rows < 2) return await FailAsync(kind, "Minor/cofactor requires a square matrix of size at least 2×2.", token);
        if (!ValidRow(_a, row) || column < 0 || column >= _a.Columns) return await FailAsync(kind, "Selected cell is outside A.", token);
        var minor = new ManualMatrix(_a.Rows - 1, _a.Columns - 1);
        var targetRow = 0;
        for (var sourceRow = 0; sourceRow < _a.Rows; sourceRow++)
        {
            if (sourceRow == row) continue;
            var targetColumn = 0;
            for (var sourceColumn = 0; sourceColumn < _a.Columns; sourceColumn++)
            {
                if (sourceColumn == column) continue;
                minor[targetRow, targetColumn++] = _a[sourceRow, sourceColumn];
            }
            targetRow++;
        }
        MarkARow(row, MatrixCellVisualState.Eliminated);
        for (var r = 0; r < _a.Rows; r++) SetState(MatrixSlot.A, r, column, MatrixCellVisualState.Eliminated);
        ResetResult(minor.Rows, minor.Columns, $"Minor matrix M{row + 1}{column + 1}");
        _result.CopyFrom(minor);
        NotifyChanged();
        await NextStepAsync($"Delete row {row + 1} and column {column + 1}. The remaining {(minor.Rows)}×{minor.Columns} matrix defines the minor M{row + 1}{column + 1}.", token);
        var determinant = DeterminantRaw(minor);
        var value = cofactor && ((row + column) % 2 != 0) ? -determinant : determinant;
        FocusText = cofactor
            ? $"C{row + 1}{column + 1} = (−1)^({row + 1}+{column + 1}) · M{row + 1}{column + 1} = {Format(value)}."
            : $"M{row + 1}{column + 1} = {Format(determinant)}.";
        NotifyChanged();
        return new MatrixOperationResult(kind, true, FocusText, "O(n³)", 0, 0, value);
    }, token);

    private async Task<MatrixOperationResult> FailAsync(MatrixOperationKind kind, string message, CancellationToken token)
    {
        FocusText = message;
        NotifyChanged();
        await NextStepAsync(message, token);
        return new MatrixOperationResult(kind, false, message, "—", 0, 0);
    }

    private async Task<MatrixOperationResult> ExecuteExclusiveAsync(Func<Task<MatrixOperationResult>> operation, CancellationToken token)
    {
        await _operationGate.WaitAsync(token);
        try { return await operation(); }
        finally { _operationGate.Release(); }
    }

    private ManualMatrix GetEditableMatrix(MatrixSlot slot) => slot switch
    {
        MatrixSlot.A => _a,
        MatrixSlot.B => _b,
        _ => throw new InvalidOperationException("Result is derived output and is not directly editable.")
    };

    private void ResizeStates(MatrixSlot slot, int count)
    {
        if (slot == MatrixSlot.A) _aStates = new MatrixCellVisualState[count];
        else if (slot == MatrixSlot.B) _bStates = new MatrixCellVisualState[count];
    }

    private void ResetResult(int rows, int columns, string label)
    {
        _result.Resize(rows, columns, preserve: false);
        _result.Clear();
        _resultStates = new MatrixCellVisualState[_result.Count];
        ResultLabel = label;
    }

    private void ResetStates()
    {
        _aStates = new MatrixCellVisualState[_a.Count];
        _bStates = new MatrixCellVisualState[_b.Count];
        _resultStates = new MatrixCellVisualState[_result.Count];
    }

    private void ClearTransientStates(bool keepMatchedResult = false)
    {
        for (var index = 0; index < _aStates.Length; index++) _aStates[index] = MatrixCellVisualState.Normal;
        for (var index = 0; index < _bStates.Length; index++) _bStates[index] = MatrixCellVisualState.Normal;
        for (var index = 0; index < _resultStates.Length; index++)
            if (!keepMatchedResult || _resultStates[index] != MatrixCellVisualState.Matched) _resultStates[index] = MatrixCellVisualState.Normal;
    }

    private void SetState(MatrixSlot slot, int row, int column, MatrixCellVisualState state)
    {
        var matrix = slot == MatrixSlot.A ? _a : _b;
        var index = matrix.GetFlatIndex(row, column);
        if (slot == MatrixSlot.A) _aStates[index] = state;
        else _bStates[index] = state;
    }

    private void MarkARow(int row, MatrixCellVisualState state)
    {
        for (var column = 0; column < _a.Columns; column++) SetState(MatrixSlot.A, row, column, state);
    }

    private void MarkResultRow(int row, MatrixCellVisualState state)
    {
        for (var column = 0; column < _result.Columns; column++) _resultStates[(row * _result.Columns) + column] = state;
    }

    private void MarkResultCell(int row, int column, MatrixCellVisualState state) => _resultStates[(row * _result.Columns) + column] = state;

    private void CopyToResult(ManualMatrix source)
    {
        if (_result.Rows != source.Rows || _result.Columns != source.Columns)
            ResetResult(source.Rows, source.Columns, ResultLabel);
        _result.CopyFrom(source);
    }

    private MatrixGridSnapshot CreateGridSnapshot(ManualMatrix matrix, MatrixCellVisualState[] states)
    {
        var stateCopy = new MatrixCellVisualState[states.Length];
        for (var index = 0; index < states.Length; index++) stateCopy[index] = states[index];
        return new MatrixGridSnapshot(matrix.Rows, matrix.Columns, matrix.CopyRawValues(), stateCopy);
    }

    private static int FindPivotRow(ManualMatrix matrix, int startRow, int column)
    {
        var best = -1;
        var bestMagnitude = 0d;
        for (var row = startRow; row < matrix.Rows; row++)
        {
            var magnitude = Math.Abs(matrix[row, column]);
            if (magnitude > bestMagnitude + Epsilon)
            {
                bestMagnitude = magnitude;
                best = row;
            }
        }
        return best;
    }

    private static ManualMatrix Identity(int size)
    {
        var identity = new ManualMatrix(size, size);
        for (var index = 0; index < size; index++) identity[index, index] = 1d;
        return identity;
    }

    private static ManualMatrix MultiplyRaw(ManualMatrix left, ManualMatrix right, ref int arithmetic)
    {
        var product = new ManualMatrix(left.Rows, right.Columns);
        for (var row = 0; row < left.Rows; row++)
            for (var column = 0; column < right.Columns; column++)
            {
                double sum = 0d;
                for (var k = 0; k < left.Columns; k++)
                {
                    sum += left[row, k] * right[k, column];
                    arithmetic += 2;
                }
                product[row, column] = Normalize(sum);
            }
        return product;
    }

    private static double DeterminantRaw(ManualMatrix matrix)
    {
        if (matrix.Rows != matrix.Columns) throw new InvalidOperationException("Determinant requires a square matrix.");
        var work = matrix.Clone();
        var sign = 1d;
        var determinant = 1d;
        for (var column = 0; column < work.Columns; column++)
        {
            var pivotRow = FindPivotRow(work, column, column);
            if (pivotRow < 0) return 0d;
            if (pivotRow != column) { work.SwapRows(pivotRow, column); sign *= -1d; }
            var pivot = work[column, column];
            determinant *= pivot;
            for (var row = column + 1; row < work.Rows; row++)
            {
                if (NearlyZero(work[row, column])) continue;
                var factor = work[row, column] / pivot;
                for (var c = column; c < work.Columns; c++) work[row, c] = Normalize(work[row, c] - factor * work[column, c]);
            }
        }
        return Normalize(sign * determinant);
    }

    private static bool ValidRow(ManualMatrix matrix, int row) => row >= 0 && row < matrix.Rows;
    private static bool NearlyZero(double value) => Math.Abs(value) <= Epsilon;
    private static bool NearlyEqual(double left, double right) => Math.Abs(left - right) <= Epsilon;
    private static double Normalize(double value) => NearlyZero(value) ? 0d : Math.Round(value, 8);
    private static string Format(double value) => Normalize(value).ToString("0.####");
    private static string PresetLabel(MatrixPreset preset) => preset switch
    {
        MatrixPreset.GraphAdjacency => "graph adjacency",
        MatrixPreset.RandomSmall => "random small integers",
        _ => preset.ToString().ToLowerInvariant()
    };

    private void NotifyChanged() => Changed?.Invoke();

    private sealed record ReductionStats(int Rank, int Arithmetic, int RowOperations);
}

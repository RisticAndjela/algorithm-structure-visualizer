using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Matrix;

public sealed class MatrixSimulationTests
{
    [Fact]
    public void ManualMatrix_UsesRowMajorIndexAndPreservesOverlapOnResize()
    {
        var matrix = new ManualMatrix(2, 3);
        matrix[1, 2] = 9;

        Assert.Equal(5, matrix.GetFlatIndex(1, 2));

        matrix.Resize(3, 4, preserve: true);
        Assert.Equal(9, matrix[1, 2]);
        Assert.Equal(6, matrix.GetFlatIndex(1, 2));
        Assert.Equal(0, matrix[2, 3]);
    }

    [Fact]
    public async Task Add_ProducesCellWiseSum()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 2);
        matrix.Resize(MatrixSlot.B, 2, 2);
        Set2x2(matrix, MatrixSlot.A, 1, 2, 3, 4);
        Set2x2(matrix, MatrixSlot.B, 5, 6, 7, 8);

        var result = await matrix.AddAsync();
        var snapshot = matrix.CreateSnapshot().Result;

        Assert.True(result.Succeeded);
        Assert.Equal(6, snapshot.GetValue(0, 0));
        Assert.Equal(8, snapshot.GetValue(0, 1));
        Assert.Equal(10, snapshot.GetValue(1, 0));
        Assert.Equal(12, snapshot.GetValue(1, 1));
    }

    [Fact]
    public async Task Multiply_UsesRowColumnRuleAndCorrectResultDimensions()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 3);
        matrix.Resize(MatrixSlot.B, 3, 2);
        var a = new[,] { { 1d, 2d, 3d }, { 4d, 5d, 6d } };
        var b = new[,] { { 7d, 8d }, { 9d, 10d }, { 11d, 12d } };
        Set(matrix, MatrixSlot.A, a);
        Set(matrix, MatrixSlot.B, b);

        var result = await matrix.MultiplyAsync();
        var snapshot = matrix.CreateSnapshot().Result;

        Assert.True(result.Succeeded);
        Assert.Equal(2, snapshot.Rows);
        Assert.Equal(2, snapshot.Columns);
        Assert.Equal(58, snapshot.GetValue(0, 0));
        Assert.Equal(64, snapshot.GetValue(0, 1));
        Assert.Equal(139, snapshot.GetValue(1, 0));
        Assert.Equal(154, snapshot.GetValue(1, 1));
    }

    [Fact]
    public async Task Determinant_AndInverse_WorkForNonSingular2x2()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 2);
        Set2x2(matrix, MatrixSlot.A, 4, 7, 2, 6);

        var determinant = await matrix.DeterminantAsync();
        Assert.True(determinant.Succeeded);
        Assert.True(determinant.ScalarResult.HasValue);
        AssertClose(10d, determinant.ScalarResult.Value);

        var inverse = await matrix.InverseAsync();
        var snapshot = matrix.CreateSnapshot().Result;
        Assert.True(inverse.Succeeded);
        AssertClose(0.6, snapshot.GetValue(0, 0));
        AssertClose(-0.7, snapshot.GetValue(0, 1));
        AssertClose(-0.2, snapshot.GetValue(1, 0));
        AssertClose(0.4, snapshot.GetValue(1, 1));
    }

    [Fact]
    public async Task SingularMatrix_HasZeroDeterminantAndNoInverse()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 2);
        Set2x2(matrix, MatrixSlot.A, 1, 2, 2, 4);

        var determinant = await matrix.DeterminantAsync();
        var inverse = await matrix.InverseAsync();

        Assert.True(determinant.ScalarResult.HasValue);
        AssertClose(0d, determinant.ScalarResult.Value);
        Assert.False(inverse.Succeeded);
    }

    [Fact]
    public async Task RrefAndRank_DetectDependentRows()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 3, 3);
        Set(matrix, MatrixSlot.A, new[,] { { 1d, 2d, 3d }, { 2d, 4d, 6d }, { 0d, 1d, 1d } });

        var rank = await matrix.RankAsync();
        var rref = await matrix.RrefAsync();

        Assert.True(rank.Succeeded);
        Assert.True(rank.IntegerResult.HasValue);
        Assert.Equal(2, rank.IntegerResult.Value);
        Assert.True(rref.Succeeded);
    }

    [Fact]
    public async Task Solve_FindsUniqueSolutionForAxEqualsB()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 2);
        matrix.Resize(MatrixSlot.B, 2, 1);
        Set2x2(matrix, MatrixSlot.A, 2, 1, 1, -1);
        matrix.SetCell(MatrixSlot.B, 0, 0, 5);
        matrix.SetCell(MatrixSlot.B, 1, 0, 1);

        var result = await matrix.SolveAsync();
        var solution = matrix.CreateSnapshot().Result;

        Assert.True(result.Succeeded);
        AssertClose(2, solution.GetValue(0, 0));
        AssertClose(1, solution.GetValue(1, 0));
    }

    [Fact]
    public async Task ElementaryRowOperations_MutateAExactly()
    {
        var matrix = CreateSimulation();
        matrix.Resize(MatrixSlot.A, 2, 2);
        Set2x2(matrix, MatrixSlot.A, 1, 2, 3, 4);

        await matrix.SwapRowsAsync(0, 1);
        await matrix.ScaleRowAsync(0, 2);
        await matrix.AddScaledRowAsync(1, 0, -0.5);
        var a = matrix.CreateSnapshot().A;

        Assert.Equal(6, a.GetValue(0, 0));
        Assert.Equal(8, a.GetValue(0, 1));
        Assert.Equal(-2, a.GetValue(1, 0));
        Assert.Equal(-2, a.GetValue(1, 1));
    }

    [Fact]
    public void GraphAdjacencyPreset_IsSquareAndSymmetric()
    {
        var matrix = CreateSimulation();
        matrix.LoadPreset(MatrixSlot.A, MatrixPreset.GraphAdjacency);
        var properties = matrix.Analyze(MatrixSlot.A);
        var snapshot = matrix.CreateSnapshot().A;

        Assert.True(properties.IsSquare);
        Assert.True(properties.IsSymmetric);
        Assert.Equal(4, snapshot.Rows);
        Assert.Equal(snapshot.GetValue(0, 1), snapshot.GetValue(1, 0));
    }

    private static MatrixSimulation CreateSimulation() => new(new ImmediateSimulationRuntime());

    private static void Set2x2(MatrixSimulation simulation, MatrixSlot slot, double a, double b, double c, double d)
    {
        simulation.SetCell(slot, 0, 0, a);
        simulation.SetCell(slot, 0, 1, b);
        simulation.SetCell(slot, 1, 0, c);
        simulation.SetCell(slot, 1, 1, d);
    }

    private static void Set(MatrixSimulation simulation, MatrixSlot slot, double[,] values)
    {
        for (var row = 0; row < values.GetLength(0); row++)
            for (var column = 0; column < values.GetLength(1); column++)
                simulation.SetCell(slot, row, column, values[row, column]);
    }

    private static void AssertClose(double expected, double actual) => Assert.True(Math.Abs(expected - actual) < 1e-6, $"Expected {expected}, got {actual}.");

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

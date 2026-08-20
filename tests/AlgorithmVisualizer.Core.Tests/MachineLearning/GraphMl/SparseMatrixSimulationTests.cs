using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.SparseMatrix;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.GraphMl;

public sealed class SparseMatrixSimulationTests
{
    [Fact]
    public async Task DenseToCsr_StoresExpectedSlicesAndSpmvResult()
    {
        var simulation = new SparseMatrixSimulation(new ImmediateRuntime());
        simulation.Configure(new SparseMatrixConfiguration(
            [[0d, 2d, 0d], [3d, 0d, 4d]],
            [1d, 2d, 3d]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(new[] { 2d, 3d, 4d }, result.Values);
        Assert.Equal(new[] { 1, 0, 2 }, result.ColumnIndexes);
        Assert.Equal(new[] { 0, 1, 3 }, result.RowPointers);
        Assert.Equal(new[] { 4d, 15d }, result.Product);
        Assert.Equal(3, result.NonZeroCount);
    }

    [Fact]
    public void ManualCsrMatrix_MultipliesWithoutDenseStorage()
    {
        var csr = new ManualCsrMatrix(
            3,
            3,
            [2d, 5d, -1d],
            [0, 2, 1],
            [0, 1, 2, 3]);
        var vector = new ManualVector(3);
        vector.CopyFrom([3d, 4d, 2d]);

        var result = csr.Multiply(vector);

        Assert.Equal(new[] { 6d, 10d, -4d }, result.CopyValues());
        Assert.Equal(0d, csr.Get(0, 2), 10);
        Assert.Equal(5d, csr.Get(1, 2), 10);
    }

    [Fact]
    public async Task VerySparseMatrix_UsesFewerCsrSlotsThanDenseCells()
    {
        var simulation = new SparseMatrixSimulation(new ImmediateRuntime());
        simulation.Configure(new SparseMatrixConfiguration(
            [[1d,0d,0d,0d],[0d,2d,0d,0d],[0d,0d,3d,0d],[0d,0d,0d,4d]],
            [1d,1d,1d,1d]));

        var result = await simulation.ExecuteAsync();

        Assert.True(result.CsrStoredSlots < result.DenseCellCount);
        Assert.Equal(4, result.NonZeroCount);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

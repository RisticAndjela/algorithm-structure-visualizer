using AlgorithmVisualizer.Core.DataStructures.Vector;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Vector;

public sealed class VectorSimulationTests
{
    [Fact]
    public void ManualVector_StoresComponentsInStableIndexOrder()
    {
        var vector = new ManualVector(3);
        vector[0] = 2;
        vector[1] = -1;
        vector[2] = 4.5;

        Assert.Equal(new[] { 2d, -1d, 4.5d }, vector.CopyValues());
    }

    [Fact]
    public async Task Add_ProducesComponentWiseResult()
    {
        var simulation = Create([1d, 2d, 3d], [4d, -1d, 2d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.Add);

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { 5d, 1d, 5d }, result.ResultVector);
    }

    [Fact]
    public async Task DotProduct_OrthogonalVectors_ReturnsZero()
    {
        var simulation = Create([1d, 0d], [0d, 4d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.DotProduct);

        Assert.True(result.Succeeded);
        Assert.True(result.ScalarResult.HasValue);
        AssertClose(0d, result.ScalarResult.Value);
    }

    [Fact]
    public async Task Normalize_NonZeroVector_ProducesUnitL2Length()
    {
        var simulation = Create([3d, 4d], [1d, 1d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.NormalizeL2);

        Assert.True(result.Succeeded);
        AssertClose(0.6d, result.ResultVector[0]);
        AssertClose(0.8d, result.ResultVector[1]);
        AssertClose(1d, L2(result.ResultVector));
    }

    [Fact]
    public async Task Normalize_ZeroVector_IsRejected()
    {
        var simulation = Create([0d, 0d, 0d], [1d, 2d, 3d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.NormalizeL2);

        Assert.False(result.Succeeded);
        Assert.Contains("zero vector", result.FailureReason.ToLowerInvariant());
    }

    [Fact]
    public async Task CosineSimilarity_ParallelVectors_IsOne()
    {
        var simulation = Create([1d, 2d], [3d, 6d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.CosineSimilarity);

        Assert.True(result.Succeeded);
        AssertClose(1d, result.ScalarResult!.Value);
    }

    [Fact]
    public async Task Distances_ThreeFourPair_ReturnFiveAndSeven()
    {
        var simulation = Create([0d, 0d], [3d, 4d]);
        var euclidean = await simulation.ExecuteAsync(VectorOperationKind.EuclideanDistance);
        var manhattan = await simulation.ExecuteAsync(VectorOperationKind.ManhattanDistance);

        AssertClose(5d, euclidean.ScalarResult!.Value);
        AssertClose(7d, manhattan.ScalarResult!.Value);
    }

    [Fact]
    public async Task PairedOperation_DimensionMismatch_IsRejected()
    {
        var simulation = Create([1d, 2d, 3d], [4d, 5d]);

        var result = await simulation.ExecuteAsync(VectorOperationKind.DotProduct);

        Assert.False(result.Succeeded);
        Assert.Contains("dimensions must match", result.FailureReason.ToLowerInvariant());
    }

    private static VectorSimulation Create(double[] a, double[] b)
    {
        var simulation = new VectorSimulation(new ImmediateSimulationRuntime());
        simulation.LoadVectors(a, b);
        return simulation;
    }

    private static double L2(IReadOnlyList<double> values)
    {
        var sum = 0d;
        for (var index = 0; index < values.Count; index++) sum += values[index] * values[index];
        return Math.Sqrt(sum);
    }

    private static void AssertClose(double expected, double actual) =>
        Assert.True(Math.Abs(expected - actual) < 1e-8, $"Expected {expected}, got {actual}.");

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

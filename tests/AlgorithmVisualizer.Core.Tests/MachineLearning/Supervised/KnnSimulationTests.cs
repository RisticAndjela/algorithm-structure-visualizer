using AlgorithmVisualizer.Core.MachineLearning.Supervised.Knn;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Supervised;

public sealed class KnnSimulationTests
{
    [Fact]
    public async Task TwoClusters_QueryNearClassOne_IsClassOneForK3()
    {
        var simulation = Create(
            [[-3d, -2d], [-2d, -3d], [-2d, -1d], [-1d, -2d], [1d, 2d], [2d, 1d], [2d, 3d], [3d, 2d]],
            [0, 0, 0, 0, 1, 1, 1, 1],
            [1.7d, 1.6d], 3, KnnDistanceMetric.Euclidean);

        var result = await simulation.ExecuteAsync();

        Assert.Equal(1, result.PredictedClass);
        Assert.Equal(0, result.VoteClass0);
        Assert.Equal(3, result.VoteClass1);
        Assert.Equal(8, result.DistanceEvaluations);
    }

    [Fact]
    public async Task Outlier_K1AndK5CanProduceDifferentPredictions()
    {
        var features = new[]
        {
            new[] { 0.2d, 0.1d }, new[] { 0.7d, 0d }, new[] { 0d, 0.8d },
            new[] { -0.8d, 0d }, new[] { 0d, -0.9d }, new[] { 3d, 3d }
        };
        var labels = new[] { 1, 0, 0, 0, 0, 1 };

        var k1 = await Create(features, labels, [0d, 0d], 1, KnnDistanceMetric.Euclidean).ExecuteAsync();
        var k5 = await Create(features, labels, [0d, 0d], 5, KnnDistanceMetric.Euclidean).ExecuteAsync();

        Assert.Equal(1, k1.PredictedClass);
        Assert.Equal(0, k5.PredictedClass);
    }

    [Fact]
    public async Task ExactMatch_IsFirstNeighborWithZeroDistance()
    {
        var simulation = Create(
            [[-2d, -2d], [1d, 1d], [2d, 2d], [3d, -1d]],
            [0, 1, 1, 0],
            [1d, 1d], 1, KnnDistanceMetric.Euclidean);

        var result = await simulation.ExecuteAsync();

        Assert.Equal(1, result.NeighborIndices[0]);
        Assert.Equal(0d, result.NeighborDistances[0], 12);
        Assert.Equal(1, result.PredictedClass);
    }

    [Fact]
    public async Task EuclideanAndManhattanCanChooseDifferentNearestExamples()
    {
        var features = new[]
        {
            new[] { 3d, 0d }, new[] { -4d, -4d }, new[] { 2d, 2d }, new[] { 5d, 5d }
        };
        var labels = new[] { 0, 0, 1, 1 };

        var euclidean = await Create(features, labels, [0d, 0d], 1, KnnDistanceMetric.Euclidean).ExecuteAsync();
        var manhattan = await Create(features, labels, [0d, 0d], 1, KnnDistanceMetric.Manhattan).ExecuteAsync();

        Assert.Equal(1, euclidean.PredictedClass);
        Assert.Equal(0, manhattan.PredictedClass);
    }

    [Fact]
    public async Task ThreeDimensionalCore_RemainsDimensionIndependent()
    {
        var simulation = Create(
            [[0d, 0d, 0d], [1d, 1d, 1d], [8d, 8d, 8d], [9d, 9d, 9d]],
            [0, 0, 1, 1],
            [1.2d, 1.1d, 0.9d], 1, KnnDistanceMetric.Euclidean);

        var result = await simulation.ExecuteAsync();

        Assert.Equal(0, result.PredictedClass);
        Assert.Equal(3, result.Dimension);
    }

    [Fact]
    public void Configure_RejectsEvenK()
    {
        var simulation = new KnnSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new KnnConfiguration(
            [[0d, 0d], [1d, 1d]], [0, 1], [0.5d, 0.5d], 2, KnnDistanceMetric.Euclidean)));
    }

    [Fact]
    public void Configure_RejectsMismatchedDimensions()
    {
        var simulation = new KnnSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new KnnConfiguration(
            [[0d, 0d], [1d, 1d, 1d]], [0, 1], [0.5d, 0.5d], 1, KnnDistanceMetric.Euclidean)));
    }

    private static KnnSimulation Create(double[][] features, int[] labels, double[] query, int k, KnnDistanceMetric metric)
    {
        var simulation = new KnnSimulation(new ImmediateRuntime());
        simulation.Configure(new KnnConfiguration(features, labels, query, k, metric));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

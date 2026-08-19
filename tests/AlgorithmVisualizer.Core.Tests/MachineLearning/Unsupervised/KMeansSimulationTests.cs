using AlgorithmVisualizer.Core.MachineLearning.Unsupervised.KMeans;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Unsupervised;

public sealed class KMeansSimulationTests
{
    [Fact]
    public async Task ThreeSeparatedGroups_ConvergeToThreeBalancedClusters()
    {
        var simulation = Create(
            [
                [-4d,-2d],[-3.4d,-1d],[-2.6d,-2.4d],
                [.3d,3.2d],[1.1d,2.2d],[1.8d,3.5d],
                [3.6d,-1.7d],[4.4d,-.5d],[5d,-2.2d]
            ],
            3,
            [0,3,6]);

        var result = await simulation.ExecuteAsync();

        Assert.True(result.Converged);
        Assert.Equal([3,3,3], result.ClusterCounts);
        Assert.Equal(2, result.Iterations);
        Assert.Equal(-3.3333333333333335d, result.Centroids[0][0], 10);
        Assert.Equal(2.966666666666667d, result.Centroids[1][1], 10);
    }

    [Fact]
    public async Task TwoGroups_ProduceTwoClusterAssignments()
    {
        var result = await Create(
            [[-5d,-1d],[-4d,0d],[-3d,1d],[3d,-1d],[4d,0d],[5d,1d]],
            2,
            [0,3]).ExecuteAsync();

        Assert.True(result.Converged);
        Assert.Equal([3,3], result.ClusterCounts);
        Assert.Equal([0,0,0,1,1,1], result.Assignments);
    }

    [Fact]
    public async Task EmptyCluster_KeepsItsCentroidInsteadOfFailing()
    {
        var result = await Create(
            [[0d,0d],[0d,0d],[10d,10d],[10d,11d],[11d,10d]],
            3,
            [0,1,2]).ExecuteAsync();

        Assert.True(result.Converged);
        Assert.Equal(0, result.ClusterCounts[1]);
        Assert.Equal([0d, 0d], result.Centroids[1]);
        Assert.Equal(5, result.ClusterCounts[0] + result.ClusterCounts[1] + result.ClusterCounts[2]);
        Assert.All(result.Centroids, centroid => Assert.All(centroid, value => Assert.True(double.IsFinite(value))));
    }

    [Fact]
    public async Task ThreeDimensionalCore_RemainsDimensionIndependent()
    {
        var result = await Create(
            [[0d,0d,0d],[1d,1d,1d],[9d,9d,9d],[10d,10d,10d]],
            2,
            [0,2]).ExecuteAsync();

        Assert.True(result.Converged);
        Assert.Equal(3, result.Dimension);
        Assert.Equal([2,2], result.ClusterCounts);
    }

    [Fact]
    public void Configure_RejectsMoreClustersThanPoints()
    {
        var simulation = new KMeansSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new KMeansConfiguration(
            [[0d,0d],[1d,1d]], 3, [0,1,0])));
    }

    [Fact]
    public void Configure_RejectsMismatchedPointDimensions()
    {
        var simulation = new KMeansSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new KMeansConfiguration(
            [[0d,0d],[1d,1d,1d],[2d,2d]], 2, [0,2])));
    }

    private static KMeansSimulation Create(double[][] features, int clusterCount, int[] seeds)
    {
        var simulation = new KMeansSimulation(new ImmediateRuntime());
        simulation.Configure(new KMeansConfiguration(features, clusterCount, seeds));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

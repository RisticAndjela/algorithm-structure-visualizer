using AlgorithmVisualizer.Core.MachineLearning.Supervised.KdTree;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Supervised;

public sealed class KdTreeSimulationTests
{
    private static readonly double[][] Features =
    [
        [-4d, -1d], [-3d, 2d], [-2d, -3d], [-1d, 1d],
        [1d, -2d], [2d, 2d], [3d, -1d], [4d, 3d], [5d, 0d]
    ];
    private static readonly int[] Labels = [0, 0, 0, 0, 1, 1, 1, 1, 1];

    [Fact]
    public async Task RightSideQuery_FindsNearestAndPrunesNodes()
    {
        var result = await Create(Features, Labels, [2.4d, 1.5d]).ExecuteAsync();

        Assert.Equal(5, result.NearestPointIndex);
        Assert.Equal(1, result.NearestLabel);
        Assert.True(result.VisitedNodes < result.Features.Length);
        Assert.True(result.PrunedNodes > 0);
    }

    [Fact]
    public async Task LeftSideQuery_FindsClassZeroNeighbor()
    {
        var result = await Create(Features, Labels, [-3.2d, 1.6d]).ExecuteAsync();

        Assert.Equal(1, result.NearestPointIndex);
        Assert.Equal(0, result.NearestLabel);
        Assert.Equal(Math.Sqrt(.2d), result.NearestDistance, 10);
    }

    [Fact]
    public async Task ExactMatch_HasZeroDistance()
    {
        var result = await Create(Features, Labels, [2d, 2d]).ExecuteAsync();

        Assert.Equal(5, result.NearestPointIndex);
        Assert.Equal(0d, result.NearestDistance, 12);
    }

    [Fact]
    public async Task QueryNearSplitPlane_ExploresMoreThanOneDescentPath()
    {
        var result = await Create(Features, Labels, [.1d, .8d]).ExecuteAsync();

        Assert.Equal(3, result.NearestPointIndex);
        Assert.True(result.VisitedNodes >= 4);
        Assert.True(result.PrunedNodes < result.Features.Length - 1);
    }

    [Fact]
    public async Task ThreeDimensionalCore_CyclesAxesAndRemainsDimensionIndependent()
    {
        var result = await Create(
            [[0d,0d,0d],[1d,1d,1d],[3d,3d,3d],[8d,8d,8d],[9d,9d,9d]],
            [0,0,0,1,1],
            [1.2d,1.1d,.9d]).ExecuteAsync();

        Assert.Equal(1, result.NearestPointIndex);
        Assert.Equal(3, result.Dimension);
        Assert.Contains(result.Nodes, node => node.Axis == 2);
    }

    [Fact]
    public void Configure_RejectsMismatchedDimensions()
    {
        var simulation = new KdTreeSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new KdTreeConfiguration(
            [[0d,0d],[1d,1d,1d],[2d,2d]], [0,1,1], [0.5d,0.5d])));
    }

    private static KdTreeSimulation Create(double[][] features, int[] labels, double[] query)
    {
        var simulation = new KdTreeSimulation(new ImmediateRuntime());
        simulation.Configure(new KdTreeConfiguration(features, labels, query));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

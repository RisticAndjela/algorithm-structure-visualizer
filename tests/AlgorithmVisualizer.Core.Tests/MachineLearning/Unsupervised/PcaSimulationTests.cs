using AlgorithmVisualizer.Core.MachineLearning.Unsupervised.Pca;
using AlgorithmVisualizer.Core.Simulation;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Unsupervised;

public sealed class PcaSimulationTests
{
    [Fact]
    public async Task DiagonalData_FirstComponent_IsDiagonalAndExplainsMostVariance()
    {
        var simulation = new PcaSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new PcaConfiguration([
            [-4d, -4.1d], [-3d, -2.9d], [-2d, -2.1d], [2d, 2.2d], [3d, 2.8d], [4d, 4.1d]
        ]));

        var result = await simulation.ExecuteAsync();

        Assert.True(Math.Abs(result.PrincipalComponent[0]) > 0.6d);
        Assert.True(Math.Abs(result.PrincipalComponent[1]) > 0.6d);
        Assert.True(result.ExplainedVarianceRatio > 0.95d);
    }

    [Fact]
    public async Task HorizontalData_FirstComponent_FavorsFirstFeature()
    {
        var simulation = new PcaSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new PcaConfiguration([
            [-5d, .1d], [-3d, -.2d], [-1d, .2d], [1d, -.1d], [3d, .15d], [5d, -.05d]
        ]));

        var result = await simulation.ExecuteAsync();

        Assert.True(Math.Abs(result.PrincipalComponent[0]) > 0.95d);
        Assert.True(Math.Abs(result.PrincipalComponent[1]) < 0.32d);
    }

    [Fact]
    public async Task ProjectionCoordinates_AreOneValuePerPoint()
    {
        var simulation = new PcaSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new PcaConfiguration([[1d, 2d], [2d, 3d], [4d, 6d], [5d, 7d]]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(result.Count, result.Projections.Length);
        Assert.Equal(result.Count, result.ProjectedFeatures.Length);
    }

    [Fact]
    public async Task ThreeDimensionalCore_RemainsDimensionIndependent()
    {
        var simulation = new PcaSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new PcaConfiguration([
            [-3d, -2.8d, -3.1d], [-1d, -.9d, -1.2d], [1d, 1.1d, .9d], [3d, 2.9d, 3.2d]
        ]));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(3, result.Dimension);
        Assert.Equal(3, result.PrincipalComponent.Length);
        Assert.True(result.ExplainedVarianceRatio > 0.9d);
    }

    [Fact]
    public void Configure_RejectsMismatchedDimensions()
    {
        var simulation = new PcaSimulation(new ImmediateSimulationRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new PcaConfiguration([[1d, 2d], [3d, 4d, 5d]])));
    }
}

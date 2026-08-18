using AlgorithmVisualizer.Core.MachineLearning.Supervised.LinearRegression;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Supervised;

public sealed class LinearRegressionSimulationTests
{
    [Fact]
    public async Task StableLearningRate_FitsSimpleLine()
    {
        var simulation = Create([0d, 1d, 2d, 3d, 4d], [1d, 3d, 5d, 7d, 9d], 0d, 0d, 0.05d, 120, 0.01d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.InRange(result.FinalWeight, 1.9d, 2.1d);
        Assert.InRange(result.FinalBias, 0.8d, 1.2d);
    }

    [Fact]
    public async Task AlreadyFittedLine_StopsWithoutUpdate()
    {
        var simulation = Create([0d, 1d, 2d], [1d, 3d, 5d], 2d, 1d, 0.05d, 20, 0.001d);

        var result = await simulation.ExecuteAsync();

        Assert.True(result.Converged);
        Assert.Equal(0, result.IterationsCompleted);
        Assert.Equal(0d, result.InitialLoss, 10);
    }

    [Fact]
    public async Task NegativeTrend_LearnsNegativeWeight()
    {
        var simulation = Create([0d, 1d, 2d, 3d], [6d, 4d, 2d, 0d], 0d, 0d, 0.05d, 120, 0.02d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(result.FinalWeight < -1.5d);
        Assert.True(result.FinalLoss < result.InitialLoss);
    }

    [Fact]
    public async Task OversizedLearningRate_TriggersDivergenceGuard()
    {
        var simulation = Create([0d, 1d, 2d, 3d, 4d], [1d, 3d, 5d, 7d, 9d], 0d, 0d, 1.2d, 20, 0.01d);

        var result = await simulation.ExecuteAsync();

        Assert.True(result.Diverged);
        Assert.Equal(LinearRegressionStopReason.Diverged, result.StopReason);
    }

    [Fact]
    public void Configure_RejectsMismatchedPointCounts()
    {
        var simulation = new LinearRegressionSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new LinearRegressionConfiguration(
            [0d, 1d], [1d], 0d, 0d, 0.05d, 20, 0.01d)));
    }

    private static LinearRegressionSimulation Create(double[] x, double[] y, double w, double b, double lr, int max, double tolerance)
    {
        var simulation = new LinearRegressionSimulation(new ImmediateRuntime());
        simulation.Configure(new LinearRegressionConfiguration(x, y, w, b, lr, max, tolerance));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

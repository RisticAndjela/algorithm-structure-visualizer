using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Optimizers;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class OptimizerSimulationTests
{
    [Theory]
    [InlineData(OptimizerKind.Sgd)]
    [InlineData(OptimizerKind.Momentum)]
    [InlineData(OptimizerKind.Adam)]
    public async Task Optimizers_KeepFinitePathAndReduceMse(OptimizerKind kind)
    {
        var simulation = new OptimizerSimulation(new ImmediateRuntime());
        simulation.Configure(new OptimizerConfiguration(
            [-1d, 0d, 1d, 2d], [-1d, 1d, 3d, 5d],
            0d, 0d, kind == OptimizerKind.Adam ? 0.05d : 0.08d, 20, kind));
        var result = await simulation.ExecuteAsync();
        Assert.True(result.Improved);
        Assert.Equal(21, result.WeightPath.Length);
        Assert.All(result.WeightPath, value => Assert.True(double.IsFinite(value)));
        Assert.All(result.BiasPath, value => Assert.True(double.IsFinite(value)));
    }

    [Fact]
    public void Configure_RejectsMismatchedDataset()
    {
        var simulation = new OptimizerSimulation(new ImmediateRuntime());
        Assert.Throws<ArgumentException>(() => simulation.Configure(new OptimizerConfiguration([1d, 2d], [1d], 0d, 0d, 0.1d, 4, OptimizerKind.Sgd)));
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

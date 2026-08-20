using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Backpropagation;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class BackpropagationSimulationTests
{
    [Fact]
    public async Task Backpropagation_ProducesFiniteGradientsAndImprovesLoss()
    {
        var simulation = new BackpropagationSimulation(new ImmediateRuntime());
        simulation.Configure(new BackpropagationConfiguration(
            [1d, 0.5d], 2,
            [0.6d, -0.2d, -0.4d, 0.7d],
            [0.1d, -0.1d],
            [0.8d, -0.6d], 0.05d,
            ActivationKind.Tanh, ActivationKind.Sigmoid,
            1d, 0.2d));
        var result = await simulation.ExecuteAsync();
        Assert.True(result.Improved);
        Assert.All(result.HiddenDeltas, value => Assert.True(double.IsFinite(value)));
        Assert.All(result.HiddenWeightGradients, value => Assert.True(double.IsFinite(value)));
        Assert.All(result.OutputWeightGradients, value => Assert.True(double.IsFinite(value)));
    }

    [Fact]
    public async Task ZeroTarget_StillRunsReverseChain()
    {
        var simulation = new BackpropagationSimulation(new ImmediateRuntime());
        simulation.Configure(new BackpropagationConfiguration(
            [0.2d], 1, [0.4d], [0.1d], [0.5d], 0d,
            ActivationKind.Tanh, ActivationKind.Sigmoid, 0d, 0.1d));
        var result = await simulation.ExecuteAsync();
        Assert.NotEqual(0d, result.OutputDelta);
        Assert.True(result.LossAfter <= result.LossBefore + 1e-12d);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

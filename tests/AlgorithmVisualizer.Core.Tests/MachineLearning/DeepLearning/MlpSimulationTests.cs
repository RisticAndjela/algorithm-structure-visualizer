using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Mlp;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class MlpSimulationTests
{
    [Fact]
    public async Task ForwardPass_ProducesExpectedLinearOutput()
    {
        var simulation = new MlpSimulation(new ImmediateRuntime());
        simulation.Configure(new MlpConfiguration(
            [1d, 2d], 2,
            [1d, 0d, 0d, 1d],
            [0d, 0d],
            [2d, 3d], 1d,
            ActivationKind.Linear, ActivationKind.Linear));
        var result = await simulation.ExecuteAsync();
        Assert.Equal(new[] { 1d, 2d }, result.HiddenActivations);
        Assert.Equal(9d, result.Output, 10);
    }

    [Fact]
    public async Task HiddenReLU_ClampsNegativeNeuron()
    {
        var simulation = new MlpSimulation(new ImmediateRuntime());
        simulation.Configure(new MlpConfiguration(
            [1d], 2,
            [-2d, 3d],
            [0d, 0d],
            [1d, 1d], 0d,
            ActivationKind.ReLU, ActivationKind.Linear));
        var result = await simulation.ExecuteAsync();
        Assert.Equal(0d, result.HiddenActivations[0], 10);
        Assert.Equal(3d, result.HiddenActivations[1], 10);
        Assert.Equal(3d, result.Output, 10);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;
using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Neuron;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class NeuronSimulationTests
{
    [Fact]
    public async Task ReLUNeuron_ComputesWeightedSumAndActivation()
    {
        var simulation = new NeuronSimulation(new ImmediateRuntime());
        simulation.Configure(new NeuronConfiguration([1d, 2d], [2d, -0.5d], 0.5d, ActivationKind.ReLU));
        var result = await simulation.ExecuteAsync();
        Assert.Equal(1.5d, result.PreActivation, 10);
        Assert.Equal(1.5d, result.Output, 10);
        Assert.Equal(new[] { 2d, -1d }, result.Contributions);
    }

    [Fact]
    public async Task SigmoidNeuron_MapsZeroToHalf()
    {
        var simulation = new NeuronSimulation(new ImmediateRuntime());
        simulation.Configure(new NeuronConfiguration([0d], [3d], 0d, ActivationKind.Sigmoid));
        var result = await simulation.ExecuteAsync();
        Assert.Equal(0.5d, result.Output, 10);
    }

    [Fact]
    public void Configure_RejectsMismatchedShapes()
    {
        var simulation = new NeuronSimulation(new ImmediateRuntime());
        Assert.Throws<ArgumentException>(() => simulation.Configure(new NeuronConfiguration([1d, 2d], [1d], 0d, ActivationKind.ReLU)));
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

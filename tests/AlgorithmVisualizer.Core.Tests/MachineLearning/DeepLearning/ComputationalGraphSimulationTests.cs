using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.ComputationalGraph;
using AlgorithmVisualizer.Core.Simulation;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class ComputationalGraphSimulationTests
{
    [Fact]
    public async Task WeightedBiasGraph_EvaluatesDependenciesInOrder()
    {
        var simulation = new ComputationalGraphSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new ComputationalGraphConfiguration(
        [
            Input(0, "x", 2d),
            Input(1, "w", 3d),
            Input(2, "b", -1d),
            Operation(3, "x × w", ComputationalGraphOperation.Multiply, 0, 1),
            Operation(4, "output", ComputationalGraphOperation.Add, 3, 2)
        ], 4));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(5d, result.OutputValue, 10);
        Assert.Equal(new[] { 3, 4 }, result.EvaluationSequence);
        Assert.Equal(4, result.EdgeCount);
    }

    [Fact]
    public async Task BranchAndMerge_AllowsTwoNodesToBecomeReadyTogether()
    {
        var simulation = new ComputationalGraphSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new ComputationalGraphConfiguration(
        [
            Input(0, "x", 4d),
            Input(1, "y", 1d),
            Operation(2, "x + y", ComputationalGraphOperation.Add, 0, 1),
            Operation(3, "x − y", ComputationalGraphOperation.Subtract, 0, 1),
            Operation(4, "output", ComputationalGraphOperation.Multiply, 2, 3)
        ], 4));

        var initial = simulation.CreateSnapshot();
        var result = await simulation.ExecuteAsync();

        Assert.Equal(2, initial.ReadyCount);
        Assert.Equal(15d, result.OutputValue, 10);
        Assert.Equal(new[] { 2, 3, 4 }, result.EvaluationSequence);
    }

    [Fact]
    public async Task SquareNode_UsesOneDependency()
    {
        var simulation = new ComputationalGraphSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new ComputationalGraphConfiguration(
        [
            Input(0, "x", 2d),
            Input(1, "b", 3d),
            Operation(2, "x + b", ComputationalGraphOperation.Add, 0, 1),
            Operation(3, "output", ComputationalGraphOperation.Square, 2)
        ], 3));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(25d, result.OutputValue, 10);
        Assert.Equal(new[] { 2, 3 }, result.EvaluationSequence);
    }

    [Fact]
    public async Task Reconfigure_UsesCurrentInputValues()
    {
        var simulation = new ComputationalGraphSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new ComputationalGraphConfiguration(
        [
            Input(0, "a", -2d),
            Input(1, "b", 5d),
            Operation(2, "output", ComputationalGraphOperation.Add, 0, 1)
        ], 2));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(3d, result.OutputValue, 10);
    }

    [Fact]
    public async Task CyclicDependencies_AreRejectedByForwardExecution()
    {
        var simulation = new ComputationalGraphSimulation(new ImmediateSimulationRuntime());
        simulation.Configure(new ComputationalGraphConfiguration(
        [
            Input(0, "x", 1d),
            Operation(1, "a", ComputationalGraphOperation.Add, 0, 2),
            Operation(2, "output", ComputationalGraphOperation.Multiply, 0, 1)
        ], 2));

        await Assert.ThrowsAsync<InvalidOperationException>(() => simulation.ExecuteAsync());
    }

    private static ComputationalGraphNodeDefinition Input(int id, string label, double value) =>
        new(id, label, ComputationalGraphNodeKind.Input, ComputationalGraphOperation.None, [], value);

    private static ComputationalGraphNodeDefinition Operation(int id, string label, ComputationalGraphOperation operation, params int[] inputs) =>
        new(id, label, ComputationalGraphNodeKind.Operation, operation, inputs);
}

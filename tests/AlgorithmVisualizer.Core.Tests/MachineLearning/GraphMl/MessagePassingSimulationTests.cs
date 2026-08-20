using AlgorithmVisualizer.Core.MachineLearning.GraphMl.MessagePassing;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.GraphMl;

public sealed class MessagePassingSimulationTests
{
    [Fact]
    public async Task MeanAggregation_UsesOldLayerForEveryNodeThenCommitsTogether()
    {
        var simulation = new MessagePassingSimulation(new ImmediateRuntime());
        simulation.Configure(new MessagePassingConfiguration(
            Undirected(3, [(0,1),(1,2)]),
            [[1d,0d],[0d,1d],[1d,1d]],
            Identity2(),
            Identity2(),
            [0d,0d],
            MessageAggregation.Mean,
            1));

        var result = await simulation.ExecuteAsync();

        AssertVector([1d,1d], result.FinalFeatures[0]);
        AssertVector([1d,1.5d], result.FinalFeatures[1]);
        AssertVector([1d,2d], result.FinalFeatures[2]);
    }

    [Fact]
    public async Task IsolatedNode_UsesZeroNeighborMessageAndKeepsFiniteEmbedding()
    {
        var simulation = new MessagePassingSimulation(new ImmediateRuntime());
        simulation.Configure(new MessagePassingConfiguration(
            Undirected(3, [(0,1)]),
            [[1d,0d],[0d,1d],[0.5d,0.25d]],
            Identity2(),
            Identity2(),
            [0d,0d],
            MessageAggregation.Mean,
            1));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(0, result.NeighborCounts[2]);
        AssertVector([0.5d,0.25d], result.FinalFeatures[2]);
        Assert.All(result.FinalFeatures[2], value => Assert.True(double.IsFinite(value)));
    }

    [Fact]
    public async Task TwoLayers_PropagateInformationTwoHops()
    {
        var configuration = new MessagePassingConfiguration(
            Undirected(3, [(0,1),(1,2)]),
            [[2d],[0d],[0d]],
            [[0d]],
            [[1d]],
            [0d],
            MessageAggregation.Sum,
            2);
        var simulation = new MessagePassingSimulation(new ImmediateRuntime());
        simulation.Configure(configuration);

        var result = await simulation.ExecuteAsync();

        Assert.True(result.FinalFeatures[2][0] > 0d);
        Assert.Equal(2, result.Layers);
    }

    private static double[][] Identity2() => [[1d,0d],[0d,1d]];

    private static double[][] Undirected(int n, (int A, int B)[] edges)
    {
        var adjacency = new double[n][];
        for (var row = 0; row < n; row++) adjacency[row] = new double[n];
        foreach (var edge in edges)
        {
            adjacency[edge.A][edge.B] = 1d;
            adjacency[edge.B][edge.A] = 1d;
        }
        return adjacency;
    }

    private static void AssertVector(double[] expected, double[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++) Assert.Equal(expected[index], actual[index], 10);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

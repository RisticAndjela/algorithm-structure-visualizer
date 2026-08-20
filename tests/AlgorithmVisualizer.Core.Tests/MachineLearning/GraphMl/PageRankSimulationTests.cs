using AlgorithmVisualizer.Core.MachineLearning.GraphMl.PageRank;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.GraphMl;

public sealed class PageRankSimulationTests
{
    [Fact]
    public async Task DirectedCycle_RemainsUniform()
    {
        var simulation = new PageRankSimulation(new ImmediateRuntime());
        simulation.Configure(new PageRankConfiguration(
            [[0d,1d,0d],[0d,0d,1d],[1d,0d,0d]],
            MaxIterations: 20,
            Tolerance: 1e-10));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(1d, result.RankSum, 10);
        Assert.All(result.Ranks, rank => Assert.Equal(1d / 3d, rank, 8));
        Assert.True(result.Converged);
    }

    [Fact]
    public async Task IncomingHub_BecomesTopRankedNode()
    {
        var simulation = new PageRankSimulation(new ImmediateRuntime());
        simulation.Configure(new PageRankConfiguration(
            [[0d,1d,0d,0d],[1d,0d,0d,0d],[1d,0d,0d,0d],[1d,0d,0d,0d]],
            MaxIterations: 40,
            Tolerance: 1e-8));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(0, result.TopNode);
        Assert.True(result.TopRank > result.Ranks[2]);
        Assert.Equal(1d, result.RankSum, 8);
    }

    [Fact]
    public async Task DanglingNode_DoesNotLoseProbabilityMass()
    {
        var simulation = new PageRankSimulation(new ImmediateRuntime());
        simulation.Configure(new PageRankConfiguration(
            [[0d,1d,0d],[0d,0d,0d],[1d,0d,0d]],
            MaxIterations: 30,
            Tolerance: 1e-8));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(1d, result.RankSum, 8);
        Assert.All(result.Ranks, rank => Assert.True(rank > 0d && double.IsFinite(rank)));
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

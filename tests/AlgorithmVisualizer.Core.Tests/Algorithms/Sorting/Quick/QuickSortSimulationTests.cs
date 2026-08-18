using AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.Quick;

public sealed class QuickSortSimulationTests
{
    [Fact]
    public async Task SortAsync_LomutoSortsClassicInputInPlace()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([4, 2, 7, 3, 1, 6]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.LomutoLastPivot, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 6, 7 }, result.SortedValues);
        Assert.True(result.Partitions > 0);
        Assert.True(result.Comparisons > 0);
        Assert.Equal("O(1)", result.ExtraArraySpaceComplexity);
        Assert.False(result.StableAlgorithm);
    }

    [Fact]
    public async Task SortAsync_LomutoSortedInputShowsLastPivotWorstShape()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.LomutoLastPivot, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(4, result.Partitions);
        Assert.Equal(5, result.MaxDepth);
        Assert.Equal("Θ(n²)", result.WorstCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_AdvancedMedianThreeWayReducesSortedInputDepth()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.MedianOfThreeThreeWay, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.True(result.MaxDepth < 5);
        Assert.True(result.HandlesDuplicateHeavyInputBetter);
    }

    [Fact]
    public async Task SortAsync_AdvancedFinishesDuplicateBandWithoutRecursingThroughEquals()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([3, 3, 2, 3, 1, 3, 3]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.MedianOfThreeThreeWay, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 3, 3, 3, 3 }, result.SortedValues);
        Assert.True(result.Partitions < result.SortedValues.Length - 1);
        Assert.True(result.HandlesDuplicateHeavyInputBetter);
        Assert.Equal("Θ(n) when all values equal", result.BestCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_LomutoCanReverseEqualValueIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.LomutoLastPivot, runtime.SimulationCancellationToken);
        var duplicateOriginalIndexes = simulation.CreateSnapshot().Elements
            .Where(element => element.Value == 2)
            .Select(element => element.OriginalIndex)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 2 }, result.SortedValues);
        Assert.False(result.PreservedEqualValueOrder);
        Assert.Equal(new[] { 1, 0 }, duplicateOriginalIndexes);
        Assert.False(result.StableAlgorithm);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoPartition()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new QuickSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(QuickSortVariant.LomutoLastPivot, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Partitions);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.True(result.ActiveMutationRequiresRestart);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        private CancellationTokenSource _cancellation = new();
        public CancellationToken SimulationCancellationToken => _cancellation.Token;
        public string CurrentStep { get; private set; } = "Ready.";
        public void Start() { _cancellation.Dispose(); _cancellation = new CancellationTokenSource(); }
        public void SetCurrentStep(string description) => CurrentStep = description;
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

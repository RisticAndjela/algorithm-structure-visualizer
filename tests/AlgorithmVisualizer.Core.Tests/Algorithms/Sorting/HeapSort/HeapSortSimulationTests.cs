using AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.HeapSort;

public sealed class HeapSortSimulationTests
{
    [Theory]
    [InlineData(HeapSortVariant.IncrementalBuild)]
    [InlineData(HeapSortVariant.FloydBottomUp)]
    public async Task SortAsync_BothVariantsSortClassicInputInPlace(HeapSortVariant variant)
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([4, 10, 3, 5, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(variant, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 3, 4, 5, 10 }, result.SortedValues);
        Assert.Equal(4, result.Extractions);
        Assert.Equal("Θ(n log n)", result.WorstCaseComplexity);
        Assert.Equal("O(1)", result.ExtraArraySpaceComplexity);
        Assert.False(result.StableAlgorithm);
        Assert.True(result.ActiveMutationRequiresRestart);
    }

    [Fact]
    public async Task SortAsync_IncrementalBuildShowsRepeatedBubbleUpWork()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5, 6, 7]);

        runtime.Start();
        var result = await simulation.SortAsync(HeapSortVariant.IncrementalBuild, runtime.SimulationCancellationToken);

        Assert.Equal(10, result.BuildComparisons);
        Assert.Equal(10, result.BuildSwaps);
        Assert.Equal("O(n log n)", result.BuildComplexity);
        Assert.False(result.UsesLinearHeapConstruction);
    }

    [Fact]
    public async Task SortAsync_FloydBuildUsesLinearBottomUpConstruction()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5, 6, 7]);

        runtime.Start();
        var result = await simulation.SortAsync(HeapSortVariant.FloydBottomUp, runtime.SimulationCancellationToken);

        Assert.Equal(8, result.BuildComparisons);
        Assert.Equal(4, result.BuildSwaps);
        Assert.Equal("Θ(n)", result.BuildComplexity);
        Assert.True(result.UsesLinearHeapConstruction);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, result.SortedValues);
    }

    [Fact]
    public async Task SortAsync_ExistingMaxHeapNeedsNoFloydBuildSwapsButStillNeedsExtraction()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([7, 6, 5, 4, 3, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(HeapSortVariant.FloydBottomUp, runtime.SimulationCancellationToken);

        Assert.Equal(0, result.BuildSwaps);
        Assert.Equal(6, result.Extractions);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, result.SortedValues);
    }

    [Fact]
    public async Task SortAsync_DistantHeapSwapsCanReverseEqualIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(HeapSortVariant.IncrementalBuild, runtime.SimulationCancellationToken);
        var duplicateOriginalIndexes = simulation.CreateSnapshot().Elements
            .Where(element => element.Value == 2)
            .Select(element => element.OriginalIndex)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 2 }, result.SortedValues);
        Assert.False(result.PreservedEqualValueOrder);
        Assert.Equal(new[] { 1, 0 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoHeapWork()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new HeapSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(HeapSortVariant.FloydBottomUp, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Extractions);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(0, result.BuildComparisons);
        Assert.Equal(0, result.BuildSwaps);
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

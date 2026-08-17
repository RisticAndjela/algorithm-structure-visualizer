using AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.Merge;

public sealed class MergeSortSimulationTests
{
    [Fact]
    public async Task SortAsync_TopDownClassicExampleUsesExpectedDivideAndMergeWork()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([8, 3, 5, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.TopDownRecursive, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 3, 5, 8 }, result.SortedValues);
        Assert.Equal(3, result.Splits);
        Assert.Equal(3, result.Merges);
        Assert.Equal(5, result.Comparisons);
        Assert.Equal(16, result.Writes);
        Assert.True(result.PreservedEqualValueOrder);
        Assert.Equal("Θ(n log n)", result.WorstCaseComplexity);
        Assert.Equal("O(n)", result.ExtraSpaceComplexity);
    }

    [Fact]
    public async Task SortAsync_TopDownSortedInputStillUsesCanonicalRecursionTree()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.TopDownRecursive, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4 }, result.SortedValues);
        Assert.Equal(3, result.Splits);
        Assert.Equal(3, result.Merges);
        Assert.Equal(4, result.Comparisons);
        Assert.Equal("Θ(n log n)", result.BestCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_NaturalSortedInputDetectsOneRunAndSkipsMerging()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.NaturalRuns, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(1, result.InitialNaturalRunCount);
        Assert.Equal(0, result.Merges);
        Assert.Equal(0, result.NaturalPasses);
        Assert.Equal(4, result.Comparisons);
        Assert.Equal("Θ(n) on one natural run", result.BestCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_NaturalTwoRunsUsesOneMergePass()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([1, 3, 5, 2, 4, 6]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.NaturalRuns, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, result.SortedValues);
        Assert.Equal(2, result.InitialNaturalRunCount);
        Assert.Equal(1, result.Merges);
        Assert.Equal(1, result.NaturalPasses);
        Assert.True(result.PreservedEqualValueOrder);
    }

    [Fact]
    public async Task SortAsync_TopDownPreservesDuplicateIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1, 2]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.TopDownRecursive, runtime.SimulationCancellationToken);
        var duplicateOriginalIndexes = simulation.CreateSnapshot().Elements
            .Where(element => element.Value == 2)
            .Select(element => element.OriginalIndex)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 2, 2 }, result.SortedValues);
        Assert.True(result.PreservedEqualValueOrder);
        Assert.Equal(new[] { 0, 1, 3 }, duplicateOriginalIndexes);
        Assert.True(result.StableAlgorithm);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoWorkAndMutationsRequireNewRun()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new MergeSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(MergeSortVariant.TopDownRecursive, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Splits);
        Assert.Equal(0, result.Merges);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Writes);
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

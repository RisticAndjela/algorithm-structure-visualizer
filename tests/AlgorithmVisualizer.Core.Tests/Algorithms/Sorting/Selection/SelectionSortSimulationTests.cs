using AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.Selection;

public sealed class SelectionSortSimulationTests
{
    [Fact]
    public async Task SortAsync_ClassicExampleFindsMinimumBeforeEachSwap()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([64, 25, 12, 22, 11]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 11, 12, 22, 25, 64 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(3, result.Swaps);
        Assert.Equal(4, result.Passes);
    }

    [Fact]
    public async Task SortAsync_AlreadySortedStillPerformsQuadraticComparisons()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(4, result.Passes);
    }

    [Fact]
    public async Task SortAsync_ReverseInputUsesSameComparisonCountButFewDirectSwaps()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([5, 4, 3, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(2, result.Swaps);
    }

    [Fact]
    public async Task SortAsync_DirectSwapCanReverseEqualItemIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);
        var snapshot = simulation.CreateSnapshot();

        Assert.Equal(new[] { 1, 2, 2 }, result.SortedValues);
        Assert.False(result.PreservedEqualValueOrder);
        var duplicateOriginalIndexes = snapshot.Elements.Where(element => element.Value == 2).Select(element => element.OriginalIndex).ToArray();
        Assert.Equal(new[] { 1, 0 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_StableShiftPreservesDuplicateOrderWithoutDirectSwaps()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(SelectionSortVariant.StableShift, runtime.SimulationCancellationToken);
        var snapshot = simulation.CreateSnapshot();

        Assert.Equal(new[] { 1, 2, 2 }, result.SortedValues);
        Assert.Equal(SelectionSortVariant.StableShift, result.Variant);
        Assert.True(result.StableAlgorithm);
        Assert.True(result.PreservedEqualValueOrder);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(3, result.Moves);
        var duplicateOriginalIndexes = snapshot.Elements.Where(element => element.Value == 2).Select(element => element.OriginalIndex).ToArray();
        Assert.Equal(new[] { 0, 1 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoScanOrSwap()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new SelectionSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(0, result.Passes);
        Assert.All(simulation.CreateSnapshot().Elements, element => Assert.Equal(SelectionSortElementVisualState.Sorted, element.VisualState));
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

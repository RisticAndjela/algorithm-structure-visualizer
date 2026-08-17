using AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.Bubble;

public sealed class BubbleSortSimulationTests
{
    [Fact]
    public async Task SortAsync_SortsAscendingWithExpectedWork()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new BubbleSortSimulation(runtime);
        simulation.LoadValues([5, 1, 4, 2, 8]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 4, 5, 8 }, result.SortedValues);
        Assert.Equal(9, result.Comparisons);
        Assert.Equal(4, result.Swaps);
        Assert.Equal(3, result.Passes);
        Assert.True(result.UsedEarlyExit);
        Assert.True(result.Stable);
    }

    [Fact]
    public async Task SortAsync_AlreadySortedInputUsesOneLinearPass()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new BubbleSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(4, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(1, result.Passes);
        Assert.True(result.UsedEarlyExit);
    }

    [Fact]
    public async Task SortAsync_ReverseInputShowsQuadraticWorstCase()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new BubbleSortSimulation(runtime);
        simulation.LoadValues([5, 4, 3, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(10, result.Swaps);
        Assert.Equal(4, result.Passes);
        Assert.False(result.UsedEarlyExit);
    }

    [Fact]
    public async Task SortAsync_DuplicateValuesKeepOriginalRelativeOrder()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new BubbleSortSimulation(runtime);
        simulation.LoadValues([3, 2, 3, 1, 3]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);
        var snapshot = simulation.CreateSnapshot();

        Assert.Equal(new[] { 1, 2, 3, 3, 3 }, result.SortedValues);
        Assert.True(result.Stable);

        var duplicateOriginalIndexes = snapshot.Elements
            .Where(element => element.Value == 3)
            .Select(element => element.OriginalIndex)
            .ToArray();
        Assert.Equal(new[] { 0, 2, 4 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoComparison()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new BubbleSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Swaps);
        Assert.Equal(0, result.Passes);
        Assert.True(simulation.CreateSnapshot().Elements.All(element => element.VisualState == BubbleSortElementVisualState.Sorted));
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        private CancellationTokenSource _cancellation = new();

        public CancellationToken SimulationCancellationToken => _cancellation.Token;
        public string CurrentStep { get; private set; } = "Ready.";

        public void Start()
        {
            _cancellation.Dispose();
            _cancellation = new CancellationTokenSource();
        }

        public void SetCurrentStep(string description) => CurrentStep = description;
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

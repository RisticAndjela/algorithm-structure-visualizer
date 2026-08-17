using AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Sorting.Insertion;

public sealed class InsertionSortSimulationTests
{
    [Fact]
    public async Task SortAsync_LinearClassicExampleUsesExpectedComparisonsAndShifts()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([5, 2, 4, 6, 1, 3]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, result.SortedValues);
        Assert.Equal(12, result.Comparisons);
        Assert.Equal(9, result.Shifts);
        Assert.Equal(14, result.Writes);
        Assert.Equal(5, result.Passes);
        Assert.True(result.StableAlgorithm);
    }

    [Fact]
    public async Task SortAsync_LinearAlreadySortedIsAdaptiveLinearBestCase()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([1, 2, 3, 4, 5]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(4, result.Comparisons);
        Assert.Equal(0, result.Shifts);
        Assert.Equal("Θ(n)", result.BestCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_LinearReverseInputMovesEveryEarlierValue()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([5, 4, 3, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.SortedValues);
        Assert.Equal(10, result.Comparisons);
        Assert.Equal(10, result.Shifts);
        Assert.Equal("Θ(n²)", result.WorstCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_LinearPreservesDuplicateIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([2, 2, 1]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, runtime.SimulationCancellationToken);
        var duplicateOriginalIndexes = simulation.CreateSnapshot().Elements
            .Where(element => element.Occupied && element.Value == 2)
            .Select(element => element.OriginalIndex)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 2 }, result.SortedValues);
        Assert.True(result.PreservedEqualValueOrder);
        Assert.Equal(new[] { 0, 1 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_BinaryInsertionReducesComparisonsButKeepsSameShifts()
    {
        var linearRuntime = new ImmediateRuntime();
        var linear = new InsertionSortSimulation(linearRuntime);
        linear.LoadValues([5, 2, 4, 6, 1, 3]);
        linearRuntime.Start();
        var linearResult = await linear.SortAsync(InsertionSortVariant.Linear, linearRuntime.SimulationCancellationToken);

        var binaryRuntime = new ImmediateRuntime();
        var binary = new InsertionSortSimulation(binaryRuntime);
        binary.LoadValues([5, 2, 4, 6, 1, 3]);
        binaryRuntime.Start();
        var binaryResult = await binary.SortAsync(InsertionSortVariant.BinarySearch, binaryRuntime.SimulationCancellationToken);

        Assert.Equal(linearResult.SortedValues, binaryResult.SortedValues);
        Assert.Equal(10, binaryResult.Comparisons);
        Assert.True(binaryResult.Comparisons < linearResult.Comparisons);
        Assert.Equal(linearResult.Shifts, binaryResult.Shifts);
        Assert.Equal(9, binaryResult.Shifts);
        Assert.True(binaryResult.PreservedEqualValueOrder);
        Assert.Equal("Θ(n log n)", binaryResult.BestCaseComplexity);
    }

    [Fact]
    public async Task SortAsync_BinaryInsertionUsesUpperBoundAndPreservesDuplicateIdentity()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([3, 1, 3, 2, 3]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.BinarySearch, runtime.SimulationCancellationToken);
        var duplicateOriginalIndexes = simulation.CreateSnapshot().Elements
            .Where(element => element.Occupied && element.Value == 3)
            .Select(element => element.OriginalIndex)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 3, 3, 3 }, result.SortedValues);
        Assert.True(result.PreservedEqualValueOrder);
        Assert.Equal(new[] { 0, 2, 4 }, duplicateOriginalIndexes);
    }

    [Fact]
    public async Task SortAsync_SingleElementNeedsNoWork()
    {
        var runtime = new ImmediateRuntime();
        var simulation = new InsertionSortSimulation(runtime);
        simulation.LoadValues([42]);

        runtime.Start();
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, runtime.SimulationCancellationToken);

        Assert.Equal(new[] { 42 }, result.SortedValues);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.Shifts);
        Assert.Equal(0, result.Writes);
        Assert.Equal(0, result.Passes);
        Assert.True(result.SupportsOnlineInsertion);
        Assert.True(result.DeletePreservesSortedOrder);
        Assert.True(result.UpdateCanBeRepairedByReinsert);
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

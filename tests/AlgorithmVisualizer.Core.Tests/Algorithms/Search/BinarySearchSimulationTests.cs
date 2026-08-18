using AlgorithmVisualizer.Core.Algorithms.Search.Binary;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Search;

public sealed class BinarySearchSimulationTests
{
    [Fact]
    public async Task Basic_MidpointMatch_IsBestCase()
    {
        var search = CreateSearch(1, 3, 5, 7, 9);
        var result = await search.SearchAsync(5);

        Assert.True(result.Found);
        Assert.Equal(2, result.FoundIndex);
        Assert.Equal(1, result.Comparisons);
        Assert.Equal("Θ(1)", result.BestCaseComplexity);
    }

    [Fact]
    public async Task MissingTarget_UsesFewerComparisonsThanLinearScan()
    {
        var search = CreateSearch(1, 3, 5, 7, 9, 11, 13);
        var result = await search.SearchAsync(8);

        Assert.False(result.Found);
        Assert.Null(result.FoundIndex);
        Assert.True(result.Comparisons < 7);
        Assert.True(result.RangeReductions > 0);
    }

    [Fact]
    public async Task BasicDuplicates_MayReturnNonFirstMatch()
    {
        var search = CreateSearch(1, 2, 2, 2, 3);
        var result = await search.SearchAsync(2, BinarySearchVariant.AnyMatch);

        Assert.True(result.Found);
        Assert.Equal(2, result.FoundIndex);
        Assert.Equal(1, result.FirstOccurrenceIndex);
        Assert.False(result.ReturnsFirstOccurrence);
    }

    [Fact]
    public async Task FirstOccurrenceVariant_ReturnsEarliestDuplicate()
    {
        var search = CreateSearch(1, 2, 2, 2, 3);
        var result = await search.SearchAsync(2, BinarySearchVariant.FirstOccurrence);

        Assert.True(result.Found);
        Assert.Equal(1, result.FoundIndex);
        Assert.True(result.ReturnsFirstOccurrence);
        Assert.True(result.Comparisons >= 2);
    }

    [Fact]
    public void LoadValues_UnsortedInput_IsRejected()
    {
        var search = new BinarySearchSimulation(new ImmediateSimulationRuntime());
        Assert.Throws<ArgumentException>(() => search.LoadValues([1, 5, 3, 7]));
    }


    [Fact]
    public async Task InputSorter_ReusesEveryImplementedSortAlgorithm_AndProducesValidBinarySearchInput()
    {
        var sorter = new BinarySearchInputSorter();
        var unsorted = new[] { 7, 1, 11, 3, 9, 5, 3 };
        var expected = new[] { 1, 3, 3, 5, 7, 9, 11 };

        foreach (var algorithm in Enum.GetValues<BinarySearchInputSortAlgorithm>())
        {
            var prepared = await sorter.SortAsync(unsorted, algorithm);

            Assert.Equal(expected, prepared.SortedValues);
            Assert.True(BinarySearchSimulation.IsSortedNondecreasing(prepared.SortedValues));
            Assert.False(string.IsNullOrWhiteSpace(prepared.ImplementationName));

            var search = CreateSearch(prepared.SortedValues);
            var result = await search.SearchAsync(9);
            Assert.True(result.Found);
        }
    }

    [Fact]
    public async Task Search_DoesNotMutateSortedInput()
    {
        var values = new[] { 1, 3, 5, 7, 9 };
        var search = CreateSearch(values);
        var result = await search.SearchAsync(7);

        Assert.Equal(values, result.InitialValues);
        Assert.Equal(values, search.CreateSnapshot().Elements.Select(element => element.Value).ToArray());
    }

    [Fact]
    public async Task EmptyArray_PerformsNoComparisons()
    {
        var search = CreateSearch();
        var result = await search.SearchAsync(4);
        Assert.False(result.Found);
        Assert.Equal(0, result.Comparisons);
    }

    private static BinarySearchSimulation CreateSearch(params int[] values)
    {
        var search = new BinarySearchSimulation(new ImmediateSimulationRuntime());
        search.LoadValues(values);
        return search;
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using AlgorithmVisualizer.Core.Algorithms.Search.Linear;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.Search;

public sealed class LinearSearchSimulationTests
{
    [Fact]
    public async Task Search_FirstElement_IsBestCase()
    {
        var search = CreateSearch(7, 4, 9, 2);
        var result = await search.SearchAsync(7);

        Assert.True(result.Found);
        Assert.Equal(0, result.FoundIndex);
        Assert.Equal(1, result.Comparisons);
        Assert.Equal("Θ(1)", result.BestCaseComplexity);
    }

    [Fact]
    public async Task Search_MissingValue_InspectsEveryElement()
    {
        var search = CreateSearch(3, 8, 1, 6, 4);
        var result = await search.SearchAsync(99);

        Assert.False(result.Found);
        Assert.Null(result.FoundIndex);
        Assert.Equal(5, result.Comparisons);
        Assert.Equal(5, result.CheckedCount);
    }

    [Fact]
    public async Task Search_Duplicates_ReturnsFirstOccurrence()
    {
        var search = CreateSearch(5, 2, 7, 2, 9);
        var result = await search.SearchAsync(2);

        Assert.True(result.Found);
        Assert.Equal(1, result.FoundIndex);
        Assert.Equal(1, result.FirstOccurrenceIndex);
        Assert.True(result.ReturnsFirstOccurrence);
        Assert.Equal(2, result.Comparisons);
    }

    [Fact]
    public async Task Search_EmptyArray_PerformsNoComparisons()
    {
        var search = CreateSearch();
        var result = await search.SearchAsync(4);

        Assert.False(result.Found);
        Assert.Equal(0, result.Comparisons);
        Assert.Equal(0, result.CheckedCount);
    }

    [Fact]
    public async Task Search_DoesNotReorderOrMutateInput()
    {
        var values = new[] { 9, 3, 7, 1 };
        var search = CreateSearch(values);
        var result = await search.SearchAsync(7);

        Assert.Equal(values, result.InitialValues);
        var snapshot = search.CreateSnapshot();
        Assert.Equal(values, snapshot.Elements.Select(element => element.Value).ToArray());
    }

    private static LinearSearchSimulation CreateSearch(params int[] values)
    {
        var search = new LinearSearchSimulation(new ImmediateSimulationRuntime());
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

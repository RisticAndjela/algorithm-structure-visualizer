using AlgorithmVisualizer.Core.DataStructures.Heap;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Heap;

public sealed class DaryHeapSimulationTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task MinHeap_Insert_PreservesDaryInvariant(int arity)
    {
        var heap = CreateHeap();
        Assert.True(heap.TrySetArity(arity));

        foreach (var value in new[] { 90, 70, 80, 60, 50, 40, 10, 65, 25, 30, 55 })
        {
            await heap.InsertAsync(value);
            AssertDaryInvariant(heap.CreateSnapshot());
        }

        Assert.Equal(10, heap.CreateSnapshot().Root?.Value);
        Assert.Equal(arity, heap.Arity);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task MaxHeap_ExtractRoot_ChoosesBestAmongAllChildren(int arity)
    {
        var heap = CreateHeap();
        Assert.True(heap.TrySetArity(arity));
        Assert.True(heap.TrySetKind(HeapKind.Max));

        foreach (var value in new[] { 10, 90, 30, 80, 20, 70, 60, 50, 40, 100, 15 })
        {
            await heap.InsertAsync(value);
        }

        var result = await heap.ExtractRootAsync();
        var snapshot = heap.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(100, result.AffectedValue);
        Assert.Equal(90, snapshot.Root?.Value);
        AssertDaryInvariant(snapshot);
    }

    [Fact]
    public async Task BinaryHeap_IsExactlyDaryHeapWithD2IndexRule()
    {
        var heap = CreateHeap();
        Assert.True(heap.TrySetArity(2));

        foreach (var value in new[] { 40, 25, 60, 10, 35, 50, 70 })
        {
            await heap.InsertAsync(value);
        }

        var snapshot = heap.CreateSnapshot();
        for (var index = 1; index < snapshot.Count; index++)
        {
            Assert.Equal((int?)((index - 1) / 2), snapshot.Elements[index].ParentIndex(2));
        }

        AssertDaryInvariant(snapshot);
    }

    [Fact]
    public async Task ArityCannotChangeWhileNonEmpty()
    {
        var heap = CreateHeap();
        await heap.InsertAsync(10);

        Assert.False(heap.TrySetArity(4));
        Assert.Equal(3, heap.Arity);

        await heap.ClearAsync();
        Assert.True(heap.TrySetArity(4));
        Assert.Equal(4, heap.Arity);
    }

    [Fact]
    public async Task MissingSearch_RemainsLinearForDaryHeap()
    {
        var heap = CreateHeap();
        Assert.True(heap.TrySetArity(4));
        foreach (var value in new[] { 10, 20, 30, 40, 50, 60, 70 })
        {
            await heap.InsertAsync(value);
        }

        var result = await heap.SearchAsync(999);

        Assert.False(result.Succeeded);
        Assert.Equal(heap.Count, result.Comparisons);
        Assert.Equal("O(n)", result.WorstCaseComplexity);
    }

    [Fact]
    public async Task Delete_PreservesSurvivingElementIdentityAndInvariant()
    {
        var heap = CreateHeap();
        foreach (var value in new[] { 10, 20, 30, 40, 50, 60, 70, 80 })
        {
            await heap.InsertAsync(value);
        }

        var before = heap.CreateSnapshot();
        var expectedIds = before.Elements.Where(item => item.Value != 40).Select(item => item.Id).OrderBy(id => id).ToArray();

        var result = await heap.DeleteAsync(40);
        var after = heap.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(expectedIds, after.Elements.Select(item => item.Id).OrderBy(id => id).ToArray());
        AssertDaryInvariant(after);
    }

    private static DaryHeapSimulation CreateHeap() => new(new ImmediateSimulationRuntime());

    private static void AssertDaryInvariant(DaryHeapSnapshot snapshot)
    {
        for (var parentIndex = 0; parentIndex < snapshot.Count; parentIndex++)
        {
            var parent = snapshot.Elements[parentIndex];
            var firstChild = (snapshot.Arity * parentIndex) + 1;
            var lastChild = Math.Min(firstChild + snapshot.Arity - 1, snapshot.Count - 1);

            for (var childIndex = firstChild; childIndex <= lastChild && childIndex < snapshot.Count; childIndex++)
            {
                var child = snapshot.Elements[childIndex];
                if (snapshot.Kind == HeapKind.Min)
                {
                    Assert.True(parent.Value <= child.Value, $"Expected min-heap parent {parent.Value} <= child {child.Value} at {parentIndex}->{childIndex}.");
                }
                else
                {
                    Assert.True(parent.Value >= child.Value, $"Expected max-heap parent {parent.Value} >= child {child.Value} at {parentIndex}->{childIndex}.");
                }
            }
        }
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

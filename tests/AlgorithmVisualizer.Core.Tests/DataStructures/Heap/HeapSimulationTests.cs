using AlgorithmVisualizer.Core.DataStructures.Heap;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Heap;

public sealed class HeapSimulationTests
{
    [Fact]
    public async Task MinHeap_Insert_BubblesSmallestValueToRoot()
    {
        var heap = CreateHeap();
        await heap.InsertAsync(40);
        await heap.InsertAsync(25);
        var result = await heap.InsertAsync(10);

        var snapshot = heap.CreateSnapshot();
        Assert.Equal(HeapKind.Min, snapshot.Kind);
        Assert.Equal(10, snapshot.Root?.Value);
        Assert.True(result.Swaps > 0);
        AssertHeapInvariant(snapshot);
    }

    [Fact]
    public async Task MaxHeap_Insert_BubblesLargestValueToRoot()
    {
        var heap = CreateHeap();
        Assert.True(heap.TrySetKind(HeapKind.Max));

        await heap.InsertAsync(10);
        await heap.InsertAsync(25);
        await heap.InsertAsync(40);

        var snapshot = heap.CreateSnapshot();
        Assert.Equal(HeapKind.Max, snapshot.Kind);
        Assert.Equal(40, snapshot.Root?.Value);
        AssertHeapInvariant(snapshot);
    }

    [Fact]
    public async Task ExtractRoot_PreservesReplacementIdentityAndHeapProperty()
    {
        var heap = CreateHeap();
        foreach (var value in new[] { 10, 20, 30, 40, 50, 60, 70 })
        {
            await heap.InsertAsync(value);
        }

        var before = heap.CreateSnapshot();
        var last = before.Elements[^1];
        var result = await heap.ExtractRootAsync();
        var after = heap.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(10, result.AffectedValue);
        Assert.Equal(6, heap.Count);
        Assert.Contains(after.Elements, item => item.Id == last.Id);
        AssertHeapInvariant(after);
    }

    [Fact]
    public async Task Search_UsesLinearScanBecauseHeapIsNotBstOrdered()
    {
        var heap = CreateHeap();
        foreach (var value in new[] { 10, 50, 20, 90, 60 })
        {
            await heap.InsertAsync(value);
        }

        var found = await heap.SearchAsync(60);
        var missing = await heap.SearchAsync(999);

        Assert.True(found.Succeeded);
        Assert.False(missing.Succeeded);
        Assert.Equal(heap.Count, missing.Comparisons);
        Assert.Equal("O(n)", missing.WorstCaseComplexity);
    }

    [Fact]
    public async Task Delete_RepairsHeapWithoutRecreatingUnrelatedElements()
    {
        var heap = CreateHeap();
        foreach (var value in new[] { 10, 20, 30, 40, 50, 60, 70 })
        {
            await heap.InsertAsync(value);
        }

        var before = heap.CreateSnapshot();
        var survivingIds = before.Elements.Where(item => item.Value != 20).Select(item => item.Id).OrderBy(id => id).ToArray();

        var result = await heap.DeleteAsync(20);
        var after = heap.CreateSnapshot();
        var afterIds = after.Elements.Select(item => item.Id).OrderBy(id => id).ToArray();

        Assert.True(result.Succeeded);
        Assert.Equal(6, after.Count);
        Assert.Equal(survivingIds, afterIds);
        AssertHeapInvariant(after);
    }

    [Fact]
    public async Task DuplicateValues_AreAllowedAndRemainValid()
    {
        var heap = CreateHeap();
        await heap.InsertAsync(10);
        await heap.InsertAsync(10);
        await heap.InsertAsync(10);

        var snapshot = heap.CreateSnapshot();
        Assert.Equal(3, snapshot.Count);
        Assert.Equal(3, snapshot.Elements.Select(item => item.Id).Distinct().Count());
        AssertHeapInvariant(snapshot);
    }

    [Fact]
    public async Task HeapKind_CannotChangeUntilHeapIsCleared()
    {
        var heap = CreateHeap();
        await heap.InsertAsync(10);

        Assert.False(heap.TrySetKind(HeapKind.Max));
        Assert.Equal(HeapKind.Min, heap.Kind);

        await heap.ClearAsync();
        Assert.True(heap.TrySetKind(HeapKind.Max));
        Assert.Equal(HeapKind.Max, heap.Kind);
    }

    [Fact]
    public async Task Capacity_GrowsManuallyAndClearKeepsReservedCapacity()
    {
        var heap = CreateHeap();
        for (var value = 1; value <= 5; value++)
        {
            await heap.InsertAsync(value);
        }

        Assert.Equal(5, heap.Count);
        Assert.True(heap.Capacity >= 5);
        var capacity = heap.Capacity;

        await heap.ClearAsync();

        Assert.Equal(0, heap.Count);
        Assert.Equal(capacity, heap.Capacity);
    }

    private static HeapSimulation CreateHeap() => new(new ImmediateSimulationRuntime());

    private static void AssertHeapInvariant(HeapSnapshot snapshot)
    {
        for (var index = 0; index < snapshot.Count; index++)
        {
            var parent = snapshot.Elements[index];
            var left = (2 * index) + 1;
            var right = (2 * index) + 2;

            if (left < snapshot.Count)
            {
                AssertOrdered(snapshot.Kind, parent.Value, snapshot.Elements[left].Value);
            }

            if (right < snapshot.Count)
            {
                AssertOrdered(snapshot.Kind, parent.Value, snapshot.Elements[right].Value);
            }
        }
    }

    private static void AssertOrdered(HeapKind kind, int parent, int child)
    {
        if (kind == HeapKind.Min)
        {
            Assert.True(parent <= child, $"Expected min-heap parent {parent} <= child {child}.");
        }
        else
        {
            Assert.True(parent >= child, $"Expected max-heap parent {parent} >= child {child}.");
        }
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

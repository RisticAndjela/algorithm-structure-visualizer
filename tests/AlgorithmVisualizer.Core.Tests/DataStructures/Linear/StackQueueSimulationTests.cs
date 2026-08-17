using AlgorithmVisualizer.Core.DataStructures.Linear.Queue;
using AlgorithmVisualizer.Core.DataStructures.Linear.Stack;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Linear;

public sealed class StackQueueSimulationTests
{
    [Fact]
    public async Task Stack_PushPop_UsesLifoOrder()
    {
        var stack = new StackSimulation(new ImmediateRuntime());
        await stack.PushAsync(10);
        await stack.PushAsync(20);
        await stack.PushAsync(30);

        Assert.Equal(30, await stack.PopAsync());
        Assert.Equal(20, await stack.PopAsync());
        Assert.Equal(10, await stack.PopAsync());
        Assert.Null(await stack.PopAsync());
    }

    [Fact]
    public async Task Queue_EnqueueDequeue_UsesFifoOrder()
    {
        var queue = new QueueSimulation(new ImmediateRuntime());
        await queue.EnqueueAsync(10);
        await queue.EnqueueAsync(20);
        await queue.EnqueueAsync(30);

        Assert.Equal(10, await queue.DequeueAsync());
        Assert.Equal(20, await queue.DequeueAsync());
        Assert.Equal(30, await queue.DequeueAsync());
        Assert.Null(await queue.DequeueAsync());
    }

    [Fact]
    public async Task DuplicateValueSearch_FollowsTheStructuresRealTraversalDirection()
    {
        var stack = new StackSimulation(new ImmediateRuntime());
        await stack.PushAsync(7);
        await stack.PushAsync(1);
        await stack.PushAsync(7);
        var stackTopDuplicateId = stack.Items[2].Id;

        var queue = new QueueSimulation(new ImmediateRuntime());
        await queue.EnqueueAsync(7);
        await queue.EnqueueAsync(1);
        await queue.EnqueueAsync(7);
        var queueFrontDuplicateId = queue.Items[0].Id;

        var stackResult = await stack.FindByValueAsync(7);
        var queueResult = await queue.FindByValueAsync(7);

        Assert.True(stackResult.Found);
        Assert.Equal(stackTopDuplicateId, stackResult.ElementId);
        Assert.Equal(1, stackResult.Comparisons);
        Assert.Equal("TOP → bottom", stackResult.TraversalDirection);

        Assert.True(queueResult.Found);
        Assert.Equal(queueFrontDuplicateId, queueResult.ElementId);
        Assert.Equal(1, queueResult.Comparisons);
        Assert.Equal("FRONT → rear", queueResult.TraversalDirection);
    }

    [Fact]
    public async Task DeleteByValue_CompactsManualArrayWithoutChangingCapacity()
    {
        var queue = new QueueSimulation(new ImmediateRuntime());
        foreach (var value in new[] { 10, 20, 30, 40, 50 })
        {
            await queue.EnqueueAsync(value);
        }

        var capacityBefore = queue.StorageCapacity;
        var result = await queue.DeleteByValueAsync(20);

        Assert.True(result.Found);
        Assert.Equal(1, result.MatchedIndex);
        Assert.Equal(3, result.ShiftedElements);
        Assert.Equal(capacityBefore, result.CapacityBefore);
        Assert.Equal(capacityBefore, result.CapacityAfter);
        Assert.Equal(new[] { 10, 30, 40, 50 }, queue.Items.Select(item => item.Value).ToArray());
        Assert.Equal(capacityBefore, queue.StorageCapacity);
        Assert.Equal("Θ(n)", result.FullOperationComplexity);
    }

    [Fact]
    public async Task ShortDisplayedId_SearchesTheSameStableElementIdentity()
    {
        var stack = new StackSimulation(new ImmediateRuntime());
        await stack.PushAsync(10);
        await stack.PushAsync(20);
        var target = stack.Items[0];

        var result = await stack.FindByIdAsync(target.DisplayId);

        Assert.True(result.Found);
        Assert.Equal(target.Id, result.ElementId);
        Assert.Equal(2, result.Comparisons);
    }

    [Fact]
    public async Task Clear_ReleasesUsedItemsButKeepsReservedCapacityForLearningView()
    {
        var queue = new QueueSimulation(new ImmediateRuntime());
        for (var value = 0; value < 6; value++)
        {
            await queue.EnqueueAsync(value);
        }
        var capacityBefore = queue.StorageCapacity;

        await queue.ClearAsync();

        Assert.Equal(0, queue.Count);
        Assert.True(capacityBefore >= 6);
        Assert.Equal(capacityBefore, queue.StorageCapacity);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

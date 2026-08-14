using AlgorithmVisualizer.Core.DataStructures.Trees.Avl;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Trees.Avl;

public sealed class AvlSimulationTests
{
    [Theory]
    [InlineData(30, 20, 10, AvlRotationCase.LL)]
    [InlineData(10, 20, 30, AvlRotationCase.RR)]
    [InlineData(30, 10, 20, AvlRotationCase.LR)]
    [InlineData(10, 30, 20, AvlRotationCase.RL)]
    public async Task InsertAsync_RepairsAllFourRotationCases(int first, int second, int third, AvlRotationCase expectedCase)
    {
        var tree = CreateTree();
        await tree.InsertAsync(first);
        await tree.InsertAsync(second);
        var last = await tree.InsertAsync(third);

        var root = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());
        Assert.Equal(expectedCase, last.FirstRotationCase);
        Assert.Equal(20, root.Value);
        Assert.Equal(10, root.Left?.Value);
        Assert.Equal(30, root.Right?.Value);
        AssertAvlInvariant(root);
    }

    [Fact]
    public async Task IncreasingInsertionOrder_StaysLogarithmicallyShaped()
    {
        var tree = CreateTree();

        for (var value = 1; value <= 7; value++)
        {
            await tree.InsertAsync(value);
        }

        var root = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());
        Assert.Equal(7, tree.Count);
        Assert.Equal(3, tree.Height);
        Assert.Equal(4, root.Value);
        AssertAvlInvariant(root);
    }

    [Fact]
    public async Task InsertAsync_DuplicateKeyIsRejectedWithoutChangingTree()
    {
        var tree = CreateTree();
        await tree.InsertAsync(10);

        var result = await tree.InsertAsync(10);

        Assert.False(result.Succeeded);
        Assert.True(result.DuplicateRejected);
        Assert.Equal(1, tree.Count);
        Assert.Equal(10, tree.CreateSnapshot()?.Value);
    }

    [Fact]
    public async Task SearchAsync_UsesBstOrderingWithoutRotating()
    {
        var tree = await CreateClassicTreeAsync();

        var found = await tree.SearchAsync(60);
        var missing = await tree.SearchAsync(65);

        Assert.True(found.Succeeded);
        Assert.False(missing.Succeeded);
        Assert.Equal(0, found.RotationCount);
        Assert.Equal(0, missing.RotationCount);
        AssertAvlInvariant(Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot()));
    }

    [Fact]
    public async Task DeleteAsync_LeafRemovesOnlyThatLink()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 20, 10, 30 })
        {
            await tree.InsertAsync(value);
        }

        var result = await tree.DeleteAsync(10);
        var root = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(AvlDeleteCase.Leaf, result.DeleteCase);
        Assert.Null(root.Left);
        Assert.Equal(2, tree.Count);
        AssertAvlInvariant(root);
    }

    [Fact]
    public async Task DeleteAsync_OneChildPreservesPromotedNodeIdentity()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 20, 10, 30, 5 })
        {
            await tree.InsertAsync(value);
        }

        var before = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());
        var promoted = Assert.IsType<AvlNodeSnapshot>(before.Left?.Left);

        var result = await tree.DeleteAsync(10);
        var after = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(AvlDeleteCase.OneChild, result.DeleteCase);
        Assert.Equal(5, after.Left?.Value);
        Assert.Equal(promoted.Id, after.Left?.Id);
        Assert.Equal(3, tree.Count);
        AssertAvlInvariant(after);
    }

    [Fact]
    public async Task ClearAsync_RemovesRootAndResetsShapeMetrics()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 30, 20, 40 })
        {
            await tree.InsertAsync(value);
        }

        await tree.ClearAsync();

        Assert.Null(tree.CreateSnapshot());
        Assert.Equal(0, tree.Count);
        Assert.Equal(0, tree.Height);
    }

    [Fact]
    public async Task DeleteAsync_CanTriggerUpwardRebalancing()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 9, 5, 10, 0, 6, 11, -1, 1, 2 })
        {
            await tree.InsertAsync(value);
        }

        var result = await tree.DeleteAsync(10);
        var root = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.True(result.RotationCount > 0);
        Assert.Equal(8, tree.Count);
        AssertAvlInvariant(root);
    }

    [Fact]
    public async Task DeleteAsync_TwoChildrenTransplantsSuccessorIdentityAndRemainsBalanced()
    {
        var tree = await CreateClassicTreeAsync();
        var before = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());
        var successor = Assert.IsType<AvlNodeSnapshot>(before.Right?.Left);

        var result = await tree.DeleteAsync(50);
        var after = Assert.IsType<AvlNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(AvlDeleteCase.TwoChildren, result.DeleteCase);
        Assert.Equal(60, after.Value);
        Assert.Equal(successor.Id, after.Id);
        Assert.Null(after.ParentDisplayId);
        Assert.Equal(6, tree.Count);
        AssertAvlInvariant(after);
    }

    private static AvlSimulation CreateTree() => new(new ImmediateSimulationRuntime());

    private static async Task<AvlSimulation> CreateClassicTreeAsync()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 50, 30, 70, 20, 40, 60, 80 })
        {
            await tree.InsertAsync(value);
        }

        return tree;
    }

    private static int AssertAvlInvariant(
        AvlNodeSnapshot? node,
        int? minimum = null,
        int? maximum = null,
        string? expectedParentId = null)
    {
        if (node is null)
        {
            return 0;
        }

        if (minimum.HasValue)
        {
            Assert.True(node.Value > minimum.Value);
        }

        if (maximum.HasValue)
        {
            Assert.True(node.Value < maximum.Value);
        }

        Assert.Equal(expectedParentId, node.ParentDisplayId);

        var leftHeight = AssertAvlInvariant(node.Left, minimum, node.Value, node.DisplayId);
        var rightHeight = AssertAvlInvariant(node.Right, node.Value, maximum, node.DisplayId);
        var expectedHeight = 1 + Math.Max(leftHeight, rightHeight);
        var expectedBalance = leftHeight - rightHeight;

        Assert.Equal(expectedHeight, node.Height);
        Assert.Equal(expectedBalance, node.BalanceFactor);
        Assert.InRange(node.BalanceFactor, -1, 1);
        return expectedHeight;
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

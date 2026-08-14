using AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Trees.RedBlack;

public sealed class RedBlackSimulationTests
{
    [Fact]
    public async Task InsertAsync_LineCaseRotatesAndPreservesRedBlackInvariants()
    {
        var tree = CreateTree();
        await tree.InsertAsync(10);
        await tree.InsertAsync(20);
        var result = await tree.InsertAsync(30);

        var root = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());
        Assert.Equal(20, root.Value);
        Assert.Equal(RedBlackColor.Black, root.Color);
        Assert.Equal(10, root.Left?.Value);
        Assert.Equal(RedBlackColor.Red, root.Left?.Color);
        Assert.Equal(30, root.Right?.Value);
        Assert.Equal(RedBlackColor.Red, root.Right?.Color);
        Assert.True(result.RotationCount > 0);
        Assert.Equal(RedBlackRepairCase.InsertLine, result.FirstRepairCase);
        AssertRedBlackInvariant(root, requireRootBlack: true);
    }

    [Fact]
    public async Task InsertAsync_RedUncleRecolorsBeforeContinuingUpward()
    {
        var tree = CreateTree();
        await tree.InsertAsync(10);
        await tree.InsertAsync(5);
        await tree.InsertAsync(15);
        var result = await tree.InsertAsync(1);

        var root = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());
        Assert.Equal(RedBlackRepairCase.InsertUncleRed, result.FirstRepairCase);
        Assert.True(result.RecolorCount >= 3);
        AssertRedBlackInvariant(root, requireRootBlack: true);
    }

    [Fact]
    public async Task InsertAsync_DuplicateKeyIsRejectedWithoutChangingCount()
    {
        var tree = CreateTree();
        await tree.InsertAsync(12);

        var result = await tree.InsertAsync(12);

        Assert.False(result.Succeeded);
        Assert.True(result.DuplicateRejected);
        Assert.Equal(1, tree.Count);
        AssertRedBlackInvariant(Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot()), requireRootBlack: true);
    }

    [Fact]
    public async Task IncreasingInsertionOrder_RemainsWithinRedBlackHeightBound()
    {
        var tree = CreateTree();

        for (var value = 1; value <= 31; value++)
        {
            await tree.InsertAsync(value);
        }

        var root = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());
        var theoreticalMaximum = 2 * (int)Math.Ceiling(Math.Log2(tree.Count + 1));

        Assert.True(tree.Height <= theoreticalMaximum);
        AssertRedBlackInvariant(root, requireRootBlack: true);
    }

    [Fact]
    public async Task SearchAsync_UsesBstOrderingWithoutChangingTreeColors()
    {
        var tree = await CreateClassicTreeAsync();
        var before = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());
        var beforeSignature = ColorSignature(before);

        var found = await tree.SearchAsync(7);
        var missing = await tree.SearchAsync(99);
        var after = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(found.Succeeded);
        Assert.False(missing.Succeeded);
        Assert.Equal(0, found.RotationCount);
        Assert.Equal(0, missing.RecolorCount);
        Assert.Equal(beforeSignature, ColorSignature(after));
        AssertRedBlackInvariant(after, requireRootBlack: true);
    }

    [Fact]
    public async Task DeleteAsync_TwoChildrenTransplantsSuccessorIdentity()
    {
        var tree = await CreateClassicTreeAsync();
        var before = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());
        Assert.NotNull(before.Right);
        Assert.NotNull(before.Left);

        var successor = Minimum(Assert.IsType<RedBlackNodeSnapshot>(before.Right));
        var removedId = before.Id;

        var result = await tree.DeleteAsync(before.Value);
        var after = Assert.IsType<RedBlackNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(RedBlackDeleteCase.TwoChildren, result.DeleteCase);
        Assert.False(ContainsId(after, removedId));
        Assert.True(ContainsId(after, successor.Id));
        Assert.Equal(before.Value, result.RequestedValue);
        AssertRedBlackInvariant(after, requireRootBlack: true);
    }

    [Fact]
    public async Task DeleteAsync_SequenceExercisesFixupAndKeepsEveryInvariant()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 11, 2, 14, 1, 7, 15, 5, 8, 4 })
        {
            await tree.InsertAsync(value);
        }

        var sawFixupWork = false;
        foreach (var value in new[] { 1, 2, 14, 11, 7, 5, 4, 8, 15 })
        {
            var result = await tree.DeleteAsync(value);
            Assert.True(result.Succeeded);
            sawFixupWork |= result.FixupChecks > 0 || result.RecolorCount > 0 || result.RotationCount > 0;

            var snapshot = tree.CreateSnapshot();
            if (snapshot is not null)
            {
                AssertRedBlackInvariant(snapshot, requireRootBlack: true);
            }
        }

        Assert.True(sawFixupWork);
        Assert.Equal(0, tree.Count);
        Assert.Equal(0, tree.Height);
        Assert.Equal(0, tree.BlackHeight);
    }

    [Fact]
    public async Task ClearAsync_RemovesRootAndResetsMetrics()
    {
        var tree = await CreateClassicTreeAsync();

        await tree.ClearAsync();

        Assert.Null(tree.CreateSnapshot());
        Assert.Equal(0, tree.Count);
        Assert.Equal(0, tree.Height);
        Assert.Equal(0, tree.BlackHeight);
    }

    private static RedBlackSimulation CreateTree() => new(new ImmediateSimulationRuntime());

    private static async Task<RedBlackSimulation> CreateClassicTreeAsync()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 11, 2, 14, 1, 7, 15, 5, 8, 4 })
        {
            await tree.InsertAsync(value);
        }

        return tree;
    }

    private static int AssertRedBlackInvariant(
        RedBlackNodeSnapshot node,
        bool requireRootBlack,
        int? minimum = null,
        int? maximum = null,
        string? expectedParentId = null)
    {
        if (requireRootBlack)
        {
            Assert.Equal(RedBlackColor.Black, node.Color);
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

        if (node.Color == RedBlackColor.Red)
        {
            if (node.Left is not null)
            {
                Assert.Equal(RedBlackColor.Black, node.Left.Color);
            }

            if (node.Right is not null)
            {
                Assert.Equal(RedBlackColor.Black, node.Right.Color);
            }
        }

        var leftBlackHeight = node.Left is null
            ? 0
            : AssertRedBlackInvariant(node.Left, false, minimum, node.Value, node.DisplayId);
        var rightBlackHeight = node.Right is null
            ? 0
            : AssertRedBlackInvariant(node.Right, false, node.Value, maximum, node.DisplayId);

        Assert.Equal(leftBlackHeight, rightBlackHeight);
        return leftBlackHeight + (node.Color == RedBlackColor.Black ? 1 : 0);
    }

    private static RedBlackNodeSnapshot Minimum(RedBlackNodeSnapshot node)
    {
        var current = node;
        while (current.Left is not null)
        {
            current = current.Left;
        }

        return current;
    }

    private static bool ContainsId(RedBlackNodeSnapshot? node, Guid id)
    {
        if (node is null)
        {
            return false;
        }

        return node.Id == id || ContainsId(node.Left, id) || ContainsId(node.Right, id);
    }

    private static string ColorSignature(RedBlackNodeSnapshot? node)
    {
        if (node is null)
        {
            return "N";
        }

        return $"({node.Value}:{node.Color}:{ColorSignature(node.Left)}:{ColorSignature(node.Right)})";
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

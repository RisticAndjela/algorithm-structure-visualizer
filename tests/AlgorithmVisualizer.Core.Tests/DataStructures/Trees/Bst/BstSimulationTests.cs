using AlgorithmVisualizer.Core.DataStructures.Trees.Bst;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Trees.Bst;

public sealed class BstSimulationTests
{
    [Fact]
    public async Task InsertAsync_BuildsExpectedOrderedShape()
    {
        var tree = CreateTree();

        await tree.InsertAsync(50);
        await tree.InsertAsync(30);
        await tree.InsertAsync(70);
        await tree.InsertAsync(20);
        await tree.InsertAsync(40);

        var root = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());
        Assert.Equal(5, tree.Count);
        Assert.Equal(3, tree.Height);
        Assert.Equal(50, root.Value);
        Assert.Equal(30, root.Left?.Value);
        Assert.Equal(70, root.Right?.Value);
        Assert.Equal(20, root.Left?.Left?.Value);
        Assert.Equal(40, root.Left?.Right?.Value);
    }

    [Fact]
    public async Task InsertAsync_DuplicateKeyIsRejectedWithoutChangingCount()
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
    public async Task SearchAsync_FollowsOrderingForFoundAndMissingKeys()
    {
        var tree = await CreateClassicTreeAsync();

        var found = await tree.SearchAsync(60);
        var missing = await tree.SearchAsync(65);

        Assert.True(found.Succeeded);
        Assert.Equal(3, found.Comparisons);
        Assert.False(missing.Succeeded);
        Assert.Equal(3, missing.Comparisons);
        Assert.Equal(7, tree.Count);
    }

    [Fact]
    public async Task DeleteAsync_LeafDisconnectsOnlyLeaf()
    {
        var tree = await CreateClassicTreeAsync();

        var result = await tree.DeleteAsync(20);
        var root = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(BstDeleteCase.Leaf, result.DeleteCase);
        Assert.Equal(6, tree.Count);
        Assert.Null(root.Left?.Left);
        Assert.Equal(40, root.Left?.Right?.Value);
    }

    [Fact]
    public async Task DeleteAsync_OneChildPromotesExistingChildObject()
    {
        var tree = CreateTree();
        await tree.InsertAsync(50);
        await tree.InsertAsync(30);
        await tree.InsertAsync(20);

        var before = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());
        var childId = Assert.IsType<BstNodeSnapshot>(before.Left?.Left).Id;

        var result = await tree.DeleteAsync(30);
        var after = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());

        Assert.Equal(BstDeleteCase.OneChild, result.DeleteCase);
        Assert.Equal(2, tree.Count);
        Assert.Equal(20, after.Left?.Value);
        Assert.Equal(childId, after.Left?.Id);
        Assert.Equal(after.DisplayId, after.Left?.ParentDisplayId);
    }

    [Fact]
    public async Task DeleteAsync_TwoChildrenTransplantsSuccessorNodeIdentity()
    {
        var tree = await CreateClassicTreeAsync();
        var before = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());
        var successor = Assert.IsType<BstNodeSnapshot>(before.Right?.Left);

        var result = await tree.DeleteAsync(50);
        var after = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());

        Assert.Equal(BstDeleteCase.TwoChildren, result.DeleteCase);
        Assert.Equal(6, tree.Count);
        Assert.Equal(60, after.Value);
        Assert.Equal(successor.Id, after.Id);
        Assert.Equal(30, after.Left?.Value);
        Assert.Equal(70, after.Right?.Value);
        Assert.Null(after.ParentDisplayId);
        Assert.Equal(after.DisplayId, after.Left?.ParentDisplayId);
        Assert.Equal(after.DisplayId, after.Right?.ParentDisplayId);
        Assert.Null(after.Right?.Left);
    }

    [Fact]
    public async Task BalanceAsync_SkewedTreeReducesHeightAndPreservesNodeIdentity()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 10, 20, 30, 40, 50, 60, 70 })
        {
            await tree.InsertAsync(value);
        }

        var before = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());
        var identitiesBefore = CaptureIdentityByValue(before);
        Assert.Equal(7, tree.Height);

        var result = await tree.BalanceAsync();
        var after = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());
        var identitiesAfter = CaptureIdentityByValue(after);

        Assert.True(result.Succeeded);
        Assert.Equal(BstOperationKind.Balance, result.Operation);
        Assert.Equal(7, tree.Count);
        Assert.Equal(3, tree.Height);
        Assert.Equal(7, result.HeightBefore);
        Assert.Equal(3, result.HeightAfter);
        Assert.Equal(0, result.VineRotations);
        Assert.Equal(4, result.CompressionRotations);
        Assert.Equal(2, result.CompressionPasses);
        Assert.Equal("O(n)", result.WorstCaseComplexity);
        Assert.Equal("Θ(n)", result.CurrentRunComplexity);
        Assert.Equal(identitiesBefore.Count, identitiesAfter.Count);
        foreach (var pair in identitiesBefore)
        {
            Assert.True(identitiesAfter.TryGetValue(pair.Key, out var afterId));
            Assert.Equal(pair.Value, afterId);
        }
        AssertBstInvariant(after, minExclusive: null, maxExclusive: null, expectedParentDisplayId: null);
    }

    [Fact]
    public async Task BalanceAsync_LeftSkewedTreeUsesRightRotationsThenCompression()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 70, 60, 50, 40, 30, 20, 10 })
        {
            await tree.InsertAsync(value);
        }

        var result = await tree.BalanceAsync();
        var root = Assert.IsType<BstNodeSnapshot>(tree.CreateSnapshot());

        Assert.True(result.Succeeded);
        Assert.Equal(6, result.VineRotations);
        Assert.Equal(4, result.CompressionRotations);
        Assert.Equal(7, tree.Count);
        Assert.Equal(3, tree.Height);
        AssertBstInvariant(root, minExclusive: null, maxExclusive: null, expectedParentDisplayId: null);
    }

    [Fact]
    public async Task BalanceAsync_DoesNotAutomaticallyRunDuringNormalInsert()
    {
        var tree = CreateTree();

        await tree.InsertAsync(1);
        await tree.InsertAsync(2);
        await tree.InsertAsync(3);
        await tree.InsertAsync(4);
        await tree.InsertAsync(5);

        Assert.Equal(5, tree.Height);

        await tree.BalanceAsync();

        Assert.Equal(3, tree.Height);
    }

    [Fact]
    public async Task Height_ReflectsSkewedInsertionOrder()
    {
        var tree = CreateTree();

        await tree.InsertAsync(1);
        await tree.InsertAsync(2);
        await tree.InsertAsync(3);
        await tree.InsertAsync(4);
        await tree.InsertAsync(5);

        Assert.Equal(5, tree.Count);
        Assert.Equal(5, tree.Height);
    }

    private static IReadOnlyDictionary<int, Guid> CaptureIdentityByValue(BstNodeSnapshot root)
    {
        var identities = new Dictionary<int, Guid>();
        CaptureIdentityByValue(root, identities);
        return identities;
    }

    private static void CaptureIdentityByValue(BstNodeSnapshot? node, IDictionary<int, Guid> identities)
    {
        if (node is null)
        {
            return;
        }

        identities[node.Value] = node.Id;
        CaptureIdentityByValue(node.Left, identities);
        CaptureIdentityByValue(node.Right, identities);
    }

    private static void AssertBstInvariant(
        BstNodeSnapshot? node,
        int? minExclusive,
        int? maxExclusive,
        string? expectedParentDisplayId)
    {
        if (node is null)
        {
            return;
        }

        if (minExclusive.HasValue)
        {
            Assert.True(node.Value > minExclusive.Value, $"Expected {node.Value} > {minExclusive.Value}.");
        }

        if (maxExclusive.HasValue)
        {
            Assert.True(node.Value < maxExclusive.Value, $"Expected {node.Value} < {maxExclusive.Value}.");
        }

        Assert.Equal(expectedParentDisplayId, node.ParentDisplayId);
        AssertBstInvariant(node.Left, minExclusive, node.Value, node.DisplayId);
        AssertBstInvariant(node.Right, node.Value, maxExclusive, node.DisplayId);
    }

    private static BstSimulation CreateTree() => new(new ImmediateSimulationRuntime());

    private static async Task<BstSimulation> CreateClassicTreeAsync()
    {
        var tree = CreateTree();
        foreach (var value in new[] { 50, 30, 70, 20, 40, 60, 80 })
        {
            await tree.InsertAsync(value);
        }

        return tree;
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

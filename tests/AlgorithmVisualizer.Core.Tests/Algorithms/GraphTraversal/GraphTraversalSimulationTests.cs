using AlgorithmVisualizer.Core.Algorithms.GraphTraversal;
using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.GraphTraversal;

public sealed class GraphTraversalSimulationTests
{
    [Fact]
    public async Task Bfs_BranchingGraph_VisitsByNondecreasingDistance()
    {
        var graph = Build(
            ["A", "B", "C", "D", "E", "F"],
            [(0, 1), (0, 2), (1, 3), (1, 4), (2, 5)],
            directed: false);
        var bfs = new BreadthFirstSearchSimulation(new ImmediateRuntime());

        var result = await bfs.TraverseAsync(graph, 0);

        Assert.Equal(6, result.ReachableCount);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, result.TraversalOrderIndices);
        Assert.Equal(new[] { 0, 1, 1, 2, 2, 2 }, result.Distances);
        Assert.True(result.MaxQueueSize >= 2);
    }

    [Fact]
    public async Task Bfs_DirectedGraph_FollowsOutgoingEdgesOnly()
    {
        var graph = Build(["A", "B", "C"], [(0, 1), (1, 2)], directed: true);
        var bfs = new BreadthFirstSearchSimulation(new ImmediateRuntime());

        var result = await bfs.TraverseAsync(graph, 2);

        Assert.Equal(1, result.ReachableCount);
        Assert.Equal(new[] { 2 }, result.TraversalOrderIndices);
    }

    [Fact]
    public async Task Bfs_Cycle_DoesNotVisitVertexTwice()
    {
        var graph = Build(["A", "B", "C"], [(0, 1), (1, 2), (2, 0)], directed: false);
        var bfs = new BreadthFirstSearchSimulation(new ImmediateRuntime());

        var result = await bfs.TraverseAsync(graph, 0);

        Assert.Equal(3, result.ReachableCount);
        Assert.Equal(3, result.TraversalOrderIndices.Distinct().Count());
    }

    [Fact]
    public async Task Dfs_Recursive_BranchingGraph_GoesDeepAndBacktracks()
    {
        var graph = Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1), (0, 2), (1, 3), (1, 4)],
            directed: false);
        var dfs = new DepthFirstSearchSimulation(new ImmediateRuntime());

        var result = await dfs.TraverseAsync(graph, 0, DepthFirstTraversalVariant.Recursive);

        Assert.Equal(5, result.ReachableCount);
        Assert.True(result.Depths.Max() >= 2);
        Assert.True(result.BacktrackCount >= 1);
        Assert.Equal(5, result.TraversalOrderIndices.Distinct().Count());
    }

    [Fact]
    public async Task Dfs_Iterative_UsesExplicitStackAndVisitsReachableSetOnce()
    {
        var graph = Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1), (0, 2), (1, 3), (1, 4)],
            directed: false);
        var dfs = new DepthFirstSearchSimulation(new ImmediateRuntime());

        var result = await dfs.TraverseAsync(graph, 0, DepthFirstTraversalVariant.Iterative);

        Assert.Equal(DepthFirstTraversalVariant.Iterative, result.Variant);
        Assert.Equal(5, result.ReachableCount);
        Assert.Equal(5, result.TraversalOrderIndices.Distinct().Count());
        Assert.True(result.MaxFrontierDepth >= 2);
    }

    [Fact]
    public async Task Dfs_DisconnectedGraph_LeavesOtherComponentUnvisited()
    {
        var graph = Build(["A", "B", "X", "Y"], [(0, 1), (2, 3)], directed: false);
        var dfs = new DepthFirstSearchSimulation(new ImmediateRuntime());

        var result = await dfs.TraverseAsync(graph, 0, DepthFirstTraversalVariant.Recursive);

        Assert.Equal(2, result.ReachableCount);
        Assert.Equal(-1, result.Depths[2]);
        Assert.Equal(-1, result.Depths[3]);
    }

    private static GraphSnapshot Build(string[] labels, (int From, int To)[] pairs, bool directed)
    {
        var ids = labels.Select(_ => Guid.NewGuid()).ToArray();
        var edgeIds = pairs.Select(_ => Guid.NewGuid()).ToArray();
        var neighborLists = labels.Select(_ => new List<GraphNeighborSnapshot>()).ToArray();
        var inDegree = new int[labels.Length];
        var outDegree = new int[labels.Length];
        var edges = new GraphEdgeSnapshot[pairs.Length];

        for (var i = 0; i < pairs.Length; i++)
        {
            var (from, to) = pairs[i];
            var edgeId = edgeIds[i];
            edges[i] = new GraphEdgeSnapshot(edgeId, from, to, ids[from], ids[to], labels[from], labels[to], 1, directed, GraphEdgeVisualState.Normal);
            neighborLists[from].Add(new GraphNeighborSnapshot(ids[to], labels[to], edgeId, 1, to));
            outDegree[from]++;
            inDegree[to]++;
            if (!directed && from != to)
            {
                neighborLists[to].Add(new GraphNeighborSnapshot(ids[from], labels[from], edgeId, 1, from));
                outDegree[to]++;
                inDegree[from]++;
            }
        }

        var vertices = new GraphVertexSnapshot[labels.Length];
        for (var i = 0; i < labels.Length; i++)
        {
            vertices[i] = new GraphVertexSnapshot(i, ids[i], labels[i], GraphVertexVisualState.Normal, neighborLists[i].ToArray(), inDegree[i], outDegree[i]);
        }

        var cells = new GraphMatrixCellSnapshot[labels.Length * labels.Length];
        for (var row = 0; row < labels.Length; row++)
        {
            for (var column = 0; column < labels.Length; column++)
            {
                var hasEdge = pairs.Any(pair => (pair.From == row && pair.To == column) || (!directed && pair.From == column && pair.To == row));
                cells[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, hasEdge ? 1 : 0);
            }
        }

        return new GraphSnapshot(directed, false, labels.Length, pairs.Length, vertices, edges, cells);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

using AlgorithmVisualizer.Core.Algorithms.GraphOrdering.Topological;
using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.GraphOrdering;

public sealed class TopologicalSortSimulationTests
{
    [Theory]
    [InlineData(TopologicalSortVariant.KahnQueue)]
    [InlineData(TopologicalSortVariant.DfsPostorder)]
    public async Task Dag_ProducesOrderThatRespectsEveryEdge(TopologicalSortVariant variant)
    {
        var graph = Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 1d), (0, 2, 1d), (1, 3, 1d), (2, 3, 1d), (3, 4, 1d)],
            directed: true,
            weighted: false);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var result = await simulation.SortAsync(graph, variant);

        Assert.True(result.IsDag);
        Assert.False(result.CycleDetected);
        Assert.Equal(graph.VertexCount, result.OrderIndices.Length);
        AssertValidOrder(graph, result.OrderIndices);
    }

    [Fact]
    public async Task Kahn_MultipleSources_QueuesEveryInitialZeroInDegreeVertex()
    {
        var graph = Build(
            ["A", "B", "C", "D"],
            [(0, 2, 1d), (1, 2, 1d), (2, 3, 1d)],
            directed: true,
            weighted: false);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var result = await simulation.SortAsync(graph, TopologicalSortVariant.KahnQueue);

        Assert.True(result.IsDag);
        Assert.Equal(2, result.InitialReadyCount);
        Assert.Equal(graph.VertexCount, result.QueueEnqueues);
        AssertValidOrder(graph, result.OrderIndices);
    }

    [Theory]
    [InlineData(TopologicalSortVariant.KahnQueue)]
    [InlineData(TopologicalSortVariant.DfsPostorder)]
    public async Task DirectedCycle_IsReportedInsteadOfAccepted(TopologicalSortVariant variant)
    {
        var graph = Build(
            ["A", "B", "C"],
            [(0, 1, 1d), (1, 2, 1d), (2, 0, 1d)],
            directed: true,
            weighted: false);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var result = await simulation.SortAsync(graph, variant);

        Assert.True(result.CycleDetected);
        Assert.False(result.IsDag);
    }

    [Fact]
    public async Task WeightedDag_IgnoresWeightsAndStillOrdersDependencies()
    {
        var graph = Build(
            ["A", "B", "C", "D"],
            [(0, 1, 500d), (0, 2, -20d), (1, 3, 0d), (2, 3, 999d)],
            directed: true,
            weighted: true);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var result = await simulation.SortAsync(graph, TopologicalSortVariant.KahnQueue);

        Assert.True(result.IsDag);
        AssertValidOrder(graph, result.OrderIndices);
    }

    [Fact]
    public async Task UndirectedGraph_IsRejectedBeforeRun()
    {
        var graph = Build(["A", "B"], [(0, 1, 1d)], directed: false, weighted: false);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => simulation.SortAsync(graph, TopologicalSortVariant.KahnQueue));

        Assert.Contains("directed", exception.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData(TopologicalSortVariant.KahnQueue)]
    [InlineData(TopologicalSortVariant.DfsPostorder)]
    public async Task SingleVertex_IsValidBoundary(TopologicalSortVariant variant)
    {
        var graph = Build(["A"], [], directed: true, weighted: false);
        var simulation = new TopologicalSortSimulation(new ImmediateRuntime());

        var result = await simulation.SortAsync(graph, variant);

        Assert.True(result.IsDag);
        Assert.Equal(new[] { 0 }, result.OrderIndices);
    }

    private static void AssertValidOrder(GraphSnapshot graph, int[] order)
    {
        var position = new int[graph.VertexCount];
        for (var index = 0; index < order.Length; index++) position[order[index]] = index;
        foreach (var edge in graph.Edges) Assert.True(position[edge.FromIndex] < position[edge.ToIndex]);
    }

    private static GraphSnapshot Build(
        string[] labels,
        (int From, int To, double Weight)[] data,
        bool directed,
        bool weighted)
    {
        var ids = labels.Select(_ => Guid.NewGuid()).ToArray();
        var edgeIds = data.Select(_ => Guid.NewGuid()).ToArray();
        var neighborLists = labels.Select(_ => new List<GraphNeighborSnapshot>()).ToArray();
        var inDegree = new int[labels.Length];
        var outDegree = new int[labels.Length];
        var edges = new GraphEdgeSnapshot[data.Length];

        for (var index = 0; index < data.Length; index++)
        {
            var (from, to, weight) = data[index];
            var edgeId = edgeIds[index];
            var storedWeight = weighted ? weight : 1d;
            edges[index] = new GraphEdgeSnapshot(edgeId, from, to, ids[from], ids[to], labels[from], labels[to], storedWeight, directed, GraphEdgeVisualState.Normal);
            neighborLists[from].Add(new GraphNeighborSnapshot(ids[to], labels[to], edgeId, storedWeight, to));
            outDegree[from]++;
            inDegree[to]++;
            if (!directed && from != to)
            {
                neighborLists[to].Add(new GraphNeighborSnapshot(ids[from], labels[from], edgeId, storedWeight, from));
                outDegree[to]++;
                inDegree[from]++;
            }
        }

        var vertices = new GraphVertexSnapshot[labels.Length];
        for (var index = 0; index < labels.Length; index++)
        {
            vertices[index] = new GraphVertexSnapshot(index, ids[index], labels[index], GraphVertexVisualState.Normal, neighborLists[index].ToArray(), inDegree[index], outDegree[index]);
        }

        var cells = new GraphMatrixCellSnapshot[labels.Length * labels.Length];
        for (var row = 0; row < labels.Length; row++)
        {
            for (var column = 0; column < labels.Length; column++)
            {
                var hasEdge = false;
                var weight = 0d;
                for (var edgeIndex = 0; edgeIndex < data.Length; edgeIndex++)
                {
                    var edge = data[edgeIndex];
                    if ((edge.From == row && edge.To == column) || (!directed && edge.From == column && edge.To == row))
                    {
                        hasEdge = true;
                        weight = weighted ? edge.Weight : 1d;
                        break;
                    }
                }
                cells[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, weight);
            }
        }

        return new GraphSnapshot(directed, weighted, labels.Length, data.Length, vertices, edges, cells);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

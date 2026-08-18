using AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra;
using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.GraphShortestPath;

public sealed class DijkstraSimulationTests
{
    [Theory]
    [InlineData(DijkstraVariant.LinearScan)]
    [InlineData(DijkstraVariant.MinHeap)]
    public async Task FindsCheaperDetourThanDirectEdge(DijkstraVariant variant)
    {
        var graph = Build(
            ["A", "B", "C", "D"],
            [(0, 1, 9d), (0, 2, 2d), (2, 1, 2d), (1, 3, 1d)],
            directed: false);
        var dijkstra = new DijkstraSimulation(new ImmediateRuntime());

        var result = await dijkstra.TraverseAsync(graph, 0, variant);

        Assert.Equal(4d, result.Distances[1]);
        Assert.Equal(2, result.ParentIndices[1]);
        Assert.Equal(5d, result.Distances[3]);
        Assert.Equal(4, result.ReachableCount);
    }

    [Fact]
    public async Task MinHeapVariant_SkipsStaleEntryAfterDistanceImprovement()
    {
        var graph = Build(
            ["A", "B", "C", "D"],
            [(0, 1, 8d), (0, 2, 2d), (2, 1, 1d), (1, 3, 3d)],
            directed: true);
        var dijkstra = new DijkstraSimulation(new ImmediateRuntime());

        var result = await dijkstra.TraverseAsync(graph, 0, DijkstraVariant.MinHeap);

        Assert.Equal(3d, result.Distances[1]);
        Assert.True(result.FrontierPushes > result.ReachableCount);
        Assert.True(result.StalePops >= 1);
    }

    [Fact]
    public async Task DisconnectedGraph_LeavesInfinityForUnreachableVertices()
    {
        var graph = Build(["A", "B", "X"], [(0, 1, 2d)], directed: false);
        var dijkstra = new DijkstraSimulation(new ImmediateRuntime());

        var result = await dijkstra.TraverseAsync(graph, 0, DijkstraVariant.LinearScan);

        Assert.Equal(2, result.ReachableCount);
        Assert.True(double.IsPositiveInfinity(result.Distances[2]));
        Assert.Equal(-1, result.ParentIndices[2]);
    }

    [Fact]
    public async Task ZeroWeightEdge_IsValid()
    {
        var graph = Build(["A", "B", "C"], [(0, 1, 0d), (1, 2, 2d), (0, 2, 5d)], directed: true);
        var dijkstra = new DijkstraSimulation(new ImmediateRuntime());

        var result = await dijkstra.TraverseAsync(graph, 0, DijkstraVariant.MinHeap);

        Assert.Equal(0d, result.Distances[1]);
        Assert.Equal(2d, result.Distances[2]);
    }

    [Fact]
    public async Task NegativeWeight_IsRejected()
    {
        var graph = Build(["A", "B"], [(0, 1, -1d)], directed: true);
        var dijkstra = new DijkstraSimulation(new ImmediateRuntime());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dijkstra.TraverseAsync(graph, 0, DijkstraVariant.LinearScan));
        Assert.Contains("negative", exception.Message.ToLowerInvariant());
    }

    [Fact]
    public async Task BasicAndAdvanced_ProduceSameDistancesAndParents_OnUniqueShortestPaths()
    {
        var graph = Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 5d), (0, 2, 1d), (2, 1, 1d), (1, 3, 2d), (2, 4, 7d), (3, 4, 1d)],
            directed: true);
        var basic = new DijkstraSimulation(new ImmediateRuntime());
        var advanced = new DijkstraSimulation(new ImmediateRuntime());

        var basicResult = await basic.TraverseAsync(graph, 0, DijkstraVariant.LinearScan);
        var advancedResult = await advanced.TraverseAsync(graph, 0, DijkstraVariant.MinHeap);

        Assert.Equal(basicResult.Distances, advancedResult.Distances);
        Assert.Equal(basicResult.ParentIndices, advancedResult.ParentIndices);
    }

    private static GraphSnapshot Build(string[] labels, (int From, int To, double Weight)[] data, bool directed)
    {
        var ids = labels.Select(_ => Guid.NewGuid()).ToArray();
        var edgeIds = data.Select(_ => Guid.NewGuid()).ToArray();
        var neighborLists = labels.Select(_ => new List<GraphNeighborSnapshot>()).ToArray();
        var inDegree = new int[labels.Length];
        var outDegree = new int[labels.Length];
        var edges = new GraphEdgeSnapshot[data.Length];

        for (var i = 0; i < data.Length; i++)
        {
            var (from, to, weight) = data[i];
            var edgeId = edgeIds[i];
            edges[i] = new GraphEdgeSnapshot(edgeId, from, to, ids[from], ids[to], labels[from], labels[to], weight, directed, GraphEdgeVisualState.Normal);
            neighborLists[from].Add(new GraphNeighborSnapshot(ids[to], labels[to], edgeId, weight, to));
            outDegree[from]++;
            inDegree[to]++;
            if (!directed && from != to)
            {
                neighborLists[to].Add(new GraphNeighborSnapshot(ids[from], labels[from], edgeId, weight, from));
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
                var hasEdge = false;
                var weight = 0d;
                for (var i = 0; i < data.Length; i++)
                {
                    var edge = data[i];
                    if ((edge.From == row && edge.To == column) || (!directed && edge.From == column && edge.To == row))
                    {
                        hasEdge = true;
                        weight = edge.Weight;
                        break;
                    }
                }
                cells[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, weight);
            }
        }

        return new GraphSnapshot(directed, true, labels.Length, data.Length, vertices, edges, cells);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

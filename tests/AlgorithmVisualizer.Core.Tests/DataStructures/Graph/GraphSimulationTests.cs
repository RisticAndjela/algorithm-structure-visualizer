using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.DataStructures.Graph;

public sealed class GraphSimulationTests
{
    [Fact]
    public async Task UndirectedEdge_UpdatesBothAdjacencyListsAndMirroredMatrixCells()
    {
        var graph = CreateGraph();
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        var result = await graph.AddEdgeAsync("A", "B");
        var snapshot = graph.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Single(snapshot.Vertices[0].Neighbors);
        Assert.Single(snapshot.Vertices[1].Neighbors);
        Assert.True(Cell(snapshot, 0, 1).HasEdge);
        Assert.True(Cell(snapshot, 1, 0).HasEdge);
    }

    [Fact]
    public async Task DirectedEdge_DoesNotCreateReverseAdjacency()
    {
        var graph = CreateGraph();
        Assert.True(graph.TrySetDirected(true));
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        await graph.AddEdgeAsync("A", "B");

        var forward = await graph.SearchEdgeAsync("A", "B");
        var reverse = await graph.SearchEdgeAsync("B", "A");
        var snapshot = graph.CreateSnapshot();

        Assert.True(forward.Succeeded);
        Assert.False(reverse.Succeeded);
        Assert.True(Cell(snapshot, 0, 1).HasEdge);
        Assert.False(Cell(snapshot, 1, 0).HasEdge);
    }

    [Fact]
    public async Task WeightedZeroEdge_RemainsDifferentFromMissingCell()
    {
        var graph = CreateGraph();
        Assert.True(graph.TrySetDirected(true));
        Assert.True(graph.TrySetWeighted(true));
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        await graph.AddEdgeAsync("A", "B", 0d);

        var snapshot = graph.CreateSnapshot();
        Assert.True(Cell(snapshot, 0, 1).HasEdge);
        Assert.Equal(0d, Cell(snapshot, 0, 1).Weight);
        Assert.False(Cell(snapshot, 1, 0).HasEdge);
    }

    [Fact]
    public async Task RemoveVertex_RemovesEveryIncidentEdgeAndCompactsRepresentations()
    {
        var graph = CreateGraph();
        foreach (var label in new[] { "A", "B", "C" }) await graph.AddVertexAsync(label);
        await graph.AddEdgeAsync("A", "B");
        await graph.AddEdgeAsync("B", "C");
        await graph.AddEdgeAsync("A", "C");

        var result = await graph.RemoveVertexAsync("B");
        var snapshot = graph.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(2, snapshot.VertexCount);
        Assert.Equal(1, snapshot.EdgeCount);
        Assert.Equal(new[] { "A", "C" }, snapshot.Vertices.Select(v => v.Label).ToArray());
        Assert.True(Cell(snapshot, 0, 1).HasEdge);
    }

    [Fact]
    public async Task DuplicateVertexLabel_IsRejectedCaseInsensitively()
    {
        var graph = CreateGraph();
        Assert.True((await graph.AddVertexAsync("Node")).Succeeded);
        Assert.False((await graph.AddVertexAsync("node")).Succeeded);
        Assert.Equal(1, graph.VertexCount);
    }

    [Fact]
    public async Task UndirectedReverseDuplicateEdge_IsRejected()
    {
        var graph = CreateGraph();
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        Assert.True((await graph.AddEdgeAsync("A", "B")).Succeeded);
        Assert.False((await graph.AddEdgeAsync("B", "A")).Succeeded);
        Assert.Equal(1, graph.EdgeCount);
    }

    [Fact]
    public async Task SelfLoop_IsSupportedAndStoredOnceInUndirectedAdjacencyList()
    {
        var graph = CreateGraph();
        await graph.AddVertexAsync("A");
        Assert.True((await graph.AddEdgeAsync("A", "A")).Succeeded);
        var snapshot = graph.CreateSnapshot();

        Assert.Equal(1, snapshot.EdgeCount);
        Assert.Single(snapshot.Vertices[0].Neighbors);
        Assert.True(Cell(snapshot, 0, 0).HasEdge);
    }


    [Fact]
    public async Task RenameVertex_PreservesIdentityAndIncidentEdgeIdentity()
    {
        var graph = CreateGraph();
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        await graph.AddEdgeAsync("A", "B");
        var before = graph.CreateSnapshot();
        var vertexId = before.Vertices[0].Id;
        var edgeId = before.Edges[0].Id;

        var result = await graph.RenameVertexAsync("A", "Alpha");
        var after = graph.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(vertexId, after.Vertices[0].Id);
        Assert.Equal("Alpha", after.Vertices[0].Label);
        Assert.Equal(edgeId, after.Edges[0].Id);
        Assert.Equal("Alpha", after.Edges[0].FromLabel);
    }

    [Fact]
    public async Task UpdateEdgeWeight_PreservesEdgeIdentityAndRefreshesMatrix()
    {
        var graph = CreateGraph();
        Assert.True(graph.TrySetDirected(true));
        Assert.True(graph.TrySetWeighted(true));
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        await graph.AddEdgeAsync("A", "B", 3d);
        var edgeId = graph.CreateSnapshot().Edges[0].Id;

        var result = await graph.UpdateEdgeWeightAsync("A", "B", -2.5d);
        var after = graph.CreateSnapshot();

        Assert.True(result.Succeeded);
        Assert.Equal(edgeId, after.Edges[0].Id);
        Assert.Equal(-2.5d, after.Edges[0].Weight);
        Assert.True(Cell(after, 0, 1).HasEdge);
        Assert.Equal(-2.5d, Cell(after, 0, 1).Weight);
    }

    [Fact]
    public async Task ModeCannotChangeWhileEdgesExist()
    {
        var graph = CreateGraph();
        await graph.AddVertexAsync("A");
        await graph.AddVertexAsync("B");
        await graph.AddEdgeAsync("A", "B");

        Assert.False(graph.TrySetDirected(true));
        Assert.False(graph.TrySetWeighted(true));
    }

    [Fact]
    public async Task GraphCore_CanGrowBeyondMatrixPageEightByEightTeachingLimit()
    {
        var graph = CreateGraph();
        const int vertexCount = 12;

        for (var i = 0; i < vertexCount; i++)
        {
            Assert.True((await graph.AddVertexAsync($"V{i}")).Succeeded);
        }

        var snapshot = graph.CreateSnapshot();
        Assert.Equal(vertexCount, snapshot.VertexCount);
        Assert.Equal(vertexCount * vertexCount, snapshot.MatrixCells.Length);

        Assert.True((await graph.AddEdgeAsync("V0", "V11")).Succeeded);
        snapshot = graph.CreateSnapshot();
        Assert.True(Cell(snapshot, 0, 11).HasEdge);
        Assert.True(Cell(snapshot, 11, 0).HasEdge);
    }

    private static GraphSimulation CreateGraph() => new(new ImmediateRuntime());
    private static GraphMatrixCellSnapshot Cell(GraphSnapshot snapshot, int row, int col) => snapshot.MatrixCells[(row * snapshot.VertexCount) + col];

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

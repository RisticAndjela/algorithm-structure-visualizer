using AlgorithmVisualizer.Core.Algorithms.GraphSpanningTree.Mst;
using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.Algorithms.GraphSpanningTree;

public sealed class MstSimulationTests
{
    [Theory]
    [InlineData(MstVariant.Prim)]
    [InlineData(MstVariant.Kruskal)]
    public async Task ConnectedGraph_ProducesMinimumTree(MstVariant variant)
    {
        var graph = Build(["A","B","C","D"], [(0,1,4d),(0,2,2d),(1,2,1d),(1,3,5d),(2,3,3d)], false, true);
        var simulation = new MstSimulation(new ImmediateRuntime());

        var result = await simulation.BuildAsync(graph, variant, 0);

        Assert.True(result.IsMinimumSpanningTree);
        Assert.Equal(3, result.SelectedEdgeIndices.Length);
        Assert.Equal(6d, result.TotalWeight, 6);
        Assert.Equal(1, result.ComponentCount);
    }

    [Fact]
    public async Task PrimAndKruskal_AgreeOnMinimumWeight()
    {
        var graph = Build(["A","B","C","D","E"], [(0,1,2d),(0,2,3d),(1,2,1d),(1,3,4d),(2,3,2d),(2,4,5d),(3,4,1d)], false, true);
        var prim = new MstSimulation(new ImmediateRuntime());
        var kruskal = new MstSimulation(new ImmediateRuntime());

        var left = await prim.BuildAsync(graph, MstVariant.Prim, 0);
        var right = await kruskal.BuildAsync(graph, MstVariant.Kruskal, 0);

        Assert.True(left.IsMinimumSpanningTree);
        Assert.True(right.IsMinimumSpanningTree);
        Assert.Equal(left.TotalWeight, right.TotalWeight, 6);
    }

    [Theory]
    [InlineData(MstVariant.Prim)]
    [InlineData(MstVariant.Kruskal)]
    public async Task NegativeAndZeroWeights_AreValid(MstVariant variant)
    {
        var graph = Build(["A","B","C","D"], [(0,1,-2d),(0,2,3d),(1,2,0d),(1,3,4d),(2,3,1d)], false, true);
        var simulation = new MstSimulation(new ImmediateRuntime());

        var result = await simulation.BuildAsync(graph, variant, 0);

        Assert.True(result.IsMinimumSpanningTree);
        Assert.Equal(-1d, result.TotalWeight, 6);
    }

    [Theory]
    [InlineData(MstVariant.Prim)]
    [InlineData(MstVariant.Kruskal)]
    public async Task DisconnectedGraph_ProducesMinimumForest(MstVariant variant)
    {
        var graph = Build(["A","B","C","X","Y"], [(0,1,2d),(1,2,1d),(3,4,3d)], false, true);
        var simulation = new MstSimulation(new ImmediateRuntime());

        var result = await simulation.BuildAsync(graph, variant, 0);

        Assert.False(result.IsMinimumSpanningTree);
        Assert.True(result.IsMinimumSpanningForest);
        Assert.Equal(2, result.ComponentCount);
        Assert.Equal(3, result.SelectedEdgeIndices.Length);
    }

    [Fact]
    public async Task Kruskal_RejectsCycleEdgeUsingDsu()
    {
        var graph = Build(["A","B","C"], [(0,1,1d),(1,2,2d),(0,2,3d)], false, true);
        var simulation = new MstSimulation(new ImmediateRuntime());

        var result = await simulation.BuildAsync(graph, MstVariant.Kruskal);

        Assert.True(result.IsMinimumSpanningTree);
        Assert.True(result.CycleSkips > 0);
        Assert.Equal(2, result.UnionOperations);
    }

    [Fact]
    public async Task DirectedGraph_IsRejected()
    {
        var graph = Build(["A","B"], [(0,1,1d)], true, true);
        var simulation = new MstSimulation(new ImmediateRuntime());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => simulation.BuildAsync(graph, MstVariant.Prim));

        Assert.Contains("undirected", error.Message.ToLowerInvariant());
    }

    [Theory]
    [InlineData(MstVariant.Prim)]
    [InlineData(MstVariant.Kruskal)]
    public async Task SingleVertex_IsZeroEdgeTree(MstVariant variant)
    {
        var graph = Build(["A"], [], false, true);
        var simulation = new MstSimulation(new ImmediateRuntime());
        var result = await simulation.BuildAsync(graph, variant);
        Assert.True(result.IsMinimumSpanningTree);
        Assert.Empty(result.SelectedEdgeIndices);
        Assert.Equal(0d, result.TotalWeight, 6);
    }

    private static GraphSnapshot Build(string[] labels, (int From,int To,double Weight)[] data, bool directed, bool weighted)
    {
        var ids = labels.Select(_ => Guid.NewGuid()).ToArray();
        var edgeIds = data.Select(_ => Guid.NewGuid()).ToArray();
        var neighborLists = labels.Select(_ => new List<GraphNeighborSnapshot>()).ToArray();
        var inDegree = new int[labels.Length]; var outDegree = new int[labels.Length];
        var edges = new GraphEdgeSnapshot[data.Length];
        for(var index=0;index<data.Length;index++)
        {
            var (from,to,raw)=data[index]; var weight=weighted?raw:1d; var edgeId=edgeIds[index];
            edges[index]=new GraphEdgeSnapshot(edgeId,from,to,ids[from],ids[to],labels[from],labels[to],weight,directed,GraphEdgeVisualState.Normal);
            neighborLists[from].Add(new GraphNeighborSnapshot(ids[to],labels[to],edgeId,weight,to,index)); outDegree[from]++; inDegree[to]++;
            if(!directed&&from!=to){neighborLists[to].Add(new GraphNeighborSnapshot(ids[from],labels[from],edgeId,weight,from,index));outDegree[to]++;inDegree[from]++;}
        }
        var vertices=new GraphVertexSnapshot[labels.Length];
        for(var i=0;i<labels.Length;i++)vertices[i]=new GraphVertexSnapshot(i,ids[i],labels[i],GraphVertexVisualState.Normal,neighborLists[i].ToArray(),inDegree[i],outDegree[i]);
        var cells=new GraphMatrixCellSnapshot[labels.Length*labels.Length];
        for(var r=0;r<labels.Length;r++)for(var c=0;c<labels.Length;c++){var has=false;var weight=0d;for(var i=0;i<data.Length;i++){var e=data[i];if((e.From==r&&e.To==c)||(!directed&&e.From==c&&e.To==r)){has=true;weight=weighted?e.Weight:1d;break;}}cells[r*labels.Length+c]=new GraphMatrixCellSnapshot(r,c,has,weight);}
        return new GraphSnapshot(directed,weighted,labels.Length,data.Length,vertices,edges,cells);
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

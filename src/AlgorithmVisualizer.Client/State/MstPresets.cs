using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Client.State;

public enum MstPreset
{
    Classic,
    EqualWeights,
    NegativeAndZero,
    Disconnected,
    CycleHeavy,
    Unweighted,
    DirectedInvalid,
    Single
}

public static class MstPresets
{
    public static GraphSnapshot Create(MstPreset preset) => preset switch
    {
        MstPreset.Classic => Build(
            ["A", "B", "C", "D", "E", "F"],
            [(0,1,4d),(0,2,2d),(1,2,1d),(1,3,5d),(2,3,8d),(2,4,10d),(3,4,2d),(3,5,6d),(4,5,3d)],
            directed:false, weighted:true),
        MstPreset.EqualWeights => Build(
            ["A", "B", "C", "D"],
            [(0,1,1d),(0,2,1d),(1,3,1d),(2,3,1d),(1,2,2d)],
            directed:false, weighted:true),
        MstPreset.NegativeAndZero => Build(
            ["A", "B", "C", "D", "E"],
            [(0,1,-3d),(0,2,2d),(1,2,0d),(1,3,4d),(2,3,1d),(2,4,5d),(3,4,-1d)],
            directed:false, weighted:true),
        MstPreset.Disconnected => Build(
            ["A", "B", "C", "X", "Y", "Z"],
            [(0,1,2d),(1,2,1d),(3,4,3d),(4,5,2d),(3,5,7d)],
            directed:false, weighted:true),
        MstPreset.CycleHeavy => Build(
            ["A", "B", "C", "D", "E"],
            [(0,1,1d),(1,2,2d),(2,3,3d),(3,4,4d),(4,0,5d),(0,2,2.5d),(1,3,3.5d),(2,4,4.5d)],
            directed:false, weighted:true),
        MstPreset.Unweighted => Build(
            ["A", "B", "C", "D", "E"],
            [(0,1,1d),(0,2,1d),(1,3,1d),(2,3,1d),(3,4,1d)],
            directed:false, weighted:false),
        MstPreset.DirectedInvalid => Build(
            ["A", "B", "C"],
            [(0,1,2d),(1,2,1d)],
            directed:true, weighted:true),
        MstPreset.Single => Build(["A"], [], directed:false, weighted:true),
        _ => Build(["A"], [], directed:false, weighted:true)
    };

    private static GraphSnapshot Build(
        string[] labels,
        (int From, int To, double Weight)[] edgeData,
        bool directed,
        bool weighted)
    {
        var vertexIds = new Guid[labels.Length];
        for (var index = 0; index < labels.Length; index++) vertexIds[index] = Guid.NewGuid();
        var edgeIds = new Guid[edgeData.Length];
        for (var index = 0; index < edgeData.Length; index++) edgeIds[index] = Guid.NewGuid();

        var neighborCounts = new int[labels.Length];
        var inDegrees = new int[labels.Length];
        var outDegrees = new int[labels.Length];
        for (var edgeIndex = 0; edgeIndex < edgeData.Length; edgeIndex++)
        {
            var (from, to, _) = edgeData[edgeIndex];
            neighborCounts[from]++;
            outDegrees[from]++;
            inDegrees[to]++;
            if (!directed && from != to)
            {
                neighborCounts[to]++;
                outDegrees[to]++;
                inDegrees[from]++;
            }
        }

        var neighborArrays = new GraphNeighborSnapshot[labels.Length][];
        var nextNeighbor = new int[labels.Length];
        for (var index = 0; index < labels.Length; index++) neighborArrays[index] = new GraphNeighborSnapshot[neighborCounts[index]];

        var edges = new GraphEdgeSnapshot[edgeData.Length];
        for (var edgeIndex = 0; edgeIndex < edgeData.Length; edgeIndex++)
        {
            var (from, to, rawWeight) = edgeData[edgeIndex];
            var weight = weighted ? rawWeight : 1d;
            var edgeId = edgeIds[edgeIndex];
            edges[edgeIndex] = new GraphEdgeSnapshot(edgeId, from, to, vertexIds[from], vertexIds[to], labels[from], labels[to], weight, directed, GraphEdgeVisualState.Normal);
            neighborArrays[from][nextNeighbor[from]++] = new GraphNeighborSnapshot(vertexIds[to], labels[to], edgeId, weight, to, edgeIndex);
            if (!directed && from != to)
                neighborArrays[to][nextNeighbor[to]++] = new GraphNeighborSnapshot(vertexIds[from], labels[from], edgeId, weight, from, edgeIndex);
        }

        var vertices = new GraphVertexSnapshot[labels.Length];
        for (var index = 0; index < labels.Length; index++)
            vertices[index] = new GraphVertexSnapshot(index, vertexIds[index], labels[index], GraphVertexVisualState.Normal, neighborArrays[index], inDegrees[index], outDegrees[index]);

        var matrix = new GraphMatrixCellSnapshot[labels.Length * labels.Length];
        for (var row = 0; row < labels.Length; row++)
        for (var column = 0; column < labels.Length; column++)
        {
            var hasEdge = false;
            var weight = 0d;
            for (var edgeIndex = 0; edgeIndex < edgeData.Length; edgeIndex++)
            {
                var edge = edgeData[edgeIndex];
                if ((edge.From == row && edge.To == column) || (!directed && edge.From == column && edge.To == row))
                {
                    hasEdge = true;
                    weight = weighted ? edge.Weight : 1d;
                    break;
                }
            }
            matrix[row * labels.Length + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, weight);
        }

        return new GraphSnapshot(directed, weighted, labels.Length, edges.Length, vertices, edges, matrix);
    }
}

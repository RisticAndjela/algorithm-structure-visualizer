using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Client.State;

public enum DijkstraPreset
{
    CheaperDetour,
    ReRelaxation,
    Directed,
    Disconnected,
    ZeroWeight,
    NegativeEdge,
    Single
}

public static class DijkstraPresets
{
    public static GraphSnapshot Create(DijkstraPreset preset) => preset switch
    {
        DijkstraPreset.CheaperDetour => Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 9d), (0, 2, 2d), (2, 1, 2d), (1, 3, 2d), (2, 3, 8d), (3, 4, 1d)],
            directed: false),
        DijkstraPreset.ReRelaxation => Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 8d), (0, 2, 2d), (2, 1, 1d), (1, 3, 3d), (2, 3, 10d), (3, 4, 2d)],
            directed: true),
        DijkstraPreset.Directed => Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 3d), (0, 2, 7d), (1, 2, 1d), (2, 3, 2d), (4, 0, 1d)],
            directed: true),
        DijkstraPreset.Disconnected => Build(
            ["A", "B", "C", "X", "Y"],
            [(0, 1, 2d), (1, 2, 3d), (3, 4, 1d)],
            directed: false),
        DijkstraPreset.ZeroWeight => Build(
            ["A", "B", "C", "D"],
            [(0, 1, 0d), (0, 2, 5d), (1, 2, 1d), (2, 3, 2d)],
            directed: true),
        DijkstraPreset.NegativeEdge => Build(
            ["A", "B", "C"],
            [(0, 1, 2d), (1, 2, -4d), (0, 2, 5d)],
            directed: true),
        DijkstraPreset.Single => Build(["A"], [], directed: false),
        _ => Build(["A"], [], directed: false)
    };

    private static GraphSnapshot Build(string[] labels, (int From, int To, double Weight)[] edgeData, bool directed)
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
            var (from, to, weight) = edgeData[edgeIndex];
            var edgeId = edgeIds[edgeIndex];
            edges[edgeIndex] = new GraphEdgeSnapshot(
                edgeId,
                from,
                to,
                vertexIds[from],
                vertexIds[to],
                labels[from],
                labels[to],
                weight,
                directed,
                GraphEdgeVisualState.Normal);

            neighborArrays[from][nextNeighbor[from]++] = new GraphNeighborSnapshot(vertexIds[to], labels[to], edgeId, weight, to);
            if (!directed && from != to)
            {
                neighborArrays[to][nextNeighbor[to]++] = new GraphNeighborSnapshot(vertexIds[from], labels[from], edgeId, weight, from);
            }
        }

        var vertices = new GraphVertexSnapshot[labels.Length];
        for (var index = 0; index < labels.Length; index++)
        {
            vertices[index] = new GraphVertexSnapshot(index, vertexIds[index], labels[index], GraphVertexVisualState.Normal, neighborArrays[index], inDegrees[index], outDegrees[index]);
        }

        var matrix = new GraphMatrixCellSnapshot[labels.Length * labels.Length];
        for (var row = 0; row < labels.Length; row++)
        {
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
                        weight = edge.Weight;
                        break;
                    }
                }
                matrix[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, weight);
            }
        }

        return new GraphSnapshot(directed, true, labels.Length, edges.Length, vertices, edges, matrix);
    }
}

using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Client.State;

public enum TopologicalSortPreset
{
    Prerequisites,
    MultipleSources,
    Diamond,
    DisconnectedDag,
    WeightedDag,
    Cycle,
    Undirected,
    Single
}

public static class TopologicalSortPresets
{
    public static GraphSnapshot Create(TopologicalSortPreset preset) => preset switch
    {
        TopologicalSortPreset.Prerequisites => Build(
            ["Plan", "Design", "Code", "Test", "Deploy"],
            [(0, 1, 1d), (1, 2, 1d), (2, 3, 1d), (3, 4, 1d)],
            directed: true,
            weighted: false),
        TopologicalSortPreset.MultipleSources => Build(
            ["A", "B", "C", "D", "E", "F"],
            [(0, 2, 1d), (1, 2, 1d), (1, 3, 1d), (2, 4, 1d), (3, 4, 1d), (4, 5, 1d)],
            directed: true,
            weighted: false),
        TopologicalSortPreset.Diamond => Build(
            ["A", "B", "C", "D"],
            [(0, 1, 1d), (0, 2, 1d), (1, 3, 1d), (2, 3, 1d)],
            directed: true,
            weighted: false),
        TopologicalSortPreset.DisconnectedDag => Build(
            ["A", "B", "C", "X", "Y", "Z"],
            [(0, 1, 1d), (1, 2, 1d), (3, 4, 1d)],
            directed: true,
            weighted: false),
        TopologicalSortPreset.WeightedDag => Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1, 7d), (0, 2, 2d), (1, 3, 12d), (2, 3, 1d), (3, 4, 4d)],
            directed: true,
            weighted: true),
        TopologicalSortPreset.Cycle => Build(
            ["A", "B", "C", "D"],
            [(0, 1, 1d), (1, 2, 1d), (2, 0, 1d), (2, 3, 1d)],
            directed: true,
            weighted: false),
        TopologicalSortPreset.Undirected => Build(
            ["A", "B", "C"],
            [(0, 1, 1d), (1, 2, 1d)],
            directed: false,
            weighted: false),
        TopologicalSortPreset.Single => Build(["A"], [], directed: true, weighted: false),
        _ => Build(["A"], [], directed: true, weighted: false)
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
            var (from, to, weight) = edgeData[edgeIndex];
            var edgeId = edgeIds[edgeIndex];
            var storedWeight = weighted ? weight : 1d;
            edges[edgeIndex] = new GraphEdgeSnapshot(
                edgeId, from, to, vertexIds[from], vertexIds[to], labels[from], labels[to], storedWeight, directed, GraphEdgeVisualState.Normal);
            neighborArrays[from][nextNeighbor[from]++] = new GraphNeighborSnapshot(vertexIds[to], labels[to], edgeId, storedWeight, to);
            if (!directed && from != to)
            {
                neighborArrays[to][nextNeighbor[to]++] = new GraphNeighborSnapshot(vertexIds[from], labels[from], edgeId, storedWeight, from);
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
                        weight = weighted ? edge.Weight : 1d;
                        break;
                    }
                }
                matrix[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, weight);
            }
        }

        return new GraphSnapshot(directed, weighted, labels.Length, edges.Length, vertices, edges, matrix);
    }
}

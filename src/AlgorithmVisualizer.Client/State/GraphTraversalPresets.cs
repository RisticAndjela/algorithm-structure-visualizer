using AlgorithmVisualizer.Core.DataStructures.Graph;

namespace AlgorithmVisualizer.Client.State;

public enum GraphTraversalPreset
{
    Branching,
    Cycle,
    Directed,
    Disconnected,
    SelfLoop,
    Single
}

public static class GraphTraversalPresets
{
    public static GraphSnapshot Create(GraphTraversalPreset preset) => preset switch
    {
        GraphTraversalPreset.Branching => Build(
            ["A", "B", "C", "D", "E", "F"],
            [(0, 1), (0, 2), (1, 3), (1, 4), (2, 5)],
            directed: false),
        GraphTraversalPreset.Cycle => Build(
            ["A", "B", "C", "D"],
            [(0, 1), (1, 2), (2, 0), (2, 3)],
            directed: false),
        GraphTraversalPreset.Directed => Build(
            ["A", "B", "C", "D", "E"],
            [(0, 1), (0, 2), (1, 3), (2, 4), (4, 0)],
            directed: true),
        GraphTraversalPreset.Disconnected => Build(
            ["A", "B", "C", "X", "Y"],
            [(0, 1), (1, 2), (3, 4)],
            directed: false),
        GraphTraversalPreset.SelfLoop => Build(
            ["A", "B", "C"],
            [(0, 0), (0, 1), (1, 2)],
            directed: false),
        GraphTraversalPreset.Single => Build(["A"], [], directed: false),
        _ => Build(["A"], [], directed: false)
    };

    private static GraphSnapshot Build(string[] labels, (int From, int To)[] edgePairs, bool directed)
    {
        var vertexIds = new Guid[labels.Length];
        for (var index = 0; index < labels.Length; index++) vertexIds[index] = Guid.NewGuid();

        var edgeIds = new Guid[edgePairs.Length];
        for (var index = 0; index < edgePairs.Length; index++) edgeIds[index] = Guid.NewGuid();

        var neighborCounts = new int[labels.Length];
        var inDegrees = new int[labels.Length];
        var outDegrees = new int[labels.Length];

        for (var edgeIndex = 0; edgeIndex < edgePairs.Length; edgeIndex++)
        {
            var (from, to) = edgePairs[edgeIndex];
            neighborCounts[from]++;
            outDegrees[from]++;
            inDegrees[to]++;

            if (!directed && from != to)
            {
                neighborCounts[to]++;
                outDegrees[to]++;
                inDegrees[from]++;
            }
            else if (!directed && from == to)
            {
                // One logical self-loop is one adjacency entry in the existing Graph implementation.
                inDegrees[from]++;
            }
        }

        var neighborArrays = new GraphNeighborSnapshot[labels.Length][];
        var nextNeighbor = new int[labels.Length];
        for (var index = 0; index < labels.Length; index++)
        {
            neighborArrays[index] = new GraphNeighborSnapshot[neighborCounts[index]];
        }

        var edges = new GraphEdgeSnapshot[edgePairs.Length];
        for (var edgeIndex = 0; edgeIndex < edgePairs.Length; edgeIndex++)
        {
            var (from, to) = edgePairs[edgeIndex];
            var edgeId = edgeIds[edgeIndex];
            edges[edgeIndex] = new GraphEdgeSnapshot(
                edgeId,
                from,
                to,
                vertexIds[from],
                vertexIds[to],
                labels[from],
                labels[to],
                1d,
                directed,
                GraphEdgeVisualState.Normal);

            neighborArrays[from][nextNeighbor[from]++] = new GraphNeighborSnapshot(vertexIds[to], labels[to], edgeId, 1d, to);
            if (!directed && from != to)
            {
                neighborArrays[to][nextNeighbor[to]++] = new GraphNeighborSnapshot(vertexIds[from], labels[from], edgeId, 1d, from);
            }
        }

        var vertices = new GraphVertexSnapshot[labels.Length];
        for (var index = 0; index < labels.Length; index++)
        {
            vertices[index] = new GraphVertexSnapshot(
                index,
                vertexIds[index],
                labels[index],
                GraphVertexVisualState.Normal,
                neighborArrays[index],
                inDegrees[index],
                outDegrees[index]);
        }

        var matrix = new GraphMatrixCellSnapshot[labels.Length * labels.Length];
        for (var row = 0; row < labels.Length; row++)
        {
            for (var column = 0; column < labels.Length; column++)
            {
                var hasEdge = false;
                for (var edgeIndex = 0; edgeIndex < edgePairs.Length; edgeIndex++)
                {
                    var (from, to) = edgePairs[edgeIndex];
                    if ((from == row && to == column) || (!directed && from == column && to == row))
                    {
                        hasEdge = true;
                        break;
                    }
                }

                matrix[(row * labels.Length) + column] = new GraphMatrixCellSnapshot(row, column, hasEdge, hasEdge ? 1d : 0d);
            }
        }

        return new GraphSnapshot(directed, false, labels.Length, edges.Length, vertices, edges, matrix);
    }
}

using AlgorithmVisualizer.Core.DataStructures.Graph;
using AlgorithmVisualizer.Core.DataStructures.Linear;

namespace AlgorithmVisualizer.Core.Algorithms.GraphTraversal;

internal static class GraphTraversalSupport
{
    public static int FindVertexIndex(GraphSnapshot graph, Guid id)
    {
        for (var index = 0; index < graph.Vertices.Length; index++)
        {
            if (graph.Vertices[index].Id == id) return index;
        }

        return -1;
    }

    public static int GetNeighborVertexIndex(GraphSnapshot graph, GraphNeighborSnapshot neighbor)
    {
        if (neighbor.VertexIndex >= 0 && neighbor.VertexIndex < graph.VertexCount &&
            graph.Vertices[neighbor.VertexIndex].Id == neighbor.VertexId)
        {
            return neighbor.VertexIndex;
        }

        // Compatibility fallback for older/synthetic snapshots that predate VertexIndex.
        return FindVertexIndex(graph, neighbor.VertexId);
    }

    public static Guid? FindEdgeId(GraphSnapshot graph, int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= graph.Vertices.Length || toIndex < 0 || toIndex >= graph.Vertices.Length)
        {
            return null;
        }

        var targetId = graph.Vertices[toIndex].Id;
        var neighbors = graph.Vertices[fromIndex].Neighbors;
        for (var index = 0; index < neighbors.Length; index++)
        {
            if (neighbors[index].VertexId == targetId) return neighbors[index].EdgeId;
        }

        return null;
    }

    public static int[] Copy(ManualDynamicArray<int> source)
    {
        var result = new int[source.Count];
        for (var index = 0; index < source.Count; index++) result[index] = source[index];
        return result;
    }


    public static int[] CopyRange(ManualDynamicArray<int> source, int startIndex)
    {
        if (startIndex < 0 || startIndex > source.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        var result = new int[source.Count - startIndex];
        for (var index = startIndex; index < source.Count; index++)
        {
            result[index - startIndex] = source[index];
        }

        return result;
    }

    public static int[] Copy(int[] source)
    {
        var result = new int[source.Length];
        for (var index = 0; index < source.Length; index++) result[index] = source[index];
        return result;
    }

    public static int[] CreateFilled(int length, int value)
    {
        var result = new int[length];
        for (var index = 0; index < length; index++) result[index] = value;
        return result;
    }

    public static void ValidateStart(GraphSnapshot graph, int startIndex)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.VertexCount == 0)
        {
            throw new InvalidOperationException("Traversal requires at least one graph vertex.");
        }

        if (startIndex < 0 || startIndex >= graph.VertexCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
    }
}

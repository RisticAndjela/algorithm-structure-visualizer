using AlgorithmVisualizer.Core.DataStructures.Linear;

namespace AlgorithmVisualizer.Core.DataStructures.Graph;

internal sealed class GraphVertex
{
    public GraphVertex(string label)
    {
        Label = label;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string Label { get; private set; }
    public GraphVertexVisualState VisualState { get; set; }
    public void Rename(string label) => Label = label;
    public ManualDynamicArray<GraphNeighbor> Neighbors { get; } = new();
}

internal sealed record GraphNeighbor(GraphVertex Vertex, GraphEdge Edge);

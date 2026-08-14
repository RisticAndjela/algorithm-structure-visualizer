namespace AlgorithmVisualizer.Core.DataStructures.Graph;

internal sealed class GraphEdge
{
    public GraphEdge(GraphVertex from, GraphVertex to, double weight)
    {
        From = from;
        To = to;
        Weight = weight;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public GraphVertex From { get; }
    public GraphVertex To { get; }
    public double Weight { get; private set; }
    public GraphEdgeVisualState VisualState { get; set; }
    public void SetWeight(double weight) => Weight = weight;
}

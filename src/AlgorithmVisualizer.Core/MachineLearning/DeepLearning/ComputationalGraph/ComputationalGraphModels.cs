namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.ComputationalGraph;

public enum ComputationalGraphNodeKind
{
    Input,
    Operation
}

public enum ComputationalGraphOperation
{
    None,
    Add,
    Subtract,
    Multiply,
    Square
}

public enum ComputationalGraphPhase
{
    Ready,
    InputsLoaded,
    SelectingReadyNode,
    Evaluating,
    Complete
}

public sealed record ComputationalGraphNodeDefinition(
    int Id,
    string Label,
    ComputationalGraphNodeKind Kind,
    ComputationalGraphOperation Operation,
    int[] InputNodeIds,
    double InputValue = 0d);

public sealed record ComputationalGraphConfiguration(
    ComputationalGraphNodeDefinition[] Nodes,
    int OutputNodeId);

public sealed record ComputationalGraphNodeSnapshot(
    int Id,
    string Label,
    ComputationalGraphNodeKind Kind,
    ComputationalGraphOperation Operation,
    int[] InputNodeIds,
    double Value,
    bool HasValue,
    bool IsReady,
    bool IsActive,
    int EvaluationOrder);

public sealed record ComputationalGraphSnapshot(
    ComputationalGraphNodeSnapshot[] Nodes,
    int OutputNodeId,
    int[] EvaluationSequence,
    ComputationalGraphPhase Phase,
    int ActiveNodeId,
    int ReadyCount,
    int ComputedCount,
    string FocusText)
{
    public int NodeCount => Nodes.Length;
    public int OperationCount
    {
        get
        {
            var count = 0;
            for (var index = 0; index < Nodes.Length; index++)
            {
                if (Nodes[index].Kind == ComputationalGraphNodeKind.Operation) count++;
            }
            return count;
        }
    }

    public double? OutputValue => OutputNodeId >= 0 && OutputNodeId < Nodes.Length && Nodes[OutputNodeId].HasValue
        ? Nodes[OutputNodeId].Value
        : null;
}

public sealed record ComputationalGraphRunResult(
    ComputationalGraphNodeSnapshot[] Nodes,
    int OutputNodeId,
    int[] EvaluationSequence,
    int EdgeCount,
    string Summary)
{
    public int NodeCount => Nodes.Length;
    public int OperationCount
    {
        get
        {
            var count = 0;
            for (var index = 0; index < Nodes.Length; index++)
            {
                if (Nodes[index].Kind == ComputationalGraphNodeKind.Operation) count++;
            }
            return count;
        }
    }

    public double OutputValue => Nodes[OutputNodeId].Value;
    public string ForwardComplexity => "O(V² + E) teaching scan";
    public string MemoryComplexity => "O(V + E)";
}

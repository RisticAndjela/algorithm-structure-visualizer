using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.ComputationalGraph;

/// <summary>
/// From-scratch scalar computational graph used to teach explicit dependencies and
/// forward evaluation. The scheduler repeatedly scans for operation nodes whose
/// dependencies are already computed; no graph/topological/autodiff library is used.
/// Backpropagation is intentionally not implemented here because it is taught later.
/// </summary>
public sealed class ComputationalGraphSimulation : SimulationAlgorithmBase
{
    private ComputationalGraphNodeDefinition[] _nodes = [];
    private double[] _values = [];
    private bool[] _computed = [];
    private bool[] _ready = [];
    private int[] _evaluationOrderByNode = [];
    private int[] _evaluationSequence = [];
    private int _evaluationSequenceCount;
    private int _outputNodeId;
    private int _activeNodeId = -1;
    private ComputationalGraphPhase _phase = ComputationalGraphPhase.Ready;
    private string _focusText = "Ready.";

    public ComputationalGraphSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(DefaultConfiguration());
    }

    public void Configure(ComputationalGraphConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);

        _nodes = CopyDefinitions(configuration.Nodes);
        _outputNodeId = configuration.OutputNodeId;
        ResetRunState();
    }

    public ComputationalGraphSnapshot CreateSnapshot()
    {
        RefreshReadyFlags();
        return new ComputationalGraphSnapshot(
            BuildNodeSnapshots(),
            _outputNodeId,
            CopyEvaluationSequence(),
            _phase,
            _activeNodeId,
            CountReady(),
            CountComputed(),
            _focusText);
    }

    public async Task<ComputationalGraphRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        _phase = ComputationalGraphPhase.InputsLoaded;
        _focusText = $"Inputs are available first: {InputSummary()}. Operation nodes must wait for their dependencies.";
        await NextStepAsync(_focusText, cancellationToken);

        var operationsRemaining = CountOperations();
        while (operationsRemaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RefreshReadyFlags();
            var readyCount = CountReady();
            if (readyCount == 0)
            {
                throw new InvalidOperationException("The computational graph cannot continue because unfinished nodes have unresolved or cyclic dependencies.");
            }

            _phase = ComputationalGraphPhase.SelectingReadyNode;
            _activeNodeId = -1;
            _focusText = $"Ready now: {ReadySummary()}. Only nodes whose input values already exist may run.";
            await NextStepAsync(_focusText, cancellationToken);

            var nodeId = FirstReadyNode();
            _activeNodeId = nodeId;
            _phase = ComputationalGraphPhase.Evaluating;
            _values[nodeId] = EvaluateNode(nodeId);
            _computed[nodeId] = true;
            _ready[nodeId] = false;
            _evaluationSequence[_evaluationSequenceCount] = nodeId;
            _evaluationSequenceCount++;
            _evaluationOrderByNode[nodeId] = _evaluationSequenceCount;
            operationsRemaining--;

            _focusText = $"Evaluate {_nodes[nodeId].Label}: {OperationExpression(nodeId)} = {Format(_values[nodeId])}. Store that intermediate value for every downstream dependency.";
            await NextStepAsync(_focusText, cancellationToken);
            _activeNodeId = -1;
        }

        _phase = ComputationalGraphPhase.Complete;
        RefreshReadyFlags();
        var output = _values[_outputNodeId];
        _focusText = $"Forward pass complete. Output {_nodes[_outputNodeId].Label} = {Format(output)} after {_evaluationSequenceCount} operation nodes.";
        await NextStepAsync(_focusText, cancellationToken);

        return new ComputationalGraphRunResult(
            BuildNodeSnapshots(),
            _outputNodeId,
            CopyEvaluationSequence(),
            CountEdges(),
            _focusText);
    }

    private void ResetRunState()
    {
        _values = new double[_nodes.Length];
        _computed = new bool[_nodes.Length];
        _ready = new bool[_nodes.Length];
        _evaluationOrderByNode = new int[_nodes.Length];
        var operationCount = CountOperations();
        _evaluationSequence = new int[operationCount];
        _evaluationSequenceCount = 0;
        _activeNodeId = -1;
        _phase = ComputationalGraphPhase.Ready;
        _focusText = "Ready.";

        for (var index = 0; index < _nodes.Length; index++)
        {
            _evaluationOrderByNode[index] = -1;
            if (_nodes[index].Kind != ComputationalGraphNodeKind.Input) continue;
            _values[index] = _nodes[index].InputValue;
            _computed[index] = true;
        }
        RefreshReadyFlags();
    }

    private double EvaluateNode(int nodeId)
    {
        var node = _nodes[nodeId];
        if (node.Kind != ComputationalGraphNodeKind.Operation)
            throw new InvalidOperationException("Only operation nodes can be evaluated during the forward pass.");

        var first = _values[node.InputNodeIds[0]];
        return node.Operation switch
        {
            ComputationalGraphOperation.Add => first + _values[node.InputNodeIds[1]],
            ComputationalGraphOperation.Subtract => first - _values[node.InputNodeIds[1]],
            ComputationalGraphOperation.Multiply => first * _values[node.InputNodeIds[1]],
            ComputationalGraphOperation.Square => first * first,
            _ => throw new InvalidOperationException($"Operation node {node.Label} has no supported operation.")
        };
    }

    private void RefreshReadyFlags()
    {
        for (var nodeId = 0; nodeId < _nodes.Length; nodeId++)
        {
            if (_nodes[nodeId].Kind != ComputationalGraphNodeKind.Operation || _computed[nodeId])
            {
                _ready[nodeId] = false;
                continue;
            }

            var allInputsComputed = true;
            var inputs = _nodes[nodeId].InputNodeIds;
            for (var input = 0; input < inputs.Length; input++)
            {
                if (_computed[inputs[input]]) continue;
                allInputsComputed = false;
                break;
            }
            _ready[nodeId] = allInputsComputed;
        }
    }

    private ComputationalGraphNodeSnapshot[] BuildNodeSnapshots()
    {
        var snapshots = new ComputationalGraphNodeSnapshot[_nodes.Length];
        for (var nodeId = 0; nodeId < _nodes.Length; nodeId++)
        {
            var node = _nodes[nodeId];
            snapshots[nodeId] = new ComputationalGraphNodeSnapshot(
                node.Id,
                node.Label,
                node.Kind,
                node.Operation,
                Copy(node.InputNodeIds),
                _values[nodeId],
                _computed[nodeId],
                _ready[nodeId],
                _activeNodeId == nodeId,
                _evaluationOrderByNode[nodeId]);
        }
        return snapshots;
    }

    private int FirstReadyNode()
    {
        for (var nodeId = 0; nodeId < _ready.Length; nodeId++) if (_ready[nodeId]) return nodeId;
        return -1;
    }

    private int CountReady()
    {
        var count = 0;
        for (var index = 0; index < _ready.Length; index++) if (_ready[index]) count++;
        return count;
    }

    private int CountComputed()
    {
        var count = 0;
        for (var index = 0; index < _computed.Length; index++) if (_computed[index]) count++;
        return count;
    }

    private int CountOperations()
    {
        var count = 0;
        for (var index = 0; index < _nodes.Length; index++) if (_nodes[index].Kind == ComputationalGraphNodeKind.Operation) count++;
        return count;
    }

    private int CountEdges()
    {
        var count = 0;
        for (var index = 0; index < _nodes.Length; index++) count += _nodes[index].InputNodeIds.Length;
        return count;
    }

    private string InputSummary()
    {
        var result = string.Empty;
        for (var nodeId = 0; nodeId < _nodes.Length; nodeId++)
        {
            if (_nodes[nodeId].Kind != ComputationalGraphNodeKind.Input) continue;
            if (result.Length > 0) result += " · ";
            result += $"{_nodes[nodeId].Label}={Format(_values[nodeId])}";
        }
        return result;
    }

    private string ReadySummary()
    {
        var result = string.Empty;
        for (var nodeId = 0; nodeId < _nodes.Length; nodeId++)
        {
            if (!_ready[nodeId]) continue;
            if (result.Length > 0) result += ", ";
            result += _nodes[nodeId].Label;
        }
        return result;
    }

    private string OperationExpression(int nodeId)
    {
        var node = _nodes[nodeId];
        var first = _values[node.InputNodeIds[0]];
        if (node.Operation == ComputationalGraphOperation.Square) return $"{Format(first)}²";
        var second = _values[node.InputNodeIds[1]];
        var symbol = node.Operation switch
        {
            ComputationalGraphOperation.Add => "+",
            ComputationalGraphOperation.Subtract => "−",
            ComputationalGraphOperation.Multiply => "×",
            _ => "?"
        };
        return $"{Format(first)} {symbol} {Format(second)}";
    }

    private int[] CopyEvaluationSequence()
    {
        var copy = new int[_evaluationSequenceCount];
        for (var index = 0; index < _evaluationSequenceCount; index++) copy[index] = _evaluationSequence[index];
        return copy;
    }

    private static ComputationalGraphNodeDefinition[] CopyDefinitions(IReadOnlyList<ComputationalGraphNodeDefinition> source)
    {
        var copy = new ComputationalGraphNodeDefinition[source.Count];
        for (var index = 0; index < source.Count; index++)
        {
            var node = source[index];
            copy[index] = new ComputationalGraphNodeDefinition(node.Id, node.Label, node.Kind, node.Operation, Copy(node.InputNodeIds), node.InputValue);
        }
        return copy;
    }

    private static int[] Copy(IReadOnlyList<int> source)
    {
        var copy = new int[source.Count];
        for (var index = 0; index < source.Count; index++) copy[index] = source[index];
        return copy;
    }

    private static void Validate(ComputationalGraphConfiguration configuration)
    {
        if (configuration.Nodes is null || configuration.Nodes.Length < 2)
            throw new ArgumentException("A computational graph needs at least two nodes.", nameof(configuration));
        if (configuration.OutputNodeId < 0 || configuration.OutputNodeId >= configuration.Nodes.Length)
            throw new ArgumentOutOfRangeException(nameof(configuration), "Output node must exist in the graph.");

        for (var nodeId = 0; nodeId < configuration.Nodes.Length; nodeId++)
        {
            var node = configuration.Nodes[nodeId] ?? throw new ArgumentException("Graph nodes cannot be null.", nameof(configuration));
            if (node.Id != nodeId)
                throw new ArgumentException("Computational graph node IDs must be contiguous and match their array index.", nameof(configuration));
            if (string.IsNullOrWhiteSpace(node.Label))
                throw new ArgumentException("Every computational graph node needs a label.", nameof(configuration));
            if (!double.IsFinite(node.InputValue))
                throw new ArgumentException("Input values must be finite.", nameof(configuration));

            if (node.Kind == ComputationalGraphNodeKind.Input)
            {
                if (node.Operation != ComputationalGraphOperation.None || node.InputNodeIds.Length != 0)
                    throw new ArgumentException($"Input node {node.Label} cannot have an operation or dependencies.", nameof(configuration));
                continue;
            }

            var expectedInputs = node.Operation == ComputationalGraphOperation.Square ? 1 : 2;
            if (node.Operation == ComputationalGraphOperation.None || node.InputNodeIds.Length != expectedInputs)
                throw new ArgumentException($"Operation node {node.Label} has the wrong operation arity.", nameof(configuration));

            for (var dependency = 0; dependency < node.InputNodeIds.Length; dependency++)
            {
                var inputNodeId = node.InputNodeIds[dependency];
                if (inputNodeId < 0 || inputNodeId >= configuration.Nodes.Length || inputNodeId == nodeId)
                    throw new ArgumentException($"Operation node {node.Label} references an invalid dependency.", nameof(configuration));
            }
        }

        if (configuration.Nodes[configuration.OutputNodeId].Kind != ComputationalGraphNodeKind.Operation)
            throw new ArgumentException("The teaching graph output must be produced by an operation node.", nameof(configuration));
    }

    private static ComputationalGraphConfiguration DefaultConfiguration() => new(
    [
        new(0, "x", ComputationalGraphNodeKind.Input, ComputationalGraphOperation.None, [], 2d),
        new(1, "w", ComputationalGraphNodeKind.Input, ComputationalGraphOperation.None, [], 3d),
        new(2, "b", ComputationalGraphNodeKind.Input, ComputationalGraphOperation.None, [], -1d),
        new(3, "x × w", ComputationalGraphNodeKind.Operation, ComputationalGraphOperation.Multiply, [0, 1]),
        new(4, "output", ComputationalGraphNodeKind.Operation, ComputationalGraphOperation.Add, [3, 2])
    ], 4);

    private static string Format(double value) => Math.Abs(value) < 1e-12d
        ? "0"
        : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}

using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.MessagePassing;

/// <summary>
/// Basic GNN-style message passing over small graphs. Each node gathers neighbor features,
/// aggregates by mean or sum, applies separate self/neighbor linear transforms plus bias,
/// then applies ReLU. The next layer reads only the fully committed previous layer.
/// </summary>
public sealed class MessagePassingSimulation : SimulationAlgorithmBase
{
    private double[][] _adjacency = [];
    private ManualCsrMatrix _csr = new(1, 1, [], [], [0, 0]);
    private double[][] _initialFeatures = [];
    private double[][] _features = [];
    private double[][] _nextFeatures = [];
    private double[][] _selfWeights = [];
    private double[][] _neighborWeights = [];
    private double[] _bias = [];
    private double[] _aggregate = [];
    private double[] _selfContribution = [];
    private double[] _neighborContribution = [];
    private double[] _preActivation = [];
    private int[] _neighborCounts = [];
    private MessageAggregation _aggregation = MessageAggregation.Mean;
    private int _layers = 1;
    private MessagePassingPhase _phase = MessagePassingPhase.Ready;
    private int _layer;
    private int _currentNode = -1;
    private string _focusText = "Ready.";

    public MessagePassingSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new MessagePassingConfiguration(
            [
                [0d,1d,1d,1d,0d],
                [1d,0d,0d,0d,0d],
                [1d,0d,0d,0d,1d],
                [1d,0d,0d,0d,0d],
                [0d,0d,1d,0d,0d]
            ],
            [[1d,0d],[0d,1d],[1d,1d],[2d,0d],[0d,2d]],
            [[0.8d,0.1d],[0.1d,0.8d]],
            [[0.45d,0.2d],[0.2d,0.45d]],
            [0d,0d],
            MessageAggregation.Mean,
            1));
    }

    public void Configure(MessagePassingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _adjacency = Copy(configuration.Adjacency);
        _csr = ManualCsrMatrix.FromDense(_adjacency);
        _initialFeatures = Copy(configuration.Features);
        _selfWeights = Copy(configuration.SelfWeights);
        _neighborWeights = Copy(configuration.NeighborWeights);
        _bias = Copy(configuration.Bias);
        _aggregation = configuration.Aggregation;
        _layers = configuration.Layers;
        ResetRunState();
    }

    public MessagePassingSnapshot CreateSnapshot() => new(
        _features.Length,
        _bias.Length,
        Flatten(_adjacency),
        Copy(_features),
        Copy(_nextFeatures),
        Copy(_aggregate),
        Copy(_selfContribution),
        Copy(_neighborContribution),
        Copy(_preActivation),
        Copy(_neighborCounts),
        _aggregation,
        _phase,
        _layer,
        _currentNode,
        _focusText);

    public async Task<MessagePassingRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        for (var layer = 1; layer <= _layers; layer++)
        {
            _layer = layer;
            Clear(_nextFeatures);
            for (var node = 0; node < _features.Length; node++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentNode = node;
                Clear(_aggregate);
                Clear(_selfContribution);
                Clear(_neighborContribution);
                Clear(_preActivation);

                _phase = MessagePassingPhase.Gather;
                var neighborCount = _neighborCounts[node];
                for (var edge = _csr.RowStart(node); edge < _csr.RowEnd(node); edge++)
                {
                    var neighbor = _csr.ColumnAt(edge);
                    for (var dimension = 0; dimension < _aggregate.Length; dimension++) _aggregate[dimension] += _features[neighbor][dimension];
                }
                _focusText = $"Layer {layer}, node {Label(node)}: gather {neighborCount} neighbor feature vector{(neighborCount == 1 ? string.Empty : "s")} from the previous committed layer.";
                await NextStepAsync(_focusText, cancellationToken);

                _phase = MessagePassingPhase.Aggregate;
                if (_aggregation == MessageAggregation.Mean && neighborCount > 0)
                {
                    for (var dimension = 0; dimension < _aggregate.Length; dimension++) _aggregate[dimension] /= neighborCount;
                }
                _focusText = neighborCount == 0
                    ? $"Node {Label(node)} has no neighbors, so its neighbor aggregate is the zero vector."
                    : $"Aggregate neighbors with {_aggregation.ToString().ToLowerInvariant()}: m_{Label(node)} = {VectorText(_aggregate)}.";
                await NextStepAsync(_focusText, cancellationToken);

                _phase = MessagePassingPhase.Transform;
                Multiply(_selfWeights, _features[node], _selfContribution);
                Multiply(_neighborWeights, _aggregate, _neighborContribution);
                for (var output = 0; output < _bias.Length; output++)
                    _preActivation[output] = _selfContribution[output] + _neighborContribution[output] + _bias[output];
                _focusText = $"Combine self and neighborhood: Wself·h + Wnbr·m + b = {VectorText(_preActivation)}.";
                await NextStepAsync(_focusText, cancellationToken);

                _phase = MessagePassingPhase.Activate;
                for (var output = 0; output < _bias.Length; output++) _nextFeatures[node][output] = Math.Max(0d, _preActivation[output]);
                _focusText = $"Apply ReLU and stage node {Label(node)}'s next embedding = {VectorText(_nextFeatures[node])}. Other nodes still read the old layer until commit.";
                await NextStepAsync(_focusText, cancellationToken);
            }

            _phase = MessagePassingPhase.CommitLayer;
            for (var node = 0; node < _features.Length; node++)
                for (var dimension = 0; dimension < _bias.Length; dimension++)
                    _features[node][dimension] = _nextFeatures[node][dimension];
            _currentNode = -1;
            _focusText = $"Commit layer {layer}. All nodes now switch together to the staged embeddings, so update order cannot leak into the result.";
            await NextStepAsync(_focusText, cancellationToken);
        }

        _phase = MessagePassingPhase.Complete;
        _focusText = $"Message passing complete after {_layers} layer{(_layers == 1 ? string.Empty : "s")}. Each node embedding now mixes its own features with information from up to {_layers} graph hop{(_layers == 1 ? string.Empty : "s")}.";
        await NextStepAsync(_focusText, cancellationToken);
        return new MessagePassingRunResult(Copy(_initialFeatures), Copy(_features), Copy(_neighborCounts), _aggregation, _layers, _focusText);
    }

    private void ResetRunState()
    {
        _features = Copy(_initialFeatures);
        _nextFeatures = CreateMatrix(_features.Length, _bias.Length);
        _aggregate = new double[_bias.Length];
        _selfContribution = new double[_bias.Length];
        _neighborContribution = new double[_bias.Length];
        _preActivation = new double[_bias.Length];
        _neighborCounts = new int[_features.Length];
        for (var node = 0; node < _features.Length; node++) _neighborCounts[node] = _csr.RowEnd(node) - _csr.RowStart(node);
        _phase = MessagePassingPhase.Ready;
        _layer = 0;
        _currentNode = -1;
        _focusText = "Ready.";
    }

    private static void Multiply(double[][] matrix, double[] vector, double[] destination)
    {
        for (var row = 0; row < matrix.Length; row++)
        {
            var sum = 0d;
            for (var column = 0; column < vector.Length; column++) sum += matrix[row][column] * vector[column];
            destination[row] = sum;
        }
    }

    private static void Validate(MessagePassingConfiguration configuration)
    {
        if (configuration.Adjacency is null || configuration.Adjacency.Length < 2 || configuration.Adjacency.Length > 10) throw new ArgumentException("Use 2–10 graph nodes.", nameof(configuration));
        var n = configuration.Adjacency.Length;
        for (var row = 0; row < n; row++)
        {
            if (configuration.Adjacency[row] is null || configuration.Adjacency[row].Length != n) throw new ArgumentException("Adjacency must be square.", nameof(configuration));
            for (var column = 0; column < n; column++)
            {
                var value = configuration.Adjacency[row][column];
                if (!double.IsFinite(value) || (value != 0d && Math.Abs(value - 1d) > 1e-12)) throw new ArgumentException("This teaching GNN uses binary adjacency values 0 or 1.", nameof(configuration));
            }
        }
        if (configuration.Features is null || configuration.Features.Length != n || configuration.Features[0] is null || configuration.Features[0].Length < 1 || configuration.Features[0].Length > 4) throw new ArgumentException("Provide one 1–4D feature vector per node.", nameof(configuration));
        var inputDimension = configuration.Features[0].Length;
        for (var node = 0; node < n; node++)
        {
            if (configuration.Features[node] is null || configuration.Features[node].Length != inputDimension) throw new ArgumentException("Node feature vectors must have the same dimension.", nameof(configuration));
            for (var dimension = 0; dimension < inputDimension; dimension++) if (!double.IsFinite(configuration.Features[node][dimension])) throw new ArgumentException("Node features must be finite.", nameof(configuration));
        }
        ValidateWeightMatrix(configuration.SelfWeights, inputDimension, nameof(configuration.SelfWeights));
        ValidateWeightMatrix(configuration.NeighborWeights, inputDimension, nameof(configuration.NeighborWeights));
        if (configuration.Bias is null || configuration.Bias.Length != inputDimension) throw new ArgumentException("Bias dimension must match the feature dimension.", nameof(configuration));
        for (var index = 0; index < configuration.Bias.Length; index++) if (!double.IsFinite(configuration.Bias[index])) throw new ArgumentException("Bias values must be finite.", nameof(configuration));
        if (configuration.Layers < 1 || configuration.Layers > 3) throw new ArgumentOutOfRangeException(nameof(configuration.Layers), "Use 1–3 message-passing layers in the teaching lab.");
    }

    private static void ValidateWeightMatrix(double[][] matrix, int dimension, string name)
    {
        if (matrix is null || matrix.Length != dimension) throw new ArgumentException("Weight matrix must be square and match the feature dimension.", name);
        for (var row = 0; row < dimension; row++)
        {
            if (matrix[row] is null || matrix[row].Length != dimension) throw new ArgumentException("Weight matrix must be square and match the feature dimension.", name);
            for (var column = 0; column < dimension; column++) if (!double.IsFinite(matrix[row][column])) throw new ArgumentException("Weight values must be finite.", name);
        }
    }

    private static double[][] CreateMatrix(int rows, int columns) { var result = new double[rows][]; for (var row = 0; row < rows; row++) result[row] = new double[columns]; return result; }
    private static void Clear(double[] values) { for (var index = 0; index < values.Length; index++) values[index] = 0d; }
    private static void Clear(double[][] values) { for (var row = 0; row < values.Length; row++) Clear(values[row]); }
    private static double[][] Copy(double[][] source) { var result = new double[source.Length][]; for (var row = 0; row < source.Length; row++) result[row] = Copy(source[row]); return result; }
    private static double[] Copy(double[] source) { var result = new double[source.Length]; for (var index = 0; index < source.Length; index++) result[index] = source[index]; return result; }
    private static int[] Copy(int[] source) { var result = new int[source.Length]; for (var index = 0; index < source.Length; index++) result[index] = source[index]; return result; }
    private static double[] Flatten(double[][] source) { var n = source.Length; var result = new double[n * n]; for (var row = 0; row < n; row++) for (var column = 0; column < n; column++) result[(row * n) + column] = source[row][column]; return result; }
    private static string Label(int node) => ((char)('A' + node)).ToString();
    private static string VectorText(double[] values) { var parts = new string[values.Length]; for (var index = 0; index < values.Length; index++) parts[index] = Math.Abs(values[index]) < 1e-12 ? "0" : values[index].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture); return "[" + string.Join(", ", parts) + "]"; }
}

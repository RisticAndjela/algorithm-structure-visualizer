using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.PageRank;

/// <summary>
/// PageRank over a directed graph stored in project-owned CSR adjacency.
/// Each source distributes rank along its outgoing edges explicitly. Dangling rank
/// and teleport mass are kept visible so probability cannot silently disappear.
/// </summary>
public sealed class PageRankSimulation : SimulationAlgorithmBase
{
    private double[][] _adjacency = [];
    private ManualCsrMatrix _csr = new(1, 1, [], [], [0, 0]);
    private double _damping = 0.85d;
    private int _maxIterations = 16;
    private double _tolerance = 1e-6;
    private double[] _ranks = [1d];
    private double[] _nextRanks = [1d];
    private double[] _currentContributions = [0d];
    private int[] _outDegrees = [0];
    private PageRankPhase _phase = PageRankPhase.Ready;
    private int _iteration;
    private int _currentSource = -1;
    private double _danglingMass;
    private double _delta;
    private string _focusText = "Ready.";

    public PageRankSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
        Configure(new PageRankConfiguration(
            [
                [0d, 0d, 0d, 0d, 0d],
                [1d, 0d, 0d, 0d, 0d],
                [1d, 0d, 0d, 0d, 0d],
                [1d, 0d, 0d, 0d, 0d],
                [1d, 0d, 0d, 0d, 0d]
            ]));
    }

    public void Configure(PageRankConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration);
        _adjacency = Copy(configuration.Adjacency);
        _csr = ManualCsrMatrix.FromDense(_adjacency);
        _damping = configuration.Damping;
        _maxIterations = configuration.MaxIterations;
        _tolerance = configuration.Tolerance;
        ResetRunState();
    }

    public PageRankSnapshot CreateSnapshot() => new(
        _adjacency.Length,
        Flatten(_adjacency),
        _csr.CopyRowPointers(),
        _csr.CopyColumnIndexes(),
        Copy(_ranks),
        Copy(_nextRanks),
        Copy(_currentContributions),
        Copy(_outDegrees),
        _phase,
        _iteration,
        _currentSource,
        _danglingMass,
        _delta,
        _focusText);

    public async Task<PageRankRunResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ResetRunState();
        var count = _adjacency.Length;
        var converged = false;

        for (var iteration = 1; iteration <= _maxIterations; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _iteration = iteration;
            _currentSource = -1;
            _phase = PageRankPhase.Teleport;
            var teleport = (1d - _damping) / count;
            for (var node = 0; node < count; node++) _nextRanks[node] = teleport;
            _focusText = $"Iteration {iteration}: every node first receives teleport mass (1 − d)/N = {Format(teleport)}.";
            await NextStepAsync(_focusText, cancellationToken);

            _phase = PageRankPhase.DanglingMass;
            _danglingMass = 0d;
            for (var node = 0; node < count; node++) if (_outDegrees[node] == 0) _danglingMass += _ranks[node];
            var danglingShare = _damping * _danglingMass / count;
            if (danglingShare != 0d) for (var node = 0; node < count; node++) _nextRanks[node] += danglingShare;
            _focusText = _danglingMass > 0d
                ? $"Dangling nodes hold {Format(_danglingMass)} rank. Redistribute d·dangling/N = {Format(danglingShare)} to every node so probability mass is preserved."
                : "No dangling node holds rank in this graph, so there is no dangling-mass redistribution this iteration.";
            await NextStepAsync(_focusText, cancellationToken);

            _phase = PageRankPhase.Distributing;
            for (var source = 0; source < count; source++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentSource = source;
                Clear(_currentContributions);
                var degree = _outDegrees[source];
                if (degree == 0)
                {
                    _focusText = $"Node {Label(source)} has no outgoing edges. Its rank was already handled as dangling mass.";
                    await NextStepAsync(_focusText, cancellationToken);
                    continue;
                }

                var share = _damping * _ranks[source] / degree;
                for (var index = _csr.RowStart(source); index < _csr.RowEnd(source); index++)
                {
                    var destination = _csr.ColumnAt(index);
                    _nextRanks[destination] += share;
                    _currentContributions[destination] += share;
                }
                _focusText = $"Node {Label(source)} distributes d·rank/outdegree = {Format(share)} across {degree} outgoing edge{(degree == 1 ? string.Empty : "s")}.";
                await NextStepAsync(_focusText, cancellationToken);
            }

            _phase = PageRankPhase.Commit;
            _currentSource = -1;
            _delta = 0d;
            var sum = 0d;
            for (var node = 0; node < count; node++) sum += _nextRanks[node];
            if (sum <= 0d || !double.IsFinite(sum)) throw new InvalidOperationException("PageRank produced invalid probability mass.");
            for (var node = 0; node < count; node++)
            {
                _nextRanks[node] /= sum;
                _delta += Math.Abs(_nextRanks[node] - _ranks[node]);
            }
            for (var node = 0; node < count; node++) _ranks[node] = _nextRanks[node];
            _focusText = $"Commit iteration {iteration}: normalize rank to sum 1. L1 change = {Format(_delta)}.";
            await NextStepAsync(_focusText, cancellationToken);

            if (_delta <= _tolerance)
            {
                converged = true;
                break;
            }
        }

        var top = 0;
        var rankSum = 0d;
        for (var node = 0; node < _ranks.Length; node++)
        {
            rankSum += _ranks[node];
            if (_ranks[node] > _ranks[top]) top = node;
        }

        _phase = PageRankPhase.Complete;
        _currentSource = -1;
        Clear(_currentContributions);
        _focusText = $"PageRank {(converged ? "converged" : "stopped at the iteration budget")}. Node {Label(top)} has the largest rank, {Format(_ranks[top])}, and total rank remains {Format(rankSum)}.";
        await NextStepAsync(_focusText, cancellationToken);
        return new PageRankRunResult(Copy(_ranks), _iteration, _delta, top, _ranks[top], rankSum, converged, _focusText);
    }

    private void ResetRunState()
    {
        var count = _adjacency.Length;
        _ranks = new double[count];
        _nextRanks = new double[count];
        _currentContributions = new double[count];
        _outDegrees = new int[count];
        for (var node = 0; node < count; node++)
        {
            _ranks[node] = 1d / count;
            _outDegrees[node] = _csr.RowEnd(node) - _csr.RowStart(node);
        }
        _phase = PageRankPhase.Ready;
        _iteration = 0;
        _currentSource = -1;
        _danglingMass = 0d;
        _delta = 0d;
        _focusText = "Ready.";
    }

    private static void Validate(PageRankConfiguration configuration)
    {
        if (configuration.Adjacency is null || configuration.Adjacency.Length < 2 || configuration.Adjacency.Length > 10) throw new ArgumentException("Use 2–10 graph nodes.", nameof(configuration));
        var count = configuration.Adjacency.Length;
        for (var row = 0; row < count; row++)
        {
            if (configuration.Adjacency[row] is null || configuration.Adjacency[row].Length != count) throw new ArgumentException("PageRank adjacency must be square.", nameof(configuration));
            for (var column = 0; column < count; column++)
            {
                var value = configuration.Adjacency[row][column];
                if (!double.IsFinite(value) || value < 0d) throw new ArgumentException("Adjacency values must be finite and non-negative.", nameof(configuration));
                if (value != 0d && Math.Abs(value - 1d) > 1e-12) throw new ArgumentException("This teaching PageRank uses binary directed edges (0 or 1).", nameof(configuration));
            }
        }
        if (!double.IsFinite(configuration.Damping) || configuration.Damping <= 0d || configuration.Damping >= 1d) throw new ArgumentOutOfRangeException(nameof(configuration.Damping), "Damping must be between 0 and 1.");
        if (configuration.MaxIterations < 1 || configuration.MaxIterations > 40) throw new ArgumentOutOfRangeException(nameof(configuration.MaxIterations));
        if (!double.IsFinite(configuration.Tolerance) || configuration.Tolerance <= 0d) throw new ArgumentOutOfRangeException(nameof(configuration.Tolerance));
    }

    private static string Label(int node) => ((char)('A' + node)).ToString();
    private static void Clear(double[] values) { for (var index = 0; index < values.Length; index++) values[index] = 0d; }
    private static double[][] Copy(double[][] source) { var copy = new double[source.Length][]; for (var row = 0; row < source.Length; row++) copy[row] = Copy(source[row]); return copy; }
    private static double[] Copy(double[] source) { var copy = new double[source.Length]; for (var index = 0; index < source.Length; index++) copy[index] = source[index]; return copy; }
    private static int[] Copy(int[] source) { var copy = new int[source.Length]; for (var index = 0; index < source.Length; index++) copy[index] = source[index]; return copy; }
    private static double[] Flatten(double[][] source) { var n = source.Length; var result = new double[n * n]; for (var row = 0; row < n; row++) for (var column = 0; column < n; column++) result[(row * n) + column] = source[row][column]; return result; }
    private static string Format(double value) => Math.Abs(value) < 1e-12 ? "0" : value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
}

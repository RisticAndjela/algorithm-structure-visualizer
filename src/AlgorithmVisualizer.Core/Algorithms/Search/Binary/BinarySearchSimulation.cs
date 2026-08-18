using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Search.Binary;

/// <summary>
/// Iterative Binary Search over a fixed nondecreasing raw array. Basic mode returns
/// the first matching midpoint encountered. FirstOccurrence mode remembers a match
/// and continues into the left half until no earlier duplicate can remain.
/// </summary>
public sealed class BinarySearchSimulation : SimulationAlgorithmBase
{
    private BinarySearchElement[] _elements = Array.Empty<BinarySearchElement>();
    private int[] _initialValues = Array.Empty<int>();
    private int _target;
    private BinarySearchVariant _variant = BinarySearchVariant.AnyMatch;
    private int _left;
    private int _right = -1;
    private int _mid = -1;
    private int _comparisons;
    private int _rangeReductions;
    private int? _candidateIndex;
    private int? _foundIndex;
    private BinarySearchPhase _phase = BinarySearchPhase.Ready;

    public BinarySearchSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime) { }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (!IsSortedNondecreasing(values))
            throw new ArgumentException("Binary Search requires values sorted in nondecreasing order.", nameof(values));

        _elements = new BinarySearchElement[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new BinarySearchElement(values[index], index);
            _initialValues[index] = values[index];
        }
        ResetRunState();
    }

    public void Configure(int target, BinarySearchVariant variant)
    {
        _target = target;
        _variant = variant;
        ResetRunState(keepConfiguration: true);
    }

    public BinarySearchSnapshot CreateSnapshot()
    {
        var elements = new BinarySearchElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            elements[index] = new BinarySearchElementSnapshot(element.Value, element.OriginalIndex, element.VisualState);
        }

        return new BinarySearchSnapshot(elements, _target, _variant, _left, _right, _mid,
            _comparisons, _rangeReductions, _candidateIndex, _foundIndex, _phase);
    }

    public async Task<BinarySearchResult> SearchAsync(
        int target,
        BinarySearchVariant variant = BinarySearchVariant.AnyMatch,
        CancellationToken cancellationToken = default)
    {
        _target = target;
        _variant = variant;
        CaptureInitialValues();
        ResetRunState(keepConfiguration: true);

        if (_elements.Length == 0)
        {
            _phase = BinarySearchPhase.NotFound;
            await NextStepAsync($"Search for {target}: the sorted array is empty, so there is no active range and the target is not found.", cancellationToken);
            _phase = BinarySearchPhase.Complete;
            return BuildResult();
        }

        await NextStepAsync(
            $"Start Binary Search for {target} with left = 0 and right = {_elements.Length - 1}. Only this inclusive range can still contain the answer.",
            cancellationToken);

        while (_left <= _right)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _mid = _left + ((_right - _left) / 2);
            _phase = BinarySearchPhase.Checking;
            _comparisons++;
            RefreshVisualStates();

            var midValue = _elements[_mid].Value;
            await NextStepAsync(
                $"Comparison {_comparisons}: mid = {_mid}. Compare a[{_mid}] = {midValue} with target {target} inside range [{_left}, {_right}].",
                cancellationToken);

            if (midValue == target)
            {
                if (_variant == BinarySearchVariant.AnyMatch)
                {
                    _foundIndex = _mid;
                    _phase = BinarySearchPhase.Found;
                    RefreshVisualStates();
                    await NextStepAsync($"Match found at midpoint {_mid}. Basic Binary Search may return this matching index immediately.", cancellationToken);
                    _phase = BinarySearchPhase.Complete;
                    return BuildResult();
                }

                _candidateIndex = _mid;
                _phase = BinarySearchPhase.CandidateFound;
                RefreshVisualStates();
                await NextStepAsync(
                    $"Match found at index {_mid}, but First-occurrence mode keeps it only as the current candidate. Continue left to check whether an earlier duplicate exists.",
                    cancellationToken);

                _right = _mid - 1;
                _rangeReductions++;
                _mid = -1;
                RefreshVisualStates();
                await NextStepAsync($"Discard indexes {_candidateIndex} and to its right from the first-occurrence search. New candidate range is [{_left}, {_right}].", cancellationToken);
                continue;
            }

            if (midValue < target)
            {
                var oldLeft = _left;
                _left = _mid + 1;
                _rangeReductions++;
                _mid = -1;
                RefreshVisualStates();
                await NextStepAsync(
                    $"{midValue} < {target}. Every index from {oldLeft} through {_left - 1} is too small, so discard that half. New range is [{_left}, {_right}].",
                    cancellationToken);
            }
            else
            {
                var oldRight = _right;
                _right = _mid - 1;
                _rangeReductions++;
                _mid = -1;
                RefreshVisualStates();
                await NextStepAsync(
                    $"{midValue} > {target}. Every index from {_right + 1} through {oldRight} is too large, so discard that half. New range is [{_left}, {_right}].",
                    cancellationToken);
            }
        }

        _mid = -1;
        if (_variant == BinarySearchVariant.FirstOccurrence && _candidateIndex is int candidate)
        {
            _foundIndex = candidate;
            _phase = BinarySearchPhase.Found;
            RefreshVisualStates();
            await NextStepAsync($"The range is empty, so no earlier duplicate can exist. Return candidate index {candidate} as the first occurrence.", cancellationToken);
        }
        else
        {
            _phase = BinarySearchPhase.NotFound;
            RefreshVisualStates();
            await NextStepAsync($"The active range is empty (left > right). Target {target} is not present in the sorted array.", cancellationToken);
        }

        _phase = BinarySearchPhase.Complete;
        return BuildResult();
    }

    public static bool IsSortedNondecreasing(IReadOnlyList<int> values)
    {
        for (var index = 1; index < values.Count; index++)
            if (values[index - 1] > values[index]) return false;
        return true;
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) _initialValues[index] = _elements[index].Value;
    }

    private void ResetRunState(bool keepConfiguration = false)
    {
        if (!keepConfiguration)
        {
            _target = 0;
            _variant = BinarySearchVariant.AnyMatch;
        }
        _left = 0;
        _right = _elements.Length - 1;
        _mid = -1;
        _comparisons = 0;
        _rangeReductions = 0;
        _candidateIndex = null;
        _foundIndex = null;
        _phase = BinarySearchPhase.Ready;
        RefreshVisualStates();
    }

    private void RefreshVisualStates()
    {
        for (var index = 0; index < _elements.Length; index++)
            _elements[index].VisualState = index >= _left && index <= _right
                ? BinarySearchElementVisualState.Active
                : BinarySearchElementVisualState.Eliminated;

        if (_candidateIndex is int candidate && candidate >= 0 && candidate < _elements.Length)
            _elements[candidate].VisualState = BinarySearchElementVisualState.Candidate;

        if (_mid >= 0 && _mid < _elements.Length && _phase == BinarySearchPhase.Checking)
            _elements[_mid].VisualState = BinarySearchElementVisualState.Current;

        if (_foundIndex is int found && found >= 0 && found < _elements.Length)
            _elements[found].VisualState = BinarySearchElementVisualState.Found;
    }

    private BinarySearchResult BuildResult() => new(
        Clone(_initialValues),
        _target,
        _variant,
        _foundIndex is not null,
        _foundIndex,
        _comparisons,
        _rangeReductions,
        FindFirstOccurrence(_target));

    private int? FindFirstOccurrence(int target)
    {
        for (var index = 0; index < _elements.Length; index++)
            if (_elements[index].Value == target) return index;
        return null;
    }

    private static int[] Clone(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }
}

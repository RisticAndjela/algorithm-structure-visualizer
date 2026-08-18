using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Search.Linear;

/// <summary>
/// Canonical first-match Linear Search implemented manually over a fixed raw array.
/// The algorithm checks indexes from 0 to n-1, stops on the first matching value,
/// and never uses LINQ/search helpers that would hide the traversal being taught.
/// </summary>
public sealed class LinearSearchSimulation : SimulationAlgorithmBase
{
    private LinearSearchElement[] _elements = Array.Empty<LinearSearchElement>();
    private int[] _initialValues = Array.Empty<int>();
    private int _target;
    private int _currentIndex = -1;
    private int _comparisons;
    private int _checkedCount;
    private int? _foundIndex;
    private LinearSearchPhase _phase = LinearSearchPhase.Ready;

    public LinearSearchSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _elements = new LinearSearchElement[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new LinearSearchElement(values[index], index);
            _initialValues[index] = values[index];
        }

        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState(keepTarget: true);

    public void SetTarget(int target)
    {
        _target = target;
        _currentIndex = -1;
        _comparisons = 0;
        _checkedCount = 0;
        _foundIndex = null;
        _phase = LinearSearchPhase.Ready;
        RefreshVisualStates();
    }

    public LinearSearchSnapshot CreateSnapshot()
    {
        var snapshot = new LinearSearchElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            snapshot[index] = new LinearSearchElementSnapshot(element.Value, element.OriginalIndex, element.VisualState);
        }

        return new LinearSearchSnapshot(
            snapshot,
            _target,
            _currentIndex,
            _comparisons,
            _checkedCount,
            _foundIndex,
            _phase);
    }

    public async Task<LinearSearchResult> SearchAsync(int target, CancellationToken cancellationToken = default)
    {
        _target = target;
        CaptureInitialValues();
        ResetRunState(keepTarget: true);

        if (_elements.Length == 0)
        {
            _phase = LinearSearchPhase.NotFound;
            await NextStepAsync(
                $"Search for {target}: the array is empty, so there are no indexes to inspect and the target is not found.",
                cancellationToken);
            _phase = LinearSearchPhase.Complete;
            return BuildResult();
        }

        await NextStepAsync(
            $"Search for {target} from left to right. Linear Search cannot skip an unchecked index, so begin at index 0.",
            cancellationToken);

        for (var index = 0; index < _elements.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _currentIndex = index;
            _phase = LinearSearchPhase.Checking;
            _comparisons++;
            _checkedCount = index + 1;
            RefreshVisualStates();

            await NextStepAsync(
                $"Comparison {_comparisons}: inspect index {index}. Its value is {_elements[index].Value}; compare it with target {target}.",
                cancellationToken);

            if (_elements[index].Value == target)
            {
                _foundIndex = index;
                _phase = LinearSearchPhase.Found;
                RefreshVisualStates();
                await NextStepAsync(
                    $"Match found at index {index}. Stop immediately: canonical Linear Search returns the first matching occurrence.",
                    cancellationToken);

                _phase = LinearSearchPhase.Complete;
                return BuildResult();
            }

            _currentIndex = -1;
            RefreshVisualStates();
            await NextStepAsync(
                index + 1 < _elements.Length
                    ? $"Index {index} does not match. Mark it checked and continue to index {index + 1}."
                    : $"Index {index} does not match. It is the final slot, so every array position has now been checked.",
                cancellationToken);
        }

        _currentIndex = -1;
        _phase = LinearSearchPhase.NotFound;
        RefreshVisualStates();
        await NextStepAsync(
            $"Target {target} is not present. Linear Search inspected all {_elements.Length} indexes, so this run required {_comparisons} comparisons.",
            cancellationToken);

        _phase = LinearSearchPhase.Complete;
        return BuildResult();
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            _initialValues[index] = _elements[index].Value;
        }
    }

    private void ResetRunState(bool keepTarget = false)
    {
        if (!keepTarget) _target = 0;
        _currentIndex = -1;
        _comparisons = 0;
        _checkedCount = 0;
        _foundIndex = null;
        _phase = LinearSearchPhase.Ready;
        RefreshVisualStates();
    }

    private void RefreshVisualStates()
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = index < _checkedCount
                ? LinearSearchElementVisualState.Checked
                : LinearSearchElementVisualState.Unvisited;
        }

        if (_foundIndex is int foundIndex && foundIndex >= 0 && foundIndex < _elements.Length)
        {
            _elements[foundIndex].VisualState = LinearSearchElementVisualState.Found;
            return;
        }

        if (_currentIndex >= 0 && _currentIndex < _elements.Length && _phase == LinearSearchPhase.Checking)
        {
            _elements[_currentIndex].VisualState = LinearSearchElementVisualState.Current;
        }
    }

    private LinearSearchResult BuildResult()
    {
        var values = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) values[index] = _elements[index].Value;

        return new LinearSearchResult(
            _initialValues.Length == values.Length ? Clone(_initialValues) : values,
            _target,
            _foundIndex is not null,
            _foundIndex,
            _comparisons,
            _checkedCount,
            FindFirstOccurrence(_target));
    }

    private int? FindFirstOccurrence(int target)
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            if (_elements[index].Value == target) return index;
        }
        return null;
    }

    private static int[] Clone(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }
}

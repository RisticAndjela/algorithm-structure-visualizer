using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;

/// <summary>
/// Ascending Selection Sort implemented directly over a fixed raw array of teaching elements.
/// Classic mode performs a direct minimum swap. StableShift mode preserves equal-item order by
/// shifting the block between target and minimum one slot right before inserting the minimum.
/// Both variants keep the canonical full minimum scan and therefore remain Θ(n²).
/// </summary>
public sealed class SelectionSortSimulation : SimulationAlgorithmBase
{
    private SelectionSortElement[] _elements = Array.Empty<SelectionSortElement>();
    private int[] _initialValues = Array.Empty<int>();
    private int _currentPass;
    private int _comparisons;
    private int _swaps;
    private int _moves;
    private int _targetIndex = -1;
    private int _scanIndex = -1;
    private int _minimumIndex = -1;
    private int _sortedPrefixLength;
    private SelectionSortPhase _phase = SelectionSortPhase.Ready;
    private SelectionSortVariant _variant = SelectionSortVariant.Classic;

    public SelectionSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _elements = new SelectionSortElement[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new SelectionSortElement(values[index], index);
            _initialValues[index] = values[index];
        }

        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public SelectionSortSnapshot CreateSnapshot()
    {
        var snapshot = new SelectionSortElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            snapshot[index] = new SelectionSortElementSnapshot(element.Value, element.OriginalIndex, element.VisualState);
        }

        return new SelectionSortSnapshot(
            snapshot,
            _currentPass,
            _comparisons,
            _swaps,
            _moves,
            _targetIndex,
            _scanIndex,
            _minimumIndex,
            _sortedPrefixLength,
            _phase,
            _variant);
    }

    public Task<SelectionSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(SelectionSortVariant.Classic, cancellationToken);

    public async Task<SelectionSortResult> SortAsync(SelectionSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = SelectionSortPhase.Complete;
            await NextStepAsync("The array is empty. There is no minimum to search for, so it is already sorted.", cancellationToken);
            return BuildResult();
        }

        if (_elements.Length == 1)
        {
            _elements[0].VisualState = SelectionSortElementVisualState.Sorted;
            _sortedPrefixLength = 1;
            _phase = SelectionSortPhase.Complete;
            await NextStepAsync($"The array contains only {_elements[0].Value}. One item is already sorted, so Selection Sort performs 0 comparisons and no movement.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            _variant == SelectionSortVariant.Classic
                ? $"Start with {_elements.Length} values in Classic mode. For each output position, scan the whole unsorted suffix, remember its minimum, then use at most one direct swap."
                : $"Start with {_elements.Length} values in Stable Shift mode. The minimum search is unchanged, but instead of a distant swap the selected minimum is inserted at the target while the values in between shift one slot right.",
            cancellationToken);

        for (var target = 0; target < _elements.Length - 1; target++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentPass = target + 1;
            _targetIndex = target;
            _scanIndex = -1;
            _minimumIndex = target;
            _phase = SelectionSortPhase.Selecting;
            RefreshVisualStates();

            await NextStepAsync(
                $"Pass {_currentPass}: index {target} is the next position to fix. Start by treating {_elements[_minimumIndex].Value} at index {_minimumIndex} as the current minimum.",
                cancellationToken);

            for (var scan = target + 1; scan < _elements.Length; scan++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _scanIndex = scan;
                _phase = SelectionSortPhase.Comparing;
                _comparisons++;
                RefreshVisualStates();

                await NextStepAsync(
                    $"Compare scan value {_elements[scan].Value} at index {scan} with current minimum {_elements[_minimumIndex].Value} at index {_minimumIndex}.",
                    cancellationToken);

                if (_elements[scan].Value < _elements[_minimumIndex].Value)
                {
                    _minimumIndex = scan;
                    _phase = SelectionSortPhase.NewMinimum;
                    RefreshVisualStates();
                    await NextStepAsync(
                        $"New minimum found: {_elements[_minimumIndex].Value} at index {_minimumIndex}. Keep scanning because a later value may be even smaller.",
                        cancellationToken);
                }
                else
                {
                    await NextStepAsync(
                        $"Keep {_elements[_minimumIndex].Value} as the minimum. {_elements[scan].Value} is not smaller, so the candidate does not change.",
                        cancellationToken);
                }
            }

            _scanIndex = -1;
            _phase = SelectionSortPhase.SelectionComplete;
            RefreshVisualStates();
            await NextStepAsync(
                $"Scan complete. {_elements[_minimumIndex].Value} at index {_minimumIndex} is the smallest value in the unsorted region, so it belongs at index {_targetIndex}.",
                cancellationToken);

            if (_minimumIndex != _targetIndex)
            {
                if (_variant == SelectionSortVariant.Classic)
                {
                    await PlaceMinimumWithClassicSwapAsync(cancellationToken);
                }
                else
                {
                    await PlaceMinimumWithStableShiftAsync(cancellationToken);
                }
            }
            else
            {
                await NextStepAsync(
                    $"No movement is needed: {_elements[_targetIndex].Value} was already the minimum of the unsorted region and is already sitting at index {_targetIndex}.",
                    cancellationToken);
            }

            _sortedPrefixLength = target + 1;
            _phase = SelectionSortPhase.PassComplete;
            _scanIndex = -1;
            _minimumIndex = -1;
            RefreshVisualStates();
            await NextStepAsync(
                $"Pass {_currentPass} complete. The first {_sortedPrefixLength} position{(_sortedPrefixLength == 1 ? string.Empty : "s")} form a fixed sorted prefix; later passes never change them.",
                cancellationToken);
        }

        _sortedPrefixLength = _elements.Length;
        _targetIndex = -1;
        _scanIndex = -1;
        _minimumIndex = -1;
        _phase = SelectionSortPhase.Complete;
        RefreshVisualStates();
        await NextStepAsync(
            _variant == SelectionSortVariant.Classic
                ? $"Classic Selection Sort is complete after {_currentPass} passes, {_comparisons} comparisons, and {_swaps} direct swaps."
                : $"Stable Shift Selection Sort is complete after {_currentPass} passes and {_comparisons} comparisons. It used {_moves} array-slot moves instead of long-distance swaps so equal values could keep their relative order.",
            cancellationToken);

        return BuildResult();
    }

    private async Task PlaceMinimumWithClassicSwapAsync(CancellationToken cancellationToken)
    {
        var selectedValue = _elements[_minimumIndex].Value;
        var displacedValue = _elements[_targetIndex].Value;
        _phase = SelectionSortPhase.Swapping;
        MarkClassicMovePair();
        await NextStepAsync(
            $"Classic move: swap minimum {selectedValue} into index {_targetIndex}, while {displacedValue} jumps directly to index {_minimumIndex}.",
            cancellationToken);

        var temporary = _elements[_targetIndex];
        _elements[_targetIndex] = _elements[_minimumIndex];
        _elements[_minimumIndex] = temporary;
        _swaps++;
        _moves += 2;
        MarkClassicMovedPair();

        await NextStepAsync(
            $"Direct swap complete. Index {_targetIndex} now contains {selectedValue}. Because an element can jump across equal values, Classic Selection Sort is not stable.",
            cancellationToken);
    }

    private async Task PlaceMinimumWithStableShiftAsync(CancellationToken cancellationToken)
    {
        var sourceIndex = _minimumIndex;
        var selected = _elements[sourceIndex];
        var distance = sourceIndex - _targetIndex;
        _phase = SelectionSortPhase.Shifting;
        MarkStableShiftRange(_targetIndex, sourceIndex, SelectionSortElementVisualState.ShiftRequired);

        await NextStepAsync(
            $"Stable move: temporarily hold minimum {selected.Value}, shift the {distance} item{(distance == 1 ? string.Empty : "s")} between indexes {_targetIndex} and {sourceIndex - 1} one slot right, then insert the minimum at index {_targetIndex}.",
            cancellationToken);

        for (var index = sourceIndex; index > _targetIndex; index--)
        {
            _elements[index] = _elements[index - 1];
            _moves++;
        }
        _elements[_targetIndex] = selected;
        _moves++;
        _minimumIndex = _targetIndex;
        MarkStableShiftRange(_targetIndex, sourceIndex, SelectionSortElementVisualState.Shifted);

        await NextStepAsync(
            $"Stable insertion complete using {distance + 1} array-slot writes. No equal item was jumped over by a distant swap, so their original relative order is preserved.",
            cancellationToken);
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) _initialValues[index] = _elements[index].Value;
    }

    private void ResetRunState()
    {
        _currentPass = 0;
        _comparisons = 0;
        _swaps = 0;
        _moves = 0;
        _targetIndex = -1;
        _scanIndex = -1;
        _minimumIndex = -1;
        _sortedPrefixLength = 0;
        _phase = SelectionSortPhase.Ready;
        RefreshVisualStates();
    }

    private void RefreshVisualStates()
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = index < _sortedPrefixLength
                ? SelectionSortElementVisualState.Sorted
                : SelectionSortElementVisualState.Normal;
        }

        if (_minimumIndex >= _sortedPrefixLength && _minimumIndex < _elements.Length)
        {
            _elements[_minimumIndex].VisualState = _phase == SelectionSortPhase.NewMinimum
                ? SelectionSortElementVisualState.NewMinimum
                : SelectionSortElementVisualState.SelectedMinimum;
        }

        if (_scanIndex >= _sortedPrefixLength && _scanIndex < _elements.Length && _scanIndex != _minimumIndex)
        {
            _elements[_scanIndex].VisualState = SelectionSortElementVisualState.Comparing;
        }
    }

    private void MarkClassicMovePair()
    {
        RefreshVisualStates();
        _elements[_targetIndex].VisualState = SelectionSortElementVisualState.SwapRequired;
        _elements[_minimumIndex].VisualState = SelectionSortElementVisualState.SwapRequired;
    }

    private void MarkClassicMovedPair()
    {
        ClearToBaseStates();
        _elements[_targetIndex].VisualState = SelectionSortElementVisualState.Swapped;
        _elements[_minimumIndex].VisualState = SelectionSortElementVisualState.Swapped;
    }

    private void MarkStableShiftRange(int start, int end, SelectionSortElementVisualState state)
    {
        ClearToBaseStates();
        for (var index = start; index <= end && index < _elements.Length; index++)
        {
            _elements[index].VisualState = state;
        }
    }

    private void ClearToBaseStates()
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = index < _sortedPrefixLength
                ? SelectionSortElementVisualState.Sorted
                : SelectionSortElementVisualState.Normal;
        }
    }

    private SelectionSortResult BuildResult()
    {
        var initial = new int[_initialValues.Length];
        var sorted = new int[_elements.Length];
        for (var index = 0; index < _initialValues.Length; index++) initial[index] = _initialValues[index];
        for (var index = 0; index < _elements.Length; index++) sorted[index] = _elements[index].Value;

        return new SelectionSortResult(
            initial,
            sorted,
            _comparisons,
            _swaps,
            _moves,
            _currentPass,
            PreservedEqualValueOrder(),
            _variant);
    }

    private bool PreservedEqualValueOrder()
    {
        for (var left = 0; left < _elements.Length; left++)
        {
            for (var right = left + 1; right < _elements.Length; right++)
            {
                if (_elements[left].Value == _elements[right].Value &&
                    _elements[left].OriginalIndex > _elements[right].OriginalIndex)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;

/// <summary>
/// Stable ascending Bubble Sort implemented directly over a primitive raw array of
/// teaching elements. Basic mode runs every canonical shrinking pass; Optimized mode
/// adds the no-swap early exit. Adjacent comparisons, swaps, and pass boundaries are
/// exposed as semantic simulation steps.
/// </summary>
public sealed class BubbleSortSimulation : SimulationAlgorithmBase
{
    private BubbleSortElement[] _elements = new BubbleSortElement[0];
    private int[] _initialValues = new int[0];
    private int _currentPass;
    private int _comparisons;
    private int _swaps;
    private int _passSwaps;
    private int _activeLeftIndex = -1;
    private int _activeRightIndex = -1;
    private int _sortedSuffixStart;
    private bool _earlyExit;
    private BubbleSortPhase _phase = BubbleSortPhase.Ready;
    private BubbleSortVariant _variant = BubbleSortVariant.Optimized;

    public BubbleSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _elements = new BubbleSortElement[values.Length];
        _initialValues = new int[values.Length];

        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new BubbleSortElement(values[index], index);
            _initialValues[index] = values[index];
        }

        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public BubbleSortSnapshot CreateSnapshot()
    {
        var snapshot = new BubbleSortElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            snapshot[index] = new BubbleSortElementSnapshot(
                element.Value,
                element.OriginalIndex,
                element.VisualState);
        }

        return new BubbleSortSnapshot(
            snapshot,
            _currentPass,
            _comparisons,
            _swaps,
            _passSwaps,
            _activeLeftIndex,
            _activeRightIndex,
            _sortedSuffixStart,
            _earlyExit,
            _phase,
            _variant);
    }

    public Task<BubbleSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(BubbleSortVariant.Optimized, cancellationToken);

    public async Task<BubbleSortResult> SortAsync(BubbleSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = BubbleSortPhase.Complete;
            await NextStepAsync("The array is empty, so there are no adjacent values to compare and it is already sorted.", cancellationToken);
            return BuildResult();
        }

        if (_elements.Length == 1)
        {
            _elements[0].VisualState = BubbleSortElementVisualState.Sorted;
            _sortedSuffixStart = 0;
            _phase = BubbleSortPhase.Complete;
            await NextStepAsync($"The array contains only {_elements[0].Value}. One value is already sorted, so Bubble Sort performs 0 comparisons and 0 swaps.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            _variant == BubbleSortVariant.Optimized
                ? $"Start with {_elements.Length} values in Optimized mode. Bubble Sort still scans adjacent pairs, but it also records whether a whole pass made zero swaps so it can stop early."
                : $"Start with {_elements.Length} values in Basic mode. Bubble Sort performs the canonical shrinking passes even when the input is already sorted; there is no early-exit shortcut.",
            cancellationToken);

        for (var pass = 0; pass < _elements.Length - 1; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentPass = pass + 1;
            _passSwaps = 0;
            var lastUnsortedIndex = _elements.Length - 1 - pass;

            for (var left = 0; left < lastUnsortedIndex; left++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var right = left + 1;
                SetActivePair(left, right, BubbleSortElementVisualState.Comparing);
                _phase = BubbleSortPhase.Comparing;
                _comparisons++;

                await NextStepAsync(
                    $"Pass {_currentPass}: compare neighbors {_elements[left].Value} and {_elements[right].Value} at indexes {left} and {right}.",
                    cancellationToken);

                if (_elements[left].Value > _elements[right].Value)
                {
                    _phase = BubbleSortPhase.Deciding;
                    _elements[left].VisualState = BubbleSortElementVisualState.SwapRequired;
                    _elements[right].VisualState = BubbleSortElementVisualState.SwapRequired;
                    await NextStepAsync(
                        $"{_elements[left].Value} is greater than {_elements[right].Value}, so this adjacent pair is out of ascending order. Swap the two items.",
                        cancellationToken);

                    _phase = BubbleSortPhase.Swapping;
                    var temporary = _elements[left];
                    _elements[left] = _elements[right];
                    _elements[right] = temporary;
                    _swaps++;
                    _passSwaps++;
                    _elements[left].VisualState = BubbleSortElementVisualState.Swapped;
                    _elements[right].VisualState = BubbleSortElementVisualState.Swapped;

                    await NextStepAsync(
                        $"Swap complete: {_elements[left].Value} moves to index {left} and {_elements[right].Value} moves to index {right}. The larger value has moved one step toward the end.",
                        cancellationToken);
                }
                else
                {
                    _phase = BubbleSortPhase.Deciding;
                    _elements[left].VisualState = BubbleSortElementVisualState.Kept;
                    _elements[right].VisualState = BubbleSortElementVisualState.Kept;
                    await NextStepAsync(
                        $"Keep the pair: {_elements[left].Value} ≤ {_elements[right].Value}, so these two neighbors are already in ascending order.",
                        cancellationToken);
                }

                ClearTransientStates();
            }

            _sortedSuffixStart = lastUnsortedIndex;
            MarkSortedSuffix();
            _phase = BubbleSortPhase.PassComplete;
            await NextStepAsync(
                $"Pass {_currentPass} complete with {_passSwaps} swap{(_passSwaps == 1 ? string.Empty : "s")}. Index {lastUnsortedIndex} is now fixed: no later pass needs to compare it again.",
                cancellationToken);

            if (_variant == BubbleSortVariant.Optimized && _passSwaps == 0)
            {
                _earlyExit = true;
                _sortedSuffixStart = 0;
                MarkAllSorted();
                _phase = BubbleSortPhase.Complete;
                await NextStepAsync(
                    "This entire pass needed no swaps. That proves every adjacent pair in the remaining region is already ordered, so the optimized Bubble Sort stops early.",
                    cancellationToken);
                return BuildResult();
            }
        }

        _sortedSuffixStart = 0;
        MarkAllSorted();
        _phase = BubbleSortPhase.Complete;
        await NextStepAsync(
            $"Bubble Sort is complete after {_currentPass} passes, {_comparisons} comparisons, and {_swaps} swaps. Every array position is now in ascending order.",
            cancellationToken);

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

    private void ResetRunState()
    {
        _currentPass = 0;
        _comparisons = 0;
        _swaps = 0;
        _passSwaps = 0;
        _activeLeftIndex = -1;
        _activeRightIndex = -1;
        _sortedSuffixStart = _elements.Length;
        _earlyExit = false;
        _phase = BubbleSortPhase.Ready;

        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = BubbleSortElementVisualState.Normal;
        }
    }

    private void SetActivePair(int left, int right, BubbleSortElementVisualState state)
    {
        ClearTransientStates();
        _activeLeftIndex = left;
        _activeRightIndex = right;
        _elements[left].VisualState = state;
        _elements[right].VisualState = state;
        MarkSortedSuffix();
    }

    private void ClearTransientStates()
    {
        _activeLeftIndex = -1;
        _activeRightIndex = -1;

        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = index >= _sortedSuffixStart
                ? BubbleSortElementVisualState.Sorted
                : BubbleSortElementVisualState.Normal;
        }
    }

    private void MarkSortedSuffix()
    {
        for (var index = _sortedSuffixStart; index < _elements.Length; index++)
        {
            _elements[index].VisualState = BubbleSortElementVisualState.Sorted;
        }
    }

    private void MarkAllSorted()
    {
        _activeLeftIndex = -1;
        _activeRightIndex = -1;
        for (var index = 0; index < _elements.Length; index++)
        {
            _elements[index].VisualState = BubbleSortElementVisualState.Sorted;
        }
    }

    private BubbleSortResult BuildResult()
    {
        var initial = new int[_initialValues.Length];
        for (var index = 0; index < _initialValues.Length; index++)
        {
            initial[index] = _initialValues[index];
        }

        var sorted = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            sorted[index] = _elements[index].Value;
        }

        return new BubbleSortResult(
            initial,
            sorted,
            _comparisons,
            _swaps,
            _currentPass,
            _earlyExit,
            IsStableForEqualValues(),
            _variant);
    }

    private bool IsStableForEqualValues()
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

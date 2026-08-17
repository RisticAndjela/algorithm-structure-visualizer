using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;

/// <summary>
/// Stable ascending Insertion Sort implemented manually over a fixed teaching array.
/// Linear mode scans the sorted prefix from right to left. BinarySearch mode uses a stable
/// upper-bound binary search to reduce key comparisons before performing the same shifts.
/// </summary>
public sealed class InsertionSortSimulation : SimulationAlgorithmBase
{
    private InsertionSortElement?[] _elements = Array.Empty<InsertionSortElement?>();
    private int[] _initialValues = Array.Empty<int>();
    private int _currentPass;
    private int _comparisons;
    private int _shifts;
    private int _writes;
    private int _sortedPrefixLength;
    private int _keySourceIndex = -1;
    private InsertionSortElement? _heldKey;
    private int _compareIndex = -1;
    private int _insertionIndex = -1;
    private int _gapIndex = -1;
    private int _searchLow = -1;
    private int _searchHigh = -1;
    private int _searchMid = -1;
    private InsertionSortPhase _phase = InsertionSortPhase.Ready;
    private InsertionSortVariant _variant = InsertionSortVariant.Linear;

    public InsertionSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _elements = new InsertionSortElement?[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new InsertionSortElement(values[index], index);
            _initialValues[index] = values[index];
        }

        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public InsertionSortSnapshot CreateSnapshot()
    {
        var snapshot = new InsertionSortElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            snapshot[index] = element is null
                ? new InsertionSortElementSnapshot(false, 0, -1, InsertionSortElementVisualState.Normal)
                : new InsertionSortElementSnapshot(true, element.Value, element.OriginalIndex, element.VisualState);
        }

        return new InsertionSortSnapshot(
            snapshot,
            _currentPass,
            _comparisons,
            _shifts,
            _writes,
            _sortedPrefixLength,
            _keySourceIndex,
            _heldKey?.Value,
            _heldKey?.OriginalIndex,
            _compareIndex,
            _insertionIndex,
            _gapIndex,
            _searchLow,
            _searchHigh,
            _searchMid,
            _phase,
            _variant);
    }

    public Task<InsertionSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(InsertionSortVariant.Linear, cancellationToken);

    public async Task<InsertionSortResult> SortAsync(InsertionSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = InsertionSortPhase.Complete;
            await NextStepAsync("The array is empty. There is no key to insert, so it is already sorted.", cancellationToken);
            return BuildResult();
        }

        _sortedPrefixLength = 1;
        RefreshVisualStates();

        if (_elements.Length == 1)
        {
            _phase = InsertionSortPhase.Complete;
            RefreshVisualStates();
            await NextStepAsync($"The array contains only {_elements[0]!.Value}. The first item is already a sorted prefix of length 1, so Insertion Sort performs no comparisons or shifts.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            _variant == InsertionSortVariant.Linear
                ? $"Start Linear Insertion Sort with {_elements.Length} values. Treat index 0 as sorted, then take each next key and scan left through the sorted prefix until its insertion point is found."
                : $"Start Binary Insertion Sort with {_elements.Length} values. Treat index 0 as sorted, use binary search to find where each next key belongs, then shift the block right and insert it there.",
            cancellationToken);

        for (var keyIndex = 1; keyIndex < _elements.Length; keyIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _currentPass = keyIndex;
            _keySourceIndex = keyIndex;
            _heldKey = _elements[keyIndex] ?? throw new InvalidOperationException("The teaching array cannot contain an unexpected gap before selecting the next key.");
            _elements[keyIndex] = null;
            _gapIndex = keyIndex;
            _compareIndex = -1;
            _insertionIndex = keyIndex;
            _searchLow = -1;
            _searchHigh = -1;
            _searchMid = -1;
            _phase = InsertionSortPhase.SelectKey;
            RefreshVisualStates();

            await NextStepAsync(
                $"Pass {_currentPass}: hold key {_heldKey.Value} from index {keyIndex} outside the array. The prefix at indexes 0…{keyIndex - 1} is already sorted; the empty slot marks the gap that may move left.",
                cancellationToken);

            if (_variant == InsertionSortVariant.Linear)
            {
                await FindInsertionPointLinearlyAsync(keyIndex, cancellationToken);
            }
            else
            {
                await FindInsertionPointWithBinarySearchAsync(keyIndex, cancellationToken);
            }

            await ShiftAndInsertHeldKeyAsync(keyIndex, cancellationToken);

            _sortedPrefixLength = keyIndex + 1;
            _keySourceIndex = -1;
            _compareIndex = -1;
            _insertionIndex = -1;
            _gapIndex = -1;
            _searchLow = -1;
            _searchHigh = -1;
            _searchMid = -1;
            _phase = InsertionSortPhase.PassComplete;
            RefreshVisualStates();

            await NextStepAsync(
                $"Pass {_currentPass} complete. Indexes 0…{_sortedPrefixLength - 1} are sorted again, so the sorted prefix has grown to {_sortedPrefixLength} items.",
                cancellationToken);
        }

        _phase = InsertionSortPhase.Complete;
        _sortedPrefixLength = _elements.Length;
        RefreshVisualStates();
        await NextStepAsync(
            _variant == InsertionSortVariant.Linear
                ? $"Linear Insertion Sort is complete after {_currentPass} passes, {_comparisons} key comparisons, and {_shifts} right shifts."
                : $"Binary Insertion Sort is complete after {_currentPass} passes, {_comparisons} binary-search comparisons, and {_shifts} right shifts. Binary search reduces comparisons, but it cannot remove the cost of moving array elements.",
            cancellationToken);

        return BuildResult();
    }

    private async Task FindInsertionPointLinearlyAsync(int keyIndex, CancellationToken cancellationToken)
    {
        var scan = keyIndex - 1;
        _insertionIndex = keyIndex;

        while (scan >= 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var candidate = _elements[scan] ?? throw new InvalidOperationException("Linear search encountered an unexpected gap.");
            _compareIndex = scan;
            _phase = InsertionSortPhase.Searching;
            _comparisons++;
            RefreshVisualStates();

            await NextStepAsync(
                $"Compare held key {_heldKey!.Value} with {candidate.Value} at index {scan}. If {candidate.Value} is larger, it must shift right to make room for the key.",
                cancellationToken);

            if (candidate.Value <= _heldKey!.Value)
            {
                _insertionIndex = scan + 1;
                await NextStepAsync(
                    $"Stop scanning: {candidate.Value} ≤ {_heldKey.Value}. Because the prefix is sorted, the stable insertion point is index {_insertionIndex}, immediately after this value.",
                    cancellationToken);
                return;
            }

            _insertionIndex = scan;
            scan--;
        }

        _compareIndex = -1;
        _insertionIndex = 0;
        await NextStepAsync($"The key {_heldKey!.Value} is smaller than every value in the sorted prefix, so it belongs at index 0.", cancellationToken);
    }

    private async Task FindInsertionPointWithBinarySearchAsync(int keyIndex, CancellationToken cancellationToken)
    {
        var low = 0;
        var high = keyIndex;

        while (low < high)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mid = low + ((high - low) / 2);
            var candidate = _elements[mid] ?? throw new InvalidOperationException("Binary search encountered an unexpected gap inside the sorted prefix.");
            _searchLow = low;
            _searchHigh = high;
            _searchMid = mid;
            _compareIndex = mid;
            _phase = InsertionSortPhase.Searching;
            _comparisons++;
            RefreshVisualStates();

            await NextStepAsync(
                $"Binary search compares held key {_heldKey!.Value} with {candidate.Value} at mid index {mid}. Current insertion range is [{low}, {high}).",
                cancellationToken);

            if (_heldKey!.Value < candidate.Value)
            {
                high = mid;
                _searchLow = low;
                _searchHigh = high;
                _searchMid = -1;
                _compareIndex = -1;
                RefreshVisualStates();
                await NextStepAsync($"{_heldKey.Value} < {candidate.Value}, so the insertion point must be in the left half. New range: [{low}, {high}).", cancellationToken);
            }
            else
            {
                low = mid + 1;
                _searchLow = low;
                _searchHigh = high;
                _searchMid = -1;
                _compareIndex = -1;
                RefreshVisualStates();
                await NextStepAsync($"{_heldKey.Value} ≥ {candidate.Value}, so insert after this equal-or-smaller value. New range: [{low}, {high}). This upper-bound rule preserves duplicate order.", cancellationToken);
            }
        }

        _searchLow = low;
        _searchHigh = high;
        _searchMid = -1;
        _compareIndex = -1;
        _insertionIndex = low;
        await NextStepAsync($"Binary search finished. The stable insertion point for {_heldKey!.Value} is index {_insertionIndex}.", cancellationToken);
    }

    private async Task ShiftAndInsertHeldKeyAsync(int keyIndex, CancellationToken cancellationToken)
    {
        if (_insertionIndex < keyIndex)
        {
            _phase = InsertionSortPhase.Shifting;
            MarkShiftRange(_insertionIndex, keyIndex - 1, InsertionSortElementVisualState.ShiftRequired);
            await NextStepAsync(
                $"Create room at index {_insertionIndex}: shift indexes {_insertionIndex}…{keyIndex - 1} one slot right. The gap moves left while the held key stays outside the array.",
                cancellationToken);

            for (var index = keyIndex; index > _insertionIndex; index--)
            {
                _elements[index] = _elements[index - 1];
                _elements[index - 1] = null;
                _gapIndex = index - 1;
                _shifts++;
                _writes++;
                MarkShiftRange(index, index, InsertionSortElementVisualState.Shifted);
                await NextStepAsync(
                    $"Shift {_elements[index]!.Value} from index {index - 1} to index {index}. The gap is now at index {_gapIndex}.",
                    cancellationToken);
            }
        }
        else
        {
            await NextStepAsync($"No shifts are needed. The held key {_heldKey!.Value} already belongs at index {keyIndex}.", cancellationToken);
        }

        _phase = InsertionSortPhase.Inserting;
        _elements[_insertionIndex] = _heldKey;
        _writes++;
        _gapIndex = -1;
        _heldKey!.VisualState = InsertionSortElementVisualState.Inserted;
        RefreshVisualStates(preserveInserted: true);

        await NextStepAsync(
            $"Insert held key {_heldKey.Value} into index {_insertionIndex}. The prefix is contiguous and sorted again.",
            cancellationToken);

        _heldKey = null;
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            _initialValues[index] = _elements[index]?.Value ?? 0;
        }
    }

    private void ResetRunState()
    {
        _currentPass = 0;
        _comparisons = 0;
        _shifts = 0;
        _writes = 0;
        _sortedPrefixLength = 0;
        _keySourceIndex = -1;
        _heldKey = null;
        _compareIndex = -1;
        _insertionIndex = -1;
        _gapIndex = -1;
        _searchLow = -1;
        _searchHigh = -1;
        _searchMid = -1;
        _phase = InsertionSortPhase.Ready;
        RefreshVisualStates();
    }

    private void RefreshVisualStates(bool preserveInserted = false)
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            if (element is null) continue;
            if (preserveInserted && element.VisualState == InsertionSortElementVisualState.Inserted) continue;
            element.VisualState = index < _sortedPrefixLength
                ? InsertionSortElementVisualState.Sorted
                : InsertionSortElementVisualState.Normal;
        }

        if (_compareIndex >= 0 && _compareIndex < _elements.Length && _elements[_compareIndex] is not null)
        {
            _elements[_compareIndex]!.VisualState = InsertionSortElementVisualState.Comparing;
        }
    }

    private void MarkShiftRange(int start, int end, InsertionSortElementVisualState state)
    {
        RefreshVisualStates();
        for (var index = Math.Max(0, start); index <= end && index < _elements.Length; index++)
        {
            if (_elements[index] is not null) _elements[index]!.VisualState = state;
        }
    }

    private InsertionSortResult BuildResult()
    {
        var initial = new int[_initialValues.Length];
        var sorted = new int[_elements.Length];
        for (var index = 0; index < _initialValues.Length; index++) initial[index] = _initialValues[index];
        for (var index = 0; index < _elements.Length; index++) sorted[index] = _elements[index]?.Value ?? throw new InvalidOperationException("Sorting completed with an unexpected gap.");

        return new InsertionSortResult(
            initial,
            sorted,
            _comparisons,
            _shifts,
            _writes,
            _currentPass,
            PreservedEqualValueOrder(),
            _variant);
    }

    private bool PreservedEqualValueOrder()
    {
        for (var left = 0; left < _elements.Length; left++)
        {
            var leftElement = _elements[left];
            if (leftElement is null) continue;
            for (var right = left + 1; right < _elements.Length; right++)
            {
                var rightElement = _elements[right];
                if (rightElement is null) continue;
                if (leftElement.Value == rightElement.Value && leftElement.OriginalIndex > rightElement.OriginalIndex) return false;
            }
        }
        return true;
    }
}

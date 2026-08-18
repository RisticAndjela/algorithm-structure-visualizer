using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;

/// <summary>
/// Manual in-place Quick Sort teaching implementation.
/// Basic mode uses Lomuto partitioning with the last item as pivot.
/// Advanced mode chooses a median-of-three pivot value and performs a Dutch-national-flag
/// three-way partition so duplicate-heavy inputs do not recurse through an equal-value band.
/// </summary>
public sealed class QuickSortSimulation : SimulationAlgorithmBase
{
    private QuickSortElement[] _elements = Array.Empty<QuickSortElement>();
    private int[] _initialValues = Array.Empty<int>();
    private bool[] _finalized = Array.Empty<bool>();
    private int _comparisons;
    private int _swaps;
    private int _partitions;
    private int _currentDepth;
    private int _maxDepth;
    private int _activeStart = -1;
    private int _activeEnd = -1;
    private int _pivotIndex = -1;
    private int? _pivotValue;
    private int _scanIndex = -1;
    private int _boundaryIndex = -1;
    private int _lessEnd = -1;
    private int _equalStart = -1;
    private int _equalEnd = -1;
    private int _greaterStart = -1;
    private int _swapLeft = -1;
    private int _swapRight = -1;
    private QuickSortPhase _phase = QuickSortPhase.Ready;
    private QuickSortVariant _variant = QuickSortVariant.LomutoLastPivot;

    public QuickSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _elements = new QuickSortElement[values.Length];
        _initialValues = new int[values.Length];
        _finalized = new bool[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new QuickSortElement(values[index], index);
            _initialValues[index] = values[index];
        }
        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public QuickSortSnapshot CreateSnapshot()
    {
        var snapshots = new QuickSortElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            snapshots[index] = new QuickSortElementSnapshot(
                _elements[index].Value,
                _elements[index].OriginalIndex,
                GetVisualState(index));
        }

        var finalized = new bool[_finalized.Length];
        Array.Copy(_finalized, finalized, _finalized.Length);
        var initial = new int[_initialValues.Length];
        Array.Copy(_initialValues, initial, _initialValues.Length);

        return new QuickSortSnapshot(
            initial,
            snapshots,
            finalized,
            _comparisons,
            _swaps,
            _partitions,
            _currentDepth,
            _maxDepth,
            _activeStart,
            _activeEnd,
            _pivotIndex,
            _pivotValue,
            _scanIndex,
            _boundaryIndex,
            _lessEnd,
            _equalStart,
            _equalEnd,
            _greaterStart,
            _phase,
            _variant);
    }

    public Task<QuickSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(QuickSortVariant.LomutoLastPivot, cancellationToken);

    public async Task<QuickSortResult> SortAsync(QuickSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = QuickSortPhase.Complete;
            await NextStepAsync("The array is empty, so Quick Sort has no partition to create.", cancellationToken);
            return BuildResult();
        }

        if (_elements.Length == 1)
        {
            _activeStart = 0;
            _activeEnd = 0;
            _finalized[0] = true;
            _phase = QuickSortPhase.BaseCase;
            await NextStepAsync($"One value ({_elements[0].Value}) is already sorted. No pivot or partition is needed.", cancellationToken);
            _phase = QuickSortPhase.Complete;
            await NextStepAsync("Quick Sort is complete.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            variant == QuickSortVariant.LomutoLastPivot
                ? $"Start Basic Quick Sort with {_elements.Length} values. For every active range, use its last element as the pivot and create a Lomuto partition."
                : $"Start Advanced Quick Sort with {_elements.Length} values. Choose a median-of-three pivot value, then build < pivot, = pivot, and > pivot regions in one partition pass.",
            cancellationToken);

        if (variant == QuickSortVariant.LomutoLastPivot)
        {
            await QuickSortLomutoAsync(0, _elements.Length - 1, 1, cancellationToken);
        }
        else
        {
            await QuickSortThreeWayAsync(0, _elements.Length - 1, 1, cancellationToken);
        }

        for (var index = 0; index < _finalized.Length; index++) _finalized[index] = true;
        ClearPointers();
        _phase = QuickSortPhase.Complete;
        await NextStepAsync(
            $"Quick Sort is complete after {_partitions} partition(s), {_comparisons} value comparison(s), and {_swaps} swap(s).",
            cancellationToken);
        return BuildResult();
    }

    private async Task QuickSortLomutoAsync(int low, int high, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (low > high) return;

        _currentDepth = depth;
        _maxDepth = Math.Max(_maxDepth, depth);
        _activeStart = low;
        _activeEnd = high;
        ClearPartitionPointers();

        if (low == high)
        {
            _finalized[low] = true;
            _phase = QuickSortPhase.BaseCase;
            await NextStepAsync($"Range [{low}..{high}] contains one value ({_elements[low].Value}), so it is already in its final sorted position.", cancellationToken);
            return;
        }

        _pivotIndex = high;
        var pivot = _elements[high].Value;
        _pivotValue = pivot;
        _boundaryIndex = low - 1;
        _phase = QuickSortPhase.ChoosePivot;
        await NextStepAsync($"Partition [{low}..{high}]. Basic Quick Sort chooses the last value {_pivotValue} at index {high} as the pivot.", cancellationToken);

        var boundary = low - 1;
        for (var scan = low; scan < high; scan++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _scanIndex = scan;
            _boundaryIndex = boundary;
            _phase = QuickSortPhase.Scanning;
            _comparisons++;
            await NextStepAsync(
                $"Compare {_elements[scan].Value} at index {scan} with pivot {_pivotValue}. Values ≤ the pivot belong in the left partition; larger values stay on the right for now.",
                cancellationToken);

            if (_elements[scan].Value <= pivot)
            {
                boundary++;
                _boundaryIndex = boundary;
                if (boundary != scan)
                {
                    await SwapWithStepAsync(boundary, scan,
                        $"{_elements[scan].Value} belongs on the ≤ pivot side. Swap indexes {boundary} and {scan} so the left partition grows.",
                        cancellationToken);
                }
                else
                {
                    await NextStepAsync($"{_elements[scan].Value} is already at the next left-partition slot, so no physical swap is needed.", cancellationToken);
                }
            }
        }

        var pivotDestination = boundary + 1;
        _phase = QuickSortPhase.PlacePivot;
        if (pivotDestination != high)
        {
            await SwapWithStepAsync(pivotDestination, high,
                $"Scanning is finished. Move pivot {_pivotValue} from index {high} to index {pivotDestination}. This is the pivot's final sorted position.",
                cancellationToken);
        }
        else
        {
            await NextStepAsync($"Scanning is finished and pivot {_pivotValue} is already at index {high}, its final sorted position.", cancellationToken);
        }

        _pivotIndex = pivotDestination;
        _finalized[pivotDestination] = true;
        _partitions++;
        _phase = QuickSortPhase.PartitionComplete;
        await NextStepAsync(
            $"Partition complete: pivot {_elements[pivotDestination].Value} is fixed at index {pivotDestination}. Quick Sort now recurses only on [{low}..{pivotDestination - 1}] and [{pivotDestination + 1}..{high}].",
            cancellationToken);

        await QuickSortLomutoAsync(low, pivotDestination - 1, depth + 1, cancellationToken);
        await QuickSortLomutoAsync(pivotDestination + 1, high, depth + 1, cancellationToken);
    }

    private async Task QuickSortThreeWayAsync(int low, int high, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (low > high) return;

        _currentDepth = depth;
        _maxDepth = Math.Max(_maxDepth, depth);
        _activeStart = low;
        _activeEnd = high;
        ClearPartitionPointers();

        if (low == high)
        {
            _finalized[low] = true;
            _phase = QuickSortPhase.BaseCase;
            await NextStepAsync($"Range [{low}..{high}] contains one value ({_elements[low].Value}), so it is already sorted.", cancellationToken);
            return;
        }

        var middle = low + ((high - low) / 2);
        var pivotChoice = MedianOfThreeIndex(low, middle, high);
        _pivotIndex = pivotChoice;
        var pivot = _elements[pivotChoice].Value;
        _pivotValue = pivot;
        _phase = QuickSortPhase.ChoosePivot;
        await NextStepAsync(
            $"Partition [{low}..{high}]. Compare first {_elements[low].Value}, middle {_elements[middle].Value}, and last {_elements[high].Value}; choose median value {_pivotValue} as the pivot. This avoids blindly trusting an endpoint.",
            cancellationToken);

        var lt = low;
        var scan = low;
        var gt = high;
        _equalStart = lt;
        _equalEnd = gt;
        _greaterStart = gt + 1;

        while (scan <= gt)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _scanIndex = scan;
            _lessEnd = lt - 1;
            _equalStart = lt;
            _equalEnd = scan - 1;
            _greaterStart = gt + 1;
            _phase = QuickSortPhase.Scanning;
            _comparisons++;
            var value = _elements[scan].Value;
            await NextStepAsync(
                $"Compare {value} at index {scan} with pivot value {_pivotValue}. Put it into the <, =, or > pivot region.",
                cancellationToken);

            if (value < pivot)
            {
                if (lt != scan)
                {
                    await SwapWithStepAsync(lt, scan,
                        $"{value} is smaller than pivot {_pivotValue}. Swap it to index {lt}, extending the < pivot region.",
                        cancellationToken);
                }
                lt++;
                scan++;
            }
            else if (value > pivot)
            {
                if (scan != gt)
                {
                    await SwapWithStepAsync(scan, gt,
                        $"{value} is larger than pivot {_pivotValue}. Swap it toward the right region at index {gt}. Re-check the value moved into index {scan}.",
                        cancellationToken);
                }
                gt--;
            }
            else
            {
                scan++;
            }
        }

        _lessEnd = lt - 1;
        _equalStart = lt;
        _equalEnd = gt;
        _greaterStart = gt + 1;
        for (var index = lt; index <= gt; index++) _finalized[index] = true;
        _partitions++;
        _phase = QuickSortPhase.PartitionComplete;
        await NextStepAsync(
            $"Three-way partition complete: [{low}..{lt - 1}] < {_pivotValue}, [{lt}..{gt}] = {_pivotValue}, [{gt + 1}..{high}] > {_pivotValue}. The equal band is finished and never recursed into again.",
            cancellationToken);

        await QuickSortThreeWayAsync(low, lt - 1, depth + 1, cancellationToken);
        await QuickSortThreeWayAsync(gt + 1, high, depth + 1, cancellationToken);
    }

    private int MedianOfThreeIndex(int first, int middle, int last)
    {
        var a = _elements[first].Value;
        var b = _elements[middle].Value;
        var c = _elements[last].Value;
        _comparisons += 3;

        if ((a <= b && b <= c) || (c <= b && b <= a)) return middle;
        if ((b <= a && a <= c) || (c <= a && a <= b)) return first;
        return last;
    }

    private async Task SwapWithStepAsync(int left, int right, string description, CancellationToken cancellationToken)
    {
        _swapLeft = left;
        _swapRight = right;
        _phase = QuickSortPhase.Swapping;
        var temporary = _elements[left];
        _elements[left] = _elements[right];
        _elements[right] = temporary;
        _swaps++;
        await NextStepAsync(description, cancellationToken);
        _swapLeft = -1;
        _swapRight = -1;
    }

    private QuickSortElementVisualState GetVisualState(int index)
    {
        if (_finalized.Length > index && _finalized[index]) return QuickSortElementVisualState.Sorted;
        if (index == _swapLeft || index == _swapRight) return QuickSortElementVisualState.Swapping;
        if (index == _scanIndex) return QuickSortElementVisualState.Scan;
        if (_variant == QuickSortVariant.LomutoLastPivot)
        {
            if (index == _pivotIndex && _pivotIndex >= 0) return QuickSortElementVisualState.Pivot;
            if (index == _boundaryIndex && _boundaryIndex >= _activeStart) return QuickSortElementVisualState.Boundary;
            if (_boundaryIndex >= _activeStart && index >= _activeStart && index <= _boundaryIndex) return QuickSortElementVisualState.LessRegion;
        }
        else if (_activeStart >= 0 && index >= _activeStart && index <= _activeEnd)
        {
            if (_equalStart >= 0 && _equalEnd >= _equalStart && index >= _equalStart && index <= _equalEnd) return QuickSortElementVisualState.EqualRegion;
            if (_lessEnd >= _activeStart && index <= _lessEnd) return QuickSortElementVisualState.LessRegion;
            if (_greaterStart >= 0 && index >= _greaterStart && index <= _activeEnd) return QuickSortElementVisualState.GreaterRegion;
            if (_pivotIndex == index && _phase == QuickSortPhase.ChoosePivot) return QuickSortElementVisualState.Pivot;
        }
        if (_activeStart >= 0 && index >= _activeStart && index <= _activeEnd) return QuickSortElementVisualState.ActiveRange;
        return QuickSortElementVisualState.Normal;
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) _initialValues[index] = _elements[index].Value;
    }

    private void ResetRunState()
    {
        _comparisons = 0;
        _swaps = 0;
        _partitions = 0;
        _currentDepth = 0;
        _maxDepth = 0;
        _phase = QuickSortPhase.Ready;
        _activeStart = -1;
        _activeEnd = -1;
        _pivotIndex = -1;
        _pivotValue = null;
        _scanIndex = -1;
        _boundaryIndex = -1;
        _lessEnd = -1;
        _equalStart = -1;
        _equalEnd = -1;
        _greaterStart = -1;
        _swapLeft = -1;
        _swapRight = -1;
        if (_finalized.Length != _elements.Length) _finalized = new bool[_elements.Length];
        else Array.Clear(_finalized, 0, _finalized.Length);
    }

    private void ClearPartitionPointers()
    {
        _pivotIndex = -1;
        _pivotValue = null;
        _scanIndex = -1;
        _boundaryIndex = -1;
        _lessEnd = -1;
        _equalStart = -1;
        _equalEnd = -1;
        _greaterStart = -1;
        _swapLeft = -1;
        _swapRight = -1;
    }

    private void ClearPointers()
    {
        _activeStart = -1;
        _activeEnd = -1;
        ClearPartitionPointers();
        _currentDepth = 0;
    }

    private QuickSortResult BuildResult()
    {
        var sorted = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) sorted[index] = _elements[index].Value;
        var initial = new int[_initialValues.Length];
        Array.Copy(_initialValues, initial, _initialValues.Length);
        return new QuickSortResult(initial, sorted, _comparisons, _swaps, _partitions, _maxDepth, PreservedEqualOrder(), _variant);
    }

    private bool PreservedEqualOrder()
    {
        for (var left = 0; left < _elements.Length; left++)
        {
            for (var right = left + 1; right < _elements.Length; right++)
            {
                if (_elements[left].Value == _elements[right].Value && _elements[left].OriginalIndex > _elements[right].OriginalIndex) return false;
            }
        }
        return true;
    }
}

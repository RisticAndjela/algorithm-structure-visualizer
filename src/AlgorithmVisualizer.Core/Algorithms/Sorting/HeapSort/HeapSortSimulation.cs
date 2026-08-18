using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;

/// <summary>
/// Manual in-place Heap Sort teaching implementation.
/// Basic mode builds a Max Heap incrementally with the same bubble-up idea taught by the Binary Heap lab.
/// Advanced mode uses Floyd's bottom-up build-heap, then both modes repeatedly move the maximum root
/// into the sorted suffix and repair the reduced heap with explicit sift-down.
/// </summary>
public sealed class HeapSortSimulation : SimulationAlgorithmBase
{
    private HeapSortElement[] _elements = Array.Empty<HeapSortElement>();
    private int[] _initialValues = Array.Empty<int>();
    private int _heapSize;
    private int _comparisons;
    private int _swaps;
    private int _buildComparisons;
    private int _buildSwaps;
    private int _extractions;
    private int _siftDownCalls;
    private int _buildIndex = -1;
    private int _parentIndex = -1;
    private int _leftChildIndex = -1;
    private int _rightChildIndex = -1;
    private int _candidateIndex = -1;
    private int _swapLeftIndex = -1;
    private int _swapRightIndex = -1;
    private bool _buildFinished;
    private HeapSortPhase _phase = HeapSortPhase.Ready;
    private HeapSortVariant _variant = HeapSortVariant.IncrementalBuild;

    public HeapSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime)
    {
    }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _elements = new HeapSortElement[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new HeapSortElement(values[index], index);
            _initialValues[index] = values[index];
        }
        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public HeapSortSnapshot CreateSnapshot()
    {
        var elementSnapshots = new HeapSortElementSnapshot[_elements.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            elementSnapshots[index] = new HeapSortElementSnapshot(
                _elements[index].Value,
                _elements[index].OriginalIndex,
                GetVisualState(index));
        }

        var initial = new int[_initialValues.Length];
        Array.Copy(_initialValues, initial, _initialValues.Length);

        return new HeapSortSnapshot(
            initial,
            elementSnapshots,
            _heapSize,
            _comparisons,
            _swaps,
            _buildComparisons,
            _buildSwaps,
            _extractions,
            _siftDownCalls,
            _buildIndex,
            _parentIndex,
            _leftChildIndex,
            _rightChildIndex,
            _candidateIndex,
            _swapLeftIndex,
            _swapRightIndex,
            _buildFinished,
            _phase,
            _variant);
    }

    public Task<HeapSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(HeapSortVariant.IncrementalBuild, cancellationToken);

    public async Task<HeapSortResult> SortAsync(HeapSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = HeapSortPhase.Complete;
            await NextStepAsync("The array is empty, so Heap Sort has no heap to build.", cancellationToken);
            return BuildResult();
        }

        if (_elements.Length == 1)
        {
            _heapSize = 1;
            _buildFinished = true;
            _phase = HeapSortPhase.Complete;
            await NextStepAsync($"One value ({_elements[0].Value}) is already sorted. No heap repair or extraction is needed.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            variant == HeapSortVariant.IncrementalBuild
                ? $"Start Basic Heap Sort with {_elements.Length} values. Build a Max Heap one item at a time with bubble-up, then repeatedly extract the root into the sorted suffix."
                : $"Start Advanced Heap Sort with {_elements.Length} values. Build the Max Heap bottom-up with Floyd heapify, then repeatedly extract the root into the sorted suffix.",
            cancellationToken);

        if (variant == HeapSortVariant.IncrementalBuild)
        {
            await BuildIncrementallyAsync(cancellationToken);
        }
        else
        {
            await BuildWithFloydAsync(cancellationToken);
        }

        _buildFinished = true;
        _heapSize = _elements.Length;
        ClearPointers();
        _phase = HeapSortPhase.BuildComplete;
        await NextStepAsync(
            $"Max Heap built. The largest value {_elements[0].Value} is at root index 0. The whole array is still the active heap; the sorted suffix is empty.",
            cancellationToken);

        for (var end = _elements.Length - 1; end > 0; end--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _heapSize = end + 1;
            _swapLeftIndex = 0;
            _swapRightIndex = end;
            _phase = HeapSortPhase.ExtractRoot;
            await NextStepAsync(
                $"The root {_elements[0].Value} is the maximum of the active heap [0..{end}]. Swap it with index {end}; that value will never move again.",
                cancellationToken);

            await SwapAsync(0, end, isBuild: false, HeapSortPhase.ExtractRoot,
                $"Move maximum {_elements[0].Value} to sorted position {end} and bring {_elements[end].Value} to the root slot.",
                cancellationToken);

            _extractions++;
            _heapSize = end;
            _swapLeftIndex = -1;
            _swapRightIndex = -1;
            _phase = HeapSortPhase.ShrinkHeap;
            await NextStepAsync(
                $"Shrink the heap boundary to [0..{end - 1}]. Index {end} is now part of the final sorted suffix and must not participate in heap repair.",
                cancellationToken);

            if (_heapSize > 1)
            {
                await SiftDownAsync(0, _heapSize, isBuild: false, cancellationToken);
            }

            ClearComparisonPointers();
            _phase = HeapSortPhase.RepairComplete;
            await NextStepAsync(
                _heapSize > 0
                    ? $"Heap property restored inside [0..{_heapSize - 1}]. The sorted suffix begins at index {_heapSize}."
                    : "No active heap remains.",
                cancellationToken);
        }

        _heapSize = 0;
        ClearPointers();
        _phase = HeapSortPhase.Complete;
        await NextStepAsync(
            $"Heap Sort is complete after {_extractions} root extraction(s), {_comparisons} comparison(s), and {_swaps} swap(s).",
            cancellationToken);
        return BuildResult();
    }

    private async Task BuildIncrementallyAsync(CancellationToken cancellationToken)
    {
        _heapSize = 1;
        _buildIndex = 0;
        _phase = HeapSortPhase.BuildInsert;
        await NextStepAsync($"Start the Max Heap with index 0 ({_elements[0].Value}). A one-item prefix already satisfies the heap property.", cancellationToken);

        for (var index = 1; index < _elements.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _heapSize = index + 1;
            _buildIndex = index;
            _phase = HeapSortPhase.BuildInsert;
            await NextStepAsync(
                $"Add index {index} ({_elements[index].Value}) to the active heap prefix [0..{index}]. Bubble it upward while it is greater than its parent.",
                cancellationToken);

            var child = index;
            while (child > 0)
            {
                var parent = (child - 1) / 2;
                _parentIndex = parent;
                _candidateIndex = child;
                _leftChildIndex = -1;
                _rightChildIndex = -1;
                _phase = HeapSortPhase.BuildCompare;
                _comparisons++;
                _buildComparisons++;
                await NextStepAsync(
                    $"Compare child {_elements[child].Value} at index {child} with parent {_elements[parent].Value} at index {parent}.",
                    cancellationToken);

                if (_elements[child].Value <= _elements[parent].Value)
                {
                    await NextStepAsync(
                        $"{_elements[parent].Value} ≥ {_elements[child].Value}, so this insertion already satisfies the Max Heap rule. Stop bubbling.",
                        cancellationToken);
                    break;
                }

                await SwapAsync(parent, child, isBuild: true, HeapSortPhase.BuildSwap,
                    $"Child {_elements[child].Value} is larger than parent {_elements[parent].Value}; swap them and continue from index {parent}.",
                    cancellationToken);
                child = parent;
            }
        }
    }

    private async Task BuildWithFloydAsync(CancellationToken cancellationToken)
    {
        _heapSize = _elements.Length;
        var lastParent = (_elements.Length - 2) / 2;
        _phase = HeapSortPhase.BuildHeapify;
        await NextStepAsync(
            $"Floyd build starts at the last parent, index {lastParent}. Leaf indexes already satisfy the heap property by themselves, so they need no bubble-up work.",
            cancellationToken);

        for (var parent = lastParent; parent >= 0; parent--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _buildIndex = parent;
            _phase = HeapSortPhase.BuildHeapify;
            await NextStepAsync(
                $"Heapify subtree rooted at index {parent} ({_elements[parent].Value}). All child subtrees below it are already heaps.",
                cancellationToken);
            await SiftDownAsync(parent, _heapSize, isBuild: true, cancellationToken);
        }
    }

    private async Task SiftDownAsync(int start, int heapSize, bool isBuild, CancellationToken cancellationToken)
    {
        _siftDownCalls++;
        var parent = start;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var left = (2 * parent) + 1;
            if (left >= heapSize) return;

            var right = left + 1;
            var largerChild = left;
            _parentIndex = parent;
            _leftChildIndex = left;
            _rightChildIndex = right < heapSize ? right : -1;
            _candidateIndex = left;
            _phase = isBuild ? HeapSortPhase.BuildCompare : HeapSortPhase.RepairCompare;

            if (right < heapSize)
            {
                _comparisons++;
                if (isBuild) _buildComparisons++;
                await NextStepAsync(
                    $"Compare children {_elements[left].Value} (index {left}) and {_elements[right].Value} (index {right}) to choose the larger child for Max Heap repair.",
                    cancellationToken);
                if (_elements[right].Value > _elements[left].Value) largerChild = right;
            }

            _candidateIndex = largerChild;
            _comparisons++;
            if (isBuild) _buildComparisons++;
            await NextStepAsync(
                $"Compare parent {_elements[parent].Value} at index {parent} with larger child {_elements[largerChild].Value} at index {largerChild}.",
                cancellationToken);

            if (_elements[parent].Value >= _elements[largerChild].Value)
            {
                await NextStepAsync(
                    $"{_elements[parent].Value} ≥ {_elements[largerChild].Value}. The Max Heap property is satisfied on this path, so sift-down stops.",
                    cancellationToken);
                return;
            }

            await SwapAsync(parent, largerChild, isBuild, isBuild ? HeapSortPhase.BuildSwap : HeapSortPhase.RepairSwap,
                $"Swap {_elements[parent].Value} with larger child {_elements[largerChild].Value}. Continue repairing from index {largerChild}.",
                cancellationToken);
            parent = largerChild;
        }
    }

    private async Task SwapAsync(
        int left,
        int right,
        bool isBuild,
        HeapSortPhase phase,
        string description,
        CancellationToken cancellationToken)
    {
        _swapLeftIndex = left;
        _swapRightIndex = right;
        _phase = phase;

        var temporary = _elements[left];
        _elements[left] = _elements[right];
        _elements[right] = temporary;
        _swaps++;
        if (isBuild) _buildSwaps++;

        await NextStepAsync(description, cancellationToken);
        _swapLeftIndex = -1;
        _swapRightIndex = -1;
    }

    private HeapSortElementVisualState GetVisualState(int index)
    {
        if (_buildFinished && index >= _heapSize) return HeapSortElementVisualState.SortedSuffix;
        if (index == _swapLeftIndex || index == _swapRightIndex) return HeapSortElementVisualState.Swapping;
        if (index == _candidateIndex) return HeapSortElementVisualState.ChildCandidate;
        if (index == _parentIndex) return HeapSortElementVisualState.Parent;
        if (index == _buildIndex && !_buildFinished) return HeapSortElementVisualState.BuildItem;
        if (_buildFinished && index == 0 && _heapSize > 0) return HeapSortElementVisualState.Root;
        if (!_buildFinished && _variant == HeapSortVariant.IncrementalBuild && index >= _heapSize) return HeapSortElementVisualState.Unbuilt;
        if (index < _heapSize) return HeapSortElementVisualState.ActiveHeap;
        return HeapSortElementVisualState.Normal;
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) _initialValues[index] = _elements[index].Value;
    }

    private void ResetRunState()
    {
        _heapSize = 0;
        _comparisons = 0;
        _swaps = 0;
        _buildComparisons = 0;
        _buildSwaps = 0;
        _extractions = 0;
        _siftDownCalls = 0;
        _buildIndex = -1;
        _buildFinished = false;
        _phase = HeapSortPhase.Ready;
        ClearPointers();
    }

    private void ClearComparisonPointers()
    {
        _parentIndex = -1;
        _leftChildIndex = -1;
        _rightChildIndex = -1;
        _candidateIndex = -1;
        _swapLeftIndex = -1;
        _swapRightIndex = -1;
    }

    private void ClearPointers()
    {
        _buildIndex = -1;
        ClearComparisonPointers();
    }

    private HeapSortResult BuildResult()
    {
        var sorted = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) sorted[index] = _elements[index].Value;
        var initial = new int[_initialValues.Length];
        Array.Copy(_initialValues, initial, _initialValues.Length);
        return new HeapSortResult(
            initial,
            sorted,
            _comparisons,
            _swaps,
            _buildComparisons,
            _buildSwaps,
            _extractions,
            _siftDownCalls,
            PreservedEqualOrder(),
            _variant);
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

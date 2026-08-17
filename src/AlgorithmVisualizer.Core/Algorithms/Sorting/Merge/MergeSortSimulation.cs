using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;

/// <summary>
/// Stable ascending Merge Sort implemented manually over teaching elements.
/// TopDownRecursive demonstrates canonical divide-and-conquer recursion.
/// NaturalRuns detects existing nondecreasing runs and merges only the runs that are present.
/// Both variants use one O(n) auxiliary buffer and preserve equal-item order by taking from the left run on equality.
/// </summary>
public sealed class MergeSortSimulation : SimulationAlgorithmBase
{
    private MergeSortElement[] _elements = Array.Empty<MergeSortElement>();
    private MergeSortElement?[] _buffer = Array.Empty<MergeSortElement?>();
    private int[] _initialValues = Array.Empty<int>();
    private int _comparisons;
    private int _writes;
    private int _merges;
    private int _splits;
    private int _currentDepth;
    private int _maxDepth;
    private int _activeStart = -1;
    private int _leftEnd = -1;
    private int _rightStart = -1;
    private int _activeEnd = -1;
    private int _leftReadIndex = -1;
    private int _rightReadIndex = -1;
    private int _writeIndex = -1;
    private int _naturalRunCount;
    private int _initialNaturalRunCount;
    private int _naturalPass;
    private MergeSortPhase _phase = MergeSortPhase.Ready;
    private MergeSortVariant _variant = MergeSortVariant.TopDownRecursive;

    public MergeSortSimulation(ISimulationRuntime simulationRuntime) : base(simulationRuntime) { }

    public int Count => _elements.Length;

    public void LoadValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _elements = new MergeSortElement[values.Length];
        _buffer = new MergeSortElement?[values.Length];
        _initialValues = new int[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            _elements[index] = new MergeSortElement(values[index], index);
            _initialValues[index] = values[index];
        }
        ResetRunState();
    }

    public void ResetVisualState() => ResetRunState();

    public MergeSortSnapshot CreateSnapshot()
    {
        var elements = new MergeSortElementSnapshot[_elements.Length];
        var buffer = new MergeSortBufferSlotSnapshot[_buffer.Length];
        for (var index = 0; index < _elements.Length; index++)
        {
            var element = _elements[index];
            elements[index] = new MergeSortElementSnapshot(element.Value, element.OriginalIndex, element.VisualState);
            var buffered = _buffer[index];
            buffer[index] = buffered is null
                ? new MergeSortBufferSlotSnapshot(false, 0, -1, index == _writeIndex)
                : new MergeSortBufferSlotSnapshot(true, buffered.Value, buffered.OriginalIndex, index == _writeIndex);
        }

        return new MergeSortSnapshot(
            elements, buffer, _comparisons, _writes, _merges, _splits, _currentDepth, _maxDepth,
            _activeStart, _leftEnd, _rightStart, _activeEnd, _leftReadIndex, _rightReadIndex,
            _writeIndex, _naturalRunCount, _naturalPass, _phase, _variant);
    }

    public Task<MergeSortResult> SortAsync(CancellationToken cancellationToken = default)
        => SortAsync(MergeSortVariant.TopDownRecursive, cancellationToken);

    public async Task<MergeSortResult> SortAsync(MergeSortVariant variant, CancellationToken cancellationToken = default)
    {
        _variant = variant;
        CaptureInitialValues();
        ResetRunState();

        if (_elements.Length == 0)
        {
            _phase = MergeSortPhase.Complete;
            await NextStepAsync("The array is empty, so Merge Sort has nothing to divide or merge.", cancellationToken);
            return BuildResult();
        }

        if (_elements.Length == 1)
        {
            _elements[0].VisualState = MergeSortElementVisualState.Sorted;
            _phase = MergeSortPhase.Complete;
            await NextStepAsync($"The array contains only {_elements[0].Value}. One item is already sorted, so no split, comparison, or merge is required.", cancellationToken);
            return BuildResult();
        }

        await NextStepAsync(
            variant == MergeSortVariant.TopDownRecursive
                ? $"Start Top-down Merge Sort with {_elements.Length} values. Recursively split each range until single items remain, then merge those sorted pieces back together."
                : $"Start Natural Merge Sort with {_elements.Length} values. First detect runs that are already nondecreasing, then repeatedly merge neighboring runs until only one sorted run remains.",
            cancellationToken);

        if (variant == MergeSortVariant.TopDownRecursive)
        {
            await SortRangeAsync(0, _elements.Length - 1, 0, cancellationToken);
        }
        else
        {
            await NaturalSortAsync(cancellationToken);
        }

        ClearRoles();
        _phase = MergeSortPhase.Complete;
        for (var index = 0; index < _elements.Length; index++) _elements[index].VisualState = MergeSortElementVisualState.Sorted;
        await NextStepAsync(
            variant == MergeSortVariant.TopDownRecursive
                ? $"Top-down Merge Sort is complete: {_merges} merges, {_comparisons} value comparisons, {_writes} buffer/array writes, maximum recursion depth {_maxDepth}."
                : $"Natural Merge Sort is complete: the input began as {_initialNaturalRunCount} natural run{(_initialNaturalRunCount == 1 ? string.Empty : "s")} and finished after {_naturalPass} merge pass{(_naturalPass == 1 ? string.Empty : "es")}, {_merges} merges, and {_comparisons} comparisons.",
            cancellationToken);

        return BuildResult();
    }

    private async Task SortRangeAsync(int start, int end, int depth, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (start >= end) return;

        _currentDepth = depth;
        _maxDepth = Math.Max(_maxDepth, depth + 1);
        var middle = start + ((end - start) / 2);
        _activeStart = start;
        _leftEnd = middle;
        _rightStart = middle + 1;
        _activeEnd = end;
        _phase = MergeSortPhase.Splitting;
        _splits++;
        RefreshVisualStates();

        await NextStepAsync(
            $"Divide range [{start}..{end}] at the middle: left [{start}..{middle}] and right [{middle + 1}..{end}]. Neither half is merged yet; first solve both smaller sorting problems.",
            cancellationToken);

        await SortRangeAsync(start, middle, depth + 1, cancellationToken);
        await SortRangeAsync(middle + 1, end, depth + 1, cancellationToken);
        await MergeRunsAsync(start, middle, middle + 1, end, depth, cancellationToken);
    }

    private async Task NaturalSortAsync(CancellationToken cancellationToken)
    {
        var firstDetection = true;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var runs = DetectNaturalRuns(out var runCount);
            _naturalRunCount = runCount;
            if (firstDetection)
            {
                _initialNaturalRunCount = runCount;
                firstDetection = false;
            }

            _phase = MergeSortPhase.DetectingRuns;
            ClearRoles();
            RefreshVisualStates();
            await NextStepAsync(
                runCount == 1
                    ? $"Run detection found one already-sorted run [{runs[0].Start}..{runs[0].End}]. No merge is necessary."
                    : $"Run detection found {runCount} already-sorted runs: {FormatRuns(runs, runCount)}. Merge neighboring runs instead of recursively splitting them again.",
                cancellationToken);

            if (runCount <= 1) return;

            _naturalPass++;
            for (var index = 0; index + 1 < runCount; index += 2)
            {
                var left = runs[index];
                var right = runs[index + 1];
                await MergeRunsAsync(left.Start, left.End, right.Start, right.End, 0, cancellationToken);
            }

            _phase = MergeSortPhase.MergeComplete;
            ClearRoles();
            RefreshVisualStates();
            await NextStepAsync(
                $"Natural merge pass {_naturalPass} complete. Detect the new larger sorted runs before deciding what still needs to be merged.",
                cancellationToken);
        }
    }

    private RunRange[] DetectNaturalRuns(out int runCount)
    {
        var runs = new RunRange[_elements.Length];
        runCount = 0;
        if (_elements.Length == 0) return runs;

        var start = 0;
        for (var index = 1; index < _elements.Length; index++)
        {
            _comparisons++;
            if (_elements[index - 1].Value <= _elements[index].Value) continue;
            runs[runCount++] = new RunRange(start, index - 1);
            start = index;
        }
        runs[runCount++] = new RunRange(start, _elements.Length - 1);
        return runs;
    }

    private async Task MergeRunsAsync(int leftStart, int leftEnd, int rightStart, int rightEnd, int depth, CancellationToken cancellationToken)
    {
        _merges++;
        _currentDepth = depth;
        _activeStart = leftStart;
        _leftEnd = leftEnd;
        _rightStart = rightStart;
        _activeEnd = rightEnd;
        _leftReadIndex = leftStart;
        _rightReadIndex = rightStart;
        _writeIndex = leftStart;
        ClearBufferRange(0, _buffer.Length - 1);
        _phase = MergeSortPhase.Comparing;
        RefreshVisualStates();

        await NextStepAsync(
            $"Merge sorted runs [{leftStart}..{leftEnd}] and [{rightStart}..{rightEnd}]. Compare only the front unread value of each run and write the smaller one into the temporary buffer.",
            cancellationToken);

        while (_leftReadIndex <= leftEnd && _rightReadIndex <= rightEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _comparisons++;
            _phase = MergeSortPhase.Comparing;
            RefreshVisualStates();
            var left = _elements[_leftReadIndex];
            var right = _elements[_rightReadIndex];
            await NextStepAsync(
                $"Compare left {left.Value} at index {_leftReadIndex} with right {right.Value} at index {_rightReadIndex}. On equality, choose the left item first so duplicate order stays stable.",
                cancellationToken);

            if (left.Value <= right.Value)
            {
                await BufferElementAsync(left, "left", cancellationToken);
                _leftReadIndex++;
            }
            else
            {
                await BufferElementAsync(right, "right", cancellationToken);
                _rightReadIndex++;
            }
        }

        while (_leftReadIndex <= leftEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var element = _elements[_leftReadIndex++];
            await BufferElementAsync(element, "left remainder", cancellationToken);
        }

        while (_rightReadIndex <= rightEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var element = _elements[_rightReadIndex++];
            await BufferElementAsync(element, "right remainder", cancellationToken);
        }

        _phase = MergeSortPhase.CopyingBack;
        _leftReadIndex = -1;
        _rightReadIndex = -1;
        for (var index = leftStart; index <= rightEnd; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _writeIndex = index;
            var element = _buffer[index] ?? throw new InvalidOperationException("Merge buffer unexpectedly contains an empty slot.");
            _elements[index] = element;
            _writes++;
            RefreshVisualStates();
            await NextStepAsync(
                $"Copy buffer[{index}] = {element.Value} back into array[{index}]. The merged range becomes the new sorted input for a larger merge.",
                cancellationToken);
        }

        _phase = MergeSortPhase.MergeComplete;
        _writeIndex = -1;
        RefreshVisualStates();
        await NextStepAsync(
            $"Merge complete. Range [{leftStart}..{rightEnd}] is now sorted and can act as one run in the next level.",
            cancellationToken);
    }

    private async Task BufferElementAsync(MergeSortElement element, string source, CancellationToken cancellationToken)
    {
        _phase = MergeSortPhase.FillingBuffer;
        _buffer[_writeIndex] = element;
        _writes++;
        RefreshVisualStates();
        await NextStepAsync(
            $"Choose {element.Value} from the {source} and write it to buffer[{_writeIndex}]. Only the chosen run advances; the other front value remains available for the next comparison.",
            cancellationToken);
        _writeIndex++;
    }

    private void CaptureInitialValues()
    {
        _initialValues = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) _initialValues[index] = _elements[index].Value;
    }

    private void ResetRunState()
    {
        _comparisons = 0;
        _writes = 0;
        _merges = 0;
        _splits = 0;
        _currentDepth = 0;
        _maxDepth = 0;
        _naturalRunCount = 0;
        _initialNaturalRunCount = 0;
        _naturalPass = 0;
        ClearRoles();
        if (_buffer.Length != _elements.Length) _buffer = new MergeSortElement?[_elements.Length];
        else for (var index = 0; index < _buffer.Length; index++) _buffer[index] = null;
        _phase = MergeSortPhase.Ready;
        for (var index = 0; index < _elements.Length; index++) _elements[index].VisualState = MergeSortElementVisualState.Normal;
    }

    private void ClearRoles()
    {
        _activeStart = -1;
        _leftEnd = -1;
        _rightStart = -1;
        _activeEnd = -1;
        _leftReadIndex = -1;
        _rightReadIndex = -1;
        _writeIndex = -1;
    }

    private void ClearBufferRange(int start, int end)
    {
        for (var index = start; index <= end; index++) _buffer[index] = null;
    }

    private void RefreshVisualStates()
    {
        for (var index = 0; index < _elements.Length; index++)
        {
            var state = MergeSortElementVisualState.Normal;
            if (_phase == MergeSortPhase.Complete) state = MergeSortElementVisualState.Sorted;
            else if (_activeStart >= 0 && index >= _activeStart && index <= _activeEnd)
            {
                state = index <= _leftEnd ? MergeSortElementVisualState.LeftRun : MergeSortElementVisualState.RightRun;
                if (index == _leftReadIndex || index == _rightReadIndex) state = MergeSortElementVisualState.Comparing;
                if (_phase == MergeSortPhase.CopyingBack && index == _writeIndex) state = MergeSortElementVisualState.Writing;
            }
            _elements[index].VisualState = state;
        }
    }

    private MergeSortResult BuildResult()
    {
        var sorted = new int[_elements.Length];
        for (var index = 0; index < _elements.Length; index++) sorted[index] = _elements[index].Value;
        return new MergeSortResult(
            Clone(_initialValues), sorted, _comparisons, _writes, _merges, _splits, _maxDepth,
            _initialNaturalRunCount, _naturalPass, PreservedEqualValueOrder(), _variant);
    }

    private bool PreservedEqualValueOrder()
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

    private static int[] Clone(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++) copy[index] = source[index];
        return copy;
    }

    private static string FormatRuns(RunRange[] runs, int runCount)
    {
        var labels = new string[runCount];
        for (var index = 0; index < runCount; index++) labels[index] = $"[{runs[index].Start}..{runs[index].End}]";
        return string.Join(", ", labels);
    }
    private readonly record struct RunRange(int Start, int End);
}

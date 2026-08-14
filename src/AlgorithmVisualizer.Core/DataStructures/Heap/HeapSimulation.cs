using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Heap;

/// <summary>
/// Binary min/max heap implemented from scratch over a custom growable array.
/// The complete-tree shape is encoded by indexes: parent=(i-1)/2, left=2i+1, right=2i+2.
/// Heap order is restored manually with swaps during bubble-up and bubble-down.
/// </summary>
public sealed class HeapSimulation : SimulationAlgorithmBase
{
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly ManualHeapArray<HeapElement> _items = new();

    public HeapSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public HeapKind Kind { get; private set; } = HeapKind.Min;
    public int Count => _items.Count;
    public int Capacity => _items.Capacity;
    public int Height => Count == 0 ? 0 : (int)Math.Floor(Math.Log2(Count)) + 1;

    public event Action? Changed;

    public HeapSnapshot CreateSnapshot()
    {
        var elements = new HeapElementSnapshot[_items.Count];
        for (var index = 0; index < _items.Count; index++)
        {
            var element = _items[index];
            elements[index] = new HeapElementSnapshot(index, element.Id, element.Value, element.VisualState);
        }

        return new HeapSnapshot(Kind, Count, Capacity, elements);
    }

    /// <summary>
    /// Heap type is a lab configuration, not a hidden rebuild. A non-empty heap must be cleared
    /// before switching because the same values can require a completely different ordering.
    /// </summary>
    public bool TrySetKind(HeapKind kind)
    {
        if (_items.Count != 0)
        {
            return false;
        }

        Kind = kind;
        NotifyChanged();
        return true;
    }

    public Task<HeapOperationResult> InsertAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            ResetVisualStates();
            var initialCount = Count;
            var capacityBefore = Capacity;
            var inserted = new HeapElement(value)
            {
                VisualState = HeapElementVisualState.Added
            };

            _items.Add(inserted);
            var startIndex = Count - 1;
            NotifyChanged();

            if (Capacity != capacityBefore)
            {
                await NextStepAsync(
                    $"The backing array had no free slot, so capacity grew from {capacityBefore} to {Capacity}. Existing element references were copied with our own loop before the new reference was stored.",
                    cancellationToken);
            }

            await NextStepAsync(
                $"Append {value} (#{inserted.DisplayId}) at array index {startIndex}. Appending preserves the complete-tree shape; now heap order may need repair.",
                cancellationToken);

            var repair = await BubbleUpAsync(startIndex, cancellationToken);
            ResetVisualStates();
            NotifyChanged();

            await NextStepAsync(
                repair.Swaps == 0
                    ? $"{value} already satisfies the {KindLabel} property at index {repair.FinalIndex}. No swap was needed."
                    : $"Bubble-up finished after {repair.Swaps} swap(s). {value} now rests at index {repair.FinalIndex}, and every parent-child relation satisfies the {KindLabel} property.",
                cancellationToken);

            return new HeapOperationResult(
                HeapOperationKind.Insert, Kind, value, true, inserted.Id, inserted.Value,
                repair.Comparisons, repair.Swaps, initialCount, Count, capacityBefore, Capacity,
                startIndex, repair.FinalIndex, HeapRepairDirection.BubbleUp);
        }, cancellationToken);

    public Task<HeapOperationResult> ExtractRootAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            ResetVisualStates();
            var initialCount = Count;
            var capacityBefore = Capacity;

            if (Count == 0)
            {
                await NextStepAsync("The heap is empty, so there is no root to extract.", cancellationToken);
                return new HeapOperationResult(
                    HeapOperationKind.ExtractRoot, Kind, null, false, null, null,
                    0, 0, initialCount, Count, capacityBefore, Capacity,
                    null, null, HeapRepairDirection.None);
            }

            var removed = _items[0];
            removed.VisualState = HeapElementVisualState.Removing;
            NotifyChanged();

            await NextStepAsync(
                $"The root is {removed.Value} (#{removed.DisplayId}). In a {KindLabel}, this is the {(Kind == HeapKind.Min ? "minimum" : "maximum")} value, so extract-root removes it directly.",
                cancellationToken);

            if (Count == 1)
            {
                _items.RemoveLast();
                NotifyChanged();
                await NextStepAsync("The root was the only element. Removing the last array slot leaves an empty heap.", cancellationToken);

                return new HeapOperationResult(
                    HeapOperationKind.ExtractRoot, Kind, null, true, removed.Id, removed.Value,
                    0, 0, initialCount, Count, capacityBefore, Capacity,
                    0, null, HeapRepairDirection.None);
            }

            var lastIndex = Count - 1;
            var replacement = _items[lastIndex];
            replacement.VisualState = HeapElementVisualState.Candidate;
            NotifyChanged();

            await NextStepAsync(
                $"Move the last element {replacement.Value} (#{replacement.DisplayId}) from index {lastIndex} to index 0, then remove the now-unused last slot. This preserves the complete-tree shape.",
                cancellationToken);

            _items[0] = replacement;
            _items.RemoveLast();
            replacement.VisualState = HeapElementVisualState.Repairing;
            NotifyChanged();

            var repair = await BubbleDownAsync(0, cancellationToken);
            ResetVisualStates();
            NotifyChanged();

            await NextStepAsync(
                repair.Swaps == 0
                    ? $"The replacement already satisfies the {KindLabel} property at the root. Extraction is complete."
                    : $"Bubble-down finished after {repair.Swaps} swap(s). The {KindLabel} property is restored across the heap.",
                cancellationToken);

            return new HeapOperationResult(
                HeapOperationKind.ExtractRoot, Kind, null, true, removed.Id, removed.Value,
                repair.Comparisons, repair.Swaps, initialCount, Count, capacityBefore, Capacity,
                0, repair.FinalIndex, HeapRepairDirection.BubbleDown);
        }, cancellationToken);

    public Task<HeapOperationResult> SearchAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            ResetVisualStates();
            var initialCount = Count;
            var capacityBefore = Capacity;
            var comparisons = 0;

            for (var index = 0; index < Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var current = _items[index];
                comparisons++;
                var match = current.Value == value;
                current.VisualState = match ? HeapElementVisualState.Matched : HeapElementVisualState.Checking;
                NotifyChanged();

                await NextStepAsync(
                    match
                        ? $"Check index {index}: {current.Value} matches {value}. Search stops after {comparisons} check(s)."
                        : $"Check index {index}: {current.Value} is not {value}. Heap order cannot tell us which subtree contains an arbitrary value, so continue with the next array slot.",
                    cancellationToken);

                if (match)
                {
                    return new HeapOperationResult(
                        HeapOperationKind.Search, Kind, value, true, current.Id, current.Value,
                        comparisons, 0, initialCount, Count, capacityBefore, Capacity,
                        index, index, HeapRepairDirection.None);
                }

                current.VisualState = HeapElementVisualState.Normal;
            }

            await NextStepAsync(
                $"All {Count} used array slot(s) were checked. {value} is not present; arbitrary heap search is O(n), not O(log n).",
                cancellationToken);

            return new HeapOperationResult(
                HeapOperationKind.Search, Kind, value, false, null, null,
                comparisons, 0, initialCount, Count, capacityBefore, Capacity,
                null, null, HeapRepairDirection.None);
        }, cancellationToken);

    public Task<HeapOperationResult> DeleteAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            ResetVisualStates();
            var initialCount = Count;
            var capacityBefore = Capacity;
            var comparisons = 0;
            var targetIndex = -1;
            HeapElement? target = null;

            for (var index = 0; index < Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var current = _items[index];
                comparisons++;
                var match = current.Value == value;
                current.VisualState = match ? HeapElementVisualState.Matched : HeapElementVisualState.Checking;
                NotifyChanged();

                await NextStepAsync(
                    match
                        ? $"Delete search found {value} at index {index} (#{current.DisplayId}) after {comparisons} check(s)."
                        : $"Delete search checks index {index}: {current.Value} is not {value}, so continue linearly.",
                    cancellationToken);

                if (match)
                {
                    targetIndex = index;
                    target = current;
                    break;
                }

                current.VisualState = HeapElementVisualState.Normal;
            }

            if (target is null)
            {
                await NextStepAsync($"{value} is not present, so the heap stays unchanged.", cancellationToken);
                return new HeapOperationResult(
                    HeapOperationKind.Delete, Kind, value, false, null, null,
                    comparisons, 0, initialCount, Count, capacityBefore, Capacity,
                    null, null, HeapRepairDirection.None);
            }

            target.VisualState = HeapElementVisualState.Removing;
            NotifyChanged();

            if (targetIndex == Count - 1)
            {
                _items.RemoveLast();
                NotifyChanged();
                await NextStepAsync(
                    $"The matched element is already the last array slot, so removing it preserves both complete shape and heap order. No repair is needed.",
                    cancellationToken);

                return new HeapOperationResult(
                    HeapOperationKind.Delete, Kind, value, true, target.Id, target.Value,
                    comparisons, 0, initialCount, Count, capacityBefore, Capacity,
                    targetIndex, null, HeapRepairDirection.None);
            }

            var lastIndex = Count - 1;
            var replacement = _items[lastIndex];
            replacement.VisualState = HeapElementVisualState.Candidate;
            NotifyChanged();

            await NextStepAsync(
                $"Replace index {targetIndex} with the last element {replacement.Value} (#{replacement.DisplayId}) from index {lastIndex}, then release the last slot.",
                cancellationToken);

            _items[targetIndex] = replacement;
            _items.RemoveLast();
            replacement.VisualState = HeapElementVisualState.Repairing;
            NotifyChanged();

            var repairDirection = HeapRepairDirection.None;
            var repairComparisons = 0;
            var swaps = 0;
            var finalIndex = targetIndex;

            if (targetIndex > 0)
            {
                var parentIndex = ParentIndex(targetIndex);
                repairComparisons++;
                var parent = _items[parentIndex];
                replacement.VisualState = HeapElementVisualState.Checking;
                parent.VisualState = HeapElementVisualState.Candidate;
                NotifyChanged();

                await NextStepAsync(
                    $"First compare replacement {replacement.Value} at index {targetIndex} with parent {parent.Value} at index {parentIndex}. This tells us whether repair must move upward or downward.",
                    cancellationToken);

                if (IsHigherPriority(replacement.Value, parent.Value))
                {
                    var up = await BubbleUpAsync(targetIndex, cancellationToken);
                    repairDirection = HeapRepairDirection.BubbleUp;
                    repairComparisons += up.Comparisons;
                    swaps += up.Swaps;
                    finalIndex = up.FinalIndex;
                }
                else
                {
                    var down = await BubbleDownAsync(targetIndex, cancellationToken);
                    repairDirection = HeapRepairDirection.BubbleDown;
                    repairComparisons += down.Comparisons;
                    swaps += down.Swaps;
                    finalIndex = down.FinalIndex;
                }
            }
            else
            {
                var down = await BubbleDownAsync(0, cancellationToken);
                repairDirection = HeapRepairDirection.BubbleDown;
                repairComparisons += down.Comparisons;
                swaps += down.Swaps;
                finalIndex = down.FinalIndex;
            }

            ResetVisualStates();
            NotifyChanged();

            await NextStepAsync(
                swaps == 0
                    ? $"The replacement already fits at index {finalIndex}; the {KindLabel} property needs no swap."
                    : $"Repair used {repairDirection.ToString().Replace("Bubble", "bubble-").ToLowerInvariant()} and {swaps} swap(s). The {KindLabel} property is restored.",
                cancellationToken);

            return new HeapOperationResult(
                HeapOperationKind.Delete, Kind, value, true, target.Id, target.Value,
                comparisons + repairComparisons, swaps, initialCount, Count, capacityBefore, Capacity,
                targetIndex, finalIndex, repairDirection);
        }, cancellationToken);

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            if (Count == 0)
            {
                await NextStepAsync("The heap is already empty.", cancellationToken);
                return true;
            }

            ResetVisualStates();
            NotifyChanged();
            await NextStepAsync(
                $"Clear releases references from all {Count} used slot(s). Reserved capacity may remain available for the next run.",
                cancellationToken);

            _items.Clear();
            NotifyChanged();
            await NextStepAsync("The heap now has zero elements. You may also switch between Min Heap and Max Heap while it is empty.", cancellationToken);
            return true;
        }, cancellationToken);

    private async Task<RepairResult> BubbleUpAsync(int startIndex, CancellationToken cancellationToken)
    {
        var currentIndex = startIndex;
        var comparisons = 0;
        var swaps = 0;

        while (currentIndex > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parentIndex = ParentIndex(currentIndex);
            var current = _items[currentIndex];
            var parent = _items[parentIndex];
            comparisons++;
            current.VisualState = HeapElementVisualState.Checking;
            parent.VisualState = HeapElementVisualState.Candidate;
            NotifyChanged();

            var shouldSwap = IsHigherPriority(current.Value, parent.Value);
            await NextStepAsync(
                shouldSwap
                    ? $"Bubble-up: compare index {currentIndex} ({current.Value}) with parent index {parentIndex} ({parent.Value}). {current.Value} has higher {PriorityWord} priority, so swap them."
                    : $"Bubble-up: compare index {currentIndex} ({current.Value}) with parent index {parentIndex} ({parent.Value}). Heap order is already valid, so stop.",
                cancellationToken);

            if (!shouldSwap)
            {
                current.VisualState = HeapElementVisualState.Repairing;
                parent.VisualState = HeapElementVisualState.Normal;
                break;
            }

            current.VisualState = HeapElementVisualState.Swapping;
            parent.VisualState = HeapElementVisualState.Swapping;
            _items.Swap(currentIndex, parentIndex);
            swaps++;
            NotifyChanged();

            await NextStepAsync(
                $"Swap complete: #{current.DisplayId} moves to index {parentIndex}; #{parent.DisplayId} moves to index {currentIndex}. Element identity stays the same while array references exchange positions.",
                cancellationToken);

            current.VisualState = HeapElementVisualState.Repairing;
            parent.VisualState = HeapElementVisualState.Normal;
            currentIndex = parentIndex;
        }

        return new RepairResult(comparisons, swaps, currentIndex);
    }

    private async Task<RepairResult> BubbleDownAsync(int startIndex, CancellationToken cancellationToken)
    {
        var currentIndex = startIndex;
        var comparisons = 0;
        var swaps = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var leftIndex = LeftChildIndex(currentIndex);
            if (leftIndex >= Count)
            {
                break;
            }

            var rightIndex = RightChildIndex(currentIndex);
            var preferredChildIndex = leftIndex;
            var parent = _items[currentIndex];
            var left = _items[leftIndex];
            left.VisualState = HeapElementVisualState.Candidate;

            if (rightIndex < Count)
            {
                var right = _items[rightIndex];
                right.VisualState = HeapElementVisualState.Candidate;
                comparisons++;
                preferredChildIndex = IsHigherPriority(right.Value, left.Value) ? rightIndex : leftIndex;
                NotifyChanged();

                await NextStepAsync(
                    $"Bubble-down: compare children {left.Value} (index {leftIndex}) and {right.Value} (index {rightIndex}). Choose {(preferredChildIndex == leftIndex ? left.Value : right.Value)} because it has higher {PriorityWord} priority.",
                    cancellationToken);
            }
            else
            {
                NotifyChanged();
                await NextStepAsync(
                    $"Bubble-down: index {currentIndex} has only left child {left.Value} at index {leftIndex}, so that is the only repair candidate.",
                    cancellationToken);
            }

            var preferredChild = _items[preferredChildIndex];
            parent.VisualState = HeapElementVisualState.Checking;
            preferredChild.VisualState = HeapElementVisualState.Candidate;
            comparisons++;
            var shouldSwap = IsHigherPriority(preferredChild.Value, parent.Value);
            NotifyChanged();

            await NextStepAsync(
                shouldSwap
                    ? $"Compare parent {parent.Value} at index {currentIndex} with chosen child {preferredChild.Value} at index {preferredChildIndex}. The child must move up, so swap."
                    : $"Compare parent {parent.Value} at index {currentIndex} with chosen child {preferredChild.Value}. Heap order is valid here, so bubble-down stops.",
                cancellationToken);

            if (!shouldSwap)
            {
                parent.VisualState = HeapElementVisualState.Repairing;
                preferredChild.VisualState = HeapElementVisualState.Normal;
                if (rightIndex < Count)
                {
                    _items[rightIndex].VisualState = HeapElementVisualState.Normal;
                }
                left.VisualState = HeapElementVisualState.Normal;
                break;
            }

            parent.VisualState = HeapElementVisualState.Swapping;
            preferredChild.VisualState = HeapElementVisualState.Swapping;
            _items.Swap(currentIndex, preferredChildIndex);
            swaps++;
            NotifyChanged();

            await NextStepAsync(
                $"Swap complete: #{preferredChild.DisplayId} moves to index {currentIndex}; #{parent.DisplayId} moves to index {preferredChildIndex}. Continue checking from the lower index.",
                cancellationToken);

            parent.VisualState = HeapElementVisualState.Repairing;
            preferredChild.VisualState = HeapElementVisualState.Normal;
            if (rightIndex < Count && rightIndex != preferredChildIndex)
            {
                _items[rightIndex].VisualState = HeapElementVisualState.Normal;
            }
            if (leftIndex != preferredChildIndex)
            {
                _items[leftIndex].VisualState = HeapElementVisualState.Normal;
            }

            currentIndex = preferredChildIndex;
        }

        return new RepairResult(comparisons, swaps, currentIndex);
    }

    private bool IsHigherPriority(int candidate, int other) => Kind switch
    {
        HeapKind.Min => candidate < other,
        HeapKind.Max => candidate > other,
        _ => false
    };

    private string KindLabel => Kind == HeapKind.Min ? "min-heap" : "max-heap";
    private string PriorityWord => Kind == HeapKind.Min ? "smaller-value" : "larger-value";

    private static int ParentIndex(int index) => (index - 1) / 2;
    private static int LeftChildIndex(int index) => (2 * index) + 1;
    private static int RightChildIndex(int index) => (2 * index) + 2;

    private void ResetVisualStates()
    {
        for (var index = 0; index < Count; index++)
        {
            _items[index].VisualState = HeapElementVisualState.Normal;
        }
    }

    private void NotifyChanged() => Changed?.Invoke();

    private async Task<T> ExecuteExclusiveAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            SimulationRuntime.SimulationCancellationToken);

        await _operationGate.WaitAsync(linked.Token);
        try
        {
            return await operation();
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private sealed record RepairResult(int Comparisons, int Swaps, int FinalIndex);
}

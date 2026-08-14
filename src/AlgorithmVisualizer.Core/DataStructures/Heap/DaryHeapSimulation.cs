using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Heap;

/// <summary>
/// Generalized d-ary min/max heap implemented from scratch over the same custom raw-array storage
/// used by the binary heap. The binary heap is exactly the d = 2 special case. This implementation
/// allows d >= 2 and teaches how branching factor changes the index formulas and tree height.
/// </summary>
public sealed class DaryHeapSimulation : SimulationAlgorithmBase
{
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly ManualHeapArray<HeapElement> _items = new();

    public DaryHeapSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public HeapKind Kind { get; private set; } = HeapKind.Min;
    public int Arity { get; private set; } = 3;
    public int Count => _items.Count;
    public int Capacity => _items.Capacity;
    public int Height
    {
        get
        {
            if (Count == 0)
            {
                return 0;
            }

            var height = 1;
            var levelCapacity = 1;
            var covered = 1;
            while (covered < Count)
            {
                checked
                {
                    levelCapacity *= Arity;
                    covered += levelCapacity;
                }
                height++;
            }

            return height;
        }
    }

    public event Action? Changed;

    public DaryHeapSnapshot CreateSnapshot()
    {
        var elements = new DaryHeapElementSnapshot[_items.Count];
        for (var index = 0; index < _items.Count; index++)
        {
            var element = _items[index];
            elements[index] = new DaryHeapElementSnapshot(index, element.Id, element.Value, element.VisualState);
        }

        return new DaryHeapSnapshot(Kind, Arity, Count, Capacity, elements);
    }

    public bool TrySetKind(HeapKind kind)
    {
        if (Count != 0)
        {
            return false;
        }

        Kind = kind;
        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Changing d changes every parent/child index relationship, so a non-empty heap is never
    /// silently reinterpreted. The learner must clear it first unless a future module implements
    /// and visualizes a real rebuild operation.
    /// </summary>
    public bool TrySetArity(int arity)
    {
        if (arity < 2 || arity > 8 || Count != 0)
        {
            return false;
        }

        Arity = arity;
        NotifyChanged();
        return true;
    }

    public Task<HeapOperationResult> InsertAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            ResetVisualStates();
            var initialCount = Count;
            var capacityBefore = Capacity;
            var inserted = new HeapElement(value) { VisualState = HeapElementVisualState.Added };

            _items.Add(inserted);
            var startIndex = Count - 1;
            NotifyChanged();

            if (Capacity != capacityBefore)
            {
                await NextStepAsync(
                    $"The raw backing array was full, so capacity grew from {capacityBefore} to {Capacity}. Existing references were copied with our own loop before the new reference was stored.",
                    cancellationToken);
            }

            await NextStepAsync(
                $"Append {value} (#{inserted.DisplayId}) at index {startIndex}. In a {Arity}-ary heap, appending preserves the complete d-ary shape; only heap priority may now be violated.",
                cancellationToken);

            var repair = await BubbleUpAsync(startIndex, cancellationToken);
            ResetVisualStates();
            NotifyChanged();

            await NextStepAsync(
                repair.Swaps == 0
                    ? $"{value} already satisfies the {KindLabel} property at index {repair.FinalIndex}."
                    : $"Bubble-up finished after {repair.Swaps} swap(s). The same element object now rests at index {repair.FinalIndex}.",
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
                await NextStepAsync("The heap is empty, so there is no priority root to extract.", cancellationToken);
                return new HeapOperationResult(
                    HeapOperationKind.ExtractRoot, Kind, null, false, null, null,
                    0, 0, initialCount, Count, capacityBefore, Capacity,
                    null, null, HeapRepairDirection.None);
            }

            var removed = _items[0];
            removed.VisualState = HeapElementVisualState.Removing;
            NotifyChanged();

            await NextStepAsync(
                $"The root is {removed.Value} (#{removed.DisplayId}). The {KindLabel} property guarantees that this is the {(Kind == HeapKind.Min ? "minimum" : "maximum")} value.",
                cancellationToken);

            if (Count == 1)
            {
                _items.RemoveLast();
                NotifyChanged();
                await NextStepAsync("The root was the only element, so removing the last used slot leaves an empty heap.", cancellationToken);
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
                $"Move the last element {replacement.Value} (#{replacement.DisplayId}) from index {lastIndex} to root index 0, then release the old last slot. This keeps the complete {Arity}-ary shape compact.",
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
                    ? "The replacement already has correct priority over every existing child."
                    : $"Bubble-down finished after {repair.Swaps} swap(s). At each level we chose the highest-priority child among as many as {Arity} children.",
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
                        ? $"Check index {index}: {current.Value} matches {value}."
                        : $"Check index {index}: {current.Value} is not {value}. Parent/child priority still does not tell us which branch contains an arbitrary target, so continue the array scan.",
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
                $"All {Count} used slot(s) were checked. A d-ary heap changes branching factor, not arbitrary-search ordering, so missing-value search is still O(n).",
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
                        ? $"Delete search found {value} at index {index} (#{current.DisplayId})."
                        : $"Delete search checks index {index}: {current.Value} is not {value}.",
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
                await NextStepAsync("The target already occupies the last used slot, so removing it cannot create a shape gap or heap-order violation.", cancellationToken);
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
                    $"Compare replacement {replacement.Value} at index {targetIndex} with parent {parent.Value} at index {parentIndex}. If it outranks the parent, repair upward; otherwise check children and repair downward.",
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
                    ? $"The replacement already fits at index {finalIndex}."
                    : $"Repair used {RepairLabel(repairDirection)} and {swaps} swap(s). The {KindLabel} property is restored.",
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
                $"Clear releases references from all {Count} used slot(s). Reserved array capacity remains available for another run.",
                cancellationToken);

            _items.Clear();
            NotifyChanged();
            await NextStepAsync("The d-ary heap is empty. You may now change Min/Max mode or branching factor d.", cancellationToken);
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
                    ? $"Bubble-up: parent({currentIndex}) = ({currentIndex} − 1) / {Arity} = {parentIndex}. {current.Value} outranks parent {parent.Value}, so swap."
                    : $"Bubble-up: compare {current.Value} at index {currentIndex} with parent {parent.Value} at {parentIndex}. Heap order is valid, so stop.",
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
                $"Swap references: #{current.DisplayId} moves to index {parentIndex}; #{parent.DisplayId} moves to index {currentIndex}. Their values and IDs do not change.",
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
            var firstChild = FirstChildIndex(currentIndex);
            if (firstChild >= Count)
            {
                break;
            }

            var lastChild = Math.Min(LastPossibleChildIndex(currentIndex), Count - 1);
            var preferredChildIndex = firstChild;
            var preferredChild = _items[firstChild];
            preferredChild.VisualState = HeapElementVisualState.Candidate;

            await NextStepAsync(
                $"Index {currentIndex} can have children from {firstChild} through {LastPossibleChildIndex(currentIndex)}. Existing children currently end at {lastChild}; choose the highest-priority one before comparing with the parent.",
                cancellationToken);

            for (var childIndex = firstChild + 1; childIndex <= lastChild; childIndex++)
            {
                var child = _items[childIndex];
                child.VisualState = HeapElementVisualState.Candidate;
                comparisons++;
                NotifyChanged();

                await NextStepAsync(
                    $"Compare child candidate {child.Value} at index {childIndex} with current best child {preferredChild.Value} at index {preferredChildIndex}.",
                    cancellationToken);

                if (IsHigherPriority(child.Value, preferredChild.Value))
                {
                    preferredChild.VisualState = HeapElementVisualState.Normal;
                    preferredChildIndex = childIndex;
                    preferredChild = child;
                    preferredChild.VisualState = HeapElementVisualState.Candidate;
                }
                else
                {
                    child.VisualState = HeapElementVisualState.Normal;
                }
            }

            var parent = _items[currentIndex];
            parent.VisualState = HeapElementVisualState.Checking;
            preferredChild.VisualState = HeapElementVisualState.Candidate;
            comparisons++;
            var shouldSwap = IsHigherPriority(preferredChild.Value, parent.Value);
            NotifyChanged();

            await NextStepAsync(
                shouldSwap
                    ? $"Best child {preferredChild.Value} at index {preferredChildIndex} outranks parent {parent.Value} at index {currentIndex}, so swap."
                    : $"Parent {parent.Value} already outranks every existing child. Bubble-down stops at index {currentIndex}.",
                cancellationToken);

            if (!shouldSwap)
            {
                parent.VisualState = HeapElementVisualState.Repairing;
                ClearChildCandidates(firstChild, lastChild, exceptIndex: -1);
                break;
            }

            parent.VisualState = HeapElementVisualState.Swapping;
            preferredChild.VisualState = HeapElementVisualState.Swapping;
            _items.Swap(currentIndex, preferredChildIndex);
            swaps++;
            NotifyChanged();

            await NextStepAsync(
                $"Swap complete. Continue from index {preferredChildIndex}; its next children, if any, begin at index {(Arity * preferredChildIndex) + 1}.",
                cancellationToken);

            ClearChildCandidates(firstChild, lastChild, preferredChildIndex);
            parent.VisualState = HeapElementVisualState.Repairing;
            preferredChild.VisualState = HeapElementVisualState.Normal;
            currentIndex = preferredChildIndex;
        }

        return new RepairResult(comparisons, swaps, currentIndex);
    }

    private void ClearChildCandidates(int firstChild, int lastChild, int exceptIndex)
    {
        for (var index = firstChild; index <= lastChild; index++)
        {
            if (index != exceptIndex && index < Count)
            {
                _items[index].VisualState = HeapElementVisualState.Normal;
            }
        }
    }

    private bool IsHigherPriority(int candidate, int other) => Kind switch
    {
        HeapKind.Min => candidate < other,
        HeapKind.Max => candidate > other,
        _ => false
    };

    private int ParentIndex(int index) => (index - 1) / Arity;
    private int FirstChildIndex(int index) => (Arity * index) + 1;
    private int LastPossibleChildIndex(int index) => (Arity * index) + Arity;
    private string KindLabel => Kind == HeapKind.Min ? "min-heap" : "max-heap";

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

    private static string RepairLabel(HeapRepairDirection direction) => direction switch
    {
        HeapRepairDirection.BubbleUp => "bubble-up",
        HeapRepairDirection.BubbleDown => "bubble-down",
        _ => "no directional repair"
    };

    private sealed record RepairResult(int Comparisons, int Swaps, int FinalIndex);
}

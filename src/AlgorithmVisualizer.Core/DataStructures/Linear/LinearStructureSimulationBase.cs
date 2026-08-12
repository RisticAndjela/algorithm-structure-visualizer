using System.Collections.ObjectModel;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Provides shared synchronization and transient-state cleanup for linear structure simulations.
/// </summary>
public abstract class LinearStructureSimulationBase : SimulationAlgorithmBase
{
    private readonly List<LinearElement> _items = [];
    private readonly ReadOnlyCollection<LinearElement> _readOnlyItems;
    private readonly SemaphoreSlim _operationGate = new(1, 1);

    protected LinearStructureSimulationBase(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
        _readOnlyItems = _items.AsReadOnly();
    }

    public IReadOnlyList<LinearElement> Items => _readOnlyItems;
    public int Count => _items.Count;

    /// <summary>
    /// Number of slots currently reserved by the List backing store.
    /// Exposed only so the learning UI can explain Count versus Capacity.
    /// </summary>
    public int StorageCapacity => _items.Capacity;

    protected List<LinearElement> MutableItems => _items;

    public event Action? Changed;

    protected async Task ExecuteExclusiveAsync(
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates();
            await operation();
        }
        finally
        {
            NormalizeVisualStates();
            NotifyChanged();
            _operationGate.Release();
        }
    }

    protected async Task<TResult> ExecuteExclusiveAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates();
            return await operation();
        }
        finally
        {
            NormalizeVisualStates();
            NotifyChanged();
            _operationGate.Release();
        }
    }

    /// <summary>
    /// Runs a renderer-neutral linear traversal. The caller chooses whether traversal starts at
    /// the first item (queue/front) or last item (stack/top), plus the predicate and criterion label.
    /// </summary>
    protected Task<LinearTraversalResult> ExecuteTraversalAsync(
        string structureName,
        string traversalDirection,
        bool reverse,
        LinearTraversalOperation operation,
        LinearLookupCriterion criterion,
        string targetDescription,
        Func<LinearElement, bool> matches,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(structureName);
        ArgumentException.ThrowIfNullOrWhiteSpace(traversalDirection);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDescription);
        ArgumentNullException.ThrowIfNull(matches);

        return ExecuteExclusiveAsync(async () =>
        {
            var initialCount = MutableItems.Count;

            if (initialCount == 0)
            {
                await NextStepAsync(
                    $"{structureName} is empty, so there is nothing to check. This empty check is Θ(1).",
                    cancellationToken);

                return new LinearTraversalResult(
                    StructureName: structureName,
                    Operation: operation,
                    Criterion: criterion,
                    Found: false,
                    ElementId: null,
                    ElementValue: null,
                    Comparisons: 0,
                    InitialCount: 0,
                    TraversalDirection: traversalDirection,
                    CurrentRunComplexity: "Θ(1)");
            }

            var comparisons = 0;
            LinearElement? match = null;
            int? matchIndex = null;

            for (var offset = 0; offset < initialCount; offset++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var index = reverse ? initialCount - 1 - offset : offset;
                var element = MutableItems[index];
                comparisons++;

                var isMatch = matches(element);
                element.VisualState = isMatch
                    ? LinearElementVisualState.Matched
                    : LinearElementVisualState.Checking;
                NotifyChanged();

                var comparisonResult = isMatch ? "MATCH" : "no match";
                await NextStepAsync(
                    $"Check {comparisons}/{initialCount}: #{element.DisplayId} stores {element.Value}. Moving {traversalDirection}. Looking for {targetDescription} → {comparisonResult}.",
                    cancellationToken);

                if (isMatch)
                {
                    match = element;
                    matchIndex = index;
                    break;
                }

                element.VisualState = LinearElementVisualState.Visited;
                NotifyChanged();
            }

            var found = match is not null;
            var currentRunComplexity = GetCurrentRunComplexity(comparisons, initialCount, found);

            if (!found)
            {
                await NextStepAsync(
                    $"Not found. The app checked all {comparisons} item(s). This run is {currentRunComplexity}; a full linear search is O(n).",
                    cancellationToken);

                return new LinearTraversalResult(
                    StructureName: structureName,
                    Operation: operation,
                    Criterion: criterion,
                    Found: false,
                    ElementId: null,
                    ElementValue: null,
                    Comparisons: comparisons,
                    InitialCount: initialCount,
                    TraversalDirection: traversalDirection,
                    CurrentRunComplexity: currentRunComplexity)
                {
                    FullOperationComplexity = currentRunComplexity
                };
            }

            var matchedElement = match!;
            var matchedIndex = matchIndex!.Value;
            var shiftedElements = 0;
            int? capacityBefore = null;
            int? capacityAfter = null;
            var fullOperationComplexity = currentRunComplexity;

            if (operation == LinearTraversalOperation.Search)
            {
                await NextStepAsync(
                    $"Found #{matchedElement.DisplayId} (value {matchedElement.Value}) after {comparisons} check(s). This run is {currentRunComplexity}; the worst case for this kind of search is O(n).",
                    cancellationToken);
            }
            else
            {
                matchedElement.VisualState = LinearElementVisualState.Removing;
                NotifyChanged();

                shiftedElements = initialCount - matchedIndex - 1;
                capacityBefore = MutableItems.Capacity;
                fullOperationComplexity = GetFullDeleteRunComplexity(
                    comparisons,
                    shiftedElements,
                    initialCount);

                await NextStepAsync(
                    $"Found it in List slot {matchedIndex}. Removing it makes {shiftedElements} later reference(s) move one slot left, so there is no blank item in the List. Count changes {initialCount} → {initialCount - 1}; Capacity stays {capacityBefore}.",
                    cancellationToken);

                // We already know the exact index from traversal. RemoveAt avoids List.Remove(element),
                // which would perform a second hidden linear search before deleting.
                MutableItems.RemoveAt(matchedIndex);
                capacityAfter = MutableItems.Capacity;
                NotifyChanged();

                await NextStepAsync(
                    $"After delete: Count is {MutableItems.Count} and Capacity is {capacityAfter}. The List no longer keeps the removed reference. If nothing else points to that object, .NET can clean it up later.",
                    cancellationToken);

                await NextStepAsync(
                    $"Delete complete: {comparisons} check(s) and {shiftedElements} reference move(s). Search part: {currentRunComplexity}. Whole List-backed delete: {fullOperationComplexity}.",
                    cancellationToken);
            }

            return new LinearTraversalResult(
                StructureName: structureName,
                Operation: operation,
                Criterion: criterion,
                Found: true,
                ElementId: matchedElement.Id,
                ElementValue: matchedElement.Value,
                Comparisons: comparisons,
                InitialCount: initialCount,
                TraversalDirection: traversalDirection,
                CurrentRunComplexity: currentRunComplexity)
            {
                MatchedIndex = matchedIndex,
                ShiftedElements = shiftedElements,
                CapacityBefore = capacityBefore,
                CapacityAfter = capacityAfter,
                FullOperationComplexity = fullOperationComplexity
            };
        }, cancellationToken);
    }


    private static string GetFullDeleteRunComplexity(
        int comparisons,
        int shiftedElements,
        int initialCount)
    {
        var work = comparisons + shiftedElements;

        if (initialCount <= 1 || work <= 1)
        {
            return "Θ(1)";
        }

        if (work >= initialCount)
        {
            return "Θ(n)";
        }

        return "Θ(k)";
    }

    protected void NormalizeVisualStates()
    {
        foreach (var item in _items)
        {
            item.VisualState = LinearElementVisualState.Normal;
        }
    }

    protected void NotifyChanged() => Changed?.Invoke();

    private static string GetCurrentRunComplexity(int comparisons, int initialCount, bool found)
    {
        if (initialCount == 0 || comparisons <= 1)
        {
            return "Θ(1)";
        }

        if (!found || comparisons >= initialCount)
        {
            return "Θ(n)";
        }

        return "Θ(k)";
    }
}

using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Linear.Queue;

/// <summary>
/// Owns queue data and orchestrates its educational simulation steps.
/// Rendering remains the responsibility of the Client project.
/// </summary>
public sealed class QueueSimulation : LinearStructureSimulationBase
{
    public QueueSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public Task EnqueueAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var element = new LinearElement(value, LinearElementVisualState.Adding);
            MutableItems.Add(element);
            NotifyChanged();

            await NextStepAsync(
                $"Adding {value} at the rear of the queue.",
                cancellationToken);

            element.VisualState = LinearElementVisualState.PointerTarget;
            NotifyChanged();

            await NextStepAsync(
                $"Rear now points to {value}.",
                cancellationToken);
        }, cancellationToken);

    public Task<int?> DequeueAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync<int?>(async () =>
        {
            if (MutableItems.Count == 0)
            {
                await NextStepAsync(
                    "The queue is empty. There is no element to dequeue.",
                    cancellationToken);
                return null;
            }

            var front = MutableItems[0];
            front.VisualState = LinearElementVisualState.Removing;
            NotifyChanged();

            await NextStepAsync(
                $"Removing {front.Value} from the front of the queue.",
                cancellationToken);

            MutableItems.RemoveAt(0);

            if (MutableItems.Count == 0)
            {
                NotifyChanged();
                await NextStepAsync("The queue is now empty.", cancellationToken);
            }
            else
            {
                var newFront = MutableItems[0];
                newFront.VisualState = LinearElementVisualState.PointerTarget;
                NotifyChanged();

                await NextStepAsync(
                    $"Front moves to {newFront.Value}.",
                    cancellationToken);
            }

            return front.Value;
        }, cancellationToken);

    public Task<LinearTraversalResult> FindByIdAsync(
        string normalizedId,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Queue",
            "FRONT → rear",
            reverse: false,
            LinearTraversalOperation.Search,
            LinearLookupCriterion.Id,
            FormatIdTarget(normalizedId),
            item => MatchesId(item, normalizedId),
            cancellationToken);

    public Task<LinearTraversalResult> FindByValueAsync(
        int value,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Queue",
            "FRONT → rear",
            reverse: false,
            LinearTraversalOperation.Search,
            LinearLookupCriterion.Value,
            $"value {value}",
            item => item.Value == value,
            cancellationToken);

    public Task<LinearTraversalResult> DeleteByIdAsync(
        string normalizedId,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Queue",
            "FRONT → rear",
            reverse: false,
            LinearTraversalOperation.Delete,
            LinearLookupCriterion.Id,
            FormatIdTarget(normalizedId),
            item => MatchesId(item, normalizedId),
            cancellationToken);

    public Task<LinearTraversalResult> DeleteByValueAsync(
        int value,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Queue",
            "FRONT → rear",
            reverse: false,
            LinearTraversalOperation.Delete,
            LinearLookupCriterion.Value,
            $"value {value}",
            item => item.Value == value,
            cancellationToken);

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            if (MutableItems.Count == 0)
            {
                await NextStepAsync("The queue is already empty.", cancellationToken);
                return;
            }

            foreach (var item in MutableItems)
            {
                item.VisualState = LinearElementVisualState.Removing;
            }

            NotifyChanged();

            await NextStepAsync(
                $"Clearing all {MutableItems.Count} elements from the queue.",
                cancellationToken);

            MutableItems.Clear();
            NotifyChanged();

            await NextStepAsync("The queue is empty.", cancellationToken);
        }, cancellationToken);

    private static bool MatchesId(LinearElement item, string normalizedId) =>
        item.Id.ToString("N").StartsWith(normalizedId, StringComparison.OrdinalIgnoreCase);

    private static string FormatIdTarget(string normalizedId) => $"ID #{normalizedId}";
}

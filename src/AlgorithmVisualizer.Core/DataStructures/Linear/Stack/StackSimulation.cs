using AlgorithmVisualizer.Core.DataStructures.Linear;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Linear.Stack;

/// <summary>
/// Owns stack data and orchestrates its educational simulation steps.
/// Rendering remains the responsibility of the Client project.
/// </summary>
public sealed class StackSimulation : LinearStructureSimulationBase
{
    public StackSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public Task PushAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var element = new LinearElement(value, LinearElementVisualState.Adding);
            MutableItems.Add(element);
            NotifyChanged();

            await NextStepAsync(
                $"Adding {value} to the top of the stack.",
                cancellationToken);

            element.VisualState = LinearElementVisualState.PointerTarget;
            NotifyChanged();

            await NextStepAsync(
                $"Top now points to {value}.",
                cancellationToken);
        }, cancellationToken);

    public Task<int?> PopAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync<int?>(async () =>
        {
            if (MutableItems.Count == 0)
            {
                await NextStepAsync(
                    "The stack is empty. There is no element to pop.",
                    cancellationToken);
                return null;
            }

            var top = MutableItems[^1];
            top.VisualState = LinearElementVisualState.Removing;
            NotifyChanged();

            await NextStepAsync(
                $"Removing {top.Value} from the top of the stack.",
                cancellationToken);

            MutableItems.RemoveAt(MutableItems.Count - 1);

            if (MutableItems.Count == 0)
            {
                NotifyChanged();
                await NextStepAsync("The stack is now empty.", cancellationToken);
            }
            else
            {
                var newTop = MutableItems[^1];
                newTop.VisualState = LinearElementVisualState.PointerTarget;
                NotifyChanged();

                await NextStepAsync(
                    $"Top moves to {newTop.Value}.",
                    cancellationToken);
            }

            return top.Value;
        }, cancellationToken);

    public Task<LinearTraversalResult> FindByIdAsync(
        string normalizedId,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Stack",
            "TOP → bottom",
            reverse: true,
            LinearTraversalOperation.Search,
            LinearLookupCriterion.Id,
            FormatIdTarget(normalizedId),
            item => MatchesId(item, normalizedId),
            cancellationToken);

    public Task<LinearTraversalResult> FindByValueAsync(
        int value,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Stack",
            "TOP → bottom",
            reverse: true,
            LinearTraversalOperation.Search,
            LinearLookupCriterion.Value,
            $"value {value}",
            item => item.Value == value,
            cancellationToken);

    public Task<LinearTraversalResult> DeleteByIdAsync(
        string normalizedId,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Stack",
            "TOP → bottom",
            reverse: true,
            LinearTraversalOperation.Delete,
            LinearLookupCriterion.Id,
            FormatIdTarget(normalizedId),
            item => MatchesId(item, normalizedId),
            cancellationToken);

    public Task<LinearTraversalResult> DeleteByValueAsync(
        int value,
        CancellationToken cancellationToken = default) =>
        ExecuteTraversalAsync(
            "Stack",
            "TOP → bottom",
            reverse: true,
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
                await NextStepAsync("The stack is already empty.", cancellationToken);
                return;
            }

            foreach (var item in MutableItems)
            {
                item.VisualState = LinearElementVisualState.Removing;
            }

            NotifyChanged();

            await NextStepAsync(
                $"Clearing all {MutableItems.Count} elements from the stack.",
                cancellationToken);

            MutableItems.Clear();
            NotifyChanged();

            await NextStepAsync("The stack is empty.", cancellationToken);
        }, cancellationToken);

    private static bool MatchesId(LinearElement item, string normalizedId) =>
        item.Id.ToString("N").StartsWith(normalizedId, StringComparison.OrdinalIgnoreCase);

    private static string FormatIdTarget(string normalizedId) => $"ID #{normalizedId}";
}

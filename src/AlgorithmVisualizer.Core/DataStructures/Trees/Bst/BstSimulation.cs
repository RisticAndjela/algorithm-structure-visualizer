using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Trees.Bst;

/// <summary>
/// Binary Search Tree implemented from scratch with explicit node references.
/// No built-in tree, sorted collection, dictionary, list, or binary-search helper implements the taught algorithm.
/// Duplicate keys are rejected so the invariant is strict: left &lt; node &lt; right.
/// </summary>
public sealed class BstSimulation : SimulationAlgorithmBase
{
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private BstNode? _root;
    private int _count;

    public BstSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public int Count => _count;
    public int Height => GetHeight(_root);
    public string? RootDisplayId => _root?.DisplayId;
    public BstNodeSnapshot? CreateSnapshot() => BstNodeSnapshot.Capture(_root);

    public event Action? Changed;

    public Task<BstOperationResult> InsertAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;

            if (_root is null)
            {
                _root = new BstNode(value, parent: null)
                {
                    VisualState = BstNodeVisualState.Adding
                };
                _count = 1;
                NotifyChanged();

                await NextStepAsync(
                    $"The tree is empty, so {value} becomes the root. No comparison is needed.",
                    cancellationToken);

                return new BstOperationResult(
                    BstOperationKind.Insert, value, true, false, _root.Id,
                    Comparisons: 0, SuccessorChecks: 0,
                    initialCount, _count, heightBefore, Height, BstDeleteCase.None);
            }

            var comparisons = 0;
            var current = _root;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                current.VisualState = value == current.Value
                    ? BstNodeVisualState.Matched
                    : BstNodeVisualState.Checking;
                NotifyChanged();

                if (value == current.Value)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: equal. This BST keeps unique keys, so the duplicate is not inserted.",
                        cancellationToken);

                    return new BstOperationResult(
                        BstOperationKind.Insert, value, false, true, current.Id,
                        comparisons, SuccessorChecks: 0,
                        initialCount, _count, heightBefore, Height, BstDeleteCase.None);
                }

                var goLeft = value < current.Value;
                var direction = goLeft ? "left" : "right";
                var next = goLeft ? current.Left : current.Right;

                await NextStepAsync(
                    $"Compare {value} with {current.Value} at #{current.DisplayId}: {value} is {(goLeft ? "smaller" : "larger")}, so follow the {direction} link.",
                    cancellationToken);

                current.VisualState = BstNodeVisualState.Visited;

                if (next is not null)
                {
                    current = next;
                    continue;
                }

                current.VisualState = BstNodeVisualState.PointerTarget;
                var inserted = new BstNode(value, current)
                {
                    VisualState = BstNodeVisualState.Adding
                };

                if (goLeft)
                {
                    current.Left = inserted;
                }
                else
                {
                    current.Right = inserted;
                }

                _count++;
                NotifyChanged();

                await NextStepAsync(
                    $"The {direction} link of {current.Value} is empty. Connect new node {value} (#{inserted.DisplayId}) there. The BST ordering rule is preserved.",
                    cancellationToken);

                return new BstOperationResult(
                    BstOperationKind.Insert, value, true, false, inserted.Id,
                    comparisons, SuccessorChecks: 0,
                    initialCount, _count, heightBefore, Height, BstDeleteCase.None);
            }
        }, cancellationToken);

    public Task<BstOperationResult> SearchAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;

            if (_root is null)
            {
                await NextStepAsync(
                    "The tree is empty, so the target cannot be present. This needs only one empty-root check.",
                    cancellationToken);

                return new BstOperationResult(
                    BstOperationKind.Search, value, false, false, null,
                    Comparisons: 0, SuccessorChecks: 0,
                    initialCount, _count, heightBefore, Height, BstDeleteCase.None);
            }

            var comparisons = 0;
            var current = _root;

            while (current is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var isMatch = value == current.Value;
                current.VisualState = isMatch
                    ? BstNodeVisualState.Matched
                    : BstNodeVisualState.Checking;
                NotifyChanged();

                if (isMatch)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: MATCH. Search stops after {comparisons} comparison(s).",
                        cancellationToken);

                    return new BstOperationResult(
                        BstOperationKind.Search, value, true, false, current.Id,
                        comparisons, SuccessorChecks: 0,
                        initialCount, _count, heightBefore, Height, BstDeleteCase.None);
                }

                var goLeft = value < current.Value;
                await NextStepAsync(
                    $"Compare {value} with {current.Value} at #{current.DisplayId}: go {(goLeft ? "left because the target is smaller" : "right because the target is larger")}.",
                    cancellationToken);

                current.VisualState = BstNodeVisualState.Visited;
                current = goLeft ? current.Left : current.Right;
            }

            await NextStepAsync(
                $"The required child link is empty. {value} is not in the tree after {comparisons} comparison(s).",
                cancellationToken);

            return new BstOperationResult(
                BstOperationKind.Search, value, false, false, null,
                comparisons, SuccessorChecks: 0,
                initialCount, _count, heightBefore, Height, BstDeleteCase.None);
        }, cancellationToken);

    public Task<BstOperationResult> DeleteAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;

            if (_root is null)
            {
                await NextStepAsync(
                    "The tree is empty, so there is no node to delete.",
                    cancellationToken);

                return new BstOperationResult(
                    BstOperationKind.Delete, value, false, false, null,
                    Comparisons: 0, SuccessorChecks: 0,
                    initialCount, _count, heightBefore, Height, BstDeleteCase.None);
            }

            var comparisons = 0;
            var target = _root;

            while (target is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var isMatch = value == target.Value;
                target.VisualState = isMatch
                    ? BstNodeVisualState.Matched
                    : BstNodeVisualState.Checking;
                NotifyChanged();

                if (isMatch)
                {
                    await NextStepAsync(
                        $"Found {value} at #{target.DisplayId} after {comparisons} comparison(s). Now choose the correct BST delete case.",
                        cancellationToken);
                    break;
                }

                var goLeft = value < target.Value;
                await NextStepAsync(
                    $"Delete search: {value} is {(goLeft ? "smaller" : "larger")} than {target.Value}, so follow the {(goLeft ? "left" : "right")} link.",
                    cancellationToken);

                target.VisualState = BstNodeVisualState.Visited;
                target = goLeft ? target.Left : target.Right;
            }

            if (target is null)
            {
                await NextStepAsync(
                    $"The search path ended at an empty link. {value} is not present, so the tree is unchanged.",
                    cancellationToken);

                return new BstOperationResult(
                    BstOperationKind.Delete, value, false, false, null,
                    comparisons, SuccessorChecks: 0,
                    initialCount, _count, heightBefore, Height, BstDeleteCase.None);
            }

            var targetId = target.Id;
            target.VisualState = BstNodeVisualState.Removing;
            NotifyChanged();

            if (target.Left is null && target.Right is null)
            {
                await NextStepAsync(
                    $"Delete case: leaf. #{target.DisplayId} has no children, so its parent link can simply become empty.",
                    cancellationToken);

                Transplant(target, replacement: null);
                _count--;
                NotifyChanged();

                await NextStepAsync(
                    $"Leaf {value} is disconnected. Count is now {_count}; every remaining node still satisfies left < node < right.",
                    cancellationToken);

                return BuildDeleteResult(value, targetId, comparisons, 0, initialCount, heightBefore, BstDeleteCase.Leaf);
            }

            if (target.Left is null || target.Right is null)
            {
                var child = target.Left ?? target.Right!;
                child.VisualState = BstNodeVisualState.Replacement;
                NotifyChanged();

                await NextStepAsync(
                    $"Delete case: one child. Node {value} is replaced by its child {child.Value} (#{child.DisplayId}); the subtree stays connected.",
                    cancellationToken);

                Transplant(target, child);
                _count--;
                NotifyChanged();

                await NextStepAsync(
                    $"The parent now points directly to {child.Value}. Count is {_count}; no subtree values had to be re-sorted.",
                    cancellationToken);

                return BuildDeleteResult(value, targetId, comparisons, 0, initialCount, heightBefore, BstDeleteCase.OneChild);
            }

            await NextStepAsync(
                $"Delete case: two children. We will replace {value} with its in-order successor: the smallest node in its right subtree.",
                cancellationToken);

            var successorChecks = 0;
            var successor = target.Right;

            while (successor.Left is not null)
            {
                successorChecks++;
                successor.VisualState = BstNodeVisualState.Checking;
                NotifyChanged();

                await NextStepAsync(
                    $"Successor search {successorChecks}: {successor.Value} still has a left child, so move left to find a smaller value.",
                    cancellationToken);

                successor.VisualState = BstNodeVisualState.Visited;
                successor = successor.Left;
            }

            successorChecks++;
            successor.VisualState = BstNodeVisualState.Replacement;
            NotifyChanged();

            await NextStepAsync(
                $"Successor found: {successor.Value} (#{successor.DisplayId}). It is the leftmost node of the right subtree, so it is the next larger key.",
                cancellationToken);

            if (!ReferenceEquals(successor.Parent, target))
            {
                var successorRight = successor.Right;

                await NextStepAsync(
                    successorRight is null
                        ? $"Detach successor {successor.Value} from its old parent. Its old parent will have an empty left link."
                        : $"Detach successor {successor.Value} from its old parent and connect successor's right child {successorRight.Value} in its place.",
                    cancellationToken);

                Transplant(successor, successorRight);
                successor.Right = target.Right;
                successor.Right.Parent = successor;
            }

            await NextStepAsync(
                $"Move successor {successor.Value} into {value}'s position and keep both ordered subtrees attached to it.",
                cancellationToken);

            Transplant(target, successor);
            successor.Left = target.Left;
            successor.Left.Parent = successor;
            _count--;
            NotifyChanged();

            await NextStepAsync(
                $"Two-child delete complete. {successor.Value} now occupies the removed node's position. Count is {_count} and the BST ordering rule still holds.",
                cancellationToken);

            return BuildDeleteResult(value, targetId, comparisons, successorChecks, initialCount, heightBefore, BstDeleteCase.TwoChildren);
        }, cancellationToken);

    /// <summary>
    /// Lab utility: drops the root reference so the learner can start another example.
    /// This is not presented as a BST search/insert/delete algorithm.
    /// </summary>
    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            if (_root is null)
            {
                await NextStepAsync("The BST is already empty.", cancellationToken);
                return;
            }

            await NextStepAsync(
                $"Reset the lab by removing the root reference. The {_count} node object(s) become unreachable from this tree.",
                cancellationToken);

            _root = null;
            _count = 0;
            NotifyChanged();

            await NextStepAsync(
                "The BST is empty. In .NET, unreachable node objects can be reclaimed later by garbage collection.",
                cancellationToken);
        }, cancellationToken);

    private BstOperationResult BuildDeleteResult(
        int value,
        Guid targetId,
        int comparisons,
        int successorChecks,
        int initialCount,
        int heightBefore,
        BstDeleteCase deleteCase) =>
        new(
            BstOperationKind.Delete,
            value,
            Succeeded: true,
            DuplicateRejected: false,
            AffectedNodeId: targetId,
            comparisons,
            successorChecks,
            initialCount,
            _count,
            heightBefore,
            Height,
            deleteCase);

    private async Task<TResult> ExecuteExclusiveAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates(_root);
            NotifyChanged();
            return await operation();
        }
        finally
        {
            NormalizeVisualStates(_root);
            NotifyChanged();
            _operationGate.Release();
        }
    }

    private async Task ExecuteExclusiveAsync(
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates(_root);
            NotifyChanged();
            await operation();
        }
        finally
        {
            NormalizeVisualStates(_root);
            NotifyChanged();
            _operationGate.Release();
        }
    }

    private void Transplant(BstNode node, BstNode? replacement)
    {
        if (node.Parent is null)
        {
            _root = replacement;
        }
        else if (ReferenceEquals(node, node.Parent.Left))
        {
            node.Parent.Left = replacement;
        }
        else
        {
            node.Parent.Right = replacement;
        }

        if (replacement is not null)
        {
            replacement.Parent = node.Parent;
        }
    }

    private static int GetHeight(BstNode? node)
    {
        if (node is null)
        {
            return 0;
        }

        var leftHeight = GetHeight(node.Left);
        var rightHeight = GetHeight(node.Right);
        return 1 + Math.Max(leftHeight, rightHeight);
    }

    private static void NormalizeVisualStates(BstNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.VisualState = BstNodeVisualState.Normal;
        NormalizeVisualStates(node.Left);
        NormalizeVisualStates(node.Right);
    }

    private void NotifyChanged() => Changed?.Invoke();
}

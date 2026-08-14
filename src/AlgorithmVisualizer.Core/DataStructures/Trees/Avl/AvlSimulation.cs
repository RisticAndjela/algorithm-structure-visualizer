using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Trees.Avl;

/// <summary>
/// AVL tree implemented from scratch with explicit node references, cached heights,
/// balance-factor checks, and manual left/right rotations.
/// No built-in tree, sorted collection, or balancing helper implements the taught algorithm.
/// Duplicate keys are rejected so the BST invariant remains strict.
/// </summary>
public sealed class AvlSimulation : SimulationAlgorithmBase
{
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private AvlNode? _root;
    private int _count;

    public AvlSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public int Count => _count;
    public int Height => GetHeight(_root);
    public string? RootDisplayId => _root?.DisplayId;
    public AvlNodeSnapshot? CreateSnapshot() => AvlNodeSnapshot.Capture(_root);

    public event Action? Changed;

    public Task<AvlOperationResult> InsertAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var stats = new RebalanceStats();

            if (_root is null)
            {
                _root = new AvlNode(value, parent: null)
                {
                    VisualState = AvlNodeVisualState.Adding
                };
                _count = 1;
                NotifyChanged();

                await NextStepAsync(
                    $"The tree is empty, so {value} becomes the root with height 1 and balance factor 0.",
                    cancellationToken);

                return BuildResult(
                    AvlOperationKind.Insert, value, succeeded: true, duplicateRejected: false, _root.Id,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, AvlDeleteCase.None);
            }

            var comparisons = 0;
            var current = _root;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var isDuplicate = value == current.Value;
                current.VisualState = isDuplicate ? AvlNodeVisualState.Matched : AvlNodeVisualState.Checking;
                NotifyChanged();

                if (isDuplicate)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: equal. This AVL tree keeps unique keys, so the duplicate is not inserted.",
                        cancellationToken);

                    return BuildResult(
                        AvlOperationKind.Insert, value, succeeded: false, duplicateRejected: true, current.Id,
                        comparisons, successorChecks: 0, stats,
                        initialCount, heightBefore, AvlDeleteCase.None);
                }

                var goLeft = value < current.Value;
                var direction = goLeft ? "left" : "right";
                var next = goLeft ? current.Left : current.Right;

                await NextStepAsync(
                    $"BST rule first: {value} is {(goLeft ? "smaller" : "larger")} than {current.Value}, so follow the {direction} link.",
                    cancellationToken);

                current.VisualState = AvlNodeVisualState.Visited;

                if (next is not null)
                {
                    current = next;
                    continue;
                }

                current.VisualState = AvlNodeVisualState.PointerTarget;
                var inserted = new AvlNode(value, current)
                {
                    VisualState = AvlNodeVisualState.Adding
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
                    $"The {direction} link of {current.Value} is empty. Connect new node {value} (#{inserted.DisplayId}) there, then walk back toward the root to repair heights and balance.",
                    cancellationToken);

                await RebalanceFromAsync(current, stats, cancellationToken);

                return BuildResult(
                    AvlOperationKind.Insert, value, succeeded: true, duplicateRejected: false, inserted.Id,
                    comparisons, successorChecks: 0, stats,
                    initialCount, heightBefore, AvlDeleteCase.None);
            }
        }, cancellationToken);

    public Task<AvlOperationResult> SearchAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var comparisons = 0;
            var current = _root;
            var stats = new RebalanceStats();

            if (current is null)
            {
                await NextStepAsync(
                    "The AVL tree is empty, so the target cannot be present.",
                    cancellationToken);

                return BuildResult(
                    AvlOperationKind.Search, value, succeeded: false, duplicateRejected: false, null,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, AvlDeleteCase.None);
            }

            while (current is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var isMatch = value == current.Value;
                current.VisualState = isMatch ? AvlNodeVisualState.Matched : AvlNodeVisualState.Checking;
                NotifyChanged();

                if (isMatch)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: MATCH. Search stops after {comparisons} comparison(s).",
                        cancellationToken);

                    return BuildResult(
                        AvlOperationKind.Search, value, succeeded: true, duplicateRejected: false, current.Id,
                        comparisons, successorChecks: 0, stats,
                        initialCount, heightBefore, AvlDeleteCase.None);
                }

                var goLeft = value < current.Value;
                await NextStepAsync(
                    $"Compare {value} with {current.Value}: go {(goLeft ? "left because the target is smaller" : "right because the target is larger")}. AVL search uses the same ordering rule as BST search.",
                    cancellationToken);

                current.VisualState = AvlNodeVisualState.Visited;
                current = goLeft ? current.Left : current.Right;
            }

            await NextStepAsync(
                $"The required child link is empty. {value} is not in the AVL tree after {comparisons} comparison(s).",
                cancellationToken);

            return BuildResult(
                AvlOperationKind.Search, value, succeeded: false, duplicateRejected: false, null,
                comparisons, successorChecks: 0, stats,
                initialCount, heightBefore, AvlDeleteCase.None);
        }, cancellationToken);

    public Task<AvlOperationResult> DeleteAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var comparisons = 0;
            var successorChecks = 0;
            var stats = new RebalanceStats();
            var target = _root;

            if (target is null)
            {
                await NextStepAsync("The AVL tree is empty, so there is no node to delete.", cancellationToken);
                return BuildResult(
                    AvlOperationKind.Delete, value, succeeded: false, duplicateRejected: false, null,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, AvlDeleteCase.None);
            }

            while (target is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var isMatch = value == target.Value;
                target.VisualState = isMatch ? AvlNodeVisualState.Matched : AvlNodeVisualState.Checking;
                NotifyChanged();

                if (isMatch)
                {
                    await NextStepAsync(
                        $"Found {value} at #{target.DisplayId} after {comparisons} comparison(s). First perform the BST delete case, then rebalance toward the root.",
                        cancellationToken);
                    break;
                }

                var goLeft = value < target.Value;
                await NextStepAsync(
                    $"Delete search: {value} is {(goLeft ? "smaller" : "larger")} than {target.Value}, so follow the {(goLeft ? "left" : "right")} link.",
                    cancellationToken);

                target.VisualState = AvlNodeVisualState.Visited;
                target = goLeft ? target.Left : target.Right;
            }

            if (target is null)
            {
                await NextStepAsync(
                    $"The search path ended at an empty link. {value} is not present, so no deletion or rebalancing is needed.",
                    cancellationToken);

                return BuildResult(
                    AvlOperationKind.Delete, value, succeeded: false, duplicateRejected: false, null,
                    comparisons, successorChecks, stats,
                    initialCount, heightBefore, AvlDeleteCase.None);
            }

            var targetId = target.Id;
            target.VisualState = AvlNodeVisualState.Removing;
            NotifyChanged();

            if (target.Left is null && target.Right is null)
            {
                var leafRebalanceStart = target.Parent;
                await NextStepAsync(
                    $"{value} is a leaf. Disconnect its single parent link, then re-check balance from that parent upward.",
                    cancellationToken);

                Transplant(target, replacement: null);
                _count--;
                NotifyChanged();
                await RebalanceFromAsync(leafRebalanceStart, stats, cancellationToken);

                return BuildResult(
                    AvlOperationKind.Delete, value, succeeded: true, duplicateRejected: false, targetId,
                    comparisons, successorChecks, stats,
                    initialCount, heightBefore, AvlDeleteCase.Leaf);
            }

            if (target.Left is null || target.Right is null)
            {
                var child = target.Left ?? target.Right!;
                var oneChildRebalanceStart = target.Parent ?? child;
                child.VisualState = AvlNodeVisualState.Replacement;
                NotifyChanged();

                await NextStepAsync(
                    $"{value} has one child. Redirect the surrounding reference to child {child.Value} (#{child.DisplayId}), then repair heights and balance upward.",
                    cancellationToken);

                Transplant(target, child);
                _count--;
                NotifyChanged();
                await RebalanceFromAsync(oneChildRebalanceStart, stats, cancellationToken);

                return BuildResult(
                    AvlOperationKind.Delete, value, succeeded: true, duplicateRejected: false, targetId,
                    comparisons, successorChecks, stats,
                    initialCount, heightBefore, AvlDeleteCase.OneChild);
            }

            var successor = target.Right;
            successor.VisualState = AvlNodeVisualState.Checking;
            NotifyChanged();
            successorChecks++;

            await NextStepAsync(
                $"{value} has two children. Start at the right child {successor.Value} and follow left links to find the in-order successor.",
                cancellationToken);

            while (successor.Left is not null)
            {
                successor.VisualState = AvlNodeVisualState.Visited;
                successor = successor.Left;
                successorChecks++;
                successor.VisualState = AvlNodeVisualState.Checking;
                NotifyChanged();

                await NextStepAsync(
                    $"Successor search moves left to {successor.Value} (#{successor.DisplayId}). The smallest value in the right subtree is the replacement.",
                    cancellationToken);
            }

            successor.VisualState = AvlNodeVisualState.Replacement;
            NotifyChanged();
            await NextStepAsync(
                $"Successor found: {successor.Value} (#{successor.DisplayId}). Preserve this node object's identity and rewire references around it.",
                cancellationToken);

            AvlNode rebalanceStart;

            if (!ReferenceEquals(successor.Parent, target))
            {
                var successorOldParent = successor.Parent!;
                var successorRight = successor.Right;

                await NextStepAsync(
                    $"Successor {successor.Value} is deeper than the target. First replace its old position with its right child{(successorRight is null ? " (none)" : $" {successorRight.Value}")}.",
                    cancellationToken);

                Transplant(successor, successorRight);
                successor.Right = target.Right;
                successor.Right.Parent = successor;
                rebalanceStart = successorOldParent;
            }
            else
            {
                rebalanceStart = successor;
            }

            await NextStepAsync(
                $"Move successor {successor.Value} into {value}'s position and attach the target's left subtree. BST ordering is restored before AVL balancing begins.",
                cancellationToken);

            Transplant(target, successor);
            successor.Left = target.Left;
            successor.Left.Parent = successor;
            UpdateHeight(successor);
            _count--;
            NotifyChanged();

            await RebalanceFromAsync(rebalanceStart, stats, cancellationToken);

            return BuildResult(
                AvlOperationKind.Delete, value, succeeded: true, duplicateRejected: false, targetId,
                comparisons, successorChecks, stats,
                initialCount, heightBefore, AvlDeleteCase.TwoChildren);
        }, cancellationToken);

    /// <summary>
    /// Lab utility: removes the root reference so the learner can start another AVL example.
    /// </summary>
    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            if (_root is null)
            {
                await NextStepAsync("The AVL tree is already empty.", cancellationToken);
                return;
            }

            await NextStepAsync(
                $"Reset the lab by removing the root reference. The {_count} node object(s) become unreachable from this tree.",
                cancellationToken);

            _root = null;
            _count = 0;
            NotifyChanged();

            await NextStepAsync(
                "The AVL tree is empty. In .NET, unreachable node objects can be reclaimed later by garbage collection.",
                cancellationToken);
        }, cancellationToken);

    private async Task RebalanceFromAsync(
        AvlNode? start,
        RebalanceStats stats,
        CancellationToken cancellationToken)
    {
        var current = start;

        while (current is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateHeight(current);
            stats.Checks++;
            var balance = GetBalance(current);
            current.VisualState = Math.Abs(balance) > 1
                ? AvlNodeVisualState.Unbalanced
                : AvlNodeVisualState.Checking;
            NotifyChanged();

            await NextStepAsync(
                $"Recalculate node {current.Value}: height = {current.Height}, balance = {balance}. " +
                (Math.Abs(balance) <= 1
                    ? "It is still valid because AVL allows only -1, 0, or +1."
                    : "Its balance magnitude is greater than 1, so a rotation is required."),
                cancellationToken);

            if (balance > 1)
            {
                var leftChild = current.Left!;
                var childBalance = GetBalance(leftChild);

                if (childBalance >= 0)
                {
                    RecordRotationCase(stats, AvlRotationCase.LL, primitiveRotations: 1);
                    current.VisualState = AvlNodeVisualState.Unbalanced;
                    leftChild.VisualState = AvlNodeVisualState.RotationPivot;
                    NotifyChanged();

                    await NextStepAsync(
                        $"LL case at {current.Value}: the node is left-heavy and child {leftChild.Value} is not right-heavy. One right rotation repairs the subtree.",
                        cancellationToken);

                    var newSubtreeRoot = await RotateRightAsync(
                        current,
                        $"Rotate right around {current.Value}. Child {leftChild.Value} moves up while BST ordering stays intact.",
                        cancellationToken);
                    current = newSubtreeRoot.Parent;
                    continue;
                }

                RecordRotationCase(stats, AvlRotationCase.LR, primitiveRotations: 2);
                var middle = leftChild.Right!;
                current.VisualState = AvlNodeVisualState.Unbalanced;
                leftChild.VisualState = AvlNodeVisualState.RotationPivot;
                middle.VisualState = AvlNodeVisualState.Rotating;
                NotifyChanged();

                await NextStepAsync(
                    $"LR case at {current.Value}: the heavy path goes left, then right through {middle.Value}. Repair it with two rotations.",
                    cancellationToken);

                await RotateLeftAsync(
                    leftChild,
                    $"LR step 1: rotate left around child {leftChild.Value} so middle node {middle.Value} rises.",
                    cancellationToken);
                var lrRoot = await RotateRightAsync(
                    current,
                    $"LR step 2: rotate right around {current.Value}. {middle.Value} becomes the balanced subtree root.",
                    cancellationToken);
                current = lrRoot.Parent;
                continue;
            }

            if (balance < -1)
            {
                var rightChild = current.Right!;
                var childBalance = GetBalance(rightChild);

                if (childBalance <= 0)
                {
                    RecordRotationCase(stats, AvlRotationCase.RR, primitiveRotations: 1);
                    current.VisualState = AvlNodeVisualState.Unbalanced;
                    rightChild.VisualState = AvlNodeVisualState.RotationPivot;
                    NotifyChanged();

                    await NextStepAsync(
                        $"RR case at {current.Value}: the node is right-heavy and child {rightChild.Value} is not left-heavy. One left rotation repairs the subtree.",
                        cancellationToken);

                    var newSubtreeRoot = await RotateLeftAsync(
                        current,
                        $"Rotate left around {current.Value}. Child {rightChild.Value} moves up while BST ordering stays intact.",
                        cancellationToken);
                    current = newSubtreeRoot.Parent;
                    continue;
                }

                RecordRotationCase(stats, AvlRotationCase.RL, primitiveRotations: 2);
                var middle = rightChild.Left!;
                current.VisualState = AvlNodeVisualState.Unbalanced;
                rightChild.VisualState = AvlNodeVisualState.RotationPivot;
                middle.VisualState = AvlNodeVisualState.Rotating;
                NotifyChanged();

                await NextStepAsync(
                    $"RL case at {current.Value}: the heavy path goes right, then left through {middle.Value}. Repair it with two rotations.",
                    cancellationToken);

                await RotateRightAsync(
                    rightChild,
                    $"RL step 1: rotate right around child {rightChild.Value} so middle node {middle.Value} rises.",
                    cancellationToken);
                var rlRoot = await RotateLeftAsync(
                    current,
                    $"RL step 2: rotate left around {current.Value}. {middle.Value} becomes the balanced subtree root.",
                    cancellationToken);
                current = rlRoot.Parent;
                continue;
            }

            current.VisualState = AvlNodeVisualState.Balanced;
            NotifyChanged();
            await NextStepAsync(
                $"Node {current.Value} needs no rotation. Continue one parent upward because that ancestor's height may have changed.",
                cancellationToken);
            current = current.Parent;
        }
    }

    private async Task<AvlNode> RotateLeftAsync(
        AvlNode pivot,
        string stepText,
        CancellationToken cancellationToken)
    {
        var promoted = pivot.Right ?? throw new InvalidOperationException("A left rotation requires a right child.");
        pivot.VisualState = AvlNodeVisualState.Rotating;
        promoted.VisualState = AvlNodeVisualState.RotationPivot;
        NotifyChanged();

        await NextStepAsync(stepText, cancellationToken);

        var newRoot = RotateLeft(pivot);
        pivot.VisualState = AvlNodeVisualState.Balanced;
        newRoot.VisualState = AvlNodeVisualState.Balanced;
        NotifyChanged();

        await NextStepAsync(
            $"Left rotation complete: {newRoot.Value} is now the subtree root. Heights are {pivot.Value}:{pivot.Height} and {newRoot.Value}:{newRoot.Height}.",
            cancellationToken);

        return newRoot;
    }

    private async Task<AvlNode> RotateRightAsync(
        AvlNode pivot,
        string stepText,
        CancellationToken cancellationToken)
    {
        var promoted = pivot.Left ?? throw new InvalidOperationException("A right rotation requires a left child.");
        pivot.VisualState = AvlNodeVisualState.Rotating;
        promoted.VisualState = AvlNodeVisualState.RotationPivot;
        NotifyChanged();

        await NextStepAsync(stepText, cancellationToken);

        var newRoot = RotateRight(pivot);
        pivot.VisualState = AvlNodeVisualState.Balanced;
        newRoot.VisualState = AvlNodeVisualState.Balanced;
        NotifyChanged();

        await NextStepAsync(
            $"Right rotation complete: {newRoot.Value} is now the subtree root. Heights are {pivot.Value}:{pivot.Height} and {newRoot.Value}:{newRoot.Height}.",
            cancellationToken);

        return newRoot;
    }

    private AvlNode RotateLeft(AvlNode pivot)
    {
        var promoted = pivot.Right ?? throw new InvalidOperationException("A left rotation requires a right child.");
        var transferredSubtree = promoted.Left;
        var oldParent = pivot.Parent;

        ReplaceChildReference(oldParent, pivot, promoted);
        promoted.Parent = oldParent;
        promoted.Left = pivot;
        pivot.Parent = promoted;
        pivot.Right = transferredSubtree;

        if (transferredSubtree is not null)
        {
            transferredSubtree.Parent = pivot;
        }

        UpdateHeight(pivot);
        UpdateHeight(promoted);
        return promoted;
    }

    private AvlNode RotateRight(AvlNode pivot)
    {
        var promoted = pivot.Left ?? throw new InvalidOperationException("A right rotation requires a left child.");
        var transferredSubtree = promoted.Right;
        var oldParent = pivot.Parent;

        ReplaceChildReference(oldParent, pivot, promoted);
        promoted.Parent = oldParent;
        promoted.Right = pivot;
        pivot.Parent = promoted;
        pivot.Left = transferredSubtree;

        if (transferredSubtree is not null)
        {
            transferredSubtree.Parent = pivot;
        }

        UpdateHeight(pivot);
        UpdateHeight(promoted);
        return promoted;
    }

    private void ReplaceChildReference(AvlNode? parent, AvlNode oldChild, AvlNode newChild)
    {
        if (parent is null)
        {
            _root = newChild;
            return;
        }

        if (ReferenceEquals(parent.Left, oldChild))
        {
            parent.Left = newChild;
            return;
        }

        if (ReferenceEquals(parent.Right, oldChild))
        {
            parent.Right = newChild;
            return;
        }

        throw new InvalidOperationException("The pivot is not connected to its recorded parent.");
    }

    private void Transplant(AvlNode node, AvlNode? replacement)
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

    private AvlOperationResult BuildResult(
        AvlOperationKind operation,
        int requestedValue,
        bool succeeded,
        bool duplicateRejected,
        Guid? affectedNodeId,
        int comparisons,
        int successorChecks,
        RebalanceStats stats,
        int initialCount,
        int heightBefore,
        AvlDeleteCase deleteCase) =>
        new(
            operation,
            requestedValue,
            succeeded,
            duplicateRejected,
            affectedNodeId,
            comparisons,
            successorChecks,
            stats.Checks,
            stats.Rotations,
            stats.FirstCase,
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

    private static void RecordRotationCase(RebalanceStats stats, AvlRotationCase rotationCase, int primitiveRotations)
    {
        if (stats.FirstCase == AvlRotationCase.None)
        {
            stats.FirstCase = rotationCase;
        }

        stats.Rotations += primitiveRotations;
    }

    private static int GetHeight(AvlNode? node) => node?.Height ?? 0;

    private static int GetBalance(AvlNode node) => GetHeight(node.Left) - GetHeight(node.Right);

    private static void UpdateHeight(AvlNode node) =>
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

    private static void NormalizeVisualStates(AvlNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.VisualState = AvlNodeVisualState.Normal;
        NormalizeVisualStates(node.Left);
        NormalizeVisualStates(node.Right);
    }

    private void NotifyChanged() => Changed?.Invoke();

    private sealed class RebalanceStats
    {
        public int Checks { get; set; }
        public int Rotations { get; set; }
        public AvlRotationCase FirstCase { get; set; }
    }
}

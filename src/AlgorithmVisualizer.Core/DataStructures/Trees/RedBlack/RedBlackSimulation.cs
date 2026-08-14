using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;

/// <summary>
/// Red-Black Tree implemented from scratch with explicit node references, colors,
/// recoloring, and manual left/right rotations. Null references are treated as the
/// conceptual black NIL leaves from the standard Red-Black algorithm.
/// </summary>
public sealed class RedBlackSimulation : SimulationAlgorithmBase
{
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private RedBlackNode? _root;
    private int _count;

    public RedBlackSimulation(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
    }

    public int Count => _count;
    public int Height => GetHeight(_root);
    public int BlackHeight => GetBlackHeight(_root);
    public string? RootDisplayId => _root?.DisplayId;
    public RedBlackNodeSnapshot? CreateSnapshot() => RedBlackNodeSnapshot.Capture(_root);

    public event Action? Changed;

    public Task<RedBlackOperationResult> InsertAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var blackHeightBefore = BlackHeight;
            var stats = new FixupStats();

            if (_root is null)
            {
                _root = new RedBlackNode(value, parent: null)
                {
                    Color = RedBlackColor.Red,
                    VisualState = RedBlackNodeVisualState.Adding
                };
                _count = 1;
                NotifyChanged();

                await NextStepAsync(
                    $"The tree is empty, so {value} becomes the root. New Red-Black nodes start red before the invariant check.",
                    cancellationToken);

                Recolor(_root, RedBlackColor.Black, stats);
                RecordRepairCase(stats, RedBlackRepairCase.InsertRootBlack);
                _root.VisualState = RedBlackNodeVisualState.Fixed;
                NotifyChanged();

                await NextStepAsync(
                    $"Invariant repair: the root must be black, so recolor {value} black. All root-to-NIL paths now have the same black height.",
                    cancellationToken);

                return BuildResult(
                    RedBlackOperationKind.Insert, value, succeeded: true, duplicateRejected: false, _root.Id,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
            }

            var comparisons = 0;
            var current = _root;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var duplicate = value == current.Value;
                current.VisualState = duplicate
                    ? RedBlackNodeVisualState.Matched
                    : RedBlackNodeVisualState.Checking;
                NotifyChanged();

                if (duplicate)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: equal. This Red-Black Tree keeps unique keys, so the duplicate is not inserted.",
                        cancellationToken);

                    return BuildResult(
                        RedBlackOperationKind.Insert, value, succeeded: false, duplicateRejected: true, current.Id,
                        comparisons, successorChecks: 0, stats,
                        initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
                }

                var goLeft = value < current.Value;
                var direction = goLeft ? "left" : "right";
                var next = goLeft ? current.Left : current.Right;

                await NextStepAsync(
                    $"BST ordering still decides the path: {value} is {(goLeft ? "smaller" : "larger")} than {current.Value}, so follow the {direction} link.",
                    cancellationToken);

                current.VisualState = RedBlackNodeVisualState.Visited;

                if (next is not null)
                {
                    current = next;
                    continue;
                }

                current.VisualState = RedBlackNodeVisualState.PointerTarget;
                var inserted = new RedBlackNode(value, current)
                {
                    Color = RedBlackColor.Red,
                    VisualState = RedBlackNodeVisualState.Adding
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
                    $"The {direction} link is NIL. Connect new node {value} (#{inserted.DisplayId}) as RED. Red adds no black-height, but a red parent would create a red-red violation.",
                    cancellationToken);

                await InsertFixupAsync(inserted, stats, cancellationToken);

                return BuildResult(
                    RedBlackOperationKind.Insert, value, succeeded: true, duplicateRejected: false, inserted.Id,
                    comparisons, successorChecks: 0, stats,
                    initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
            }
        }, cancellationToken);

    public Task<RedBlackOperationResult> SearchAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var blackHeightBefore = BlackHeight;
            var comparisons = 0;
            var current = _root;
            var stats = new FixupStats();

            if (current is null)
            {
                await NextStepAsync("The Red-Black Tree is empty, so the target cannot be present.", cancellationToken);
                return BuildResult(
                    RedBlackOperationKind.Search, value, succeeded: false, duplicateRejected: false, null,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
            }

            while (current is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var match = value == current.Value;
                current.VisualState = match
                    ? RedBlackNodeVisualState.Matched
                    : RedBlackNodeVisualState.Checking;
                NotifyChanged();

                if (match)
                {
                    await NextStepAsync(
                        $"Compare {value} with {current.Value} at #{current.DisplayId}: MATCH. Search stops after {comparisons} comparison(s). Colors do not change the BST search path.",
                        cancellationToken);

                    return BuildResult(
                        RedBlackOperationKind.Search, value, succeeded: true, duplicateRejected: false, current.Id,
                        comparisons, successorChecks: 0, stats,
                        initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
                }

                var goLeft = value < current.Value;
                await NextStepAsync(
                    $"Compare {value} with {current.Value}: go {(goLeft ? "left" : "right")}. Red/black color controls balancing, not key ordering.",
                    cancellationToken);

                current.VisualState = RedBlackNodeVisualState.Visited;
                current = goLeft ? current.Left : current.Right;
            }

            await NextStepAsync(
                $"The required child link is NIL. {value} is not in the tree after {comparisons} comparison(s).",
                cancellationToken);

            return BuildResult(
                RedBlackOperationKind.Search, value, succeeded: false, duplicateRejected: false, null,
                comparisons, successorChecks: 0, stats,
                initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
        }, cancellationToken);

    public Task<RedBlackOperationResult> DeleteAsync(int value, CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            var initialCount = _count;
            var heightBefore = Height;
            var blackHeightBefore = BlackHeight;
            var comparisons = 0;
            var successorChecks = 0;
            var stats = new FixupStats();
            var target = _root;

            if (target is null)
            {
                await NextStepAsync("The Red-Black Tree is empty, so there is no node to delete.", cancellationToken);
                return BuildResult(
                    RedBlackOperationKind.Delete, value, succeeded: false, duplicateRejected: false, null,
                    comparisons: 0, successorChecks: 0, stats,
                    initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
            }

            while (target is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                comparisons++;
                var match = value == target.Value;
                target.VisualState = match
                    ? RedBlackNodeVisualState.Matched
                    : RedBlackNodeVisualState.Checking;
                NotifyChanged();

                if (match)
                {
                    await NextStepAsync(
                        $"Found {value} at #{target.DisplayId}. First perform the ordinary BST structural delete, then repair Red-Black color invariants only if a black node was removed from a path.",
                        cancellationToken);
                    break;
                }

                var goLeft = value < target.Value;
                await NextStepAsync(
                    $"Delete search: {value} is {(goLeft ? "smaller" : "larger")} than {target.Value}, so follow the {(goLeft ? "left" : "right")} link.",
                    cancellationToken);

                target.VisualState = RedBlackNodeVisualState.Visited;
                target = goLeft ? target.Left : target.Right;
            }

            if (target is null)
            {
                await NextStepAsync(
                    $"The search path ended at NIL. {value} is not present, so no links, colors, or black-height paths change.",
                    cancellationToken);

                return BuildResult(
                    RedBlackOperationKind.Delete, value, succeeded: false, duplicateRejected: false, null,
                    comparisons, successorChecks, stats,
                    initialCount, heightBefore, blackHeightBefore, RedBlackDeleteCase.None);
            }

            var targetId = target.Id;
            target.VisualState = RedBlackNodeVisualState.Removing;
            NotifyChanged();

            var moved = target;
            var removedPathColor = moved.Color;
            RedBlackNode? fixNode;
            RedBlackNode? fixParent;
            var fixNodeWasLeft = false;
            var deleteCase = RedBlackDeleteCase.None;

            if (target.Left is null)
            {
                deleteCase = target.Right is null ? RedBlackDeleteCase.Leaf : RedBlackDeleteCase.OneChild;
                fixNode = target.Right;
                fixParent = target.Parent;
                fixNodeWasLeft = target.Parent is not null && ReferenceEquals(target.Parent.Left, target);

                await NextStepAsync(
                    target.Right is null
                        ? $"{value} is a leaf. Replace its parent link with the conceptual black NIL leaf."
                        : $"{value} has one child. Redirect its parent link to child {target.Right.Value} (#{target.Right.DisplayId}).",
                    cancellationToken);

                Transplant(target, target.Right);
            }
            else if (target.Right is null)
            {
                deleteCase = RedBlackDeleteCase.OneChild;
                fixNode = target.Left;
                fixParent = target.Parent;
                fixNodeWasLeft = target.Parent is not null && ReferenceEquals(target.Parent.Left, target);

                await NextStepAsync(
                    $"{value} has one child. Redirect its parent link to child {target.Left.Value} (#{target.Left.DisplayId}).",
                    cancellationToken);

                Transplant(target, target.Left);
            }
            else
            {
                deleteCase = RedBlackDeleteCase.TwoChildren;
                moved = target.Right;
                successorChecks++;
                moved.VisualState = RedBlackNodeVisualState.Checking;
                NotifyChanged();

                await NextStepAsync(
                    $"{value} has two children. Start at right child {moved.Value} and follow left links to find the in-order successor.",
                    cancellationToken);

                while (moved.Left is not null)
                {
                    moved.VisualState = RedBlackNodeVisualState.Visited;
                    moved = moved.Left;
                    successorChecks++;
                    moved.VisualState = RedBlackNodeVisualState.Checking;
                    NotifyChanged();

                    await NextStepAsync(
                        $"Successor search moves left to {moved.Value} (#{moved.DisplayId}).",
                        cancellationToken);
                }

                moved.VisualState = RedBlackNodeVisualState.Replacement;
                removedPathColor = moved.Color;
                fixNode = moved.Right;
                NotifyChanged();

                await NextStepAsync(
                    $"Successor found: {moved.Value} (#{moved.DisplayId}), currently {moved.Color.ToString().ToUpperInvariant()}. Preserve this exact node object and transplant it into {value}'s position.",
                    cancellationToken);

                if (ReferenceEquals(moved.Parent, target))
                {
                    fixParent = moved;
                    fixNodeWasLeft = false;

                    if (fixNode is not null)
                    {
                        fixNode.Parent = moved;
                    }
                }
                else
                {
                    var successorOldParent = moved.Parent!;
                    fixParent = successorOldParent;
                    fixNodeWasLeft = true;

                    await NextStepAsync(
                        $"Successor {moved.Value} is deeper in the right subtree. First replace its old left-child position with its right child{(fixNode is null ? " NIL" : $" {fixNode.Value}")}.",
                        cancellationToken);

                    Transplant(moved, moved.Right);
                    moved.Right = target.Right;
                    moved.Right.Parent = moved;
                }

                await NextStepAsync(
                    $"Move successor {moved.Value} into {value}'s position, attach the old left subtree, and copy the removed target's color onto the successor. Node identity stays #{moved.DisplayId}.",
                    cancellationToken);

                Transplant(target, moved);
                moved.Left = target.Left;
                moved.Left.Parent = moved;
                moved.Color = target.Color;
                moved.VisualState = RedBlackNodeVisualState.Replacement;
                NotifyChanged();
            }

            _count--;
            NotifyChanged();

            if (removedPathColor == RedBlackColor.Black)
            {
                await NextStepAsync(
                    "The structurally removed node was BLACK. One root-to-NIL path may now have one fewer black node, so run delete fix-up from the replacement position.",
                    cancellationToken);

                if (fixNode is not null)
                {
                    fixParent = fixNode.Parent;
                    fixNodeWasLeft = fixParent is not null && ReferenceEquals(fixParent.Left, fixNode);
                }

                await DeleteFixupAsync(fixNode, fixParent, fixNodeWasLeft, stats, cancellationToken);
            }
            else
            {
                await NextStepAsync(
                    "The structurally removed node was RED. Removing red does not change any path's black-height, so no delete fix-up is required.",
                    cancellationToken);
            }

            if (_root is not null && _root.Color != RedBlackColor.Black)
            {
                Recolor(_root, RedBlackColor.Black, stats);
            }

            return BuildResult(
                RedBlackOperationKind.Delete, value, succeeded: true, duplicateRejected: false, targetId,
                comparisons, successorChecks, stats,
                initialCount, heightBefore, blackHeightBefore, deleteCase);
        }, cancellationToken);

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        ExecuteExclusiveAsync(async () =>
        {
            if (_root is null)
            {
                await NextStepAsync("The Red-Black Tree is already empty.", cancellationToken);
                return;
            }

            await NextStepAsync(
                $"Reset the lab by removing the root reference. The {_count} node object(s) become unreachable from this tree.",
                cancellationToken);

            _root = null;
            _count = 0;
            NotifyChanged();

            await NextStepAsync(
                "The tree is empty. In .NET, unreachable node objects can be reclaimed later by garbage collection.",
                cancellationToken);
        }, cancellationToken);

    private async Task InsertFixupAsync(
        RedBlackNode inserted,
        FixupStats stats,
        CancellationToken cancellationToken)
    {
        var current = inserted;

        while (ColorOf(current.Parent) == RedBlackColor.Red)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stats.Checks++;

            var parent = current.Parent!;
            var grandparent = parent.Parent
                ?? throw new InvalidOperationException("A red parent in a valid Red-Black insertion must have a grandparent.");

            current.VisualState = RedBlackNodeVisualState.Violation;
            parent.VisualState = RedBlackNodeVisualState.Violation;
            grandparent.VisualState = RedBlackNodeVisualState.RelativeFocus;
            NotifyChanged();

            await NextStepAsync(
                $"Red-red violation: node {current.Value} and parent {parent.Value} are both red. Inspect the uncle on the other side of grandparent {grandparent.Value}.",
                cancellationToken);

            if (ReferenceEquals(parent, grandparent.Left))
            {
                var uncle = grandparent.Right;

                if (ColorOf(uncle) == RedBlackColor.Red)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.InsertUncleRed);
                    if (uncle is not null)
                    {
                        uncle.VisualState = RedBlackNodeVisualState.RelativeFocus;
                    }
                    NotifyChanged();

                    await NextStepAsync(
                        $"Uncle {(uncle is null ? "NIL" : uncle.Value)} is RED. Recolor parent and uncle black, recolor grandparent red, then continue checking from the grandparent.",
                        cancellationToken);

                    Recolor(parent, RedBlackColor.Black, stats);
                    Recolor(uncle, RedBlackColor.Black, stats);
                    Recolor(grandparent, RedBlackColor.Red, stats);
                    MarkRecolored(parent, uncle, grandparent);
                    NotifyChanged();
                    current = grandparent;
                    continue;
                }

                if (ReferenceEquals(current, parent.Right))
                {
                    RecordRepairCase(stats, RedBlackRepairCase.InsertTriangle);
                    current = parent;
                    await RotateLeftAsync(
                        current,
                        $"Triangle case: the path bends left-right. Rotate left around parent {current.Value} to turn it into a straight line.",
                        stats,
                        cancellationToken);
                    parent = current.Parent!;
                    grandparent = parent.Parent!;
                }

                RecordRepairCase(stats, RedBlackRepairCase.InsertLine);
                Recolor(parent, RedBlackColor.Black, stats);
                Recolor(grandparent, RedBlackColor.Red, stats);
                parent.VisualState = RedBlackNodeVisualState.Recolored;
                grandparent.VisualState = RedBlackNodeVisualState.RotationPivot;
                NotifyChanged();

                await NextStepAsync(
                    $"Line case: recolor parent {parent.Value} black and grandparent {grandparent.Value} red, then rotate right around the grandparent.",
                    cancellationToken);

                await RotateRightAsync(
                    grandparent,
                    $"Right rotation lifts {parent.Value}; BST order stays intact while the red-red violation disappears.",
                    stats,
                    cancellationToken);
            }
            else
            {
                var uncle = grandparent.Left;

                if (ColorOf(uncle) == RedBlackColor.Red)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.InsertUncleRed);
                    if (uncle is not null)
                    {
                        uncle.VisualState = RedBlackNodeVisualState.RelativeFocus;
                    }
                    NotifyChanged();

                    await NextStepAsync(
                        $"Uncle {(uncle is null ? "NIL" : uncle.Value)} is RED. Recolor parent and uncle black, recolor grandparent red, then continue checking from the grandparent.",
                        cancellationToken);

                    Recolor(parent, RedBlackColor.Black, stats);
                    Recolor(uncle, RedBlackColor.Black, stats);
                    Recolor(grandparent, RedBlackColor.Red, stats);
                    MarkRecolored(parent, uncle, grandparent);
                    NotifyChanged();
                    current = grandparent;
                    continue;
                }

                if (ReferenceEquals(current, parent.Left))
                {
                    RecordRepairCase(stats, RedBlackRepairCase.InsertTriangle);
                    current = parent;
                    await RotateRightAsync(
                        current,
                        $"Triangle case: the path bends right-left. Rotate right around parent {current.Value} to turn it into a straight line.",
                        stats,
                        cancellationToken);
                    parent = current.Parent!;
                    grandparent = parent.Parent!;
                }

                RecordRepairCase(stats, RedBlackRepairCase.InsertLine);
                Recolor(parent, RedBlackColor.Black, stats);
                Recolor(grandparent, RedBlackColor.Red, stats);
                parent.VisualState = RedBlackNodeVisualState.Recolored;
                grandparent.VisualState = RedBlackNodeVisualState.RotationPivot;
                NotifyChanged();

                await NextStepAsync(
                    $"Line case: recolor parent {parent.Value} black and grandparent {grandparent.Value} red, then rotate left around the grandparent.",
                    cancellationToken);

                await RotateLeftAsync(
                    grandparent,
                    $"Left rotation lifts {parent.Value}; BST order stays intact while the red-red violation disappears.",
                    stats,
                    cancellationToken);
            }
        }

        if (_root is not null && _root.Color == RedBlackColor.Red)
        {
            RecordRepairCase(stats, RedBlackRepairCase.InsertRootBlack);
            _root.VisualState = RedBlackNodeVisualState.Recolored;
            Recolor(_root, RedBlackColor.Black, stats);
            NotifyChanged();

            await NextStepAsync(
                $"Final root rule: recolor root {_root.Value} black. The Red-Black invariants are restored.",
                cancellationToken);
        }
        else if (_root is not null)
        {
            _root.VisualState = RedBlackNodeVisualState.Fixed;
            NotifyChanged();
            await NextStepAsync(
                $"No red-red violation remains and root {_root.Value} is black. Insertion fix-up is complete.",
                cancellationToken);
        }
    }

    private async Task DeleteFixupAsync(
        RedBlackNode? current,
        RedBlackNode? parent,
        bool currentWasLeft,
        FixupStats stats,
        CancellationToken cancellationToken)
    {
        while (!ReferenceEquals(current, _root) && ColorOf(current) == RedBlackColor.Black)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (parent is null)
            {
                break;
            }

            stats.Checks++;
            var isLeft = current is not null
                ? ReferenceEquals(parent.Left, current)
                : currentWasLeft;

            parent.VisualState = RedBlackNodeVisualState.RelativeFocus;
            if (current is not null)
            {
                current.VisualState = RedBlackNodeVisualState.Violation;
            }

            var sibling = isLeft ? parent.Right : parent.Left;
            if (sibling is not null)
            {
                sibling.VisualState = RedBlackNodeVisualState.RelativeFocus;
            }
            NotifyChanged();

            await NextStepAsync(
                $"Delete fix-up: the replacement position behaves like an extra black on the {(isLeft ? "left" : "right")} side of parent {parent.Value}. Inspect sibling {(sibling is null ? "NIL" : sibling.Value)}.",
                cancellationToken);

            if (isLeft)
            {
                if (ColorOf(sibling) == RedBlackColor.Red)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteSiblingRed);
                    Recolor(sibling, RedBlackColor.Black, stats);
                    Recolor(parent, RedBlackColor.Red, stats);
                    MarkRecolored(sibling, parent);
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling {sibling!.Value} is RED. Recolor sibling black and parent red, then rotate left around parent {parent.Value}. This converts the problem to a black-sibling case.",
                        cancellationToken);

                    await RotateLeftAsync(parent, $"Rotate left around {parent.Value}; the extra-black position remains below the same parent node.", stats, cancellationToken);
                    sibling = parent.Right;
                }

                if (ColorOf(sibling?.Left) == RedBlackColor.Black &&
                    ColorOf(sibling?.Right) == RedBlackColor.Black)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteSiblingBlackChildrenBlack);
                    Recolor(sibling, RedBlackColor.Red, stats);
                    if (sibling is not null)
                    {
                        sibling.VisualState = RedBlackNodeVisualState.Recolored;
                    }
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling {(sibling is null ? "NIL" : sibling.Value)} and both sibling children are BLACK. Recolor the sibling red and move the extra black up to parent {parent.Value}.",
                        cancellationToken);

                    current = parent;
                    parent = current.Parent;
                    currentWasLeft = parent is not null && ReferenceEquals(parent.Left, current);
                    continue;
                }

                if (ColorOf(sibling?.Right) == RedBlackColor.Black)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteNearRed);
                    var nearChild = sibling?.Left;
                    Recolor(nearChild, RedBlackColor.Black, stats);
                    Recolor(sibling, RedBlackColor.Red, stats);
                    MarkRecolored(nearChild, sibling);
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling's far child is BLACK but the near child is RED. Recolor and rotate right around sibling {sibling!.Value} so a red far child is ready for the final repair.",
                        cancellationToken);

                    await RotateRightAsync(sibling, $"Rotate right around sibling {sibling.Value} to expose a red far child.", stats, cancellationToken);
                    sibling = parent.Right;
                }

                RecordRepairCase(stats, RedBlackRepairCase.DeleteFarRed);
                if (sibling is not null)
                {
                    Recolor(sibling, parent.Color, stats);
                }
                Recolor(parent, RedBlackColor.Black, stats);
                Recolor(sibling?.Right, RedBlackColor.Black, stats);
                MarkRecolored(sibling, parent, sibling?.Right);
                NotifyChanged();

                await NextStepAsync(
                    $"Far-child case: copy parent {parent.Value}'s color to sibling, make parent and the sibling's far child black, then rotate left around the parent. The extra black is absorbed.",
                    cancellationToken);

                await RotateLeftAsync(parent, $"Final left rotation restores equal black-height on both sides.", stats, cancellationToken);
                current = _root;
                parent = null;
                currentWasLeft = false;
            }
            else
            {
                if (ColorOf(sibling) == RedBlackColor.Red)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteSiblingRed);
                    Recolor(sibling, RedBlackColor.Black, stats);
                    Recolor(parent, RedBlackColor.Red, stats);
                    MarkRecolored(sibling, parent);
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling {sibling!.Value} is RED. Recolor sibling black and parent red, then rotate right around parent {parent.Value}. This converts the problem to a black-sibling case.",
                        cancellationToken);

                    await RotateRightAsync(parent, $"Rotate right around {parent.Value}; the extra-black position remains below the same parent node.", stats, cancellationToken);
                    sibling = parent.Left;
                }

                if (ColorOf(sibling?.Right) == RedBlackColor.Black &&
                    ColorOf(sibling?.Left) == RedBlackColor.Black)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteSiblingBlackChildrenBlack);
                    Recolor(sibling, RedBlackColor.Red, stats);
                    if (sibling is not null)
                    {
                        sibling.VisualState = RedBlackNodeVisualState.Recolored;
                    }
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling {(sibling is null ? "NIL" : sibling.Value)} and both sibling children are BLACK. Recolor the sibling red and move the extra black up to parent {parent.Value}.",
                        cancellationToken);

                    current = parent;
                    parent = current.Parent;
                    currentWasLeft = parent is not null && ReferenceEquals(parent.Left, current);
                    continue;
                }

                if (ColorOf(sibling?.Left) == RedBlackColor.Black)
                {
                    RecordRepairCase(stats, RedBlackRepairCase.DeleteNearRed);
                    var nearChild = sibling?.Right;
                    Recolor(nearChild, RedBlackColor.Black, stats);
                    Recolor(sibling, RedBlackColor.Red, stats);
                    MarkRecolored(nearChild, sibling);
                    NotifyChanged();

                    await NextStepAsync(
                        $"Sibling's far child is BLACK but the near child is RED. Recolor and rotate left around sibling {sibling!.Value} so a red far child is ready for the final repair.",
                        cancellationToken);

                    await RotateLeftAsync(sibling, $"Rotate left around sibling {sibling.Value} to expose a red far child.", stats, cancellationToken);
                    sibling = parent.Left;
                }

                RecordRepairCase(stats, RedBlackRepairCase.DeleteFarRed);
                if (sibling is not null)
                {
                    Recolor(sibling, parent.Color, stats);
                }
                Recolor(parent, RedBlackColor.Black, stats);
                Recolor(sibling?.Left, RedBlackColor.Black, stats);
                MarkRecolored(sibling, parent, sibling?.Left);
                NotifyChanged();

                await NextStepAsync(
                    $"Far-child case: copy parent {parent.Value}'s color to sibling, make parent and the sibling's far child black, then rotate right around the parent. The extra black is absorbed.",
                    cancellationToken);

                await RotateRightAsync(parent, $"Final right rotation restores equal black-height on both sides.", stats, cancellationToken);
                current = _root;
                parent = null;
                currentWasLeft = false;
            }
        }

        if (current is not null && current.Color != RedBlackColor.Black)
        {
            Recolor(current, RedBlackColor.Black, stats);
            current.VisualState = RedBlackNodeVisualState.Fixed;
            NotifyChanged();

            await NextStepAsync(
                $"Color the replacement node {current.Value} black. The extra black is absorbed and all Red-Black invariants are restored.",
                cancellationToken);
        }
        else if (_root is not null)
        {
            _root.VisualState = RedBlackNodeVisualState.Fixed;
            NotifyChanged();
            await NextStepAsync(
                "Delete fix-up reached the root or an already-black balanced position. Equal black-height is restored.",
                cancellationToken);
        }
    }

    private async Task<RedBlackNode> RotateLeftAsync(
        RedBlackNode pivot,
        string stepText,
        FixupStats stats,
        CancellationToken cancellationToken)
    {
        var promoted = pivot.Right ?? throw new InvalidOperationException("A left rotation requires a right child.");
        pivot.VisualState = RedBlackNodeVisualState.Rotating;
        promoted.VisualState = RedBlackNodeVisualState.RotationPivot;
        NotifyChanged();

        await NextStepAsync(stepText, cancellationToken);

        var newRoot = RotateLeft(pivot);
        stats.Rotations++;
        pivot.VisualState = RedBlackNodeVisualState.Fixed;
        newRoot.VisualState = RedBlackNodeVisualState.Fixed;
        NotifyChanged();

        await NextStepAsync(
            $"Left rotation complete: {newRoot.Value} is now the subtree root. Node IDs and BST ordering were preserved; only parent/left/right references changed.",
            cancellationToken);

        return newRoot;
    }

    private async Task<RedBlackNode> RotateRightAsync(
        RedBlackNode pivot,
        string stepText,
        FixupStats stats,
        CancellationToken cancellationToken)
    {
        var promoted = pivot.Left ?? throw new InvalidOperationException("A right rotation requires a left child.");
        pivot.VisualState = RedBlackNodeVisualState.Rotating;
        promoted.VisualState = RedBlackNodeVisualState.RotationPivot;
        NotifyChanged();

        await NextStepAsync(stepText, cancellationToken);

        var newRoot = RotateRight(pivot);
        stats.Rotations++;
        pivot.VisualState = RedBlackNodeVisualState.Fixed;
        newRoot.VisualState = RedBlackNodeVisualState.Fixed;
        NotifyChanged();

        await NextStepAsync(
            $"Right rotation complete: {newRoot.Value} is now the subtree root. Node IDs and BST ordering were preserved; only parent/left/right references changed.",
            cancellationToken);

        return newRoot;
    }

    private RedBlackNode RotateLeft(RedBlackNode pivot)
    {
        var promoted = pivot.Right ?? throw new InvalidOperationException("A left rotation requires a right child.");
        var transferred = promoted.Left;
        var oldParent = pivot.Parent;

        ReplaceChildReference(oldParent, pivot, promoted);
        promoted.Parent = oldParent;
        promoted.Left = pivot;
        pivot.Parent = promoted;
        pivot.Right = transferred;

        if (transferred is not null)
        {
            transferred.Parent = pivot;
        }

        return promoted;
    }

    private RedBlackNode RotateRight(RedBlackNode pivot)
    {
        var promoted = pivot.Left ?? throw new InvalidOperationException("A right rotation requires a left child.");
        var transferred = promoted.Right;
        var oldParent = pivot.Parent;

        ReplaceChildReference(oldParent, pivot, promoted);
        promoted.Parent = oldParent;
        promoted.Right = pivot;
        pivot.Parent = promoted;
        pivot.Left = transferred;

        if (transferred is not null)
        {
            transferred.Parent = pivot;
        }

        return promoted;
    }

    private void ReplaceChildReference(RedBlackNode? parent, RedBlackNode oldChild, RedBlackNode newChild)
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

        throw new InvalidOperationException("The rotation pivot is not connected to its recorded parent.");
    }

    private void Transplant(RedBlackNode node, RedBlackNode? replacement)
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

    private RedBlackOperationResult BuildResult(
        RedBlackOperationKind operation,
        int requestedValue,
        bool succeeded,
        bool duplicateRejected,
        Guid? affectedNodeId,
        int comparisons,
        int successorChecks,
        FixupStats stats,
        int initialCount,
        int heightBefore,
        int blackHeightBefore,
        RedBlackDeleteCase deleteCase) =>
        new(
            operation,
            requestedValue,
            succeeded,
            duplicateRejected,
            affectedNodeId,
            comparisons,
            successorChecks,
            stats.Checks,
            stats.Recolors,
            stats.Rotations,
            stats.FirstCase,
            initialCount,
            _count,
            heightBefore,
            Height,
            blackHeightBefore,
            BlackHeight,
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

    private static RedBlackColor ColorOf(RedBlackNode? node) => node?.Color ?? RedBlackColor.Black;

    private static void Recolor(RedBlackNode? node, RedBlackColor color, FixupStats stats)
    {
        if (node is null || node.Color == color)
        {
            return;
        }

        node.Color = color;
        stats.Recolors++;
    }

    private static void MarkRecolored(params RedBlackNode?[] nodes)
    {
        foreach (var node in nodes)
        {
            if (node is not null)
            {
                node.VisualState = RedBlackNodeVisualState.Recolored;
            }
        }
    }

    private static void RecordRepairCase(FixupStats stats, RedBlackRepairCase repairCase)
    {
        if (stats.FirstCase == RedBlackRepairCase.None)
        {
            stats.FirstCase = repairCase;
        }
    }

    private static int GetHeight(RedBlackNode? node)
    {
        if (node is null)
        {
            return 0;
        }

        return 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    }

    /// <summary>
    /// Counts black real nodes on the left-most root-to-NIL path. At operation boundaries
    /// the Red-Black invariants guarantee that every root-to-NIL path has the same count.
    /// </summary>
    private static int GetBlackHeight(RedBlackNode? node)
    {
        var count = 0;
        var current = node;

        while (current is not null)
        {
            if (current.Color == RedBlackColor.Black)
            {
                count++;
            }

            current = current.Left;
        }

        return count;
    }

    private static void NormalizeVisualStates(RedBlackNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.VisualState = RedBlackNodeVisualState.Normal;
        NormalizeVisualStates(node.Left);
        NormalizeVisualStates(node.Right);
    }

    private void NotifyChanged() => Changed?.Invoke();

    private sealed class FixupStats
    {
        public int Checks { get; set; }
        public int Recolors { get; set; }
        public int Rotations { get; set; }
        public RedBlackRepairCase FirstCase { get; set; }
    }
}

# Tree visualizations

Blazor presentation components for live tree modules live here. Algorithm logic stays in `AlgorithmVisualizer.Core`.

## Binary Search Tree

- `BstVisualization.razor` — ordered visual-state tree with highlighted simulation states and edges.
- `BstMemoryVisualization.razor` — managed-memory teaching view showing the root reference plus each node object's parent/left/right references.

## AVL Tree

- `AvlVisualization.razor` — ordered AVL visual state showing each node's value, short ID, cached height, balance factor, edges, and transient rebalance/rotation states.
- `AvlMemoryVisualization.razor` — node-reference view showing root, parent/left/right references, cached height, and balance factor while rotations rewire the actual custom node objects.

The visual layout is intentionally separate from the memory/reference layout. Screen coordinates are never presented as physical memory addresses.

## Red-Black Tree

- `RedBlackVisualization.razor` — ordered Red-Black visual state that keeps persistent RED/BLACK node color separate from transient fix-up states such as violation, relative inspection, recoloring, and rotation. Conceptual NIL leaves are explained as black without creating fake node objects.
- `RedBlackMemoryVisualization.razor` — node-reference view showing root, parent/left/right references, stable node identity, persistent color, and conceptual `null → NIL(B)` child semantics while recoloring and rotations run.

Red-Black SVG labels use Razor-safe `<foreignObject>` content rather than raw SVG `<text ...>` tags. Legend swatches are HTML elements with visible `background`/`border-color` styling so they cannot regress into empty checkbox-looking boxes.

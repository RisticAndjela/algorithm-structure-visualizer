# Tree visualizations

Blazor presentation components for live tree modules live here. Algorithm logic stays in `AlgorithmVisualizer.Core`.

## Binary Search Tree

- `BstVisualization.razor` — ordered visual-state tree with highlighted simulation states and edges.
- `BstMemoryVisualization.razor` — managed-memory teaching view showing the root reference plus each node object's parent/left/right references.

## AVL Tree

- `AvlVisualization.razor` — ordered AVL visual state showing each node's value, short ID, cached height, balance factor, edges, and transient rebalance/rotation states.
- `AvlMemoryVisualization.razor` — node-reference view showing root, parent/left/right references, cached height, and balance factor while rotations rewire the actual custom node objects.

The visual layout is intentionally separate from the memory/reference layout. Screen coordinates are never presented as physical memory addresses.

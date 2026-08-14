# Tree visualizations

Blazor presentation components for tree modules live here. Algorithm logic stays in `AlgorithmVisualizer.Core`.

## Implemented BST components

- `BstVisualization.razor` — SVG visual-state tree ordered by key, with highlighted simulation states and edges.
- `BstMemoryVisualization.razor` — simplified managed-memory teaching view showing the root reference plus each node object's parent/left/right references.

The visual layout is intentionally separate from the memory/reference layout. Screen coordinates are never presented as physical memory addresses.

# Graph visualization

`GraphVisualization` renders topology only. It uses SVG paths for edges and Razor-safe `foreignObject` HTML for labels/nodes, avoiding raw SVG `<text ...>` because Razor can interpret `<text>` as a Razor text block.

`GraphMemoryVisualization` shows the same snapshot as an adjacency list and the Matrix-backed adjacency table. Main explanatory labels stay readable; internal scrolling is confined to the visualization/table rather than the page. Graph Core is not capped at eight vertices; for larger snapshots the visual auto-layout switches from a ring to a grid before the learner optionally drags individual nodes.

## Interactive layout

`GraphVisualization` allows direct pointer dragging of vertices. Manual positions are UI-only overrides keyed by stable vertex ID. The component recalculates edge paths, arrows, reverse-edge curves, self-loops and weight-label positions from those coordinates on every render, so connections follow a dragged node without mutating the Core graph. `Reset layout` discards the overrides and restores the automatic ring/grid placement.

Graph dragging is handled entirely by Blazor pointer events and C# component state. There is no project-owned JavaScript module or `IJSRuntime` dependency in the graph visualization. Leaving the SVG safely terminates a drag because browser pointer capture is intentionally not requested through JavaScript.

Dragged vertices are positioned through a stable SVG `<g transform="translate(x y)">` wrapper. Dynamic SVG coordinates are formatted with invariant culture, and non-finite pointer/world values are rejected before they can enter layout state. This prevents a click or tiny drag from producing an invalid `<foreignObject>` coordinate and making the vertex disappear.


## Content-bounded unbounded canvas

The visual workspace has no fixed drag clamp. Manual vertex coordinates may move in every direction. The C# component derives the SVG stage from a logical base viewport plus the current graph extents and safety padding, so the workspace grows with content instead of allocating a permanently huge blank canvas. `Reset layout` clears manual positions and restores automatic placement.

Because project-owned JavaScript was removed, the component no longer performs DOM viewport measurement or programmatic native-scroll compensation. The graph keeps a fixed logical teaching viewport in C# and relies on normal browser overflow for any stage area that extends beyond it.

## Learning contract around the visualization

`GraphPage` now treats Visual/Memory rendering as part of a complete learning loop rather than an isolated diagram. After each structural/search action, the learner can open a three-tab explanation covering the action, synchronized adjacency-list/Matrix representation, and why the operation matters. Guided Practice is automatically validated against real graph snapshots and operation results and stores completion locally. Direct links to the Graph concepts section and Visual-vs-Memory concepts remain next to the state controls.

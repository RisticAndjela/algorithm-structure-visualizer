# Graph visualization

`GraphVisualization` renders topology only. It uses SVG paths for edges and Razor-safe `foreignObject` HTML for labels/nodes, avoiding raw SVG `<text ...>` because Razor can interpret `<text>` as a Razor text block.

`GraphMemoryVisualization` shows the same snapshot as an adjacency list and the Matrix-backed adjacency table. Main explanatory labels stay readable; internal scrolling is confined to the visualization/table rather than the page. Graph Core is not capped at eight vertices; for larger snapshots the visual auto-layout switches from a ring to a grid before the learner optionally drags individual nodes.

## Interactive layout

`GraphVisualization` allows direct pointer dragging of vertices. Manual positions are UI-only overrides keyed by stable vertex ID. The component recalculates edge paths, arrows, reverse-edge curves, self-loops and weight-label positions from those coordinates on every render, so connections follow a dragged node without mutating the Core graph. `Reset layout` discards the overrides and restores the automatic ring/grid placement.

A tiny ES module (`wwwroot/js/graph-drag.js`) is used only for SVG pointer capture, viewport measurement, and scroll compensation. It does not own graph data, run graph algorithms or persist layout state.

Dragged vertices are positioned through a stable SVG `<g transform="translate(x y)">` wrapper. Dynamic SVG coordinates are formatted with invariant culture, and non-finite pointer/world values are rejected before they can enter layout state. This prevents a click or tiny drag from producing an invalid `<foreignObject>` coordinate and making the vertex disappear.


## Content-bounded unbounded canvas

The visual workspace has no fixed drag boundary. Manual vertex coordinates may move in every direction. The component measures the visible viewport and derives the SVG stage from the base viewport plus the current graph extents and padding large enough for node circles, weighted labels, curved reverse edges and self-loops. Therefore scrollbars appear only when current graph content actually extends outside the visible region; there is no permanently allocated 10,000×10,000 blank canvas.

World coordinates are separate from rendered stage coordinates. If content expands into negative X/Y space, the stage origin shifts and the component compensates the scroll offset after render so the learner does not see the rest of the graph jump. Moving content back inside the base viewport shrinks the extra scroll range. `Reset layout` clears manual world positions and resets scroll to the automatic-layout origin.

## Learning contract around the visualization

`GraphPage` now treats Visual/Memory rendering as part of a complete learning loop rather than an isolated diagram. After each structural/search action, the learner can open a three-tab explanation covering the action, synchronized adjacency-list/Matrix representation, and why the operation matters. Guided Practice is automatically validated against real graph snapshots and operation results and stores completion locally. Direct links to the Graph concepts section and Visual-vs-Memory concepts remain next to the state controls.

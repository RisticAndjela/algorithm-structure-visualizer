# Heap visualizations

The Heap chapter has one visualization stack shared by every branching factor.

- `DaryHeapVisualization.razor` renders the complete d-ary tree plus synchronized array indexes. With `d = 2` it is the Binary Heap view; larger d values use the same component and formulas.
- `DaryHeapMemoryVisualization.razor` renders used/reserved raw-array slots, stable IDs, capacity, and d-dependent parent/child ranges through the shared `MemoryStateCard` design.

There are no separate Binary Heap visualization components. Wide heap data may scroll only inside the visualization region, not by forcing the whole application page to overflow horizontally.

No raw SVG `<text ...>` labels are used. Legend swatches remain ordinary HTML elements with explicit styling.

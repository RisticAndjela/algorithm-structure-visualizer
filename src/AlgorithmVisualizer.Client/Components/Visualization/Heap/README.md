# Heap visualizations

Two live heap labs share this visualization area:

- `DaryHeapVisualization.razor` shows the generalized complete d-ary tree plus synchronized array indexes. It makes the formula `parent=(i-1)/d`, children `di+1..di+d` visible and shows that Binary Heap is the `d=2` case.
- `DaryHeapMemoryVisualization.razor` shows used/reserved raw-array slots, stable IDs, capacity, and d-dependent parent/child ranges.
- `HeapVisualization.razor` remains the dedicated Binary Heap tree + array view.
- `HeapMemoryVisualization.razor` remains the dedicated Binary Heap memory/index view.

Wide data may scroll inside the tree/array visualization region, but these components must not force the whole application page to scroll horizontally.

No new raw SVG `<text ...>` labels are used. Legend swatches are ordinary HTML elements with explicit background/border styling, following the established visualization safety rules.

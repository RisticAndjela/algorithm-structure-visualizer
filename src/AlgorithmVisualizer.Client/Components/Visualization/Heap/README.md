# Heap visualizations

- `HeapVisualization.razor` shows one snapshot as both a complete binary tree and synchronized used array indexes.
- `HeapMemoryVisualization.razor` shows the custom backing-array slots, reserved capacity, stable element IDs, and calculated parent/left/right indexes.

No raw SVG `<text ...>` labels are used. Legend swatches are ordinary HTML elements with explicit background/border styling, following the established visualization safety rules.

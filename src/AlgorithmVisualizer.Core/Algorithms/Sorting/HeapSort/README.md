# Heap Sort

Heap Sort is implemented manually over `HeapSortElement[]`; no framework heap, `PriorityQueue`, `Array.Sort`, `List.Sort`, or LINQ ordering performs the taught algorithm.

Two explicit variants share the same extraction phase:

- **Basic · Incremental Build** grows a Max Heap prefix one element at a time and bubble-ups each new item. Heap construction is `O(n log n)` in the worst case and deliberately bridges from the Binary Heap insertion lesson.
- **Advanced · Floyd Bottom-Up** starts at the last parent and applies sift-down toward the root. Heap construction is `Θ(n)`. This improves the build phase but does not change total Heap Sort complexity.

After construction, both modes repeatedly:

1. swap the maximum root at index `0` with the last active heap slot;
2. shrink `heapSize`, making that suffix slot final;
3. sift the new root down inside the reduced heap only.

Total best/average/worst time is `Θ(n log n)`. Extra array storage is `O(1)` because the complete binary tree is encoded by the same array indexes. Heap Sort is not stable: root/end and repair swaps may reverse equal-value element identities.

The simulation exposes heap size/boundary, parent and child candidates, swaps, build comparisons/swaps, extraction count, and stable per-element identity. READ/inspection is safe, but create/update/delete mutations invalidate the active heap and sorted-suffix invariant and therefore require a new run.

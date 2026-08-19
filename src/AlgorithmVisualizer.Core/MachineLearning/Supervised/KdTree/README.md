# KD-Tree

Phase 1 step 6 implements a from-scratch KD-Tree for nearest-neighbor search.

- Feature points and the query use `ManualVector`.
- Euclidean distance reuses `VectorSimulation`.
- Tree nodes are explicit project-owned references with point index, split axis, depth, left child and right child.
- Build alternates axes by depth and selects the median after an explicit merge-sort of the active point-index range. This teaching build is `O(n log² n)`.
- Search descends to the query side first, tracks the current best point, backtracks, compares split-plane distance with the best Euclidean distance, and prunes the opposite subtree when it cannot win.
- Average balanced nearest-neighbor lookup is commonly `O(log n)`, but worst case remains `O(n)` and high dimensions reduce pruning effectiveness.

No framework tree, sort helper, spatial index, nearest-neighbor package, or ML library implements the taught behavior.

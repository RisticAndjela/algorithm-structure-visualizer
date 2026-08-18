# K-Nearest Neighbors Core

`KnnSimulation` is the Phase 1 step 5 neighbor-based classifier.

- Training examples and the query use project-owned `ManualVector` storage.
- Euclidean and Manhattan distance are delegated to the existing `VectorSimulation` primitives.
- The KNN layer itself performs the full scan, deterministic top-k insertion, and binary majority vote with explicit loops.
- `k` is odd in this beginner classifier so a binary vote cannot tie.
- No LINQ sorting, framework nearest-neighbor helper, ML package, or KD-tree is used.
- KD-Tree acceleration belongs to the next lesson; this module intentionally exposes the brute-force neighbor search first.

The visual Client is two-dimensional for clarity, while Core accepts any shared feature dimension from 1 through 12.

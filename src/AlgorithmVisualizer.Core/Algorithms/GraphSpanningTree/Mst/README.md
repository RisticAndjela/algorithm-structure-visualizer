# Minimum Spanning Tree / Forest

`MstSimulation` reuses the canonical `GraphSnapshot` and implements both taught MST strategies manually.

- **Prim** grows one component from a chosen start vertex. Its cut-edge frontier is a project-owned binary min-heap backed by `ManualHeapArray<T>`.
- **Kruskal** manually merge-sorts edge indexes by weight and uses a hand-written DSU/Union-Find with path compression and union by rank.
- Directed graphs are rejected. Negative and zero weights are valid.
- A disconnected undirected graph produces a minimum spanning **forest**, not a falsely labeled single MST.
- Neither mode mutates the source Graph; graph changes require a fresh run.

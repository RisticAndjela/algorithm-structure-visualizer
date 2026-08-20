# Computational Graph

Phase 2 Step 10 implements a scalar computational graph from scratch.

- Input nodes store scalar values.
- Operation nodes store explicit dependency indexes.
- Supported teaching operations are add, subtract, multiply, and square.
- The forward pass repeatedly scans for nodes whose dependencies are already computed, then stores each intermediate result.
- Snapshots expose ready, active, computed, and evaluation-order state for Blazor playback.
- No graph, topological-scheduling, automatic-differentiation, or deep-learning library implements the taught behavior.
- Backpropagation is intentionally deferred to the later Backpropagation lesson; this module establishes the dependency graph that reverse-mode differentiation will reuse.

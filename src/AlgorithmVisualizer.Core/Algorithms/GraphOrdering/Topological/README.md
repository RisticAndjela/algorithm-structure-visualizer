# Topological Sort

`TopologicalSortSimulation` reuses the existing manual `GraphSnapshot` representation and implements two real C# variants:

- **Kahn Queue** — computes `indegree[]`, places every zero-in-degree vertex into a FIFO backed by `ManualDynamicArray<int>`, emits one vertex, then decrements outgoing neighbors.
- **DFS Postorder** — recursive white/gray/black DFS. A gray-to-gray edge is a back edge and therefore a cycle. Finished vertices are appended to a manual postorder buffer and reversed manually.

Both variants are `O(V + E)` time and `O(V)` extra state. The input graph must be directed. Weights are ignored because topological ordering depends only on direction/dependency. Any graph mutation requires restarting the run.

A cycle is not treated as a partial success: the result reports `CycleDetected = true`, and no valid topological order is exposed for DFS. Kahn may have a partial emitted prefix internally, which is useful for explaining exactly where the ready queue became empty.

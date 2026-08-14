# Graph

The Graph module is live and implemented entirely in the Core layer.

## Representation

A single canonical graph is kept synchronized in two teaching representations:

- **Adjacency list** — each `GraphVertex` owns our existing `ManualDynamicArray<GraphNeighbor>`; no `Dictionary`, `List`, framework graph, or graph package is used.
- **Adjacency matrix** — reuses the existing `DataStructures/Matrix/ManualMatrix` row-major implementation. A parallel presence array lets weighted graphs distinguish a real zero-weight edge from an absent edge.

Graph Core has no fixed eight-vertex teaching cap. The reusable `ManualMatrix` can grow with the graph, while the standalone Matrix learning page keeps its own 8×8 UI limit for readability. Large adjacency tables are handled by internal scrolling in the Client.

## Live operations

- Add/search/rename/remove vertex.
- Add/search/update-weight/remove edge.
- Inspect one vertex's direct neighbors.
- Clear graph.
- Directed or undirected mode.
- Weighted or unweighted mode, including zero and negative weights at the generic graph-structure level.
- Self-loops are supported.

Directed/weighted mode can change while vertices exist, but only while there are no edges; existing edges are never silently reinterpreted.

## Important boundary

This module teaches graph **structure and representation**. BFS, DFS, Dijkstra, topological sort, and MST remain separate algorithm modules. They should reuse this graph implementation plus the already-live Queue/Stack/Heap implementations rather than duplicate them.

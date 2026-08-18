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

This module teaches graph **structure and representation**. BFS and DFS are now live separate algorithm modules that reuse this graph snapshot plus the project's manual Queue/Stack storage foundation. Dijkstra is now live and reuses this Graph snapshot plus the existing manual heap-storage foundation for its Advanced priority frontier. Topological Sort is now live and reuses this Graph snapshot with Kahn indegree/FIFO and DFS reverse-postorder state. MST remains future and should reuse the same Graph/Heap foundations rather than duplicate them.

Traversal snapshots include the adjacent vertex index on each `GraphNeighborSnapshot`. BFS/DFS can therefore follow an adjacency entry directly instead of performing a hidden linear vertex lookup for every edge.

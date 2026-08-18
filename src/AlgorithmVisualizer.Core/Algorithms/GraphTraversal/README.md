# Graph traversal algorithms

This folder contains the hand-written BFS and DFS learning algorithms.

- **BFS** uses the project's `ManualDynamicArray<int>` plus a manual head cursor as a FIFO queue, so dequeue is cursor movement rather than array shifting, and marks vertices when they are enqueued.
- **DFS · Recursive** exposes recursive call frames and backtracking.
- **DFS · Iterative** uses `ManualDynamicArray<int>` as an explicit LIFO stack.
- Both consume the existing `GraphSnapshot`, so directed/undirected adjacency semantics come from the already implemented Graph module. Each neighbor snapshot carries its target vertex index, keeping adjacency traversal O(1) per inspected entry.
- Edge weights are intentionally ignored by BFS/DFS traversal order; weighted shortest paths belong to Dijkstra later.
- No framework `Queue<T>`, `Stack<T>`, graph library, LINQ traversal, or JavaScript implements the taught traversal behavior.

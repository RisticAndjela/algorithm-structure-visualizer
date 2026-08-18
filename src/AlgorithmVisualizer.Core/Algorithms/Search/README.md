# Search and traversal algorithms

Search/traversal implementations live in Core and expose semantic playback states without depending on Blazor.

Implemented:

- Linear Search (`Algorithms/Search/Linear`)
- Binary Search (`Algorithms/Search/Binary`)
- Breadth-First Search / BFS (`Algorithms/GraphTraversal`)
- Depth-First Search / DFS (`Algorithms/GraphTraversal`)

BFS reuses the project Graph representation and a manual FIFO frontier. DFS reuses the same Graph and offers real recursive traversal plus a manual explicit-LIFO variant. Neither traversal delegates the taught behavior to framework `Queue<T>`/`Stack<T>` or an external graph library.

Future graph algorithms such as Dijkstra, topological sorting, and minimum spanning trees should build on the same existing Graph representation rather than creating a second graph model.

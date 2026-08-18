# Algorithm Visualizer Architecture

## Solution layout

```text
algorithm-structure-visualizer/
├─ AlgorithmVisualizer.sln
├─ README.md
├─ AGENTS.md
├─ src/
│  ├─ AlgorithmVisualizer.Core/
│  │  ├─ DataStructures/
│  │  ├─ Algorithms/Sorting/
│  │  ├─ Algorithms/Search/
│  │  ├─ Algorithms/GraphTraversal/
│  │  ├─ Algorithms/GraphShortestPath/Dijkstra/
│  │  ├─ Algorithms/GraphOrdering/Topological/
│  │  └─ Simulation/
│  ├─ AlgorithmVisualizer.Client/
│  │  ├─ Components/
│  │  ├─ Pages/
│  │  ├─ State/
│  │  ├─ Layout/
│  │  └─ wwwroot/
│  └─ AlgorithmVisualizer.Server/
│     ├─ Persistence/
│     │  └─ LearningStateDatabase.cs
│     ├─ Program.cs
│     └─ appsettings.json
└─ tests/
```

## Core / Client boundary

`AlgorithmVisualizer.Core` owns the algorithmic state and operations. It must remain independent of Blazor and CSS.

Core responsibilities include:

- manually implemented data structures and algorithms;
- mutation and traversal rules;
- renderer-neutral transient visual states;
- operation-result metadata;
- simulation-step descriptions;
- immutable snapshots when a module needs timeline playback.

`AlgorithmVisualizer.Client` owns presentation and learning workflow:

- Razor pages and components;
- SVG/HTML rendering;
- Visual state vs Memory state tabs;
- playback history review;
- guided tasks;
- result popups;
- learning preferences/progress through the C# persistence API.

## Simulation runtime

Algorithms depend on the UI-neutral `ISimulationRuntime` contract.

The Blazor client registers `SimulationState` as the scoped implementation. In Blazor WebAssembly, that scoped service lives for the current browser application's lifetime.

For every meaningful step, Core code can:

1. update renderer-neutral algorithm state;
2. publish a concise step description;
3. await `WaitForNextStepAsync`;
4. continue only when playback allows it.

This supports Play, Pause, Step forward, adjustable delay, and cancellation without moving playback concerns into the algorithms.

`Step back` does not reverse C# execution. Client pages capture immutable display snapshots and let the learner review older frames. Returning to the live edge allows the actual run to continue.

## From-scratch algorithm rule

The application is a teaching tool, so the implementation being taught must not be hidden behind a ready-made collection or library algorithm.

Examples:

- Queue/Stack use the custom `ManualDynamicArray<T>` rather than `Queue<T>`, `Stack<T>`, `List<T>`, or `Array.Copy` for the taught storage operations.
- BST uses explicit `BstNode` parent/left/right references and manual comparison/transplant logic rather than a sorted collection or library tree. Its optional `Balance BST` action runs manual Day-Stout-Warren rotations over those same nodes; ordinary BST mutations remain non-self-balancing.
- AVL uses explicit `AvlNode` references, manually maintained cached heights, balance-factor checks, and explicit left/right rotations rather than a library balancing structure.
- Red-Black Tree uses explicit `RedBlackNode` references and a color field, treats null children as conceptual black NIL leaves, and implements recoloring plus insertion/deletion fix-up with manual rotations rather than a library balanced tree.
- Heap family modules use a shared custom raw-array-backed `ManualHeapArray<HeapElement>`, explicit index arithmetic, and manual swaps rather than `PriorityQueue`, `List`, sorting, or another library heap. `HeapSimulation` is the Binary Heap (`d=2`) specialization; `DaryHeapSimulation` generalizes relationships to configurable `d`.
- BFS consumes the existing Graph snapshot and implements FIFO scheduling over the project's manual dynamic-array storage with a head cursor rather than framework `Queue<T>`; dequeue advances the cursor instead of shifting the array. `GraphNeighborSnapshot.VertexIndex` gives BFS/DFS direct access to the adjacent vertex in the immutable snapshot, preserving the taught `O(V + E)` traversal rather than hiding an `O(V)` lookup inside each edge inspection. DFS consumes the same Graph; recursive mode exposes real call-stack/backtracking behavior and iterative mode uses the same manual storage as an explicit LIFO frontier rather than framework `Stack<T>`.
- Dijkstra consumes the same Graph snapshot. Basic mode selects the next finite unsettled minimum with an explicit linear scan. Advanced mode uses a Dijkstra-specific binary min-heap frontier built on the existing `ManualHeapArray<T>` storage, with lazy duplicate entries instead of framework `PriorityQueue<TElement,TPriority>`. Both variants share the same explicit relaxation logic and reject negative edge weights before traversal.

Infrastructure types such as `Task`, `CancellationToken`, `SemaphoreSlim`, `Guid`, Blazor services, ASP.NET Core, `HttpClient`, and `Microsoft.Data.Sqlite` are allowed because they do not implement the taught algorithm. Project-owned JavaScript and JS interop are intentionally excluded. Learning progress/preferences are persisted by the C# Server project through explicit parameterized SQL in SQLite.

## Visual state vs Memory state

Every live data-structure module separates two models:

- **Visual state** — the arrangement that best explains the data-structure rule.
- **Memory state** — a simplified but truthful view of the concrete C# storage/reference model.

Current examples:

- Stack/Queue visual order differs from the custom backing-array slot view.
- BST tree coordinates communicate key ordering, while Memory state shows the root reference and parent/left/right node references. During DSW balancing, timeline snapshots expose the same node IDs while those references are rewired by rotations.
- AVL visual state adds cached height, balance factor, and rotation states; its Memory state shows the same node identities and the real reference rewiring performed by rotations.
- Red-Black visual state separates persistent RED/BLACK node color from temporary fix-up emphasis; its Memory state shows color plus root/parent/left/right references and explains null child references as conceptual black NIL leaves.
- Heap visual state shows the complete binary tree and synchronized array positions; its Memory state shows the actual used/reserved backing-array slots, stable element IDs, Count/Capacity, and calculated parent/child indexes.

Learning labels such as `MEM-#A1B2C3` represent object identity only; they are not physical RAM addresses.

## Current live modules

### Queue & Stack

The linear reference module implements add/remove, search by value/ID, delete by value/ID, reset, timeline playback, memory view, run explanations, and guided practice.

### Binary Search Tree

The first tree module implements:

- insert;
- search by key;
- delete by key;
- leaf / one-child / two-child deletion;
- in-order successor search;
- manual node-reference transplant;
- strict duplicate rejection;
- tree-height reporting;
- tree visual state;
- node-reference Memory state;
- playback history;
- result explanations;
- guided practice.

### AVL Tree

The second tree module builds on the BST ordering model and adds:

- cached node heights and balance factors;
- upward rebalance checks after insert and delete;
- LL and RR single-rotation repairs;
- LR and RL double-rotation repairs;
- explicit parent/left/right pointer rewiring;
- AVL-specific visual states for unbalanced nodes, rotation pivots, active rotation, and restored balance;
- Memory state with node identity, links, height, and balance factor;
- rotation-focused guided practice and persistent progress.

### Red-Black Tree

The third tree module keeps the same strict BST ordering and adds:

- explicit red/black node color and conceptual black NIL semantics for null children;
- root-black, no-red-red, and equal-black-height invariants;
- insertion fix-up through red-uncle recoloring, triangle repair, and line repair;
- BST structural deletion plus color-aware delete fix-up using sibling/near/far child cases;
- real successor-node transplant for two-child deletion;
- explicit identity-preserving left/right pointer rotations;
- Visual state that separates persistent node color from temporary fix-up states;
- Memory state with color plus root/parent/left/right references;
- black-height metrics, result explanations, and guided practice with persistent progress.


### Generalized d-ary Heap

The broader Heap learning module makes the heap-family distinction concrete without inventing an inaccurate “ordinary heap” type:

- configurable branching factor `d` (Core 2..8; UI presets 2/3/4/5);
- Min/Max priority rule;
- complete d-ary shape encoded in the same custom raw array;
- `parent=(i-1)/d`, children `di+1..di+d`;
- append + bubble-up insertion;
- last-to-root + bubble-down extraction;
- bubble-down child-candidate selection across up to `d` existing children;
- linear arbitrary search and delete-by-value repair;
- Visual and Memory views that compare directly with Binary Heap;
- arity changes only while empty.

### Binary Heap

The dedicated Binary Heap specialization adds:

- Min Heap and Max Heap semantics;
- complete-tree shape encoded by array indexes;
- custom manual array growth;
- stable `HeapElement` identity across index swaps;
- insert through append + bubble-up;
- extract-root through last-element replacement + bubble-down;
- truthful O(n) arbitrary search;
- delete-by-value with linear locate plus upward/downward repair;
- Visual state with tree + array synchronization;
- Memory state with used/reserved slots and capacity;
- heap-specific guided practice and persistent progress.

### Graph

The Graph structure module adds canonical `GraphVertex`/`GraphEdge` objects, manually stored adjacency lists, and a synchronized adjacency matrix backed by the existing `ManualMatrix`. Graph Core has no fixed eight-vertex cap; `ManualMatrix` is reusable storage whose dimensions follow the graph, while MatrixPage keeps its separate 8×8 teaching/input limit. The Client renders topology separately from representation memory, scrolls large matrix views internally, and uses ring-then-grid automatic topology placement before optional manual dragging. BFS and DFS are now live algorithm modules that consume this exact Graph snapshot instead of introducing a second graph representation. Dijkstra now follows this reuse pattern: it consumes the same Graph snapshot and adds renderer-neutral shortest-path state. Topological Sort and Prim/Kruskal MST now follow the same reuse pattern.

### Breadth-First Search and Depth-First Search

Both graph traversals keep Core independent from Blazor and use the existing renderer-neutral simulation runtime. BFS records `visited[]`, `parent[]`, unweighted `distance[]`, visit order, edge checks and its FIFO frontier. DFS records `visited[]`, `parent[]`, `depth[]`, visit order, edge checks and either recursive call-stack or explicit-LIFO state. Directed graphs follow outgoing adjacency only; weights are deliberately ignored. Graph mutation invalidates the current traversal snapshot and requires restart.

### Dijkstra

Dijkstra reuses the immutable Graph snapshot and renderer-neutral simulation runtime. Core owns `dist[]`, `parent[]`, `settled[]`, settlement order, relaxation metrics and either a linear tentative-set scan or a custom binary min-heap frontier. Negative weights are validated and rejected before a run; zero weights are valid. The Client adds only learning/presentation state and persists practice evidence through the existing C# API + SQLite path.

## Planned extension path

Future graph/path modules should reuse the same Core/runtime/Client separation rather than duplicating playback infrastructure. Dijkstra is live: Basic mode uses manual linear minimum selection and Advanced mode uses a Dijkstra-specific binary min-heap built on the existing `ManualHeapArray` storage priority frontier with lazy stale entries. Topological Sort is live and reuses the established Graph plus the manual Queue/recursive DFS foundations. MST is also live: Prim reuses `ManualHeapArray` for its min-edge frontier and Kruskal uses a hand-written DSU with path compression and union by rank.


## Matrix module

The Matrix lab is a Core/runtime/Client implementation inserted before Graph because the live Graph module reuses its row-major adjacency-matrix storage. The Client exposes two equivalent data-entry paths for A and B: direct cell editing and validated bulk row input; bulk input auto-resizes the existing Core matrix and then delegates all computation to the same `MatrixSimulation` algorithms.

- `Core/DataStructures/Matrix/ManualMatrix.cs` owns row-major `double[]` storage, indexing, resize and elementary row primitives. Core dimensions are not capped at 8×8; the standalone Matrix learning page imposes that smaller UI limit for readability.
- `MatrixSimulation` owns A, B and derived-result workspaces plus semantic cell states.
- arithmetic, multiplication, transpose, powers, determinant, minors/cofactors, REF/RREF/rank, inverse and `A·X=B` solving are manual algorithms; no numerical library is used.
- Client matrix editors mutate only A/B between runs; algorithmic operations publish intermediate frames through the shared `SimulationState` playback runtime.
- Visual state presents mathematical grids and active cells; Memory state exposes contiguous row-major slots and the flat-index formula.
- Graph reuses this Matrix implementation for adjacency-matrix storage/education instead of implementing a duplicate matrix engine.

The Matrix page established a readability floor that Graph now tightens: normal explanatory/help copy is approximately 0.95rem or larger, playback text is near 1rem, and only truly secondary uppercase metadata may sit near 0.82rem.


### Topological Sort

Topological Sort consumes the immutable directed `GraphSnapshot` and adds only renderer-neutral ordering state. Kahn mode owns `indegree[]`, a head-index FIFO backed by the existing manual dynamic-array storage, and output order. DFS mode owns white/gray/black color state, recursion path, manual postorder and final reversed output. Both variants inspect vertices/adjacency in `O(V + E)`, keep `O(V)` extra state, ignore weights, reject undirected input before a run, and report cycles instead of accepting a partial ordering. The Client layer owns presets, prediction, playback frames, Visual/Memory presentation and SQLite-backed practice evidence.

### Graph visual workspace

Graph drag coordinates are Client-only world coordinates keyed by stable vertex ID. They never enter Core graph topology. The SVG stage is a content-bounded unbounded workspace: it dynamically expands only to the current visual extents (plus rendering padding), supports negative world coordinates through a render-origin offset, and compensates scroll when that origin moves left/up.

## Server / persistence boundary

`AlgorithmVisualizer.Server` hosts the Blazor WebAssembly static assets and a same-origin learning-state API. The Client never talks to browser `localStorage`; `LearningSessionStore` loads persisted key/value state with `HttpClient` before the first Razor page renders and mirrors later changes back to the server.

The Server owns persistence only. It must not contain sorting, tree, graph, heap, matrix, traversal, or simulation logic. SQLite stores learner state in `LearningState(UserId, StateKey, StateValue, UpdatedAtUtc)`. `UserId` comes from a long-lived HttpOnly anonymous cookie created by the server. Database access uses `Microsoft.Data.Sqlite` with parameterized SQL and an upsert on `(UserId, StateKey)`.

This preserves the architectural rule: Core teaches algorithms, Client visualizes them, and Server persists learning state.


## Vector / AI-ML numerical foundation

`DataStructures/Vector/ManualVector` is a reusable numerical primitive stored in a contiguous project-owned `double[]`. It belongs in Core because later ML algorithms need vector arithmetic independently of rendering. `VectorSimulation` owns renderer-neutral state for aligned component reads, result writes and scalar reductions. The Client owns text parsing, operation selection, Visual/Memory presentation, prediction, playback review, practice state and SQLite evidence.

The Vector module must remain dependency-light: no numerical package, `System.Numerics.Vector<T>`, or framework search/aggregation helper may replace the explicit loops being taught. Gradient Descent now reuses this Core for L2 norm, scalar multiplication and subtraction; later regression, KNN, K-Means and PCA modules should follow the same composition rule.


## Gradient Descent / ML optimization boundary

`MachineLearning/Optimization/GradientDescent/GradientDescentSimulation` owns optimizer-specific orchestration and the analytical gradient of the lesson's convex quadratic objective. It does **not** own generic vector arithmetic. An internal immediate `ISimulationRuntime` lets it call the existing renderer-neutral `VectorSimulation` for gradient L2 norm, scalar multiplication and subtraction without replaying nested Vector lesson steps into the outer Gradient Descent timeline.

The live Client route `/ml-foundations/gradient-descent` owns input parsing, Basic/Advanced variant selection, categorical prediction, loss-landscape/path rendering, Visual/Memory presentation, playback history, popup explanations and persisted practice evidence. The Core remains renderer-neutral. Runtime working state is O(n); Client-visible review history intentionally stores O(k·n) snapshots to support rewind and visualization.

Linear Regression at `/ml-foundations/linear-regression` is currently a planned `LearningPlaceholder`. When implemented, model-specific prediction/loss/gradient logic should compose with the existing Vector and Gradient Descent foundations rather than moving numerical behavior into Client or Server code.

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
│  │  │  ├─ Linear/
│  │  │  │  ├─ ManualDynamicArray.cs
│  │  │  │  ├─ Stack/
│  │  │  │  └─ Queue/
│  │  │  ├─ Trees/
│  │  │  │  ├─ Bst/             # implemented
│  │  │  │  ├─ Avl/             # implemented
│  │  │  │  └─ RedBlack/        # implemented
│  │  │  └─ Heap/               # live generalized d-ary Heap + Binary Heap
│  │  ├─ Algorithms/Sorting/    # planned modules
│  │  └─ Simulation/
│  │     ├─ Contracts/
│  │     └─ SimulationAlgorithmBase.cs
│  └─ AlgorithmVisualizer.Client/
│     ├─ Components/
│     │  ├─ Common/
│     │  └─ Visualization/
│     │     ├─ Linear/
│     │     ├─ Trees/
│     │     └─ Heap/
│     ├─ Pages/
│     │  ├─ DataStructures/
│     │  ├─ Learn/
│     │  └─ Sorting/
│     ├─ State/
│     ├─ Layout/
│     └─ wwwroot/
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
- browser-local preferences/progress.

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

Infrastructure types such as `Task`, `CancellationToken`, `SemaphoreSlim`, `Guid`, Blazor services, and browser storage interop are allowed because they do not implement the taught algorithm.

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

## Planned extension path

Graph and sorting modules should reuse the same Core/runtime/Client separation rather than duplicating playback infrastructure. Future modules may reuse the established Stack/Queue/BST/AVL/Red-Black/generalized-Heap/Binary-Heap implementations when they are genuine algorithmic dependencies.


## Matrix module

The Matrix lab is a Core/runtime/Client implementation inserted before Graphs because adjacency matrices are a planned graph representation. The Client exposes two equivalent data-entry paths for A and B: direct cell editing and validated bulk row input; bulk input auto-resizes the existing Core matrix and then delegates all computation to the same `MatrixSimulation` algorithms.

- `Core/DataStructures/Matrix/ManualMatrix.cs` owns row-major `double[]` storage, indexing, resize and elementary row primitives.
- `MatrixSimulation` owns A, B and derived-result workspaces plus semantic cell states.
- arithmetic, multiplication, transpose, powers, determinant, minors/cofactors, REF/RREF/rank, inverse and `A·X=B` solving are manual algorithms; no numerical library is used.
- Client matrix editors mutate only A/B between runs; algorithmic operations publish intermediate frames through the shared `SimulationState` playback runtime.
- Visual state presents mathematical grids and active cells; Memory state exposes contiguous row-major slots and the flat-index formula.
- Graphs should reuse this Matrix implementation for adjacency-matrix storage/education instead of implementing a duplicate matrix engine.

The Matrix page also establishes a readability floor for new modules: normal explanatory copy is approximately 0.9rem or larger, and tiny labels are avoided.

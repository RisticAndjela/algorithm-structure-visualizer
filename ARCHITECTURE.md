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
│  │  │  └─ Trees/
│  │  │     ├─ Bst/             # implemented
│  │  │     ├─ Avl/             # implemented
│  │  │     └─ RedBlack/        # live Red-Black Tree
│  │  ├─ Algorithms/Sorting/    # planned modules
│  │  └─ Simulation/
│  │     ├─ Contracts/
│  │     └─ SimulationAlgorithmBase.cs
│  └─ AlgorithmVisualizer.Client/
│     ├─ Components/
│     │  ├─ Common/
│     │  └─ Visualization/
│     │     ├─ Linear/
│     │     └─ Trees/
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

## Planned extension path

Heap, Graph, and sorting modules should reuse the same Core/runtime/Client separation rather than duplicating playback infrastructure. Future modules may reuse the established Stack/Queue/BST/AVL/Red-Black implementations when they are genuine algorithmic dependencies.

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
│  │  │     ├─ Avl/             # planned
│  │  │     └─ RedBlack/        # planned
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
- BST uses explicit `BstNode` parent/left/right references and manual comparison/transplant logic rather than a sorted collection or library tree.

Infrastructure types such as `Task`, `CancellationToken`, `SemaphoreSlim`, `Guid`, Blazor services, and browser storage interop are allowed because they do not implement the taught algorithm.

## Visual state vs Memory state

Every live data-structure module separates two models:

- **Visual state** — the arrangement that best explains the data-structure rule.
- **Memory state** — a simplified but truthful view of the concrete C# storage/reference model.

Current examples:

- Stack/Queue visual order differs from the custom backing-array slot view.
- BST SVG coordinates communicate key ordering, while Memory state shows the root reference and parent/left/right node references.

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
- SVG visual state;
- node-reference Memory state;
- playback history;
- result explanations;
- guided practice.

## Planned extension path

AVL is the natural next tree module because it can reuse the BST ordering/search concepts and tree visualization while adding explicit height/balance-factor maintenance and rotations. Other planned modules should reuse the same Core/runtime/Client separation rather than duplicating playback infrastructure.

# Phase 1 — Project Structure

## Layout

```text
algorithm-visualizer/
├─ Directory.Build.props
├─ ARCHITECTURE.md
├─ src/
│  ├─ AlgorithmVisualizer.Core/              # Pure C# logic with no Blazor dependency
│  │  ├─ DataStructures/
│  │  │  ├─ Trees/
│  │  │  │  ├─ Bst/
│  │  │  │  ├─ Avl/
│  │  │  │  └─ RedBlack/
│  │  │  ├─ Heap/
│  │  │  ├─ Graph/
│  │  │  └─ Linear/
│  │  │     ├─ Queue/
│  │  │     └─ Stack/
│  │  ├─ Algorithms/
│  │  │  └─ Sorting/
│  │  │     ├─ Bubble/
│  │  │     ├─ Selection/
│  │  │     ├─ Insertion/
│  │  │     ├─ Merge/
│  │  │     ├─ Quick/
│  │  │     └─ HeapSort/
│  │  └─ Simulation/
│  │     ├─ Contracts/                       # Future simulation-step contracts
│  │     └─ Models/                          # UI-neutral step snapshots and models
│  └─ AlgorithmVisualizer.Client/            # Blazor WebAssembly presentation layer
│     ├─ Components/
│     │  ├─ Common/                          # Shared playback controls
│     │  └─ Visualization/
│     │     ├─ Trees/
│     │     ├─ Graphs/
│     │     ├─ Linear/
│     │     └─ Sorting/
│     ├─ Pages/
│     │  ├─ DataStructures/                  # One routable page per data structure
│     │  └─ Sorting/                         # One routable page per sorting algorithm
│     ├─ State/                              # UI/playback state, not algorithm logic
│     ├─ Services/Animation/                 # Future asynchronous animation runner
│     ├─ Layout/
│     └─ wwwroot/css/
└─ tests/
   └─ AlgorithmVisualizer.Core.Tests/        # Unit tests for pure Core logic
```

## Animation State Management

A small explicit state container is sufficient for this project; a Redux/Flux-style library is not required for Phase 1.

`SimulationState` is registered as a scoped service. In Blazor WebAssembly, a scoped service effectively lives for the lifetime of the application in the current browser tab.

The state container stores playback/UI state only: whether a simulation is running or paused, the current step, the total step count, and the delay between steps. It must not contain BST, AVL, graph, heap, or sorting algorithm logic.

In a later phase, pure Core logic will produce UI-neutral `SimulationStep` snapshots or events. An `AnimationRunner` in the Client project will replay them asynchronously with `async/await`, `Task.Delay`, and `CancellationToken`. `SimulationState` will expose the current playback speed and pause/resume state, while Blazor components will render the current snapshot, primarily with SVG. This keeps the frontend C#-first and avoids a traditional JavaScript/TypeScript application layer.

This separation provides:
- deterministic, independently testable algorithm logic;
- playback speed and pause controls that do not leak into algorithm implementations;
- the ability to provide multiple visual representations for the same algorithm;
- minimal coupling between the Core and Blazor layers.

## Phase 1 Boundary

This package does not implement BST, AVL, Red-Black Tree, Heap, Graph, Queue, Stack, or sorting algorithms. The pages are intentional placeholders. The only concrete behavior beyond the Blazor application shell is the generic playback state and shared simulation control component, which make the architectural boundary explicit from the start.

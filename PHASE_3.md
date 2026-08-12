# Phase 3 — Queue and Stack

Phase 3 is intentionally limited to the first linear data-structure module. It does not implement trees, graphs, heaps, or sorting algorithms.

## Architecture

The implementation keeps three responsibilities separate:

1. **Core simulation state** — `StackSimulation` and `QueueSimulation` own values and semantic transient states such as `Adding`, `Removing`, and `PointerTarget`.
2. **Playback runtime** — the Phase 2 `ISimulationRuntime` controls delay, pause, resume, single-step execution, and cancellation.
3. **Blazor rendering** — Razor components translate semantic element state into layout, labels, and CSS highlights.

Core code never references Razor, CSS, HTML, or browser APIs.

## Async operation flow

Each public mutation is asynchronous and uses the same pattern:

```csharp
item.VisualState = LinearElementVisualState.Removing;
NotifyChanged();

await NextStepAsync(
    $"Removing {item.Value} from the front of the queue.",
    cancellationToken);

// Apply the next mutation and publish another visual state.
```

`NextStepAsync` publishes the current operation through the shared runtime and then waits according to the active playback mode. The same pattern can later be called inside loops or recursive methods because the cancellation token is passed through every nested call.

## Why intermediate UI updates work

Blazor normally renders after an event callback completes, but a simulation operation can remain alive across several awaited steps. Waiting for the entire button callback to finish would therefore hide intermediate states.

`StackSimulation` and `QueueSimulation` expose a `Changed` event. `StackQueueSandbox.razor` subscribes to those events and schedules a render with:

```csharp
_ = InvokeAsync(StateHasChanged);
```

`InvokeAsync` is used instead of calling `StateHasChanged` directly so the render is scheduled through Blazor's renderer synchronization context. The page also subscribes to `SimulationState.Changed`, which keeps button availability synchronized with Play, Pause, Step, Reset, and completion state.

The subscriptions are removed in `Dispose` to prevent stale component references.

## Reset versus Clear

The shared toolbar's **Reset** command belongs to playback infrastructure. It cancels the active simulation and resets controller state.

**Clear Stack** and **Clear Queue** are data-structure operations. They intentionally remain separate and are animated through the same playback engine.

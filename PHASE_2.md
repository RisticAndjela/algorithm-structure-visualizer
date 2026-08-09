# Phase 2: Simulation Infrastructure

This phase adds playback infrastructure only. No tree, graph, heap, queue, stack, or sorting implementation is included.

## Responsibilities

### `SimulationState`

`SimulationState` is the browser-lifetime playback controller for Blazor WebAssembly. It owns only simulation playback concerns:

- step delay in milliseconds;
- running and paused state;
- one-step release while paused;
- the current user-facing operation text;
- cancellation for the active simulation run;
- notifications used by Blazor components.

Algorithm-specific state does not belong in this service.

### `ISimulationRuntime`

Core algorithms depend on this interface instead of depending on Blazor or the client project. This preserves the dependency direction:

`AlgorithmVisualizer.Client -> AlgorithmVisualizer.Core`

The Core project remains unaware of Razor components and browser UI concerns.

### `SimulationAlgorithmBase`

This optional base class provides one protected helper:

```csharp
await NextStepAsync("Comparing the current value with the pivot.", cancellationToken);
```

The helper first publishes the textual operation and then waits for the playback controller.

## Starting and finishing a simulation

A future page or page-level coordinator starts a run immediately before invoking an algorithm:

```csharp
var cancellationToken = SimulationState.Start("Preparing simulation.");

try
{
    await algorithm.RunAsync(input, cancellationToken);
    SimulationState.Complete("Simulation completed.");
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    // Reset already returned the UI to its initial state.
}
```

`Reset()` cancels the active token, so loops, recursion, and pending animation waits can stop without polling a custom flag.

## Using the runtime in a loop

A future algorithm can publish a visual state and then wait at a deliberate visualization boundary:

```csharp
for (var index = 0; index < values.Count; index++)
{
    cancellationToken.ThrowIfCancellationRequested();

    // Update algorithm state that the visualization reads here.

    await NextStepAsync(
        $"Inspecting value {values[index]} at index {index}.",
        cancellationToken);
}
```

Do not add a wait after every line of code. Add waits only after meaningful state transitions that a student should be able to observe.

## Using the runtime in recursion

The same cancellation token must be passed through recursive calls:

```csharp
private async Task VisitAsync(Node node, CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    await NextStepAsync(
        $"Visiting node {node.Value}.",
        cancellationToken);

    if (node.Left is not null)
    {
        await VisitAsync(node.Left, cancellationToken);
    }
}
```

This keeps pause, single-step, speed changes, and reset behavior consistent at every recursion depth.

## Playback semantics

- **Play** resumes an already running simulation.
- **Pause** blocks the algorithm at the next playback boundary.
- **Step** switches to paused mode and releases exactly one playback boundary.
- **Reset** cancels the active run and restores the controller to `Ready.`.
- Changing the step delay interrupts the current timing wait and applies the new value immediately.

The toolbar does not start a specific algorithm because it intentionally has no knowledge of which page or operation is being simulated. The page-level coordinator is responsible for calling `Start()` and invoking its selected operation.

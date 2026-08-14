# Core Tests

Focused tests for pure Core algorithms and data structures live here. Rendering is intentionally excluded.

## Current coverage

`DataStructures/Trees/Bst/BstSimulationTests.cs` verifies:

- ordered insertion shape;
- strict duplicate rejection;
- found and missing search paths;
- leaf deletion;
- one-child deletion while preserving child object identity;
- two-child deletion using the actual successor node identity;
- height growth for a skewed insertion order.

The tests use a tiny immediate `ISimulationRuntime` fake so algorithm correctness is independent from Blazor playback timing.

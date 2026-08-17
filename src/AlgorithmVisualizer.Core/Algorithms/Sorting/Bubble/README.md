# Bubble Sort

`BubbleSortSimulation` is the first live sorting algorithm in Core.

Implementation rules:

- fixed raw `BubbleSortElement[]` storage for a run;
- adjacent comparisons only: indexes `j` and `j + 1`;
- explicit manual swap using one temporary element reference;
- ascending order;
- swap only when `left.Value > right.Value`, preserving duplicate stability;
- after each pass, the largest remaining value joins the sorted suffix;
- each next pass stops one position earlier;
- a complete pass with zero swaps triggers the optimized early exit;
- all visible timing flows through `ISimulationRuntime`; no artificial algorithm-side delays.

The snapshot exposes active pair indexes, comparisons, swaps, pass-local swaps, sorted-suffix boundary, early-exit state, element identities, and renderer-neutral visual states.

Complexity:

- best case: `Θ(n)`;
- average case: `Θ(n²)`;
- worst case: `Θ(n²)`;
- extra algorithmic space: `O(1)`.

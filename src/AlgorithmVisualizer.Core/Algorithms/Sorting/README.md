# Sorting algorithms

Sorting algorithm logic belongs in this pure C# Core layer. It must remain independent from Blazor, CSS, DOM APIs, and browser state.

## Live

- **Bubble Sort** — manual stable adjacent-pair sort with both Basic canonical passes and an Optimized no-swap early-exit variant.
- **Selection Sort** — manual minimum-scan sort with Classic low-swap placement and an advanced Stable Shift placement strategy.

Both current sorting modules are snapshot based: arbitrary insert/delete/update changes require the current run to restart rather than continuing against invalidated indexes or invariants.

## Planned

- Insertion Sort
- Merge Sort
- Quick Sort
- Heap Sort

Every future sorter must implement the taught behavior directly. Do not delegate to `Array.Sort`, `List.Sort`, LINQ ordering, or another framework/library sorter.

Every future sorting lab must also document and expose, where applicable:

- when the algorithm is a good or poor practical choice;
- whether live CRUD/data mutations can be incorporated safely or require restart;
- the canonical/basic implementation;
- a meaningful advanced/optimized variant when one exists, with honest trade-offs;
- links to a better algorithm when another strategy is more appropriate for the workload.

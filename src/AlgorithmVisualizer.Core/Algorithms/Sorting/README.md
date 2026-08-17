# Sorting algorithms

Sorting algorithm logic belongs in this pure C# Core layer. It must remain independent from Blazor, CSS, DOM APIs, and browser state.

## Live

- **Bubble Sort** — manual stable adjacent-pair sort with both Basic canonical passes and an Optimized no-swap early-exit variant.
- **Selection Sort** — manual minimum-scan sort with Classic low-swap placement and an advanced Stable Shift placement strategy.
- **Insertion Sort** — stable incremental prefix sort with canonical Linear search and advanced Binary Insertion search.

Bubble and Selection runs are snapshot based: arbitrary insert/delete/update changes require restart because their active invariants refer to the old data. Insertion Sort is algorithmically online: once sorted, new values can be inserted incrementally, delete preserves order, and update can be repaired by remove + reinsert. The UI playback trace itself still restarts after mutation because recorded frames describe the old snapshot.

## Planned

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

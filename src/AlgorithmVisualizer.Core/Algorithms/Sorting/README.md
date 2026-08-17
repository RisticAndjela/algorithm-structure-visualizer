# Sorting algorithms

Sorting algorithm logic belongs in this pure C# Core layer. It must remain independent from Blazor, CSS, DOM APIs, and browser state.

## Live

- **Bubble Sort** — manual stable ascending adjacent-pair sort with a no-swap early exit.

## Planned

- Selection Sort
- Insertion Sort
- Merge Sort
- Quick Sort
- Heap Sort

Every future sorter must implement the taught behavior directly. Do not delegate to `Array.Sort`, `List.Sort`, LINQ ordering, or another framework/library sorter.

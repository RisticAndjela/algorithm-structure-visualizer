# Quick Sort

The live Quick Sort module contains two manual in-place implementations over `QuickSortElement[]`.

- **Basic — Lomuto / last pivot:** the final element of each active range is the pivot. One boundary grows the `<= pivot` region, then the pivot is moved into its final index.
- **Advanced — median-of-three + three-way partition:** first/middle/last values are inspected to choose a median pivot value, then a Dutch-national-flag partition builds `< pivot`, `= pivot`, and `> pivot` regions. The equal band is finalized immediately, which avoids recursively processing every duplicate.

Neither variant delegates to `Array.Sort`, `List.Sort`, LINQ ordering, or another sorter. Both mutate the same teaching array with manual reference swaps and are intentionally **not stable**.

Complexity taught by the module:

- Basic best/average: `Θ(n log n)` when partitions stay reasonably balanced;
- Advanced best case: `Θ(n)` when all values are equal because one three-way partition finalizes the entire `= pivot` band; typical/average behavior remains `Θ(n log n)`;
- worst: `Θ(n²)` when partitioning repeatedly produces extremely unbalanced ranges;
- extra array storage: `O(1)`;
- recursion stack: `O(log n)` average, `O(n)` worst.

The active trace requires a fixed snapshot. Reads are safe, but create/update/delete changes indexes, pivots, and recursion ranges, so a mutation requires a new Quick Sort run.

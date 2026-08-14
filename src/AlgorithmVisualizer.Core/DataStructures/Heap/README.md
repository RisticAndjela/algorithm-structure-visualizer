# Binary Heap

The live Heap module is implemented from scratch in Core as an array-backed **Min Heap / Max Heap**.

## Storage

`HeapSimulation` owns a custom `ManualHeapArray<HeapElement>` backed by `T[]`. It does not use `PriorityQueue<TElement,TPriority>`, `List<T>`, `SortedSet<T>`, `Array.Sort`, or another ready-made heap implementation.

The complete binary-tree shape is encoded by array indexes:

```text
parent(i) = (i - 1) / 2
left(i)   = 2i + 1
right(i)  = 2i + 2
```

Each `HeapElement` has stable object identity (`Guid`) even when swaps move the element reference to a different array index.

## Implemented operations

- choose Min Heap or Max Heap while the heap is empty;
- insert with bubble-up;
- extract root with last-element replacement + bubble-down;
- search by value with a truthful linear scan;
- delete the first matching value, then repair upward or downward as required;
- clear.

Duplicates are allowed because heap order does not require unique keys.

## Complexity taught by the module

- root access: `O(1)` conceptually;
- insert: `O(log n)` worst case;
- extract root: `O(log n)` worst case;
- arbitrary value search: `O(n)`;
- delete by arbitrary value: `O(n)` overall because locating the value is linear even though the repair is only `O(log n)`.

The module deliberately shows both the complete-tree view and the backing-array view so learners can connect tree relationships to actual indexes and capacity changes.

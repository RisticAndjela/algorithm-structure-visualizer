# Heap family — one generalized implementation

`DaryHeapSimulation` is the single teaching implementation for the Heap family. **Binary Heap is the exact `d = 2` configuration**, while larger branching factors use the same operations, storage, snapshots, and explanations. The compatibility route `/structures/binary-heap` therefore does not require a separate Core engine.

## Storage

The simulation owns a custom `ManualHeapArray<HeapElement>` backed by `T[]`. It does not use `PriorityQueue<TElement,TPriority>`, `List<T>`, `SortedSet<T>`, `Array.Sort`, or another ready-made heap. Each `HeapElement` keeps stable `Guid` identity while swaps move its reference between array indexes.

For branching factor `d`:

```text
parent(i)   = (i - 1) / d
children(i) = di + 1 ... di + d
```

Core accepts `d` from 2 through 8. Changing `d` or Min/Max mode is rejected while non-empty because either change would reinterpret the current array and therefore requires a visible rebuild operation.

## Operations

- insert: append + bubble-up;
- extract root: last-element replacement + bubble-down;
- search by value: explicit linear slot scan;
- search by generated short ID: explicit linear slot scan, because ID is identity rather than heap priority;
- delete first matching value: linear locate + replacement + upward/downward repair;
- clear;
- load a small valid starter heap while empty, after which all normal operations remain available.

Duplicates are allowed because heap ordering does not require unique keys. Bubble-down explicitly compares all existing child candidates at the current level before choosing the highest-priority child.

## Complexity

- root access: `O(1)`;
- insert/extract: `O(log_d n)` levels (`O(log n)` asymptotically);
- bubble-down may compare up to `d` children per visited level;
- arbitrary value search: `O(n)`;
- ID search without a separate identity index: `O(n)`;
- delete by arbitrary value: `O(n)` overall because location dominates logarithmic repair.

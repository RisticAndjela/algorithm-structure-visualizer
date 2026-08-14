# Heap family: generalized d-ary + Binary Heap

This folder contains two live heap teaching implementations that intentionally share storage primitives while teaching different levels of the concept.

## Terminology

- **Heap** is a family of priority-ordered structures.
- **Binary Heap** is the `d = 2` complete-tree specialization.
- The separate `/structures/heap` lab uses a **generalized d-ary heap** so the broader family can be simulated concretely instead of inventing a misleading “ordinary heap” structure.

Both implementations support Min Heap and Max Heap semantics.

## Shared storage rule

`HeapSimulation` and `DaryHeapSimulation` both own a custom `ManualHeapArray<HeapElement>` backed by `T[]`. They do not use `PriorityQueue<TElement,TPriority>`, `List<T>`, `SortedSet<T>`, `Array.Sort`, or another ready-made heap implementation.

Each `HeapElement` has stable object identity (`Guid`) even when swaps move the element reference to a different array index.

## Binary Heap (`HeapSimulation`)

The complete binary-tree shape is encoded by:

```text
parent(i) = (i - 1) / 2
left(i)   = 2i + 1
right(i)  = 2i + 2
```

## Generalized d-ary Heap (`DaryHeapSimulation`)

The same complete-array idea is generalized to branching factor `d`:

```text
parent(i)   = (i - 1) / d
children(i) = di + 1 ... di + d
```

Core accepts `d` from 2 through 8. The learning UI defaults to 3 and exposes 3/4/5 so the visual distinction from Binary Heap is obvious. `d = 2` is covered by tests and is mathematically the Binary Heap case.

Changing `d` or Min/Max mode is rejected while non-empty because either change would alter the ordering/relationship interpretation and therefore requires an explicit rebuild algorithm.

## Implemented operations

Both labs implement:

- choose Min Heap or Max Heap while empty;
- insert with append + bubble-up;
- extract root with last-element replacement + bubble-down;
- search by value with a truthful linear scan;
- delete the first matching value, then repair upward or downward;
- clear.

The d-ary bubble-down explicitly compares the existing children at the current level before choosing the highest-priority child.

Duplicates are allowed because heap order does not require unique keys.

## Complexity taught by the modules

- root access: `O(1)`;
- Binary Heap insert/extract: `O(log n)` worst case;
- d-ary insert/extract: logarithmic tree height (`O(log_d n)` levels, still `O(log n)` asymptotically);
- d-ary bubble-down can compare up to `d` child candidates at each visited level;
- arbitrary value search: `O(n)`;
- delete by arbitrary value: `O(n)` overall because locating the value is linear even though repair is logarithmic.

Both modules show Visual and Memory states so learners can connect conceptual tree relationships to actual raw-array slots and stable object identities.

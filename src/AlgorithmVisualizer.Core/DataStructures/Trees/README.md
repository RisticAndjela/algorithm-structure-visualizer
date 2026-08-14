# Trees

Pure C# tree algorithms and data structures live here. This layer must not depend on Blazor rendering code.

## Implemented

- `Bst/` — manually linked Binary Search Tree with insert, search, delete, reset, immutable display snapshots, height calculation, and semantic visual states.

## Planned

- `Avl/`
- `RedBlack/`

Tree implementations must follow the project-wide from-scratch rule: do not replace the taught structure with `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, another built-in sorted collection, or a library tree implementation.

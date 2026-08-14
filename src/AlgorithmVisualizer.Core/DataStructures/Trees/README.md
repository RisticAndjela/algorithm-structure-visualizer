# Trees

Pure C# tree algorithms and data structures live here. This layer must not depend on Blazor rendering code.

## Implemented

- `Bst/` — manually linked Binary Search Tree with insert, search, delete, reset, immutable display snapshots, height calculation, and semantic visual states.
- `Avl/` — manually linked AVL Tree that keeps the BST ordering rule, caches node heights, computes balance factors, implements LL/RR/LR/RL repairs with explicit pointer rotations, and rebalances upward after insert/delete.
- `RedBlack/` — manually linked Red-Black Tree with explicit node colors, conceptual black NIL leaves, insertion/deletion fix-up, recoloring, successor transplant, and identity-preserving rotations.

Tree implementations must follow the project-wide from-scratch rule: do not replace the taught structure with `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, another built-in sorted collection, or a library tree/balancing implementation.

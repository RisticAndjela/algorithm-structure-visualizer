# Red-Black Tree core

`RedBlackSimulation` is a from-scratch Red-Black Tree implementation used by the learning UI.

Implemented behavior:

- strict BST ordering with duplicate-key rejection;
- iterative search and insertion paths;
- new inserted nodes begin red, then insertion fix-up restores the invariants;
- insertion handles red-uncle recoloring plus triangle/line rotation cases and their mirror images;
- BST leaf / one-child / two-child deletion;
- two-child deletion transplants the real in-order-successor node object rather than copying its value;
- deletion tracks the color actually removed from a root-to-NIL path and runs black-height fix-up only when required;
- delete fix-up handles red sibling, black sibling with black children, near-red, and far-red cases plus their mirror images;
- explicit left/right pointer rotations preserve node identity and BST ordering;
- null child references are treated as conceptual black NIL leaves without allocating sentinel objects;
- renderer-neutral transient states support step-by-step playback;
- immutable snapshots expose node identity, color, and parent/child references.

The taught algorithm does not delegate to `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, `Dictionary<TKey,TValue>`, `List<T>`, or another ready-made balanced-tree implementation.

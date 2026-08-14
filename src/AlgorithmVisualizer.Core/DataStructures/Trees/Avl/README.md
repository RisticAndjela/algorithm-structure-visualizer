# AVL Tree core

`AvlSimulation` is a from-scratch AVL implementation used by the learning UI.

Implemented behavior:

- strict BST ordering with duplicate-key rejection;
- iterative search and insertion paths;
- BST leaf / one-child / two-child deletion;
- two-child deletion rewires the real in-order-successor node object instead of copying its value;
- cached node heights maintained manually;
- balance factor `height(left) - height(right)`;
- LL, RR, LR, and RL repair cases implemented with explicit left/right pointer rotations;
- upward rebalancing after insert and delete;
- renderer-neutral transient node states for step-by-step playback;
- immutable snapshots exposing node identity, parent/child links, height, and balance factor.

The taught algorithm does not delegate to `SortedSet<T>`, `Dictionary<TKey,TValue>`, `List<T>`, `PriorityQueue<T>`, or any other ready-made tree/balancing implementation.

# Binary Search Tree

Implemented from scratch in `BstSimulation` with explicit `BstNode` references.

## Implemented operations

- Insert
- Search by value
- Delete by value
- Reset lab state

The tree rejects duplicate keys and maintains the strict invariant:

```text
all keys in left subtree < node key < all keys in right subtree
```

Deletion explicitly simulates all three structural cases:

1. leaf;
2. one child;
3. two children using the in-order successor (leftmost node of the right subtree).

The two-child case rewires node references with a manual transplant operation. It does not delegate ordering, searching, or node removal to a built-in tree or sorted collection.

## Complexity

Search, insert, and delete are `O(h)`, where `h` is tree height. An approximately balanced tree has height near `log n`; a highly skewed tree can have height `n`.

## Visualization contract

Core exposes immutable `BstNodeSnapshot` values for timeline playback. Visual state and memory state are renderer concerns in the Client project; the Core algorithm only publishes semantic node states and simulation-step descriptions.

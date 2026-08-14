# Binary Search Tree

Implemented from scratch in `BstSimulation` with explicit `BstNode` references.

## Implemented operations

- Insert
- Search by value
- Delete by value
- Explicit Day-Stout-Warren balance
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

## Explicit balance

`BalanceAsync` is deliberately separate from `InsertAsync` and `DeleteAsync`: this remains an ordinary BST, so mutations do not self-balance.

The balance operation implements Day-Stout-Warren manually over the existing `BstNode` references:

1. scan the tree and use right rotations to remove all left links, producing a sorted right-only vine;
2. compute the largest perfect-tree node count `2^k - 1` that fits the current node count;
3. compress the vine with spaced left rotations;
4. repeat smaller compression passes until the tree is near-complete.

No node values or identities are copied into replacement nodes. `Count` and in-order key order stay unchanged; only `root`, `parent`, `left`, and `right` references are rewired.

Complexity: `O(n)` time and `O(1)` algorithmic extra space.

## Complexity

Search, insert, and delete are `O(h)`, where `h` is tree height. An approximately balanced tree has height near `log n`; a highly skewed tree can have height `n`. The explicit DSW balance operation is `O(n)` and is not part of normal insert/delete.

## Visualization contract

Core exposes immutable `BstNodeSnapshot` values for timeline playback. Visual state and memory state are renderer concerns in the Client project; the Core algorithm only publishes semantic node states and simulation-step descriptions.

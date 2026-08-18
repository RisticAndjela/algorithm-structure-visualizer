# Linear Search

Manual first-match Linear Search used by the learning UI.

- scans a fixed raw array from index `0` to `n - 1`;
- compares exactly one element per visited index;
- stops at the first matching value;
- returns not-found only after every index is inspected;
- does not use LINQ (`First`, `FirstOrDefault`, `Contains`, etc.) or framework search helpers;
- keeps stable teaching identities so duplicate values remain distinguishable in Memory State;
- best case `Θ(1)`, average/worst case `Θ(n)`, extra algorithmic space `O(1)`;
- array mutations invalidate an active traversal and require restart.

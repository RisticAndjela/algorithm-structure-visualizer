# Matrix module

The Matrix lab is implemented from scratch over `ManualMatrix`, which stores values in a single row-major `double[]`.
No matrix, linear-algebra, numerical, or graph library performs the taught operations.

Implemented learning operations:

- resize/edit/zero/identity/sequence/diagonal/symmetric/random/adjacency presets;
- copy A to B, swap A/B, and copy derived Result back into A or B for operation chaining;
- addition, subtraction, Hadamard product;
- scalar multiplication and matrix multiplication;
- transpose and integer matrix powers;
- trace, determinant, minor and cofactor;
- elementary row operations;
- REF, RREF and rank;
- inverse through Gauss-Jordan elimination;
- solve `A·X=B` for square non-singular A;
- property analysis: square, zero, identity, diagonal, upper/lower triangular, symmetric;
- row-major memory visualization using `index = row * columns + column`.

The module intentionally precedes Graphs because an adjacency matrix is one of the two graph representations planned for the project.

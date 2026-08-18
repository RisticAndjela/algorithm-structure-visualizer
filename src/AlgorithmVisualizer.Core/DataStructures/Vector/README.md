# Manual Vector

`ManualVector` is the project-owned numerical Vector data structure. It is taught in the Data Structures track and reused later by Machine Learning.

## Storage rule

- contiguous project-owned `double[]`
- no `List<T>`, LINQ vector helpers, `System.Numerics.Vector<T>`, or numerical library
- component `i` is stored in slot `i`; a `double` occupies 8 bytes conceptually in the Memory State view

## Implemented operations

- addition / subtraction
- scalar multiplication
- Hadamard product
- dot product
- L1 and L2 norms
- L2 normalization
- Euclidean and Manhattan distance
- cosine similarity

All taught reductions and component-wise operations use explicit loops and publish semantic simulation steps.
Binary operations that pair components require equal dimensions. L2 normalization rejects the zero vector, and cosine similarity rejects a zero-length operand.

## Why this module exists

Vector is useful on its own whenever ordered numeric components must keep stable indexes. The same Core implementation is then reused by later ML modules: Gradient Descent parameter vectors, linear/logistic model weights, KNN distances, K-Means centroids, PCA projections, and neural-network tensors at a larger scale.

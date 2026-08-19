# PCA Core

`PcaSimulation` implements the first principal component from scratch over project-owned `ManualVector` storage.

The explicit teaching path is:

1. compute the mean vector;
2. center every point;
3. build the sample covariance matrix;
4. find the dominant covariance direction with manual power iteration;
5. compute the dominant eigenvalue and explained-variance ratio;
6. project every centered point onto that one principal component.

No numerical, eigenvalue, PCA, or ML library is used. The Core implementation is dimension-independent; the learner-facing visualization uses two dimensions so the geometry remains visible.

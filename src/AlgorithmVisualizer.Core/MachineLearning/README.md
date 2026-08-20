# Machine Learning Core

This namespace contains project-owned Machine Learning and Deep Learning teaching implementations. Taught behavior is implemented manually in C#; ML, tensor, numerical-optimizer, automatic-differentiation, or neural-network libraries do not replace the algorithm being visualized.

Live modules currently include:

1. `Optimization/GradientDescent` — explicit convex loss/gradient orchestration that reuses `ManualVector`.
2. `Supervised/LinearRegression` — explicit prediction, residual, MSE, derivative, and full-batch Gradient Descent loops.
3. `Supervised/LogisticRegression` — explicit score, stable sigmoid, threshold, BCE, derivatives, and full-batch updates.
4. `Supervised/Knn` — ManualVector feature/query storage, explicit scan/top-k maintenance, and majority vote.
5. `Supervised/KdTree` — explicit alternating-axis median build, nearest-first descent, backtracking, and split-plane pruning.
6. `Unsupervised/KMeans` — explicit nearest-centroid assignment, mean updates, empty-cluster handling, inertia, and convergence.
7. `Supervised/DecisionTree` — explicit threshold generation, Gini/entropy, recursive split construction, stopping rules, and prediction.
8. `Unsupervised/Pca` — explicit mean centering, covariance, power iteration, explained variance, and PC1 projection.
9. `DeepLearning/Neuron` — weighted contributions, bias, pre-activation, and project-owned activation functions.
10. `DeepLearning/Mlp` — one-hidden-layer dense forward pass built from `ManualVector` and `ManualMatrix`.
11. `DeepLearning/Backpropagation` — explicit forward cache, chain-rule deltas, parameter gradients, and one learning update.
12. `DeepLearning/Optimizers` — deterministic stochastic-gradient comparison of SGD, Momentum, and Adam including optimizer memory.
13. `GraphMl/SparseMatrix` — explicit dense-to-CSR conversion and sparse matrix-vector multiplication over project-owned arrays.
14. `GraphMl/PageRank` — directed PageRank over CSR adjacency with teleportation, dangling-mass redistribution, normalization, and convergence.
15. `GraphMl/SpectralClustering` — normalized Laplacian, project-owned Jacobi eigensolver, spectral embedding, and reuse of manual K-Means.
16. `GraphMl/MessagePassing` — basic GNN-style gather/aggregate/transform/ReLU layers with synchronous double-buffered node embeddings.

`DataStructures/Vector` is intentionally taught in the Data Structures track, not duplicated here. It remains the reusable numerical foundation for ML modules. Phase 1 is complete through PCA (roadmap step 9). Phase 2 is complete through SGD / Momentum / Adam (roadmap step 13). `Computational Graph` is intentionally not a standalone lesson; dependency and value-flow concepts are taught inside the concrete neuron, MLP, and Backpropagation modules instead. Phase 3 is complete with Sparse Matrix (step 14), PageRank (15), Spectral Clustering (16), and Message Passing / basic GNN (17).

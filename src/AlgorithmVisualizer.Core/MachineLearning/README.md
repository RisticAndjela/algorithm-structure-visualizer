# Machine Learning Core

This namespace contains project-owned Machine Learning teaching implementations.

Live Machine Learning modules currently include:

1. `Optimization/GradientDescent` — explicit convex loss/gradient orchestration that reuses the existing Data Structures `ManualVector` implementation for vector arithmetic.
2. `Supervised/LinearRegression` — explicit `yHat = w*x + b`, residual, MSE, derivative, and full-batch Gradient Descent loops; X/Y/prediction/residual storage reuses `ManualVector`.
3. `Supervised/LogisticRegression` — explicit score, numerically stable sigmoid, 0.5 threshold, binary cross-entropy, derivative, and full-batch Gradient Descent loops; X/label/score/probability/error storage reuses `ManualVector`.
4. `Supervised/Knn` — project-owned ManualVector feature/query storage, reused VectorSimulation distance primitives, explicit full scan, ordered top-k maintenance, and majority vote.
5. `Supervised/KdTree` — project-owned spatial nodes over ManualVector points, explicit alternating-axis median build, nearest-first descent, backtracking, and split-plane pruning.

`DataStructures/Vector` is intentionally taught in the Data Structures track, not duplicated here. It remains the reusable numerical foundation for ML modules. No ML, statistics, numerical-vector, or optimizer library may replace the algorithmic behavior being taught.

The next planned Phase 1 module is K-Means, followed by Decision Tree and PCA. KD-Tree is now live as the spatial acceleration companion to KNN; it changes neighbor lookup organization, not KNN vote semantics.

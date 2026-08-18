# Machine Learning Core

This namespace contains project-owned Machine Learning teaching implementations.

Live Machine Learning modules currently include:

1. `Optimization/GradientDescent` — explicit convex loss/gradient orchestration that reuses the existing Data Structures `ManualVector` implementation for vector arithmetic.
2. `Supervised/LinearRegression` — explicit `yHat = w*x + b`, residual, MSE, derivative, and full-batch Gradient Descent loops; X/Y/prediction/residual storage reuses `ManualVector`.

`DataStructures/Vector` is intentionally taught in the Data Structures track, not duplicated here. It remains the reusable numerical foundation for ML modules. No ML, statistics, numerical-vector, or optimizer library may replace the algorithmic behavior being taught.

The next planned Phase 1 model is Logistic Regression, followed by KNN, KD-Tree, K-Means, Decision Tree and PCA.

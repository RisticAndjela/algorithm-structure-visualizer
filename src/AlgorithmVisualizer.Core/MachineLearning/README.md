# Machine Learning Core

This namespace contains project-owned Machine Learning teaching implementations.

Live Machine Learning modules currently include:

1. `Optimization/GradientDescent` — explicit loss/gradient logic that reuses the existing Data Structures `ManualVector` implementation for vector arithmetic.

`DataStructures/Vector` is intentionally taught in the Data Structures track, not duplicated here. It remains a reusable numerical dependency for Gradient Descent and future Linear Regression, Logistic Regression, KNN, K-Means, PCA and neural-network lessons.

Planned Phase 1 models include Linear Regression, Logistic Regression, KNN, KD-Tree, K-Means, Decision Tree and PCA. New lessons should reuse existing manual primitives rather than reimplementing them.

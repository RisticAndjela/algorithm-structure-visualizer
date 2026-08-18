# Machine Learning Core

This namespace contains project-owned numerical and ML teaching implementations.

Live Phase 1 foundations currently include:

1. `Vector` under `DataStructures/Vector` — the reusable numerical primitive.
2. `Optimization/GradientDescent` — explicit loss/gradient logic that reuses Vector Core for vector arithmetic.

Planned classical ML consumers include Linear Regression, Logistic Regression, KNN, KD-Tree, K-Means, Decision Tree and PCA. New lessons should reuse existing manual primitives rather than reimplementing them.

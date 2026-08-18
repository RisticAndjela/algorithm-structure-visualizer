# Linear Regression

The live Linear Regression lesson implements univariate regression from scratch.

- Model: `yHat = weight * x + bias`.
- Loss: mean squared error over the current training points.
- Training: full-batch Gradient Descent with explicit derivative and update loops.
- Storage: the project-owned `ManualVector` stores X values, Y values, predictions, and residuals.
- No ML, statistics, or numerical-optimization library performs prediction, loss, derivatives, or training.
- Core working space is `O(n)` for the current dataset-derived vectors; the UI rewind history is teaching-only state.

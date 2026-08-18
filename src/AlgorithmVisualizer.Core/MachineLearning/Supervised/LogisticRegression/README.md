# Logistic Regression

Phase 1 step 4 is a from-scratch univariate binary Logistic Regression lesson.

The Core keeps the first model deliberately small enough to visualize end-to-end:

1. compute `z = w*x + b` for every example;
2. map each score with a numerically stable sigmoid;
3. convert probability to class with the visible `0.5` threshold;
4. average stable binary cross-entropy;
5. compute `dw = mean((p-y)*x)` and `db = mean(p-y)`;
6. update weight and bias with full-batch Gradient Descent.

`X`, labels, scores, probabilities and probability errors use the project-owned `ManualVector`. Predicted classes use a plain `int[]` because they are discrete labels rather than numerical vector arithmetic. No ML, statistics, optimizer, or numerical package performs the taught model behavior.

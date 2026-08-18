# Gradient Descent

The live Gradient Descent lesson uses a transparent convex quadratic objective:

`J(theta) = 1/2 * Σ curvature[i] * (theta[i] - target[i])²`

This keeps the optimization mechanics visible without coupling the lesson to a regression model too early.

## Implementation boundary

- Loss and the analytical objective gradient are implemented with explicit C# loops in `GradientDescentSimulation`.
- `VectorSimulation` is reused for `||gradient||₂`, `learningRate * gradient`, and `theta - scaledGradient`.
- No Math.NET, `System.Numerics`, numerical optimizer, or authored JavaScript is used.
- The simulation stores review history only so the UI can step backwards and draw the optimization path over the loss landscape. The optimizer itself needs `O(n)` working state.

## Variants

- **Basic:** fixed learning rate.
- **Advanced:** inverse learning-rate decay, `eta_t = eta_0 / (1 + decay * t)`.

Momentum and Adam belong to the later Deep Learning optimizer lesson rather than this Phase 1 foundation.

## Stopping behavior

A run can:

- converge when the L2 gradient norm reaches the configured tolerance,
- stop at the maximum iteration budget,
- or be guarded as divergent after repeated loss growth / unsafe numeric growth.

Changing parameters, target, curvature, learning rate, tolerance, iteration budget, or variant requires a fresh run so one timeline always represents one stable configuration.

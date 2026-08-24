# SGD / Momentum / Adam

Phase 2 step 11. `OptimizerSimulation` feeds the same deterministic cyclic stochastic-gradient stream to three optimizer rules so their state can be compared directly. SGD uses the raw gradient, Momentum keeps a first-moment velocity, and Adam keeps first/second moments with bias correction.

The optimizer consumes gradients; gradient creation remains the responsibility of the model/loss lesson and Backpropagation.

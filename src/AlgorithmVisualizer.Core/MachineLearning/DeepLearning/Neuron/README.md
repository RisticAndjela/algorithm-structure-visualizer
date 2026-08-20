# Neuron + Activation Functions

Phase 2 step 10. `NeuronSimulation` explicitly computes each weighted contribution `xᵢ·wᵢ`, sums contributions with bias into `z`, then applies a project-owned activation from `ActivationMath`. Inputs, weights, and contributions use `ManualVector`; no tensor or neural-network library is used.

The learner-facing page compares Linear, ReLU, Leaky ReLU, Sigmoid, and Tanh while keeping weighted sum and activation as separate playback stages.

# Message Passing / basic GNN — Phase 3 step 17

The final Graph ML lesson runs one to three synchronous message-passing layers. Each node gathers neighbor features from CSR adjacency, aggregates by mean or sum, applies separate self and neighbor linear transforms plus bias, applies ReLU, and commits all nodes simultaneously.

The implementation is intentionally small and explicit: no GNN framework, tensor library, or hidden graph convolution primitive is used.

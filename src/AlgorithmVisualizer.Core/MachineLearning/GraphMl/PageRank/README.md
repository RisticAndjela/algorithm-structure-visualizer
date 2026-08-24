# PageRank — Phase 3 step 13

PageRank runs directly over `ManualCsrMatrix`. Each iteration exposes teleport mass, dangling-node redistribution, explicit outgoing-edge contributions, normalization, and convergence delta.

No graph-ranking library is used. Rank mass is kept normalized to 1 and the deterministic presets include cycles, authorities, and dangling nodes.

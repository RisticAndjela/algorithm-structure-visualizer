# Sparse Matrix — Phase 3 step 14

The Graph ML track starts with project-owned CSR storage. `SparseMatrixSimulation` scans a small dense matrix explicitly, stores only non-zero values plus column indexes and row pointers, then performs sparse matrix-vector multiplication by visiting only stored entries.

No sparse matrix, numerical, or graph library is used. `ManualCsrMatrix` is shared by later PageRank and message-passing modules.

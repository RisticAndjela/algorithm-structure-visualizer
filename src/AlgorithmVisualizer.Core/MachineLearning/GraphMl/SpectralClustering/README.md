# Spectral Clustering — Phase 3 step 16

The teaching implementation constructs the normalized Laplacian manually, diagonalizes the symmetric matrix with project-owned Jacobi rotations, keeps the smallest eigenvectors, row-normalizes the spectral embedding, and reuses the existing from-scratch K-Means implementation.

The UI defaults to two clusters but the Core supports two or three for small graphs. No eigensolver, spectral clustering, or graph-ML library is used.

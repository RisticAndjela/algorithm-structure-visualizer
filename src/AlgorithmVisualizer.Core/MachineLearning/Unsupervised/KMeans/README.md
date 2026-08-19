# K-Means

Project-owned K-Means learning implementation.

- Points and centroids are stored with `ManualVector`.
- Euclidean distance is reused through the existing `VectorSimulation`.
- Assignment scans every centroid explicitly.
- Centroid means are computed with explicit sum/count loops.
- Empty clusters keep their previous centroid in this teaching implementation.
- No ML, clustering, numerical-vector, or framework sorting library performs the taught algorithm.

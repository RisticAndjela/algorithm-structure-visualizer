# DataStructures

Pure C# teaching implementations live here and must not depend on Blazor.

Live modules currently include:

- `Linear/` — custom-array Stack and Queue;
- `Trees/Bst/` — manually linked Binary Search Tree plus explicit DSW balance;
- `Trees/Avl/` — manually linked AVL with cached heights and rotations;
- `Trees/RedBlack/` — manually linked Red-Black Tree with color fix-up;
- `Heap/` — one custom-array generalized d-ary Min/Max Heap; Binary Heap is the `d = 2` mode of that same implementation;
- `Matrix/` — row-major `double[]` Matrix workspace with arithmetic, multiplication, elimination, inverse, rank and graph-adjacency presets;
- `Vector/` — contiguous `double[]` numerical structure with component arithmetic, reductions, norms, normalization, distance and cosine similarity; it is taught as a Data Structure and reused by Machine Learning.

All taught mutation/search/balancing behavior is implemented explicitly rather than delegated to ready-made framework data structures.


Graph is live with manual adjacency lists plus the existing ManualMatrix adjacency representation. Traversal/path algorithms remain separate consumers of this structure. Vector is also reusable Core infrastructure: its learner-facing lab belongs to Data Structures, while the live Gradient Descent module consumes `VectorSimulation` for norm, scaling and subtraction. Later ML algorithms should continue that reuse rather than duplicate numerical loops.

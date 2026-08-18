# DataStructures

Pure C# teaching implementations live here and must not depend on Blazor.

Live modules currently include:

- `Linear/` — custom-array Stack and Queue;
- `Trees/Bst/` — manually linked Binary Search Tree plus explicit DSW balance;
- `Trees/Avl/` — manually linked AVL with cached heights and rotations;
- `Trees/RedBlack/` — manually linked Red-Black Tree with color fix-up;
- `Heap/` — shared custom-array Heap family: generalized d-ary Min/Max Heap plus the dedicated Binary Heap (`d = 2`) specialization;
- `Matrix/` — row-major `double[]` Matrix workspace with arithmetic, multiplication, elimination, inverse, rank and graph-adjacency presets;
- `Vector/` — contiguous `double[]` mathematical vector foundation with component arithmetic, reductions, norms, normalization, distance and cosine similarity for the AI/ML track.

All taught mutation/search/balancing behavior is implemented explicitly rather than delegated to ready-made framework data structures.


Graph is live with manual adjacency lists plus the existing ManualMatrix adjacency representation. Traversal/path algorithms remain separate consumers of this structure. Vector is also reusable Core infrastructure: the live Gradient Descent module already consumes `VectorSimulation` for norm, scaling and subtraction, and later ML algorithms should continue that reuse rather than duplicate numerical loops.

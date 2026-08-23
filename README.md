# Algorithm Visualizer

An interactive learning platform for understanding data structures and algorithms by **watching each operation happen step by step**.

The project is built with **Blazor WebAssembly and C#**. Its goal is not only to display the final result of an algorithm, but to explain:

- what the algorithm is doing;
- which element is currently being inspected;
- why that element is visited next;
- how many operations were required;
- what the time complexity means for the current run;
- how the visual representation differs from the way the data is stored in memory.

The application now has thirty-seven fully implemented learning modules: **Queue & Stack**, **Binary Search Tree (BST)**, **Binary Heap (Min/Max)**, **Heap (generalized d-ary)**, **AVL Tree**, **Vector**, **Matrix**, **Graph**, **Red-Black Tree**, **Bubble Sort**, **Selection Sort**, **Insertion Sort**, **Merge Sort**, **Quick Sort**, **Heap Sort**, **Topological Sort**, **Linear Search**, **Binary Search**, **Breadth-First Search (BFS)**, **Depth-First Search (DFS)**, **Dijkstra**, **Minimum Spanning Tree (Prim/Kruskal)**, **Gradient Descent**, **Linear Regression**, **Logistic Regression**, **K-Nearest Neighbors (KNN)**, **KD-Tree**, **K-Means**, **Decision Tree**, **PCA**, **Neuron + Activation Functions**, **Neural Network / MLP**, **Backpropagation**, and **SGD / Momentum / Adam**. Vector is taught in Data Structures and then reused as a numerical dependency by Machine Learning. The Home page, sidebar, Concepts & Memory navigation, and Next Lesson flow expose these modules one-to-one in the same curriculum order instead of grouping several finished lessons behind one card. Every live lab deep-links to the exact Concepts & Memory topic through dedicated Blazor routes such as `/learn/concepts/linear-search`; the shared C# `ConceptLink` component performs normal Blazor navigation without authored JavaScript or cross-page fragment timing.

---

## Project status

### Implemented

- Queue
- Stack
- Binary Search Tree (BST)
- Binary Heap (Min/Max)
- Heap (generalized d-ary Min/Max)
- AVL Tree
- Vector (data structure / numerical structure)
- Matrix
- Graph
- Red-Black Tree
- Bubble Sort
- Selection Sort
- Insertion Sort
- Merge Sort
- Quick Sort
- Heap Sort
- Topological Sort (Advanced Sorting)
- Linear Search
- Binary Search
- Breadth-First Search (BFS)
- Depth-First Search (DFS)
- Dijkstra (Advanced Search & Traversal)
- Minimum Spanning Tree (Prim/Kruskal) (Advanced Search & Traversal)
- Gradient Descent (Machine Learning optimization foundation)
- Linear Regression (Machine Learning supervised model)
- Logistic Regression (Machine Learning binary classifier)
- K-Nearest Neighbors (Machine Learning neighbor-based classifier)
- KD-Tree (Machine Learning spatial nearest-neighbor index)
- K-Means (Machine Learning unsupervised clustering)
- Decision Tree (Machine Learning supervised classification)
- PCA (Machine Learning dimensionality reduction)
- Neuron + Activation Functions (Deep Learning weighted sum + nonlinear activation)
- Neural Network / MLP (one-hidden-layer dense forward pass)
- Backpropagation (explicit chain-rule gradient propagation)
- SGD / Momentum / Adam (optimizer-state comparison)
- shared simulation runtime;
- play, pause and adjustable simulation speed;
- manual step forward;
- review of previous simulation frames with step back / step forward;
- search by value;
- search by element ID;
- delete by value;
- delete by element ID;
- visual state and memory state;
- per-run complexity explanation;
- deletion and memory-shift explanation;
- guided practice tasks with automatic completion;
- learning progress and popup preferences persisted through a C# ASP.NET Core API into SQLite;
- optional result explanation popups;
- generic Last Run modal overlay/chrome shared by learning modules so pages cannot silently render result content inline when local popup CSS is absent;
- Concepts & Memory learning page;
- shared learner-facing module chrome in `wwwroot/css/learning-modules.css`, using the mature Queue & Stack / BST / Matrix / Graph visual language for sorting Learn First panels, module headers, tips, reference links, and lesson progression;
- shared compact `OperationTile` controls for ordinary lab actions so Stack, Queue, Heap, and future modules keep BST-style operation hierarchy instead of oversized one-action cards;
- shared Memory State detail rows for label/value pairs (`parent`, `left`, `right`, `children`) so reference labels stay consistently spaced and aligned across BST, AVL, Red-Black Tree, and Heap memory cards;
- one application-wide curriculum hierarchy: Learn → Data Structures → Sorting → Search & Traversal → Machine Learning; advanced live lessons use a red difficulty dot, while planned lessons keep the neutral planned marker; Graph algorithms are classified inside those algorithm tracks rather than forming a separate navigation section: Topological Sort is Sorting; Dijkstra and MST are Search & Traversal;
- difficulty-ordered curriculum navigation with reusable `NextLessonCard` links: Queue & Stack → BST → Binary Heap → d-ary Heap → AVL → Vector → Matrix → Graph → Red-Black Tree, then Bubble → Selection → Insertion → Merge → Quick → Heap Sort → Topological Sort (Advanced), then Linear Search → Binary Search → BFS → DFS → Dijkstra (Advanced) → Minimum Spanning Tree (Advanced), then Machine Learning continues Gradient Descent → Linear Regression → Logistic Regression → KNN → KD-Tree → K-Means → Decision Tree → PCA → Neuron + Activation Functions → Neural Network / MLP → Backpropagation → SGD / Momentum / Adam;
- exact route-based links from every live lab to the relevant Concepts & Memory topic, plus reverse links from the concept sections back to the matching lesson;
- BST insert, search, delete, explicit DSW balance, and reset;
- BST leaf / one-child / two-child deletion simulation;
- BST Day-Stout-Warren vine + compression simulation with node-identity preservation;
- in-order-successor visualization;
- BST visual-state and node-reference memory-state views;
- BST guided practice and persistent progress;
- AVL insert, search, delete, and reset;
- AVL cached height and balance-factor maintenance;
- LL / RR single rotations and LR / RL double rotations;
- upward rebalancing after insert and delete;
- AVL visual-state and node-reference memory-state views;
- AVL rotation-focused guided practice and persistent progress;
- Red-Black insert, search, delete, and reset;
- explicit red/black node colors with conceptual black NIL leaves;
- insertion fix-up with red-uncle recoloring, triangle repair, and line repair;
- deletion fix-up with sibling/near/far child color cases and mirror cases;
- Red-Black node-identity-preserving rotations and successor transplant;
- Red-Black Visual/Memory views with black-height, color, references, and transient fix-up states;
- Red-Black guided practice and persistent progress;
- generalized d-ary Heap with configurable branching factor `d = 2/3/4/5` in the UI, making Binary Heap visibly the `d = 2` special case;
- generalized parent/child formulas `parent=(i-1)/d`, children `di+1..di+d`;
- d-ary bubble-down that explicitly compares all existing child candidates before choosing the highest-priority child;
- Heap-family vs Binary-Heap learning copy and side-by-side navigation;
- Min Heap / Max Heap mode selection while empty;
- Heap insert with append + bubble-up;
- Heap extract-root with last-element replacement + bubble-down;
- Heap arbitrary value search with a truthful O(n) scan;
- Heap delete-by-value with direction-aware repair;
- Heap Visual/Memory views showing complete-tree indexes, stable IDs, used slots, capacity, and manual array growth;
- Heap guided practice and persistent progress;
- comprehensive Matrix workspace with editable A/B matrices and a two-level Memory State: row-list mental model first, actual row-major raw-array storage second;
- Matrix values can be entered cell-by-cell or pasted as complete rows; bulk input validates rectangular shape, accepts up to 8×8, and resizes the target matrix automatically;
- matrix resize, zero/identity/sequence/diagonal/symmetric/graph-adjacency presets, copy and swap helpers;
- matrix addition, subtraction, Hadamard product, scalar multiplication and matrix multiplication;
- transpose, integer powers, trace, determinant, minor and cofactor;
- elementary row swap/scale/replacement operations;
- REF, RREF, rank, inverse through Gauss-Jordan elimination, and `A·X=B` solving;
- matrix property analysis (square, zero, identity, diagonal, triangular, symmetric);
- Matrix Visual/Memory views showing cell states, a list-of-row-lists memory explanation, and an expandable actual backing-array view with `index = row * columns + column`;
- Matrix guided practice with continuous auto-tracking, optional Start/Restart setup, active-task progress, and SQLite-backed completion evidence;
- Matrix Last Run explanations with Action / Memory / Why-it-matters tabs and a persisted automatic-popup preference, matching the other mature structure labs;
- Graph directed/undirected and weighted/unweighted modes;
- Graph add/search/remove vertex and add/search/remove edge operations;
- direct-neighbor inspection with semantic playback states;
- canonical stable vertex/edge identities, duplicate-edge rejection and self-loop support;
- synchronized manual adjacency-list and Matrix-backed adjacency-matrix representations;
- zero-weight edge presence kept distinct from an absent weighted edge;
- vertex deletion that removes all incident edges before representation rebuild;
- Graph Visual/Memory views plus guided practice that prepare the same structure for BFS, DFS, Dijkstra, topological sort and MST;
- Graph Core can grow beyond eight vertices; the standalone Matrix page remains intentionally limited to 8×8 while reusable `ManualMatrix` storage grows with Graph adjacency data;
- Graph Learn First and all Graph/SimulationToolbar instructional copy use a larger readability floor so helper text, playback explanations, node metadata and memory labels are legible at normal desktop zoom;
- Heap readability pass: instructional text and labels enlarged, with overflow kept inside visualization regions.
- Bubble Sort implemented from scratch over a fixed raw array of stable teaching elements;
- Bubble Sort adjacent compare / keep / swap / pass-complete semantic playback steps;
- Bubble Sort implementation selector with **Basic** canonical full-pass mode and **Optimized** no-swap early exit;
- Bubble Sort Visual state with active pair, stable item identity, and growing sorted suffix;
- Bubble Sort Memory state showing fixed array indexes and neighboring reference swaps with `O(1)` algorithmic extra space;
- first-move prediction interaction, presets for sorted/reverse/duplicate/boundary cases, automatic Last Run explanations, and verified persistent Guided Practice.
- Selection Sort implemented from scratch over a fixed raw array of teaching elements;
- Selection Sort destination/minimum/scan semantic playback, with the sorted prefix growing left-to-right;
- exact `n(n-1)/2` comparison behavior for every input order, with at most `n-1` direct swaps when self-swaps are skipped;
- Selection Sort Visual state showing destination, current minimum, scan position, and fixed prefix simultaneously;
- Selection Sort implementation selector with **Classic** direct swaps and **Stable Shift** insertion-by-shifting, both using `O(1)` extra algorithmic space;
- first-minimum prediction, instability preset, automatic Last Run explanations, Concepts integration, and verified persistent Guided Practice.
- Merge Sort implemented from scratch with **Top-down Recursive** and **Natural Merge** variants;
- Top-down Merge Sort explicit recursive split, left/right run, front-comparison, auxiliary-buffer, and copy-back semantic playback;
- Natural Merge Sort detection of maximal nondecreasing runs, including a `Θ(n)` already-sorted best case with zero merge operations;
- stable duplicate handling by taking the left-run element first on equality;
- reusable `O(n)` auxiliary buffer exposed directly in Memory State, with top-down recursion depth shown separately;
- Merge Sort Visual/Memory views, first-step prediction, practical workload guidance, mutation-restart semantics, automatic Last Run explanations, Concepts integration, and verified persistent Guided Practice.

### Shared lesson design and curriculum navigation

All sorting pages now use the same learner-facing design language as the mature data-structure modules instead of a separate sorting-specific landing-page style. Live sorting labs opt into `learning-module-page learning-module-shell`; their **LEARN FIRST** area uses the shared explanation + 2×2 concept-card pattern, followed by the same compact module-header, smart-tip, bordered lab panels, compact Visual/Memory switch with an adjacent explanation, Guided Practice styling, exact Concepts & Memory links, and reusable Next Lesson navigation. Quick Sort and Heap Sort are both live labs using the shared learning shell. `LearningPlaceholder` remains the required shell for future not-yet-implemented lessons so a future implementation replaces only the TODO workspace rather than inventing another design.

Sorting and Search & Traversal run workspaces now follow one fixed vertical hierarchy: **run action + compact `SimulationToolbar` → shared `RunMetricStrip` / current-role / invariant context → Visual/Memory state → shared `StateLegend` color key → reference links**. This applies to Bubble, Selection, Insertion, Merge, Quick, Heap, Topological, Linear Search, Binary Search, BFS, DFS, Dijkstra, and MST. Live metrics belong above the state surface rather than inside visualization components, and `SimulationToolbar.Compact` keeps playback/current-step/step-delay density consistent across these algorithm labs. Algorithm logic, code snippets, semantic frames, and Memory State implementations remain algorithm-specific.

The sidebar and Concepts & Memory page use explicit easy → hard ordering. Concepts & Memory starts with shared foundations (Visual vs Memory, memory, identity, Big-O), then presents data-structure lessons in curriculum order, sorting lessons from Bubble through Heap Sort and advanced Topological Sort, then the search/traversal track progressing from Linear Search to Binary Search, BFS, DFS, advanced Dijkstra, and advanced Minimum Spanning Tree. Dedicated topic routes such as `/learn/concepts/bst`, `/learn/concepts/matrix`, `/learn/concepts/heap-sort`, and `/learn/concepts/linear-search` let each module land on the exact explanation it needs without cross-page fragment scrolling. Bare fragment-only links are not used, including in the Concepts jump menu, because the application root `<base href="/">` can otherwise resolve them as `/#topic`. Every lesson page exposes a `NextLessonCard`, and the concept sections link back to the corresponding lab.

### Bubble Sort module details

Bubble Sort is the first live sorting-algorithm module at `/sorting/bubble`. The Core implementation performs every adjacent comparison and swap explicitly; it does not call `Array.Sort`, `List.Sort`, LINQ ordering, or another sorting routine. The learner can switch between **Basic** mode (all canonical shrinking passes, best case `Θ(n²)`) and **Optimized** mode (same stable neighbor rule plus a no-swap early exit, best case `Θ(n)`).

The learner workflow is **Build → Predict → Watch → Explain → Practice**. Inputs are editable as 1–12 integers in the Client teaching view, with prepared examples for classic, nearly sorted, reverse, duplicate, already sorted, and random cases. The learner can predict the first adjacent decision before running the algorithm.

Visual state distinguishes comparing, swap-required/moved, keep-order, and fixed/sorted states. The sorted-suffix invariant is kept visible throughout playback. Memory state keeps array indexes fixed and shows which stable `BubbleSortElement` reference occupies each slot, allowing duplicate stability to be verified directly. Equal values are not swapped merely for equality, so their original relative order is preserved.

Each run reports its implementation mode, passes, comparisons, swaps, early-exit usage, stability, and precise complexity context. The page also explains practical fit and real-time mutation semantics: Bubble Sort is mainly appropriate for teaching/tiny data; arbitrary insert/delete/update operations invalidate the active trace and require restart. Nearly sorted workloads point toward Insertion Sort, while larger stable workloads point toward Merge Sort. Guided Practice validates both Basic and Optimized behavior, reverse-order worst case, duplicate stability plus Memory inspection, and the one-element boundary.

### Selection Sort module details

Selection Sort is the second live sorting-algorithm module at `/sorting/selection`. The Core implementation always performs the minimum scan directly and offers two placement strategies: **Classic** performs at most one direct swap after each scan, while **Stable Shift** holds the selected minimum, shifts the intervening block one slot right, and inserts the minimum at the target. Neither mode calls `Array.Sort`, `List.Sort`, LINQ ordering, or another sorting routine.

The learner workflow remains **Build → Predict → Watch → Explain → Practice**, but the prediction asks for the first minimum index rather than an adjacent swap. Visual state deliberately separates four roles that beginners often merge together: the destination index, current minimum candidate, current scan index, and already fixed sorted prefix. This makes the key distinction explicit: Selection Sort spends most of its work *searching*; only after the search is complete does the selected mode either swap the minimum directly or insert it by stable shifting.

For `n >= 2`, both Selection Sort variants perform `(n-1) + (n-2) + ... + 1 = n(n-1)/2` comparisons, so best/average/worst comparison growth remains `Θ(n²)` even when the input is already sorted. Classic mode skips self-swaps and performs at most `n-1` direct swaps, but the prepared `2, 2, 1` example shows that a long-distance swap can reverse equal-item identity. Stable Shift preserves duplicate order with `O(1)` extra algorithmic space by replacing the distant swap with block shifting, at the cost of more array-slot writes.

The page explains practical fit and live-data behavior: Classic Selection Sort can be useful on tiny inputs when writes/swaps are expensive, but all variants remain quadratic and arbitrary insert/delete/update operations require restart. Heap Sort is linked as the asymptotically stronger repeated-selection strategy; Merge Sort is linked when stability and scale matter together. Guided Practice verifies Classic exact counts, its instability counterexample, the Stable Shift duplicate-preservation behavior, and boundary cases.


### Insertion Sort module details

Insertion Sort is the third live sorting-algorithm module at `/sorting/insertion`. The Core implementation is fully manual and stable. **Linear** mode is the canonical algorithm: hold the next key, scan the sorted prefix backward, shift only strictly larger values right, and insert the key into the resulting gap. **Binary Insertion** uses a stable upper-bound binary search to find the insertion point with fewer key comparisons, then performs the same explicit shifts. Neither mode delegates to `Array.Sort`, `List.Sort`, LINQ ordering, or another sorter.

The learner workflow remains **Build → Predict → Watch → Explain → Practice**. Visual state separates held key, gap, comparison/binary probe, insertion point, and sorted prefix. Memory state shows the fixed array slots plus the one temporary key reference so the shifting process is concrete. Both variants preserve duplicate identity order and use `O(1)` extra algorithmic space.

Linear Insertion Sort is adaptive: already-sorted input performs one comparison per new key and zero shifts, giving a `Θ(n)` best case. Binary Insertion reduces insertion-point comparisons but cannot remove array movement, so worst-case total time remains `Θ(n²)`. The page also teaches the algorithm's online property: once a collection is sorted, a newly arriving item can be inserted without a full re-sort, delete preserves sorted order, and update can be repaired by remove + reinsert. Recorded UI playback still restarts after mutation because existing frames describe the old snapshot. For large stable general-purpose sorting, the page points toward Merge Sort.


### Merge Sort module details

Merge Sort is the fourth live sorting-algorithm module at `/sorting/merge`. The Core implementation is fully manual and stable. **Top-down Recursive** mode demonstrates the canonical divide-and-conquer algorithm: split each range in half until single-item base cases remain, then merge the sorted halves upward. **Natural Merge** is the advanced adaptive variant: it scans for maximal nondecreasing runs already present in the input and merges neighboring runs directly rather than recursively splitting those runs again. Neither mode delegates to `Array.Sort`, `List.Sort`, LINQ ordering, or another sorter.

The learner workflow remains **Build → Predict → Watch → Explain → Practice**. In Basic Top-down mode, Visual state now uses one focus at a time: divide frames show only the active parent and the exact two child ranges being created; merge frames replace that view with the two already-sorted child runs, their unread fronts, the temporary buffer, and copy-back. The main array remains visible at the bottom as the stable reference. Natural Merge keeps a separate run-detection view because it intentionally does not use recursive midpoint splitting. Memory state shows the main `MergeSortElement[]` alongside one reusable `MergeSortElement?[]` auxiliary buffer, making the algorithm's `O(n)` extra storage visible rather than hiding it behind animation.

Top-down Merge Sort keeps `Θ(n log n)` best/average/worst growth. Natural Merge Sort has the same `Θ(n log n)` worst case but can recognize an already-sorted input as one natural run in `Θ(n)` and perform zero merges. Both variants preserve duplicate identity order by taking the left-run element when values compare equal. The page also teaches the real-time mutation policy: reads are safe, but create/update/delete during an active run invalidates range/run boundaries and buffer assumptions, so the Merge Sort trace must restart from the changed snapshot. For tiny/nearly sorted arrays the page links back to Insertion Sort; for strict auxiliary-memory constraints it points learners toward the live Quick Sort and Heap Sort comparisons.

### Quick Sort module details

Quick Sort is live at `/sorting/quick` with two manual Core variants. **Basic Lomuto** uses the last item in each active range as pivot and grows one `<= pivot` boundary before placing the pivot into its final index. **Advanced median-of-three + three-way partitioning** chooses the median of first/middle/last candidate values and forms `< pivot`, `= pivot`, and `> pivot` regions; the equal band is finalized immediately, which is particularly useful for duplicate-heavy inputs.

The learner workflow remains **Build → Predict → Watch → Explain → Practice**. Visual State focuses on one active partition, and Memory State exposes the fixed array slots, item identity, and recursive stack cost. The page teaches average `Θ(n log n)`, worst-case `Θ(n²)`, `O(1)` extra array storage, average `O(log n)` recursion space, worst `O(n)` recursion space, lack of stability, and the snapshot rule that active create/update/delete mutations require restart. Workload guidance links back to Merge Sort when stability/predictable `Θ(n log n)` matters and forward to Heap Sort when guaranteed `Θ(n log n)` with `O(1)` extra array storage is preferred.

### Heap Sort module details

Heap Sort is live at `/sorting/heap` with two manual Max-Heap construction variants. **Basic Incremental Build** grows the active heap prefix one item at a time and bubble-ups each new value, directly reusing the Binary Heap insertion mental model; construction can cost `O(n log n)`. **Advanced Floyd Bottom-Up** starts from the last parent and sifts each parent downward, producing the Max Heap in `Θ(n)` construction time.

After build, both variants share the same explicit in-place sorting phase: swap the maximum root with the last active heap slot, shrink `heapSize` so that suffix slot becomes final, and sift the new root down only inside the reduced heap. Visual State keeps the complete-tree view synchronized with the same array and makes the `ACTIVE HEAP | SORTED SUFFIX` boundary impossible to miss. Memory State shows the real fixed `HeapSortElement[]`, stable item identity labels, and the fact that the teaching tree allocates no separate tree nodes or O(n) buffer.

Heap Sort has `Θ(n log n)` best/average/worst total time, `O(1)` extra array storage, and is not stable because distant root/end and repair swaps can reverse equal-value identities. READ/inspection is safe, but create/update/delete during an active sort invalidates heap order, boundary, and suffix-finality claims and requires a new run. Workload guidance points to Merge Sort for stability, Quick Sort for strong average in-memory behavior, Insertion Sort for nearly sorted/online maintenance, and the Binary Heap structure when continuous priority mutations—not one-time sorting—are the actual requirement.

### Matrix module details

Matrix is now a live pre-Graph foundation. `ManualMatrix` stores values in one row-major `double[]`. The Memory State deliberately teaches this in two layers: it first groups values as an outer list of row lists so row/column membership is easy to read, then exposes the actual flat backing array and its index formula.

The Matrix UI uses a beginner-first **Build → Choose → Watch** workflow. Direct cell editing is the primary input path; the editor now uses compact fixed-size cells, small shape controls, `Try:` presets, and a two-chip compatibility strip so A/B remain the visual focus instead of being buried inside large setup cards. Paste controls and special presets are progressively disclosed. Invalid operations remain runnable so the simulator can teach why a shape is rejected. Row/column axes and `[row,column]` coordinates remain available without exposing screen-reader-only labels as visible UI. Four compact core operation cards (addition, multiplication, transpose and RREF) keep one short explanation plus the single rule that matters before execution; the complete advanced operation set remains available under **All matrix operations**.

Implemented capabilities include direct editing/resizing, presets (zero, identity, sequence, diagonal, symmetric, random and graph adjacency), copy/swap/result chaining, addition/subtraction/Hadamard/scalar operations, matrix multiplication, transpose, powers, trace, determinant, minor/cofactor, elementary row operations, REF/RREF/rank, inverse, and solving `A·X=B`.

No numerical or linear-algebra library performs these taught operations. The live Graph module reuses this Matrix implementation for adjacency-matrix representation rather than creating a second matrix engine.

The Client contains no project-owned JavaScript and no `IJSRuntime` calls. Learning progress and popup preferences are loaded by the scoped C# `LearningSessionStore` from the same-origin ASP.NET Core API and persisted in SQLite. The server identifies an anonymous browser with an HttpOnly cookie, so persistence survives full browser reloads and browser restarts without `localStorage` or application-level JavaScript. Framework-generated Blazor bootstrap/runtime JavaScript remains part of the .NET WebAssembly platform and is not application logic.

### Planned

The following curriculum modules are still intentionally marked as TODO until their algorithms are implemented:

#### Data structures

All currently planned base data-structure modules in the specification are now represented by live labs; traversal/path algorithms remain separate.

#### Sorting algorithms

All seven sorting lessons in the current curriculum are live: Bubble, Selection, Insertion, Merge, Quick, Heap Sort, and advanced Topological Sort.

Linear Search, Binary Search, BFS, DFS, advanced Dijkstra, and advanced Minimum Spanning Tree are live in Search & Traversal. Topological Sort is live as the advanced final Sorting lesson. BFS/DFS reuse the existing Graph representation and manual Queue/Stack foundations; Dijkstra reuses the same Graph and adds manual linear-minimum and binary-min-heap priority selection over non-negative weighted edges; Topological Sort reuses the same directed Graph with Kahn indegree/FIFO and DFS reverse-postorder variants. Dijkstra's ordinary Try/Preset row contains only runnable non-negative examples; the deliberately invalid negative-edge case remains available through the dedicated precondition practice task instead of presenting an immediate validation error as a normal example.

K-Means is live as Machine Learning step 6, Decision Tree as step 7, and PCA as step 8. Phase 1 is complete through PCA. Phase 2 is also complete: Neuron + Activation Functions is step 9, Neural Network / MLP step 10, Backpropagation step 11, and SGD / Momentum / Adam step 12. Phase 3 is complete: Sparse Matrix is step 13, PageRank step 14, Spectral Clustering step 15, and Message Passing / basic GNN step 16.

Gradient Descent Visual State now treats Parameter Space as a complete-view teaching surface: the target/minimum, full committed trajectory, current point, and candidate are fitted into one visible frame with padding instead of being discoverable only through horizontal scrolling. Every simulation marker exposes hover/focus inspection with exact θ/loss/run metadata. The former four equal-weight vector cards under the canvas are replaced by one compact update flow that reads `current slope → η-scaled step → next θ`, including current and candidate loss context.

---

# Core implementation rule

## Algorithms and data structures are implemented from scratch

A central rule of this project is:

> **Do not use a ready-made library implementation for an algorithm or data structure that the application is supposed to teach.**

For example, the Queue and Stack implementations do **not** use:

```csharp
Stack<T>
Queue<T>
List<T>
LinkedList<T>
```

as their algorithmic storage implementation.

They also do not delegate important storage operations to helpers such as:

```csharp
Array.Copy(...)
Array.Sort(...)
BinarySearch(...)
```

Instead, the current linear structures use a custom:

```csharp
ManualDynamicArray<T>
```

which is backed directly by:

```csharp
T[]
```

The project manually implements:

- capacity growth;
- allocation of a larger backing array;
- copying existing references with a loop;
- indexed access;
- insertion at the end;
- deletion at an index;
- shifting later references to the left;
- clearing used slots;
- `Count`;
- `Capacity`.

BST follows the same rule today: it uses explicit `BstNode` parent/left/right references and manual comparison/link-rewiring logic. It does not use `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, `Dictionary<TKey,TValue>`, a built-in tree, or a library binary-search operation to implement the taught structure.

AVL also follows the rule: `AvlNode` stores explicit parent/left/right references and a manually maintained cached height. Balance factors are computed from those heights, and LL/RR/LR/RL repairs explicitly rewire references with custom left/right rotations. No ready-made balanced-tree implementation performs the teaching algorithm.

This rule also applies to the live Red-Black Tree, Binary Heap, Graph, Bubble Sort, Selection Sort, Insertion Sort, and Merge Sort modules and will continue to apply to future sorting and search algorithms.

Standard .NET infrastructure such as `Task`, `CancellationToken`, `Guid`, `SemaphoreSlim`, Blazor components and browser interop may still be used. These provide application infrastructure rather than the algorithm being taught.

---

# Queue & Stack

Queue and Stack began as a simple simulation of four operations:

- Stack: `Push` / `Pop`
- Queue: `Enqueue` / `Dequeue`

The module has since been expanded into a complete learning environment.

---

## Stack

The Stack follows the **LIFO** rule:

> Last In, First Out.

Implemented operations:

- `Push`
- `Pop`
- `Reset`
- Find by value
- Find by ID
- Delete by value
- Delete by ID

Search traversal starts at:

```text
TOP → bottom
```

When searching or deleting, the simulator checks one element at a time in the same order a Stack traversal would follow.

---

## Queue

The Queue follows the **FIFO** rule:

> First In, First Out.

Implemented operations:

- `Enqueue`
- `Dequeue`
- `Reset`
- Find by value
- Find by ID
- Delete by value
- Delete by ID

Search traversal starts at:

```text
FRONT → rear
```

The simulator does not secretly locate the result before the animation starts. The algorithm itself checks each element until a match is found or the complete Queue has been inspected.

---

# Binary Search Tree (BST)

BST is the second live data-structure module and the first non-linear structure in the project.

Implemented operations:

- `Insert` by key;
- `Search` by key;
- `Delete` by key;
- explicit `Balance BST` with the Day-Stout-Warren (DSW) algorithm;
- `Reset` lab state.

This implementation uses a strict ordering invariant:

```text
left subtree values < node value < right subtree values
```

Duplicate keys are therefore rejected. The simulator still follows the real comparison path until it reaches the equal node; it does not reject a duplicate before the algorithm runs.

Each node is a manually linked `BstNode` object with:

- a value;
- a learner-facing short ID derived from an internal `Guid`;
- a parent reference;
- a left-child reference;
- a right-child reference;
- transient renderer-neutral visual state.

## BST search and insert

Search and insert compare the target key with one node at a time.

```text
target < node  -> follow left
target > node  -> follow right
target = node  -> match
```

Insert stops when the required child reference is `null`, creates one new node object, and connects that node through the empty left/right reference.

## BST delete

Deletion explicitly simulates the three structural cases:

1. **Leaf** — disconnect the parent reference.
2. **One child** — redirect the surrounding reference directly to the child.
3. **Two children** — find the in-order successor, the leftmost node of the right subtree, and rewire that node into the removed node's position.

The two-child implementation does not hide the operation by calling a collection remove method. It also does not merely copy the successor's value into the target node. The successor node object itself is transplanted by updating parent/left/right references, which lets the Memory state teach object identity truthfully.

## Explicit BST balancing (Day-Stout-Warren)

A normal BST in this project still does **not** rebalance automatically after insert or delete. That behavior is preserved intentionally so learners can see how insertion order affects height.

The separate **Balance BST** action runs a manual Day-Stout-Warren implementation over the existing `BstNode` objects:

1. **Tree → vine** — scan the tree and perform right rotations until no left links remain.
2. **Vine → balanced shape** — perform spaced left rotations in compression passes until the tree becomes near-complete.

The operation preserves:

- every stored key;
- every node `Guid` / short display ID;
- the strict BST in-order ordering;
- `Count`.

It changes only the structural references (`root`, `parent`, `left`, `right`) and therefore can reduce tree height without allocating replacement nodes. The implementation does not flatten the tree into a `List<T>`/array and rebuild it, and it does not call a framework balancing collection.

DSW balancing is:

```text
O(n) time
O(1) algorithmic extra space
```

The lab publishes each vine scan, right rotation, compression pass, and left rotation through the same `SimulationState` playback runtime, so learners can pause and step through the actual pointer rewiring.

## BST complexity

BST search, insert, and delete are:

```text
O(h)
```

where `h` is tree height. A roughly balanced tree keeps `h` near `log n`; a highly skewed tree can have `h = n`. The run explanation reports comparison count, successor checks when relevant, and height before/after the operation. Balance runs instead report vine rotations, compression rotations/passes, `Θ(n)` current-run work, and the height change produced by DSW.

## BST playback and learning UI

The BST lab reuses the shared asynchronous simulation runtime and supports:

- Play;
- Pause;
- Step forward;
- Step back through captured snapshots;
- adjustable delay;
- Visual state;
- Memory state;
- optional Last Run popups;
- guided practice with SQLite-backed completion persistence;
- explicit DSW balancing with a skewed-tree practice task.

The Visual state uses an ordered tree layout by key. The Memory state shows the root reference plus each node object's parent/left/right references. Screen coordinates are never described as memory addresses.

---

# AVL Tree

AVL is the third live data-structure module and the second live tree module. It deliberately builds on the same strict BST ordering rule:

```text
left subtree values < node value < right subtree values
```

The difference is that every mutation also maintains a height for each custom node and checks:

```text
balance factor = height(left) - height(right)
```

A node is valid when its balance factor is `-1`, `0`, or `+1`.

Implemented operations:

- `Insert` by key;
- `Search` by key;
- `Delete` by key;
- `Reset` lab state.

Duplicate keys are rejected only after the real comparison path reaches the equal node.

Each `AvlNode` stores:

- value;
- short learner-facing ID derived from an internal `Guid`;
- parent, left-child, and right-child references;
- cached height;
- transient renderer-neutral simulation state.

## AVL insert and search

Search is the same ordered search used by BST. It follows one child link per comparison and never rotates the tree.

Insertion first finds the empty child link using the BST rule. After connecting the new node, the implementation walks back toward the root, recomputes cached heights, checks balance factors, and rotates only if an ancestor becomes invalid.

## AVL rotations

The implementation manually supports all four repair cases:

- **LL** — one right rotation;
- **RR** — one left rotation;
- **LR** — left rotation on the heavy child, then right rotation on the unbalanced ancestor;
- **RL** — right rotation on the heavy child, then left rotation on the unbalanced ancestor.

Rotations do not copy values between nodes. They rewire the real custom node references while preserving BST ordering and node identity. Cached heights are updated after every primitive rotation.

## AVL delete

Deletion begins with the same three structural BST cases:

1. leaf;
2. one child;
3. two children using the in-order successor.

For the two-child case, the real successor node object is transplanted instead of copying its value. After the structural delete, AVL continues upward and may perform more than one repair while restoring the balance invariant.

## AVL complexity

Because AVL actively keeps the tree height logarithmic, search, insert, and delete are:

```text
O(log n)
```

Run explanations report key comparisons, successor checks when relevant, upward balance checks, primitive rotation count, first diagnosed rotation case, and height before/after the operation.

## AVL playback and learning UI

The AVL lab supports:

- Play;
- Pause;
- Step forward;
- Step back through captured snapshots;
- adjustable delay;
- Visual state with value, short ID, cached height, and balance factor;
- highlighted unbalanced, rotation-pivot, rotating, and restored-balanced states;
- Memory state with root/parent/left/right references and cached height;
- optional Last Run popups;
- guided rotation practice with SQLite-backed completion persistence.

The Learn First section includes compact LL/RR/LR/RL recipes so a learner can predict the repair before running it.

---

# Red-Black Tree

Red-Black Tree is the fourth live data-structure module and the third live tree module. It preserves the same strict BST ordering rule:

```text
left subtree values < node value < right subtree values
```

but controls tree height through color invariants rather than AVL's exact balance factor. Null child references are treated as conceptual black `NIL` leaves by the algorithm.

Implemented operations:

- `Insert` by key;
- `Search` by key;
- `Delete` by key;
- `Reset` lab state.

Duplicate keys are rejected only after the real BST comparison path reaches the equal node.

Each `RedBlackNode` stores:

- value;
- short learner-facing ID derived from an internal `Guid`;
- parent, left-child, and right-child references;
- explicit `Red` or `Black` color;
- transient renderer-neutral simulation state.

## Red-Black invariants

The learning UI keeps the balancing rules visible:

- the root finishes black;
- every node is red or black;
- null/NIL leaves are black conceptually;
- a red node cannot have a red parent or red child;
- every path from one node to a descendant NIL leaf has the same black-height.

These rules imply that the longest root-to-leaf path is at most about twice the shortest, so tree height remains `O(log n)`.

## Red-Black insert

Insertion first follows the ordinary BST search path and creates the new node as **red**. Starting red avoids immediately increasing black-height on only one path. If the parent is also red, insertion fix-up inspects the uncle and grandparent.

The implementation manually handles:

- **red uncle** — recolor parent and uncle black, grandparent red, then continue upward;
- **triangle** — rotate the parent to turn the bend into a line;
- **line** — recolor parent/grandparent and rotate the grandparent;
- mirrored left/right forms of the same cases;
- final root recoloring when required.

## Red-Black delete

Deletion begins with the same three structural BST cases:

1. leaf;
2. one child;
3. two children using the in-order successor.

The two-child case transplants the real successor node object instead of copying its value. The algorithm tracks the color actually removed from the affected root-to-NIL path. If that removed color was red, black-height is unchanged and no delete fix-up is needed. If it was black, delete fix-up repairs the missing black contribution.

The implementation manually covers the standard sibling cases and their mirrors:

- red sibling;
- black sibling with two black children;
- black sibling with a red near child and black far child;
- black sibling with a red far child.

Recoloring changes node color fields. Rotations rewire the same `parent`, `left`, `right`, and root references while preserving node IDs, values, and BST ordering.

## Red-Black complexity

Because the color invariants keep height logarithmic, search, insert, and delete are:

```text
O(log n)
```

Run explanations report key comparisons, successor checks when relevant, fix-up checks, recolor count, rotation count, first repair case, height before/after, and black-height before/after.

## Red-Black playback and learning UI

The Red-Black lab supports:

- Play;
- Pause;
- Step forward;
- Step back through captured snapshots;
- adjustable delay;
- Visual state with actual RED/BLACK node color separated from temporary checking/violation/recolor/rotation states;
- Memory state with node identity, color, root/parent/left/right references, and explicit `null → NIL(B)` child explanations;
- optional Last Run popups;
- guided practice for root-black, red-uncle, triangle, line, delete-fix-up, and search behavior;
- task completion kept in the scoped C# application-session store.

The `LEARN FIRST` section intentionally follows the established Queue & Stack / BST / AVL visual hierarchy instead of introducing a separate design language.

---

# Heap family and generalized d-ary Heap

The project now makes a terminology distinction that is important for correctness:

> **Heap** is a family / invariant concept. **Binary Heap** is one concrete heap implementation where each parent has at most two children.

There is therefore no separate canonical data structure called an “ordinary heap” that should be implemented beside Binary Heap. To make the broader concept executable rather than merely theoretical, the `/structures/heap` lab implements a **generalized d-ary heap**. The existing Binary Heap remains available separately at `/structures/binary-heap`.

The generalized implementation reuses the same custom `ManualHeapArray<HeapElement>` storage and stable element identities, but derives relationships with:

```text
parent(i)   = (i - 1) / d
children(i) = di + 1 ... di + d
```

The UI currently exposes `d = 3`, `d = 4`, and `d = 5` so the learner can clearly see a non-binary shape. Core also supports `d = 2`, which is tested to demonstrate that Binary Heap is exactly the special case. Changing `d` or Min/Max mode is allowed only while the heap is empty; the app never silently reinterprets or rebuilds a non-empty heap.

The generalized lab implements insert, extract-root, search, delete, clear, Visual state, Memory state, playback history, result explanations, and guided practice. Bubble-down differs pedagogically from the binary version because it may compare up to `d` children at a level before choosing the highest-priority child.

## Heap layout and explanation fixes

The Heap work also hardens the learning UI:

- the main content column no longer centers itself inside a fixed `96rem` maximum, removing the large empty gap between the sidebar and page content on wide screens;
- operation controls use bounded responsive grids so the page itself does not scroll horizontally; only visualization internals may scroll when the data genuinely needs extra width;
- Binary Heap Learn First now explains **when and why** to use a heap (priority queues, schedulers, Dijkstra, Prim, Top-K) and the trade-off between O(1) root access and O(n) arbitrary search;
- Concepts & Memory explicitly distinguishes Heap, d-ary Heap, and Binary Heap.

---

# Binary Heap (Min/Max)

Binary Heap remains the dedicated binary specialization and is now paired with the generalized d-ary Heap family lab. It deliberately teaches the same structure in two synchronized representations:

- a **complete binary tree** for understanding parent/child priority;
- a **custom dynamic array** for understanding the actual storage and index relationships.

The learner can choose **Min Heap** or **Max Heap** while the heap is empty. A non-empty heap is never silently rebuilt when switching type.

Implemented operations:

- `Insert` by value;
- `Extract root` (minimum in Min Heap, maximum in Max Heap);
- `Search` by value;
- `Delete` the first matching value;
- `Clear`;
- Min/Max mode selection while empty.

Duplicates are allowed. Equal values remain separate `HeapElement` objects with different short IDs.

## Heap storage and index model

Core owns a custom `ManualHeapArray<HeapElement>` backed by a raw `T[]`. It does not use `PriorityQueue`, `List`, `SortedSet`, `Array.Sort`, or another ready-made heap implementation. Capacity growth allocates a larger raw array and copies used references with an explicit loop.

The complete-tree relationships are calculated from indexes:

```text
parent(i) = (i - 1) / 2
left(i)   = 2i + 1
right(i)  = 2i + 2
```

Heap elements therefore do not need explicit parent/left/right fields. Moving an element reference to a different array index automatically changes its conceptual tree relationships while preserving the same object ID and value.

## Insert and bubble-up

Insert first appends the new element at the next array slot. That preserves the complete-tree shape. The algorithm then compares the new element with its parent and swaps upward while it has higher priority:

- Min Heap: smaller value has higher priority;
- Max Heap: larger value has higher priority.

Each swap is implemented manually and exposed as a simulation step.

## Extract-root and bubble-down

The root is always array index `0`, so reading the min/max root is conceptually `O(1)`. Extract-root removes that element, moves the last element reference to index `0`, releases the last used slot, and then repairs heap order downward.

When both children exist, bubble-down first chooses the child with higher heap priority, then compares that child with the current parent and swaps only when the heap property is violated.

## Search and delete

A heap is **not** ordered like a BST. Knowing that a parent has priority over its children does not tell us whether an arbitrary target value belongs in the left or right subtree. The live Search therefore checks used array slots linearly and has worst-case complexity `O(n)`.

Delete-by-value first performs that same truthful linear search. The matched slot is replaced with the last element reference, and the algorithm determines whether repair must bubble upward or downward. Because locating an arbitrary value is linear, delete-by-value is `O(n)` overall even though the repair path is only `O(log n)`.

## Heap complexity

```text
root access     O(1)
insert          O(log n)
extract root    O(log n)
search value    O(n)
delete value    O(n) overall
```

## Heap playback and learning UI

The Heap lab supports:

- Play / Pause / Step forward / Step back;
- adjustable step delay;
- Visual state showing complete-tree levels and the synchronized array representation;
- Memory state showing used versus reserved array slots, stable IDs, Count, Capacity, and index relationships;
- visible transient states for checking, candidate selection, swapping, insertion, removal, match, and repair path;
- optional Last Run popups explaining comparisons, swaps, repair direction, capacity changes, and complexity;
- guided practice for Min bubble-up, Min bubble-down, Max Heap behavior, linear search, delete repair, and capacity growth.

The `LEARN FIRST` section follows the same established hierarchy as Queue & Stack, BST, AVL, and Red-Black.

---

# Element identity

Every element is its own object.

An element contains:

- a value;
- an internal `Guid`;
- a short learner-facing ID;
- its current visual simulation state.

For example:

```text
value: 7
ID:    #A1B2C3
```

and:

```text
value: 7
ID:    #F4E5D6
```

are two different elements even though they contain the same value.

This is intentional.

Duplicate values are valid because:

```text
same value != same object
```

This distinction makes it possible to teach both:

- search by value;
- search by exact element identity.

The learner only needs the short displayed ID. Full GUID values are intentionally hidden from the UI.

---

# Search simulation

Both Stack and Queue support:

```text
Find by value
Find by ID
```

Search is implemented as a real linear traversal.

For every checked element, the simulation can show:

- the current element;
- its value;
- its short ID;
- whether it matches;
- how many elements have been checked;
- which direction the traversal is moving.

Typical visual states include:

```text
checking
visited
matched
adding
removing
pointer target
```

If the requested value or ID does not exist, the algorithm checks the entire structure.

A missing short ID therefore produces a real not-found traversal instead of being rejected by the UI before the algorithm starts.

---

# Delete simulation

Both structures support:

```text
Delete by value
Delete by ID
```

Delete is deliberately shown as two pieces of work:

1. **Find the target**
2. **Remove it from storage**

If no matching element is found, the structure remains unchanged.

If a match is found, the simulator records:

- how many comparisons were required;
- the backing-array index of the match;
- how many references were shifted;
- `Count` before and after deletion;
- `Capacity` before and after deletion;
- the complexity of the traversal;
- the complexity of the complete operation.

---

# What happens in memory after deletion?

The current Queue and Stack implementations use the custom `ManualDynamicArray<T>`.

Suppose the backing array contains:

```text
[ A ][ B ][ C ][ D ]
```

and `B` is removed.

The implementation manually shifts the later references:

```text
[ A ][ C ][ D ][ empty ]
```

The used part of the structure therefore contains **no logical hole**.

Internally, the algorithm performs this shift itself with a loop.

`Count` decreases, while `Capacity` normally stays unchanged.

For example:

```text
Count:    4 → 3
Capacity: 4 → 4
```

The removed object is no longer referenced by this structure. If no other reference to that object exists, the .NET garbage collector may reclaim it later.

The application does not pretend to display real RAM addresses. Learning labels such as:

```text
MEM-#A1B2C3
```

represent object identity only.

---

# Visual state vs Memory state

A major learning feature of the project is the separation between:

## Visual state

The structure is drawn in the form that best explains its rules.

For example:

- Stack is displayed vertically;
- its top element is marked;
- Queue shows its front and rear;
- BST is laid out as an ordered tree with smaller keys left and larger keys right;
- AVL uses the same ordering view and additionally shows cached height, balance factor, and rotation states;
- active elements/nodes are highlighted during traversal.

## Memory state

The same data is shown from the point of view of its storage implementation.

The memory view explains:

- backing-array slots for the linear module;
- occupied vs reserved capacity for the custom dynamic array;
- node parent/left/right references for BST and AVL;
- cached height and balance factor for AVL nodes;
- root references;
- individual element/node objects;
- IDs and values;
- shifts or reference rewiring caused by deletion.

These views are intentionally separate.

A drawing used to explain an algorithm is **not necessarily the same as the program's memory layout**.

BST demonstrates this distinction with ordered screen position versus the actual parent/left/right object-reference graph. AVL extends the same separation: its Visual state explains balance and rotations, while Memory state shows the real reference rewiring and cached heights. Red-Black extends that separation again by keeping persistent node color distinct from transient fix-up emphasis. Heap now demonstrates the array/tree version of this distinction: the tree is conceptual, while Memory state shows the real custom backing-array slots. The same separation will be important for the future Graph module.

---

# Complexity explanations

The simulator does not display only a generic Big-O label.

It distinguishes between:

- the amount of work performed during the current run;
- the general worst-case complexity of the operation.

The learning UI uses:

### `n`

The total number of elements in the structure.

### `k`

How far the current traversal actually travelled.

### `O(1)`

The amount of work does not grow with the number of stored elements.

### `O(n)`

The worst-case amount of work can grow linearly with the number of elements.

### `Θ(k)`

The current run inspected only part of the structure.

### `Θ(n)`

The current run inspected the complete linear structure.

### `h`

Tree height: the length, in levels, of the longest root-to-leaf path. BST search, insert, and delete are `O(h)` because they follow tree links rather than scanning every node in storage order. AVL maintains `h = O(log n)` by rebalancing after mutations, which makes its search, insert, and delete worst-case `O(log n)`.

Example:

If the first Queue element matches the search target:

```text
comparisons: 1
current traversal: Θ(1)
worst case: O(n)
```

If the target is missing:

```text
comparisons: n
current traversal: Θ(n)
worst case: O(n)
```

For deletion, traversal work and storage-shift work are reported separately so that the learner can see that finding an item and physically removing it from an array-backed structure are not always the same cost.

---

# Simulation controls

The project contains a shared simulation runtime used by the algorithms.

Available controls include:

- Play
- Pause
- Step forward
- Step back
- Reset
- adjustable step delay

Algorithm code uses asynchronous simulation steps, allowing every meaningful change to pause and become visible before execution continues.

The simulation runtime handles:

- run state;
- pause state;
- single-step execution;
- speed changes;
- cancellation;
- current-step explanation.

---

## Step back

Actual C# execution is not reversed.

Instead, the UI records visual snapshots during a run.

`Step back` lets the learner review a previous snapshot.

`Step forward` moves through the recorded history. Once the learner reaches the newest live frame, stepping forward can continue the real simulation.

This preserves correct algorithm state while still allowing the learner to review previous steps.

---

# Result explanations

After Queue/Stack Find/Delete operations, BST Insert/Search/Delete/**Balance** operations, and AVL Insert/Search/Delete operations, the application can show a **Last Run** explanation.

The explanation contains two views:

## What happened

Shows:

- whether the element was found;
- number of checks;
- current-run complexity;
- full-operation complexity;
- worst-case complexity;
- AVL balance checks, primitive rotation count, and diagnosed rotation case when relevant.

## Memory

Explains:

- which element was removed;
- which slot was affected;
- whether references moved;
- whether an empty logical position remained;
- how `Count` changed;
- how `Capacity` changed;
- what can happen to the removed object afterward.

Automatic result popups can be disabled.

The most recent explanation remains available manually even when automatic popups are turned off.

---

# Guided Practice

The Queue & Stack page contains practical exercises.

Tasks are not simple checkboxes.

The application continuously observes relevant learner actions and run results. **Start task** is an optional setup/helper action: it can load or focus the suggested scenario, but a learner does not need to press it for completion tracking to work.

A task becomes `Completed` only after its required behavior has actually been observed. Validation is intentionally semantic rather than over-fitted to one exact input: when a task asks for a sequence such as `3, 7, 2, 7`, those required values must occur in order but extra values before, between, or after them are allowed. Exact-size validation is reserved for tasks whose learning goal is specifically a boundary case such as an empty or one-item structure.

When a task auto-completes, the app stores the explanation snapshot associated with the operation/run that satisfied it. Completed cards can reopen this **Completion explanation** later, including the action summary, memory explanation, why-it-matters text, concrete metrics, and the completed case. Both completion state and evidence are persisted through the C# learning-state API into SQLite.

Current exercises cover scenarios such as:

- building Stack and Queue;
- predicting `Pop` and `Dequeue`;
- finding by ID;
- finding by value;
- deleting by ID;
- deleting by value;
- duplicate values;
- existing targets;
- missing targets;
- missing IDs;
- deleting the same value multiple times;
- searching for an element after deleting it;
- empty structures;
- one-element structures;
- deletion from the middle of the backing array;
- observing memory shifts;
- mixed final exercises.

Completed Queue & Stack progress and completion-evidence snapshots are persisted through the C# backend in SQLite.

The BST lab has its own guided tasks for:

- building a branching BST;
- successful and missing searches;
- leaf deletion;
- one-child deletion;
- two-child deletion with successor search;
- opening Memory state after structural rewiring;
- duplicate-key rejection;
- balancing a deliberately skewed BST with DSW and inspecting the same node IDs in Memory state after reference rewiring.

BST task completion and the exact completion-evidence snapshot are also persisted through the C# backend in SQLite.

The AVL lab adds guided tasks for:

- LL insertion repair;
- RR insertion repair;
- LR insertion repair;
- RL insertion repair;
- deletion that triggers upward rebalancing plus Memory-state inspection;
- successful and missing search after rotations.

AVL task completion and the exact completion-evidence snapshot are stored through the same SQLite-backed C# persistence layer.

---

# Concepts & Memory

The application includes a dedicated learning page:

```text
/learn/concepts
```

It provides simple explanations for concepts reused across the simulations, including:

- LIFO;
- FIFO;
- `n`;
- `k`;
- `O(1)`;
- `O(n)`;
- `Θ(...)`;
- value vs identity;
- object vs reference;
- `Count`;
- `Capacity`;
- memory allocation;
- garbage collection;
- Visual state vs Memory state;
- BST tree height `h` and why shape changes `O(h)` cost;
- AVL balance factor and the LL / RR / LR / RL rotation cases.

The page starts with short beginner-friendly explanations and provides links to deeper external documentation for learners who want more detail.

---

# Architecture

The solution is split into three main runtime projects plus the Core test project.

```text
AlgorithmVisualizer.sln

src/
├── AlgorithmVisualizer.Core/
├── AlgorithmVisualizer.Client/
└── AlgorithmVisualizer.Server/
```

## `AlgorithmVisualizer.Core`

Contains algorithm and data-structure behavior.

Responsibilities include:

- structure state;
- manual data-structure implementation;
- traversal;
- mutation;
- complexity information;
- simulation-step descriptions.

Core code does not render UI.

Relevant Core files include:

```text
DataStructures/
├── Linear/
│   ├── LinearElement.cs
│   ├── LinearStructureSimulationBase.cs
│   ├── LinearTraversalResult.cs
│   ├── ManualDynamicArray.cs
│   ├── Stack/StackSimulation.cs
│   └── Queue/QueueSimulation.cs
└── Trees/
    ├── Bst/
    │   ├── BstNode.cs
    │   ├── BstNodeSnapshot.cs
    │   ├── BstOperationResult.cs
    │   └── BstSimulation.cs
    └── Avl/
        ├── AvlNode.cs
        ├── AvlNodeSnapshot.cs
        ├── AvlOperationResult.cs
        └── AvlSimulation.cs
```

## `AlgorithmVisualizer.Client`

Blazor WebAssembly presentation layer.

Responsibilities include:

- Razor pages and components;
- visual rendering;
- memory rendering;
- playback controls;
- simulation history;
- guided practice;
- result modals;
- learning preferences and progress loaded from/persisted to the C# backend.

The Core project depends on the simulation runtime through:

```csharp
ISimulationRuntime
```

The Blazor client provides the concrete:

```csharp
SimulationState
```

This keeps algorithms independent from rendering concerns.

## `AlgorithmVisualizer.Server`

ASP.NET Core host and persistence layer.

Responsibilities include:

- serving the Blazor WebAssembly client;
- same-origin `/api/learning-state` endpoints;
- anonymous learner identity through an HttpOnly cookie;
- SQLite persistence for Guided Practice completion and learning preferences;
- automatic database/schema creation at startup.

The SQLite file defaults to `src/AlgorithmVisualizer.Server/App_Data/learning-state.db` and is intentionally git-ignored. The persistence layer uses explicit parameterized SQL through `Microsoft.Data.Sqlite`; it does not implement or replace any taught data structure or algorithm.

---

# Technology

- C#
- .NET 8
- Blazor WebAssembly
- Razor components
- CSS isolation
- WebAssembly
- ASP.NET Core backend hosted in C#
- SQLite persistence through `Microsoft.Data.Sqlite` and explicit parameterized SQL
- no project-owned JavaScript or JS interop
- xUnit + Microsoft.NET.Test.Sdk for Core unit tests

The algorithmic and simulation logic is implemented in C#.

Razor and CSS are used for the presentation layer.

JavaScript/TypeScript is not used to implement the algorithms.

---

# Running the application

Requirements:

- .NET 8 SDK

From the repository root:

```bash
dotnet restore
dotnet run --project src/AlgorithmVisualizer.Server
```

Or:

```bash
cd src/AlgorithmVisualizer.Server
dotnet run
```

The ASP.NET Core host serves both the Blazor WebAssembly client and the SQLite-backed learning-state API from the same origin. The server prints the local application URL.

---

# Build

```bash
dotnet build AlgorithmVisualizer.sln
```

# Tests

Focused Core tests are included in:

```text
tests/AlgorithmVisualizer.Core.Tests
```

Run them with:

```bash
dotnet test tests/AlgorithmVisualizer.Core.Tests/AlgorithmVisualizer.Core.Tests.csproj
```

The Queue/Stack test suite now covers true LIFO/FIFO removal order, duplicate-value traversal direction, short displayed-ID lookup, manual array compaction after keyed deletion, and the Count-versus-Capacity behavior of the custom backing array after Clear.

The BST test suite covers insertion shape, duplicate rejection, search success/miss, all three delete cases, successor-node identity, skewed-tree height, DSW balancing of right- and left-skewed trees, height reduction, rotation counts, parent-link/BST invariants, and preservation of node identity across balancing.

The AVL test suite covers LL/RR/LR/RL repairs, increasing-order height control, duplicate rejection, found/missing search, leaf/one-child/two-child deletion, delete-triggered rebalancing, successor/promoted-node identity, clear/reset behavior, and recursive validation of BST order, parent links, cached heights, balance factors, and the AVL balance invariant.

---

# Learning design principles

Every future module should follow the same principles established by Queue & Stack and extended by BST and AVL.

## 1. Show the algorithm, do not hide it

If an operation requires traversing five elements, the learner should see all five checks.

## 2. Use the real algorithmic order

Animations must reflect the actual implementation.

## 3. Explain the current run

Do not show only theoretical worst-case complexity.

## 4. Separate visual shape from memory storage

The teaching diagram and the storage model may differ.

## 5. Keep explanations beginner-friendly

Technical terms should be introduced only when they add learning value and should be explained in simple language.

## 6. Do not fake interactivity

A module without an implemented algorithm remains explicitly marked as TODO.

## 7. Implement teaching algorithms manually

Do not replace an implementation with a framework collection or library algorithm that hides the behavior being taught.

---

# Next implementation direction

Queue & Stack established the linear foundation; BST validated linked non-linear structures; AVL validated strict height-based balancing; Red-Black now validates invariant-driven recoloring plus insertion/deletion fix-up on top of the same learning architecture:

```text
algorithm
    ↓
simulation steps
    ↓
playback runtime
    ↓
visual state
    ↓
memory state
    ↓
complexity explanation
    ↓
guided practice
```

The structure foundation includes our custom Queue, Stack, BST, AVL, Red-Black, generalized Heap, Binary Heap, Matrix, Graph, and ManualVector implementations. BFS and DFS reuse this live Graph directly and the same manual linear-storage foundation for Queue/Stack behavior. Dijkstra reuses the same Graph plus the existing `ManualHeapArray` storage for its Advanced priority frontier; Prim/Kruskal are now live and reuse Graph/Heap foundations plus a hand-written DSU.

---

## Current milestone

**Queue & Stack: implemented as the complete linear-structure learning module.**

**Binary Search Tree: implemented as the first complete tree module, including structural deletion, explicit Day-Stout-Warren balancing, Visual/Memory views, playback history, result explanations, and guided practice.**

**AVL Tree: implemented as the strict height-balanced extension, including cached heights, balance factors, all four rotation cases, insert/delete rebalancing, Visual/Memory views, run explanations, and rotation-focused guided practice.**

**Red-Black Tree: implemented as the color-invariant balanced-tree extension, including explicit RED/BLACK nodes, conceptual black NIL leaves, insertion and deletion fix-up, recoloring, rotations, black-height teaching, Visual/Memory views, run explanations, and guided practice.**

**Generalized Heap: implemented as the d-ary family lab with d = 2/3/4/5 comparison, Min/Max modes, generalized index formulas, and explicit child-candidate selection.**

**Binary Heap: implemented as the focused d = 2 complete-tree / custom-array module with Min/Max modes, bubble-up, bubble-down, extract-root, linear search, arbitrary delete repair, Visual/Memory views, capacity teaching, run explanations, and guided practice.**

**Matrix: implemented as the pre-Graph row-major module with direct cell editing, bulk custom-value input with automatic dimension detection, arithmetic, multiplication, transpose, powers, determinant, minors/cofactors, elementary row operations, REF/RREF/rank, inverse, equation solving, graph-adjacency presets, Visual/Memory views, a row-list-first memory explanation with expandable real `double[]` backing storage, continuously auto-tracked guided practice with optional Start/Restart setup and active progress, and automatic three-view Last Run explanations.**

The project now has reusable manual linear structures, three reusable manual tree foundations, two reusable heap views of the same family, a reusable Matrix foundation, a live reusable Graph structure, complete BFS/DFS traversal labs, a complete Dijkstra weighted-shortest-path lab, a complete Topological Sort dependency-order lab, a complete Prim/Kruskal MST lab, a live Vector data-structure lab plus live Gradient Descent, Linear Regression, Logistic Regression, KNN, KD-Tree, K-Means, Decision Tree, and PCA Machine Learning foundations, and seven complete Sorting lessons: Bubble Sort, Selection Sort, Insertion Sort, Merge Sort, Quick Sort, Heap Sort, and advanced Topological Sort.

### Matrix memory visualization

The Matrix Memory State now presents the conceptual structure literally as an outer list whose row lists (`A[0]`, `A[1]`, ...) sit **side by side**. The workspace stacks Matrix A, then Matrix B, then the result vertically so the learner reads one complete nested list at a time. The real contiguous row-major `double[]` remains available as an expandable implementation detail rather than dominating the learning view.


## Graph module

Graph is now a live structure lab before the traversal/path algorithms. Graph Learn First now uses the same launchpad visual hierarchy as Queue & Stack / BST, with the four concepts ordered as vertex+edge, direction, weight, and adjacency-list-vs-matrix representation. It implements directed/undirected and weighted/unweighted graphs with explicit vertex/edge objects, manual adjacency-list storage and the existing `ManualMatrix` for the synchronized adjacency matrix. The lab supports add/search/rename/remove vertex, add/search/update-weight/remove edge, direct-neighbor inspection, self-loops, zero/negative weights at the generic structure level, Visual/Memory state, playback and guided practice. BFS and DFS are now separate live algorithm modules that reuse this graph rather than creating another graph representation. Dijkstra, Topological Sort, and Prim/Kruskal MST are now live and reuse this exact Graph snapshot. Graph Core no longer inherits the Matrix page's 8×8 teaching cap: `ManualMatrix` is reusable/growable in Core, while MatrixPage alone keeps the 8×8 input limit. Larger Graph adjacency matrices scroll inside Memory State.

Graph Visual State supports direct vertex dragging without changing graph topology. Vertex positions are UI-only overrides keyed by stable vertex ID; edges, arrows, weights and self-loops are recalculated from the moved coordinates. The workspace is content-bounded but effectively unbounded: there is no fixed drag clamp, the SVG stage expands left/right/up/down only when current graph content reaches those bounds, and the surrounding viewport gains scroll range only for that occupied extent. Expanding on the left/top compensates scroll position so existing content does not jump. `Reset layout` removes manual positions and returns to the automatic layout. The drag implementation uses stable SVG group transforms with invariant numeric formatting so clicking/dragging cannot invalidate `foreignObject` coordinates.

### Razor control-flow markup rule

Graph UI markup follows a strict Razor rule: whenever `@if`, `@foreach`, `@for`, or similar control flow renders HTML/component markup, the body is always enclosed in `{ ... }`. This avoids Razor parser failures such as “Single-statement control-flow statements in Razor documents cannot contain markup.”


### Graph guided practice and explanations

## Dijkstra shortest paths

Dijkstra is the first live weighted graph algorithm and reuses the existing `GraphSnapshot` rather than introducing another graph model. The lab accepts unweighted edges as cost 1, supports zero-weight edges, and rejects every negative edge before traversal because the settlement proof would otherwise be invalid. **Basic · Linear Scan** finds the next finite unsettled minimum with an explicit O(V) scan, giving `O(V² + E)`. **Advanced · Min-Heap** uses a Dijkstra-specific binary min-heap built on the existing `ManualHeapArray` storage priority frontier with lazy duplicate entries instead of `PriorityQueue<TElement,TPriority>`, giving `O((V + E) log V)`. Both variants share the same manual relaxation rule, `dist[]`, `parent[]`, `settled[]`, Visual/Memory views, prediction, playback, Last Run explanations, behavior-based auto-practice, and SQLite-backed completion evidence.

Graph follows the same shared learning-completion contract as every other live module. The page continuously observes real `GraphSimulation` results plus the current graph snapshot; **Start task** is only an optional scenario helper. Validation checks graph semantics rather than requiring specific labels whenever the label itself is not the concept: any suitable undirected branching example can prove symmetry, any real directed edge can prove a missing reverse direction, any zero-weight edge can prove presence-vs-weight semantics, and any sufficiently sparse topology can support list-vs-matrix comparison. Completion and its exact explanation snapshot are persisted through the C# learning-state store into SQLite.

Every completed Graph action now has a learner-facing explanation layer. The Last Run card can reopen a dismissible interactive explanation with three views: the action that happened, how the adjacency list and Matrix-backed representation changed together, and why the operation matters for later graph algorithms. Automatic result explanations can be disabled and that preference is persisted by the C# backend in SQLite. The Graph `Result popups` toggle now lives in the Playground header beside `Last run explanation`, matching the established Queue/Stack, BST, AVL, and Heap interaction pattern instead of creating a graph-specific preference strip below the result. The Graph lab also exposes direct `Graph concepts` and `Concepts & memory` links beside the Visual/Memory controls so theory is always reachable from the working area.

### Concepts navigation rule

- Concepts links from modules/tasks use dedicated routes such as `/learn/concepts/bst` or `/learn/concepts/heap-sort`; do not use `/learn/concepts#...` for cross-page navigation because Blazor WASM renders the target after the browser fragment-scroll moment. In-page jump links inside the already-rendered Concepts page may still use `#...`.


## Topological Sort dependency ordering

Topological Sort is the live advanced graph-ordering lab classified as the final **Sorting** lesson, and it reuses the existing `GraphSnapshot`. The input must be directed; weighted directed graphs are accepted but weights are ignored because only dependency direction matters. **Basic · Kahn Queue** computes `indegree[]`, enqueues every zero-in-degree vertex in a head-index FIFO backed by the project `ManualDynamicArray<int>`, emits one ready vertex and decrements outgoing neighbors. **Advanced · DFS Postorder** uses recursive white/gray/black visitation, treats any gray-to-gray edge as a cycle, stores finish order in a manual postorder buffer and reverses that buffer only after a cycle-free traversal. Both variants are `O(V + E)` time and `O(V)` extra state, never mutate the source Graph, and reject cyclic input as having no valid ordering. The Client adds Visual/Memory state, DAG prediction, Last Run explanation, behavior-based auto-practice and SQLite-backed evidence snapshots.

## Linear Search

Route: `/search/linear`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Search.Linear`

Linear Search is implemented manually over a fixed raw teaching array. The Core loop checks indexes from `0` to `n-1`, performs one equality comparison per visited slot, stops at the first matching occurrence, and returns not-found only after the complete array has been inspected. It does not delegate the taught traversal to LINQ `First`, `Contains`, `Find`, or another framework search helper.

The lesson provides:

- Build → Predict → Watch → Explain → Practice workflow;
- Visual State with current / checked / found / unvisited positions;
- Memory State showing fixed slots, stable teaching identities, zero element writes, and `O(1)` extra algorithmic state;
- first-occurrence behavior for duplicate target values;
- empty, single-item, first, middle, last, duplicate, and missing cases;
- best-case `Θ(1)` and average/worst `Θ(n)` explanations tied to the actual visited prefix;
- seven continuously auto-tracked Guided Practice tasks with behavior-based validation and SQLite-backed completion evidence;
- dedicated concept routes `/learn/concepts/linear-search`, `/learn/concepts/linear-search-complexity`, and `/learn/concepts/linear-vs-binary-search`;
- mutation semantics: read-only inspection is safe, while CREATE/UPDATE/DELETE during an active search requires restart because the checked-index history belongs to the old snapshot.

## Binary Search

Route: `/search/binary`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Search.Binary`

Binary Search is implemented manually over a fixed **sorted** raw teaching array. Core uses an iterative `left` / `right` / `mid` loop and never delegates the taught search to `Array.BinarySearch`, LINQ, or another framework helper. If the learner enters unsorted values, the lab does not fake a Binary Search or silently call a framework sort: it pauses at **Step 1 · Sort input** and lets the learner choose Bubble, Selection, Insertion, Merge, Quick, or Heap Sort. The chosen existing manual C# sorting implementation runs through an immediate/silent simulation runtime, only its final sorted array is reused, and the Binary Search lesson then continues on that valid snapshot without replaying every sorting animation step.

The lesson provides:

- Build → Predict → Watch → Explain → Practice workflow;
- Visual State for active range, midpoint, discarded ranges, saved duplicate candidate, and final match;
- Memory State showing that array slots and stable teaching identities never move while only boundary integers change;
- **Basic · Any Match** mode, which returns the first matching midpoint encountered;
- **Advanced · First Occurrence** mode, which saves an equality candidate and continues into the left half until no earlier duplicate can remain;
- best-case `Θ(1)` and average/worst `Θ(log n)` explanations tied to actual midpoint comparisons and range reductions;
- sorted-input validation, empty/single boundaries, found-left/found-right/missing cases, and duplicate semantics;
- eight continuously auto-tracked Guided Practice tasks with behavior-based validation and SQLite-backed completion evidence;
- dedicated concept routes `/learn/concepts/binary-search`, `/learn/concepts/binary-search-complexity`, `/learn/concepts/binary-search-duplicates`, and shared comparison route `/learn/concepts/linear-vs-binary-search`;
- mutation semantics: reads are safe, while CREATE/UPDATE/DELETE requires restart because indexes, sortedness, and the current candidate interval belong to the old snapshot.

## BFS and DFS graph traversal

Breadth-First Search and Depth-First Search are now live graph-search modules at `/search/bfs` and `/search/dfs`.

- Both traverse the existing hand-written Graph representation through `GraphSnapshot`; directed graphs follow outgoing adjacency and edge weights are intentionally ignored.
- BFS uses `ManualDynamicArray<int>` with a manual head cursor as a FIFO queue, so dequeue does not shift the backing array. Graph snapshots expose each adjacency neighbor's vertex index, so traversal does not perform a hidden linear vertex lookup per edge. It marks vertices when enqueued, records `parent[]` and `distance[]`, and demonstrates unweighted shortest edge-count paths.
- DFS provides Basic recursive mode (real call-stack/backtracking) and Advanced iterative mode using `ManualDynamicArray<int>` as an explicit LIFO stack.
- Both expose Visual State, Memory State, prediction, playback, Last Run explanations, flexible auto-complete Guided Practice, and SQLite-backed completion evidence.
- Traversal runs are snapshot-based: graph CREATE/UPDATE/DELETE requires restart because the existing frontier/visited proof can become invalid.
- No project-owned JavaScript, framework `Queue<T>`/`Stack<T>`, graph package, or LINQ traversal implements BFS/DFS behavior.


## Data Structures — Vector

Route: `/structures/vector`
Client page: `Pages/DataStructures/VectorPage.razor`

Core namespace: `AlgorithmVisualizer.Core.DataStructures.Vector`

Vector is a live Data Structures module and reusable numerical foundation. `ManualVector` owns contiguous project-managed `double[]` storage and does not delegate the taught mathematics to `System.Numerics.Vector<T>`, LINQ vector helpers, or a numerical library. `VectorSimulation` publishes each component read, contribution, accumulator update, and result write through the existing simulation runtime.

Implemented operations: addition, subtraction, scalar multiplication, Hadamard product, dot product, L1 norm, L2 norm, L2 normalization, Euclidean distance, Manhattan distance, and cosine similarity. Equal-dimension preconditions are explicit, zero-vector normalization is rejected, and cosine similarity rejects zero-length operands. The Client follows the beginner-first learning shell: two compact A/B editors, four starter operations (add, dot product, L2 length, Euclidean distance), progressive disclosure for the remaining tools, prediction, playback before the Visual/Memory switch, concise run metrics, concrete Vector-use connections, Last Run explanations, behavior-based auto-complete practice, and SQLite-backed evidence. Vector has no fake implementation-choice panel because there is only one taught manual storage implementation. Gradient Descent reuses this Core for gradient L2 norm, learning-rate scaling and parameter subtraction. Machine Learning modules such as Gradient Descent, regression, KNN, K-Means, and PCA should continue reusing the same primitives rather than cloning vector loops.


## Machine Learning — Gradient Descent

Route: `/ml-foundations/gradient-descent`

Core namespace: `AlgorithmVisualizer.Core.MachineLearning.Optimization.GradientDescent`

Gradient Descent is Machine Learning lesson 1. Vector remains the reusable numerical prerequisite, but it belongs to Data Structures and no longer consumes an ML lesson number. It minimizes the transparent convex objective `J(theta) = 1/2 * Σ curvature[i] * (theta[i] - target[i])²`, which keeps the optimization mechanics visible before a supervised model is introduced. The objective-specific loss and analytical gradient use explicit C# loops. The optimizer then **reuses the existing `VectorSimulation`** for L2 gradient norm, `eta * gradient`, and `theta - eta*gradient`, so the ML layer does not duplicate Vector arithmetic.

The Client follows the same global lesson order as the rest of the application: **Learn First → header → tip → implementation choice → Build → Predict → Watch → compact playback/current step → shared run metrics → Visual/Memory → state → legend/links → When it fits → Guided Practice → Next Lesson**. Basic mode uses a fixed learning rate; Advanced mode uses inverse learning-rate decay. The default Build step now shows only the example, starting point and step size; iteration budget, tolerance and decay are progressively disclosed as advanced settings so the first run is not overloaded with optimizer terminology. Runs may converge by gradient tolerance, stop at the iteration budget, or trigger a controlled divergence guard after repeated loss growth. Visual State shows the two-parameter loss landscape and committed optimization path plus theta/gradient/scaled-gradient/next-theta vectors. Memory State shows aligned vector slots and conceptual double offsets. Configuration edits require a fresh run because a timeline represents one stable objective and optimizer setup.

Six behavior-based practice tasks cover stable loss reduction, zero-update convergence, controlled divergence, a shrinking learning rate, the full iteration-budget stop, and convergence to a non-zero target. The Core is additionally tested with a three-parameter objective so the drawn 2D lesson does not become a 2D-only optimizer. Completion evidence is persisted through the existing SQLite-backed practice store. Linear Regression is now the next live model and opens directly from the Gradient Descent progression card.


## Machine Learning — Linear Regression

Route: `/ml-foundations/linear-regression`

Core namespace: `AlgorithmVisualizer.Core.MachineLearning.Supervised.LinearRegression`

Linear Regression is Machine Learning lesson 2, directly after Gradient Descent. The first live model is intentionally univariate so prediction, residuals, the fitted line, and parameter updates all remain visible. Core implements `yHat = weight * x + bias`, residuals, mean squared error, analytical `dw` / `db`, and full-batch Gradient Descent with explicit loops. X values, Y values, predictions, and residuals are stored in project-owned `ManualVector` instances; no ML, statistics, or numerical-optimization package performs the taught work.

The Client uses the same shared lesson hierarchy and shared button grammar as Search/Sort modules. Beginners start from dataset presets and only three visible model controls: starting weight, starting bias, and learning rate. Custom `x,y` points plus stopping controls are progressively disclosed. Prediction asks whether the first update should increase, decrease, or barely change the weight. Visual State draws training points, current predictions, residual distances, and the current line. Memory State shows aligned X/Y/prediction/residual vector slots and scalar model parameters. Playback can rewind every meaningful batch-training state, Last Run explains the fit, and five practice tasks cover upward/downward trends, an already-fitted line, noisy data, and unstable learning-rate behavior.

## Machine Learning — Logistic Regression

Route: `/ml-foundations/logistic-regression`

Core namespace: `AlgorithmVisualizer.Core.MachineLearning.Supervised.LogisticRegression`

Logistic Regression is Machine Learning lesson 3. The first classifier stays deliberately univariate so the entire chain remains visible: `z = w*x + b` → stable sigmoid → probability → visible `0.5` threshold → class. Full-batch binary cross-entropy training uses explicit `dw = mean((p-y)*x)` and `db = mean(p-y)` loops, then the same Gradient Descent update pattern used by the previous lessons. X, labels, scores, probabilities, and probability errors reuse project-owned `ManualVector`; predicted classes are a discrete `int[]`. No ML/statistics/optimizer library performs the taught classifier behavior.

The Client follows the shared learner-first shell but uses a cleaner classification-specific visualization: true 0/1 labels, probability points on the sigmoid curve, the horizontal `0.5` threshold, the exact vertical decision boundary, and one compact `x → z → p → class` explanation for the example nearest the boundary. The beginner surface exposes only presets, starting weight, starting bias, and learning rate; custom examples and stopping rules are progressively disclosed. Five behavior-based practice tasks verify positive/negative class direction, left/right boundary movement, and loss reduction on a noisy example. Completion evidence persists through the existing SQLite-backed practice store.

## Machine Learning — K-Nearest Neighbors

Route: `/ml-foundations/knn`

Core namespace: `AlgorithmVisualizer.Core.MachineLearning.Supervised.Knn`

K-Nearest Neighbors is Machine Learning lesson 4. The learner-facing visual is intentionally two-dimensional: labeled examples are stored, a new query point is placed in the same feature space, every distance is measured, the nearest `k` examples are retained, and their labels vote. Core remains dimension-independent and is tested beyond 2D.

Training examples and the query use project-owned `ManualVector` storage. Euclidean and Manhattan distance reuse the existing `VectorSimulation`; KNN does not clone those vector primitives. The KNN layer itself performs the full example scan, deterministic ordered top-k insertion, and binary majority vote with explicit loops. It does not call a framework sort, nearest-neighbor helper, ML package, or KD-tree implementation. This teaching implementation uses odd `k` so binary votes cannot tie. Exact prediction work is `Θ(n·d)` for distance evaluation plus `O(n·k)` for the intentionally visible top-k insertion, with `O(n + k)` derived working state.

The Client follows the shared lesson shell and keeps the first interaction small: choose a preset, move the 2D query, choose odd `k`, predict class 0/1, then watch the distance scan and vote. Custom data and Manhattan distance are progressively disclosed. Visual State shows class points, the query, the active distance, and only the current top-k links; Memory State shows ManualVector features/query, per-example distances, ordered neighbor indexes/distances, and vote counters. Guided Practice verifies both class neighborhoods, outlier smoothing with larger `k`, exact-match distance zero, and a case where Manhattan and Euclidean choose different nearest examples. KD-Tree is the next live Phase 1 step and is presented as search acceleration, not a change to KNN voting.

## Machine Learning — KD-Tree

Route: `/ml-foundations/kd-tree`

Core namespace: `AlgorithmVisualizer.Core.MachineLearning.Supervised.KdTree`

KD-Tree is Machine Learning lesson 5. It keeps the learner-facing geometry two-dimensional while Core remains dimension-independent. The build alternates coordinate axes by depth, explicitly merge-sorts the active point-index range on that axis, chooses the median point, and stores explicit left/right node links. This teaching build is `O(n log² n)`; it intentionally favors transparent median construction over hiding selection behind a library.

Nearest-neighbor search descends toward the query side first, measures true Euclidean point distance through the existing `VectorSimulation`, tracks the best candidate, backtracks, and compares absolute split-plane distance with best-so-far before deciding whether the opposite subtree must be searched. Balanced low-dimensional queries can approach `O(log n)` average behavior, while the worst case remains `O(n)` and high dimensions reduce pruning effectiveness. No framework tree, sort helper, spatial index, nearest-neighbor package, or ML library performs the taught behavior.

The Client follows the shared learning shell with minimal default controls: dataset preset plus query x/y, class prediction, playback, compact Visual/Memory switch, fit guidance, and behavior-based Guided Practice. Custom points are progressively disclosed. Visual State combines true bounded spatial partitions with visited/best/pruned point states and a compact tree-by-depth view. Memory State exposes pointIndex, split axis, depth, left/right child IDs, query/current/best state, and pruning counters. K-Means is Machine Learning lesson 6 and follows KD-Tree in the curriculum.


## Machine Learning — K-Means

K-Means is Machine Learning lesson 6. The learner-facing plot is intentionally two-dimensional so assignments and centroid movement stay visible, while Core accepts any shared feature dimension. Stored examples and centroids use project-owned `ManualVector`; Euclidean distance reuses the existing `VectorSimulation`.

The clustering layer itself is explicit project code: every point scans all `k` centroids, stores its nearest assignment and distance, cluster means are recomputed with visible sum/count loops, empty clusters keep their previous centroid, and the run stops when assignments stabilize or centroid movement falls under tolerance. No clustering/ML library, framework grouping helper, or hidden numerical optimizer performs the taught behavior.

The Client keeps the first interaction small: choose a preset and `k`, predict whether the first grouping will change, then watch **Assign → Move centroids → Repeat**. Custom points and seed indexes are progressively disclosed. Visual State shows cluster membership and centroid movement; Memory State shows fixed point vectors beside assignments, distances, cluster counts, and mutable centroid vectors. Guided Practice covers clear 2/3/4-cluster cases, poor starting centroids, and a real empty-cluster edge case. Decision Tree is the next live Machine Learning lesson, step 7.


### Decision Tree — Machine Learning step 7

The live `/ml-foundations/decision-tree` lesson teaches a binary classification tree from first principles instead of delegating fitting to an ML package. Training examples are project-owned `ManualVector` feature vectors. For each active node, Core explicitly orders example indexes by feature with insertion sort, tests thresholds between distinct feature values, partitions the node's example indexes, computes Gini or entropy impurity, and keeps the candidate with the largest weighted impurity reduction. Pure nodes, maximum depth, too-small nodes, or no useful gain become leaves that store the majority class. Prediction then follows exactly one root-to-leaf path.

The beginner UI starts with one preset dataset and Gini impurity. Entropy, max depth, and raw `x,y,label` editing are progressively disclosed. Prediction asks which feature the root question will use. Visual State shows labeled 2D examples, the current/committed threshold, and the tree node hierarchy; Memory State shows fixed examples beside custom node records (example indexes, class counts, impurity, split feature/threshold/gain, child IDs, and leaf prediction). Guided Practice validates both root features, a deeper tree, an already-pure root leaf, and an imperfect/noisy case. The shared Last Run modal explains the chosen root rule, depth, leaf count, and training accuracy. PCA is Machine Learning step 8 and follows Decision Tree.

All live ML lessons keep the **outer Visual State in normal page flow instead of turning the entire state panel into a nested scroller**. Ordinary `ml-state-surface`, `ml-visual-board`, and child teaching panels use content-driven height; coordinate/tree/graph visuals resize or reflow rather than silently clipping geometry. A deliberately marked `ml-content-canvas` is the narrow exception for a content-driven coordinate stage such as Gradient Descent (and a minimum-legibility K-Means plot): only that drawing area may scroll, only when its actual stage no longer fits, and only far enough to expose the bounded content. Playback, Current Step, shared run metrics, legends, Memory explanations, and supporting cards remain outside the drawing scroller. SVG plots preserve their viewBox aspect ratio instead of being stretched into wide fixed-height containers.

### PCA — Machine Learning step 8

The live `/ml-foundations/pca` lesson closes Phase 1 with dimensionality reduction from first principles. Core accepts any shared feature dimension and stores raw, centered, and projected rows with project-owned `ManualVector` values. It explicitly computes the mean, sample covariance matrix, dominant principal direction through power iteration, the corresponding eigenvalue/explained-variance ratio, and one scalar PC1 projection per example. No numerical linear-algebra, PCA, eigen, or hidden vectorized library performs these taught steps.

The learner-facing lesson stays 2D so the geometry is visible: original points, the mean, the principal axis, projection links, and reconstructed positions on PC1 occupy one responsive feature-space surface without a nested Visual State scroller. Memory State exposes the raw/centered/projected vectors, mean, covariance cells, PC1, and scalar coordinates. Default controls are preset → direction prediction → Run PCA; raw points remain behind progressive disclosure. Guided Practice checks diagonal/horizontal/vertical dominant directions, zero-centered transformed data, and a case where one component preserves too little variance. Neuron + Activation Functions is Machine Learning step 9 and leads into MLP, Backpropagation, and optimizer comparison.


### Deep Learning — Phase 2 steps 9–12

Phase 2 intentionally starts with a concrete trainable unit instead of a standalone Computational Graph lesson. `/deep-learning/neuron-activations` computes each weighted contribution, bias, pre-activation `z`, and activation explicitly. `/deep-learning/mlp` connects those units into one hidden layer and one output neuron with project-owned `ManualVector`/`ManualMatrix` storage. `/deep-learning/backpropagation` reuses the same small network, stores forward values, applies the chain rule backward, exposes output/hidden deltas and every parameter gradient, then performs one visible learning update. `/deep-learning/optimizers` feeds the same deterministic cyclic sample stream to SGD, Momentum, and Adam so their different state and parameter paths can be compared directly.

The learner UI keeps the approved shell and the no-nested-scroll Visual State contract: neuron/network/parameter-space drawings resize or reflow with the lesson surface; playback, Current Step, compact metrics, Memory State, decision guide, practice, and Last Run modal stay in normal page flow. Computational Graph is deliberately removed from the curriculum and codebase as a standalone module; dependency/value-flow concepts are taught inside these concrete Deep Learning lessons. Phase 3 is also complete: Sparse Matrix (13) → PageRank (14) → Spectral Clustering (15) → Message Passing / basic GNN (16).


### Graph ML — Phase 3 steps 13–16

`/graph-ml/sparse-matrix` converts a small dense matrix into project-owned CSR storage (`values`, `columnIndexes`, `rowPointers`) and then performs sparse matrix-vector multiplication by visiting only stored entries. `/graph-ml/pagerank` reuses CSR as directed adjacency and explicitly shows teleportation, dangling-node mass, outgoing rank distribution, normalization, and iterative convergence while preserving total rank mass.

`/graph-ml/spectral-clustering` builds the normalized Laplacian `I − D⁻¹ᐟ²AD⁻¹ᐟ²` with `ManualMatrix`, solves the small symmetric eigenproblem with project-owned Jacobi rotations, row-normalizes the smallest-eigenvector embedding, and reuses the existing manual K-Means implementation to produce graph communities. `/graph-ml/message-passing` implements a basic GNN-style layer over CSR neighborhoods: gather → mean/sum aggregate → `Wself·h + Wnbr·m + b` → ReLU → synchronous layer commit. Two feature buffers prevent node iteration order from leaking into the mathematics.

All four Phase 3 lessons follow the approved shared shell and the no-nested-scroll Visual State contract. Core does not call sparse-matrix, PageRank, eigensolver, spectral-clustering, graph-neural-network, tensor, or automatic-differentiation libraries. The final Message Passing lesson closes the current Machine Learning curriculum at step 16.

- Queue/Stack uses one compact BST-style tile per action; shared operation buttons preserve canonical styling through CSS isolation.

### Sorting run-workspace consistency (Aug 20, 2026)

- Bubble, Selection, Insertion, Merge, Quick, Heap, and Topological Sort now use the same run ordering: playback/current-step controls first, compact live metrics and run-role/invariant context next, Visual/Memory state after that, and the color legend after the state surface.
- `RunMetricStrip` is the shared live-metric component for Sorting, Search & Traversal, and Machine Learning labs. Do not recreate raw metric rows locally; each metric keeps a compact bordered card so moving it between components cannot drop its visual design.
- Insertion Sort now follows the same page-level ordering as the other sorting labs and uses the shared `StateLegend`; its visualization no longer owns a private legend.

### ML visual-workspace refinement — lessons 1–6 (Aug 20, 2026)

The first six Machine Learning lessons now use a denser, content-aware visual layout. Gradient Descent keeps the true optimum centered, expands its parameter-space stage from the actual current/candidate/history extent, and makes only that coordinate canvas scroll when its minimum readable drawing size exceeds the available width. The outer Visual State panel never scrolls. K-Means uses the same explicit `ml-content-canvas` exception: all points and centroids are mapped from content bounds with padding, the plot is smaller, and the existing current-step/cluster summaries occupy a right-hand tracking column.

Linear Regression no longer spends an entire screen on a handful of points: the plot shares the row with a compact Current Fit panel. Logistic Regression uses the empty area under Closest to the Decision for the threshold rule and legend. KNN moves Current top-k into a permanent right-hand tracking column. Gradient Descent, Linear Regression, Logistic Regression, and KNN use the shared two-column `ml-predict-split` checkpoint instead of stacking prediction controls above a large empty right side.

KD-Tree now explains directly in the builder that the query is a new lookup point, not a tree insertion, and that class labels do not control spatial traversal. Tree View is rendered through the dedicated `KdTreeHierarchy` component with depth lanes, split-axis badges, explicit child references, and run-state styling.

Gradient Descent Core was re-audited against the explicit quadratic objective. The implemented loss is `0.5 * Σ cᵢ(θᵢ-targetᵢ)²`, the analytical gradient is `cᵢ(θᵢ-targetᵢ)`, fixed/decayed learning rates feed the standard `θ <- θ - η∇J` update, and convergence uses the L2 gradient norm. The runtime now rejects non-finite starting loss/gradient state and non-finite candidate updates before they can enter later Vector reductions. Regression tests include an exact one-update numerical check plus non-finite-candidate divergence handling.

### Gradient Descent Parameter Space presentation (Aug 23, 2026)

Gradient Descent Visual State now fits the complete teaching trajectory into one parameter-space viewport: the target/minimum stays centered, while the numeric bounds include the current θ, the active candidate θ′, and every committed history point with safety padding. The minimum uses a persistent green halo so it remains visible even when the optimizer reaches it and the purple current marker occupies the same coordinates. Every committed marker is hover/focus inspectable with its iteration, θ, loss, and learning rate. The current update vectors are presented as one compact four-card flow — current θ, slope ∇J, scaled step η∇J, and candidate θ′ — instead of oversized disconnected blocks. The surrounding Visual State never scrolls; the coordinate canvas only gains horizontal scroll when a narrow viewport cannot preserve the minimum legible drawing width.

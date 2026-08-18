# Algorithm Visualizer

An interactive learning platform for understanding data structures and algorithms by **watching each operation happen step by step**.

The project is built with **Blazor WebAssembly and C#**. Its goal is not only to display the final result of an algorithm, but to explain:

- what the algorithm is doing;
- which element is currently being inspected;
- why that element is visited next;
- how many operations were required;
- what the time complexity means for the current run;
- how the visual representation differs from the way the data is stored in memory.

The application now has twelve fully implemented learning modules: **Queue & Stack**, **Binary Search Tree (BST)**, **AVL Tree**, **Red-Black Tree**, **Heap (generalized d-ary)**, **Binary Heap (Min/Max)**, **Matrix**, **Graph**, **Bubble Sort**, **Selection Sort**, **Insertion Sort**, and **Merge Sort**. Queue & Stack established the reusable simulation pattern; BST extended it to linked nodes; AVL added strict height balancing; Red-Black added color-invariant repair; the two Heap labs teach the heap family and Binary Heap specialization; Matrix provides the row-major foundation reused by Graph; Bubble Sort introduces adjacent comparison, stability, sorted-suffix invariants, and Basic/Optimized stopping behavior; Selection Sort adds minimum-candidate scanning, sorted-prefix invariants, fixed quadratic comparison counts, Classic low-swap placement, and Stable Shift placement; Insertion Sort adds held-key/gap mechanics, adaptive sorted-prefix growth, Binary Insertion, stability, and online maintenance semantics; Merge Sort adds recursive divide-and-conquer, stable auxiliary-buffer merging, Natural Merge run detection, and predictable Θ(n log n) general performance.

---

## Project status

### Implemented

- Queue
- Stack
- Binary Search Tree (BST)
- AVL Tree
- Red-Black Tree
- Heap (generalized d-ary Min/Max)
- Binary Heap (Min/Max)
- Matrix
- Graph
- Bubble Sort
- Selection Sort
- Insertion Sort
- Merge Sort
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
- persistent learning progress in browser storage;
- optional result explanation popups;
- Concepts & Memory learning page;
- shared learner-facing module chrome in `wwwroot/css/learning-modules.css`, using the mature Queue & Stack / BST / Matrix / Graph visual language for sorting Learn First panels, module headers, tips, reference links, and lesson progression;
- difficulty-ordered curriculum navigation with reusable `NextLessonCard` links: Queue & Stack → BST → Binary Heap → d-ary Heap → AVL → Matrix → Graph → Red-Black Tree, then Bubble → Selection → Insertion → Merge → Quick → Heap Sort;
- exact deep links from every live lab to the relevant Concepts & Memory section, plus reverse links from the concept sections back to the matching lesson;
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
- Matrix guided practice with explicit Start/Restart flow, live task-condition verification, active-task progress, and persistent completion;
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

The sidebar and Concepts & Memory page use explicit easy → hard ordering. Concepts & Memory starts with shared foundations (Visual vs Memory, memory, identity, Big-O), then presents data-structure lessons in curriculum order, then sorting lessons from Bubble through Heap Sort. Stable deep-link aliases such as `/learn/concepts#bst`, `#binary-heap`, `#matrix`, `#bubble-sort`, `#merge-sort`, `#quick-sort`, and `#heap-sort` let each module land on the exact explanation it needs. Every lesson page exposes a `NextLessonCard`, and the concept sections link back to the corresponding lab.

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

The Matrix UI uses a beginner-first **Build → Choose → Watch** workflow. Direct cell editing is the primary input path; paste controls and special presets are progressively disclosed instead of being shown simultaneously. A live shape/compatibility assistant explains whether `A ± B` and `A × B` currently fit, while invalid operations remain runnable so the simulator can teach why a shape is rejected. Row/column axis labels and `[row,column]` coordinates stay visible in both editing and playback views. Four core operation cards (addition, multiplication, transpose and RREF) explain the rule before execution; the complete advanced operation set remains available under **All matrix operations**.

Implemented capabilities include direct editing/resizing, presets (zero, identity, sequence, diagonal, symmetric, random and graph adjacency), copy/swap/result chaining, addition/subtraction/Hadamard/scalar operations, matrix multiplication, transpose, powers, trace, determinant, minor/cofactor, elementary row operations, REF/RREF/rank, inverse, and solving `A·X=B`.

No numerical or linear-algebra library performs these taught operations. The live Graph module reuses this Matrix implementation for adjacency-matrix representation rather than creating a second matrix engine.

The Client imports `Microsoft.JSInterop` globally because interactive learning pages use `IJSRuntime` for browser-side task progress persistence. This keeps Razor pages compile-safe without repeating JS interop imports in every module.

### Planned

The following modules currently have UI placeholders and are intentionally marked as TODO until their algorithms are implemented:

#### Data structures

All currently planned base data-structure modules in the specification are now represented by live labs; traversal/path algorithms remain separate.

#### Sorting algorithms

All six sorting lessons in the current curriculum are live: Bubble, Selection, Insertion, Merge, Quick, and Heap Sort.

Future search and graph algorithms will follow the same simulation architecture.

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
- guided practice with local completion persistence;
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
- guided rotation practice with local completion persistence.

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
- persistent task completion in browser storage.

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

The learner presses **Start task**, performs the required operations, and the application observes what happens.

A task becomes `Completed` only after its required scenario has actually been performed.

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

Completed Queue & Stack task progress is stored locally in the browser.

The BST lab has its own guided tasks for:

- building a branching BST;
- successful and missing searches;
- leaf deletion;
- one-child deletion;
- two-child deletion with successor search;
- opening Memory state after structural rewiring;
- duplicate-key rejection;
- balancing a deliberately skewed BST with DSW and inspecting the same node IDs in Memory state after reference rewiring.

BST task completion is also stored locally in the browser.

The AVL lab adds guided tasks for:

- LL insertion repair;
- RR insertion repair;
- LR insertion repair;
- RL insertion repair;
- deletion that triggers upward rebalancing plus Memory-state inspection;
- successful and missing search after rotations.

AVL task completion is stored locally in the browser.

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

The solution is split into two main projects.

```text
AlgorithmVisualizer.sln

src/
├── AlgorithmVisualizer.Core/
└── AlgorithmVisualizer.Client/
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
- browser-local learning preferences and progress.

The Core project depends on the simulation runtime through:

```csharp
ISimulationRuntime
```

The Blazor client provides the concrete:

```csharp
SimulationState
```

This keeps algorithms independent from rendering concerns.

---

# Technology

- C#
- .NET 8
- Blazor WebAssembly
- Razor components
- CSS isolation
- WebAssembly
- minimal browser JS interop for local browser storage
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
dotnet run --project src/AlgorithmVisualizer.Client
```

Or:

```bash
cd src/AlgorithmVisualizer.Client
dotnet run
```

The development server will print the local application URL.

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

The structure foundation now includes our custom Queue, Stack, BST, AVL, Red-Black, generalized Heap, Binary Heap, Matrix, and Graph implementations. The next graph-algorithm modules should reuse this live Graph directly: BFS should reuse our Queue, DFS should reuse our Stack, and later Dijkstra/Prim should reuse our Heap implementations instead of duplicating those structures.

---

## Current milestone

**Queue & Stack: implemented as the complete linear-structure learning module.**

**Binary Search Tree: implemented as the first complete tree module, including structural deletion, explicit Day-Stout-Warren balancing, Visual/Memory views, playback history, result explanations, and guided practice.**

**AVL Tree: implemented as the strict height-balanced extension, including cached heights, balance factors, all four rotation cases, insert/delete rebalancing, Visual/Memory views, run explanations, and rotation-focused guided practice.**

**Red-Black Tree: implemented as the color-invariant balanced-tree extension, including explicit RED/BLACK nodes, conceptual black NIL leaves, insertion and deletion fix-up, recoloring, rotations, black-height teaching, Visual/Memory views, run explanations, and guided practice.**

**Generalized Heap: implemented as the d-ary family lab with d = 2/3/4/5 comparison, Min/Max modes, generalized index formulas, and explicit child-candidate selection.**

**Binary Heap: implemented as the focused d = 2 complete-tree / custom-array module with Min/Max modes, bubble-up, bubble-down, extract-root, linear search, arbitrary delete repair, Visual/Memory views, capacity teaching, run explanations, and guided practice.**

**Matrix: implemented as the pre-Graph row-major module with direct cell editing, bulk custom-value input with automatic dimension detection, arithmetic, multiplication, transpose, powers, determinant, minors/cofactors, elementary row operations, REF/RREF/rank, inverse, equation solving, graph-adjacency presets, Visual/Memory views, a row-list-first memory explanation with expandable real `double[]` backing storage, verified Start/Restart guided practice with active progress, and automatic three-view Last Run explanations.**

The project now has reusable manual linear structures, three reusable manual tree foundations, two reusable heap views of the same family, a reusable Matrix foundation, a live reusable Graph structure ready for BFS/DFS and later path/MST algorithms, plus six complete sorting labs: Bubble Sort, Selection Sort, Insertion Sort, Merge Sort, Quick Sort, and Heap Sort.

### Matrix memory visualization

The Matrix Memory State now presents the conceptual structure as a compact list of row lists (`A[0]`, `A[1]`, ...) with values aligned by column. The real contiguous row-major `double[]` remains available as an expandable implementation detail rather than dominating the learning view.


## Graph module

Graph is now a live structure lab before the traversal/path algorithms. Graph Learn First now uses the same launchpad visual hierarchy as Queue & Stack / BST, with the four concepts ordered as vertex+edge, direction, weight, and adjacency-list-vs-matrix representation. It implements directed/undirected and weighted/unweighted graphs with explicit vertex/edge objects, manual adjacency-list storage and the existing `ManualMatrix` for the synchronized adjacency matrix. The lab supports add/search/rename/remove vertex, add/search/update-weight/remove edge, direct-neighbor inspection, self-loops, zero/negative weights at the generic structure level, Visual/Memory state, playback and guided practice. BFS, DFS, Dijkstra, topological sort and MST remain separate modules and should reuse this graph rather than create another graph representation. Graph Core no longer inherits the Matrix page's 8×8 teaching cap: `ManualMatrix` is reusable/growable in Core, while MatrixPage alone keeps the 8×8 input limit. Larger Graph adjacency matrices scroll inside Memory State.

Graph Visual State supports direct vertex dragging without changing graph topology. Vertex positions are UI-only overrides keyed by stable vertex ID; edges, arrows, weights and self-loops are recalculated from the moved coordinates. The workspace is content-bounded but effectively unbounded: there is no fixed drag clamp, the SVG stage expands left/right/up/down only when current graph content reaches those bounds, and the surrounding viewport gains scroll range only for that occupied extent. Expanding on the left/top compensates scroll position so existing content does not jump. `Reset layout` removes manual positions and returns to the automatic layout. The drag implementation uses stable SVG group transforms with invariant numeric formatting so clicking/dragging cannot invalidate `foreignObject` coordinates.

### Razor control-flow markup rule

Graph UI markup follows a strict Razor rule: whenever `@if`, `@foreach`, `@for`, or similar control flow renders HTML/component markup, the body is always enclosed in `{ ... }`. This avoids Razor parser failures such as “Single-statement control-flow statements in Razor documents cannot contain markup.”


### Graph guided practice and explanations

Graph now follows the same learning-completion contract as the mature Queue/Stack, tree, and Heap modules. Guided Practice is no longer static reading: the learner starts a task, the page observes real `GraphSimulation` results plus the current graph snapshot, validates the requested topology/operation, marks completion automatically, and persists completed task IDs in browser `localStorage`. Tasks cover undirected symmetry, directed asymmetry, a real zero-weight edge, incident-edge cleanup during vertex deletion, sparse list-vs-matrix inspection, and a branch/cycle topology prepared for later BFS/DFS.

Every completed Graph action now has a learner-facing explanation layer. The Last Run card can reopen a dismissible interactive explanation with three views: the action that happened, how the adjacency list and Matrix-backed representation changed together, and why the operation matters for later graph algorithms. Automatic result explanations can be disabled and that preference is persisted locally. The Graph `Result popups` toggle now lives in the Playground header beside `Last run explanation`, matching the established Queue/Stack, BST, AVL, and Heap interaction pattern instead of creating a graph-specific preference strip below the result. The Graph lab also exposes direct `Graph concepts` and `Concepts & memory` links beside the Visual/Memory controls so theory is always reachable from the working area.

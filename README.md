# Algorithm Visualizer

An interactive learning platform for understanding data structures and algorithms by **watching each operation happen step by step**.

The project is built with **Blazor WebAssembly and C#**. Its goal is not only to display the final result of an algorithm, but to explain:

- what the algorithm is doing;
- which element is currently being inspected;
- why that element is visited next;
- how many operations were required;
- what the time complexity means for the current run;
- how the visual representation differs from the way the data is stored in memory.

The application now has six fully implemented learning modules: **Queue & Stack**, **Binary Search Tree (BST)**, **AVL Tree**, **Red-Black Tree**, **Heap (generalized d-ary)**, and **Binary Heap (Min/Max)**. Queue & Stack established the reusable simulation pattern; BST extended it to linked nodes; AVL added strict height balancing; Red-Black added color-invariant repair; the two Heap labs now explicitly teach the difference between the heap family and the Binary Heap d = 2 specialization.

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
- generalized d-ary Heap with configurable branching factor `d = 3/4/5` in the UI;
- generalized parent/child formulas `parent=(i-1)/d`, children `di+1..di+d`;
- d-ary bubble-down that explicitly compares all existing child candidates before choosing the highest-priority child;
- Heap-family vs Binary-Heap learning copy and side-by-side navigation;
- Min Heap / Max Heap mode selection while empty;
- Heap insert with append + bubble-up;
- Heap extract-root with last-element replacement + bubble-down;
- Heap arbitrary value search with a truthful O(n) scan;
- Heap delete-by-value with direction-aware repair;
- Heap Visual/Memory views showing complete-tree indexes, stable IDs, used slots, capacity, and manual array growth;
- Heap guided practice and persistent progress.

### Planned

The following modules currently have UI placeholders and are intentionally marked as TODO until their algorithms are implemented:

#### Data structures

- Graphs

#### Sorting algorithms

- Bubble Sort
- Selection Sort
- Insertion Sort
- Merge Sort
- Quick Sort
- Heap Sort

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

This rule also applies to the live Red-Black Tree and Binary Heap and will continue to apply to future graphs, sorting algorithms and search algorithms.

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

The next module can now reuse our custom Queue, Stack, BST, AVL, Red-Black, and Heap implementations wherever that is algorithmically appropriate. **Graphs (adjacency list / matrix)** are the next data structure in the written specification; the live Heap is also now available as a future dependency for Heap Sort, priority-queue teaching, and Dijkstra work.

---

## Current milestone

**Queue & Stack: implemented as the complete linear-structure learning module.**

**Binary Search Tree: implemented as the first complete tree module, including structural deletion, explicit Day-Stout-Warren balancing, Visual/Memory views, playback history, result explanations, and guided practice.**

**AVL Tree: implemented as the strict height-balanced extension, including cached heights, balance factors, all four rotation cases, insert/delete rebalancing, Visual/Memory views, run explanations, and rotation-focused guided practice.**

**Red-Black Tree: implemented as the color-invariant balanced-tree extension, including explicit RED/BLACK nodes, conceptual black NIL leaves, insertion and deletion fix-up, recoloring, rotations, black-height teaching, Visual/Memory views, run explanations, and guided practice.**

**Binary Heap: implemented as a complete-tree / custom-array module with Min/Max modes, bubble-up, bubble-down, extract-root, linear search, arbitrary delete repair, Visual/Memory views, capacity teaching, run explanations, and guided practice.**

The project now has reusable manual linear structures, three reusable manual tree foundations, and a reusable manual Binary Heap foundation for subsequent algorithms and data structures.

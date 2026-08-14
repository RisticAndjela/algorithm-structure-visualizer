# Matrix visualizations

- `MatrixEditor.razor` edits A/B cells without JavaScript.
- `MatrixVisualization.razor` renders the mathematical grid and semantic cell states used during arithmetic and elimination.
- `MatrixMemoryVisualization.razor` teaches memory in two layers: first a learner-friendly outer list of row lists, then an expandable view of the actual contiguous row-major `double[]` used by `ManualMatrix`.

Keep wide matrices scrollable inside visualization/editor regions. Do not create page-level horizontal overflow, and keep instructional text readable at normal laptop zoom.

Matrix input UX must make editability obvious: users may edit individual cells or paste/type a complete rectangular matrix in the page bulk-input controls. Keep input text and coordinate labels readable at normal laptop zoom.

The Memory State must not imply that the nested row-list drawing is the literal C# allocation used by `ManualMatrix`. Label it as the easier mental model, and preserve the real backing-array explanation with `index = row * columns + column` as a secondary advanced view.
## Memory-state presentation

`MatrixMemoryVisualization` uses a compact nested-array mental model: one outer matrix reference, one horizontal inner list per row, and values aligned by column. Avoid nested card-per-cell layouts because they add visual noise and make the list-of-lists idea harder to read. Slot numbers and row-major storage are secondary implementation details and belong in the collapsed advanced backing-array view. Text in the primary memory view must remain comfortably readable.


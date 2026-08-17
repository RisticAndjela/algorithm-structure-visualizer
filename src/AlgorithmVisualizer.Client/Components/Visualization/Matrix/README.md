# Matrix visualizations

- `MatrixEditor.razor` edits A/B cells without JavaScript.
- `MatrixVisualization.razor` renders the mathematical grid and semantic cell states used during arithmetic and elimination.
- `MatrixMemoryVisualization.razor` teaches memory in two layers: first a learner-friendly outer list of row lists, then an expandable view of the actual contiguous row-major `double[]` used by `ManualMatrix`.

Keep wide matrices scrollable inside visualization/editor regions. Do not create page-level horizontal overflow, and keep instructional text readable at normal laptop zoom.

Matrix input UX must make editability obvious: users may edit individual cells or paste/type a complete rectangular matrix in the page bulk-input controls. Keep input text and coordinate labels readable at normal laptop zoom.

The Memory State must not imply that the nested row-list drawing is the literal C# allocation used by `ManualMatrix`. Label it as the easier mental model, and preserve the real backing-array explanation with `index = row * columns + column` as a secondary advanced view.
## Memory-state presentation

`MatrixMemoryVisualization` uses a compact nested-array mental model: one outer matrix reference, one horizontal inner list per row, and values aligned by column. Avoid nested card-per-cell layouts because they add visual noise and make the list-of-lists idea harder to read. Slot numbers and row-major storage are secondary implementation details and belong in the collapsed advanced backing-array view. Text in the primary memory view must remain comfortably readable.

## Beginner-first Matrix interaction

The page follows a **Build → Choose → Watch** progression. Keep direct cell editing visually primary. Bulk paste, special presets, manual row operations, minors/cofactors and other advanced controls should use progressive disclosure so a first-time learner is not presented with every control at once.

Always keep matrix dimensions and compatibility visible before operations. For `A ± B` and `A × B`, explain the current rule using the live shapes. Do not disable mathematically invalid operations solely because the shapes do not match: rejected runs are an intentional teaching path and are required by Guided Practice.

Editors and Visual State should show explicit row/column axes plus conventional `[row,column]` coordinates. Playback must pair color state with plain-language current-step text so color is never the only explanation of what the algorithm is doing.


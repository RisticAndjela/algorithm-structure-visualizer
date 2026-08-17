# Sorting visualization

## Bubble Sort — live

Bubble Sort is the first implemented sorting lab.

The page follows the learner flow:

1. **Build** an input array (1–12 integers in the Client learning view).
2. **Predict** whether the first adjacent pair should swap or stay.
3. **Watch** semantic compare / decide / swap / pass-complete steps through the shared `SimulationToolbar`.
4. Switch between **Visual state** and **Memory state**.
5. Review the automatic **Last Run** explanation and complete verified Guided Practice tasks.

The Visual state renders stable item identity separately from numeric value so duplicate stability can be observed. The Memory state shows fixed array indexes and the element reference currently occupying each slot; it does not pretend that screen positions are physical RAM addresses.

Playback history is UI-only review state. The sorting algorithm itself remains in `AlgorithmVisualizer.Core`.

Selection, Insertion, Merge, Quick, and Heap Sort remain TODO placeholders until their Core implementations are added.

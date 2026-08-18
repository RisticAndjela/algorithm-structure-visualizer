using AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;
using AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;
using AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Algorithms.Search.Binary;

/// <summary>
/// Reuses the already implemented sorting simulations as a silent preprocessing step
/// when Binary Search receives an unsorted input. The selected sorter runs against an
/// immediate runtime, so its internal teaching steps are not replayed on the Binary
/// Search page; only the final sorted values are returned.
/// </summary>
public sealed class BinarySearchInputSorter
{
    private static readonly ISimulationRuntime ImmediateRuntime = new ImmediateSimulationRuntime();

    public async Task<BinarySearchInputSortResult> SortAsync(
        int[] values,
        BinarySearchInputSortAlgorithm algorithm,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        var input = Clone(values);

        return algorithm switch
        {
            BinarySearchInputSortAlgorithm.BubbleSort => await SortWithBubbleAsync(input, cancellationToken),
            BinarySearchInputSortAlgorithm.SelectionSort => await SortWithSelectionAsync(input, cancellationToken),
            BinarySearchInputSortAlgorithm.InsertionSort => await SortWithInsertionAsync(input, cancellationToken),
            BinarySearchInputSortAlgorithm.MergeSort => await SortWithMergeAsync(input, cancellationToken),
            BinarySearchInputSortAlgorithm.QuickSort => await SortWithQuickAsync(input, cancellationToken),
            BinarySearchInputSortAlgorithm.HeapSort => await SortWithHeapAsync(input, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, "Unknown Binary Search preprocessing sorter.")
        };
    }

    private static async Task<BinarySearchInputSortResult> SortWithBubbleAsync(int[] values, CancellationToken token)
    {
        var simulation = new BubbleSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(BubbleSortVariant.Optimized, token);
        return Build(BinarySearchInputSortAlgorithm.BubbleSort, result.SortedValues, "Optimized Bubble Sort");
    }

    private static async Task<BinarySearchInputSortResult> SortWithSelectionAsync(int[] values, CancellationToken token)
    {
        var simulation = new SelectionSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(SelectionSortVariant.Classic, token);
        return Build(BinarySearchInputSortAlgorithm.SelectionSort, result.SortedValues, "Classic Selection Sort");
    }

    private static async Task<BinarySearchInputSortResult> SortWithInsertionAsync(int[] values, CancellationToken token)
    {
        var simulation = new InsertionSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(InsertionSortVariant.Linear, token);
        return Build(BinarySearchInputSortAlgorithm.InsertionSort, result.SortedValues, "Linear Insertion Sort");
    }

    private static async Task<BinarySearchInputSortResult> SortWithMergeAsync(int[] values, CancellationToken token)
    {
        var simulation = new MergeSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(MergeSortVariant.TopDownRecursive, token);
        return Build(BinarySearchInputSortAlgorithm.MergeSort, result.SortedValues, "Top-down Merge Sort");
    }

    private static async Task<BinarySearchInputSortResult> SortWithQuickAsync(int[] values, CancellationToken token)
    {
        var simulation = new QuickSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(QuickSortVariant.MedianOfThreeThreeWay, token);
        return Build(BinarySearchInputSortAlgorithm.QuickSort, result.SortedValues, "Median-of-three 3-way Quick Sort");
    }

    private static async Task<BinarySearchInputSortResult> SortWithHeapAsync(int[] values, CancellationToken token)
    {
        var simulation = new HeapSortSimulation(ImmediateRuntime);
        simulation.LoadValues(values);
        var result = await simulation.SortAsync(HeapSortVariant.FloydBottomUp, token);
        return Build(BinarySearchInputSortAlgorithm.HeapSort, result.SortedValues, "Floyd bottom-up Heap Sort");
    }

    private static BinarySearchInputSortResult Build(
        BinarySearchInputSortAlgorithm algorithm,
        int[] sortedValues,
        string implementationName)
        => new(algorithm, Clone(sortedValues), implementationName);

    private static int[] Clone(int[] source)
    {
        var copy = new int[source.Length];
        for (var index = 0; index < source.Length; index++)
        {
            copy[index] = source[index];
        }

        return copy;
    }

    private sealed class ImmediateSimulationRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

public enum BinarySearchInputSortAlgorithm
{
    BubbleSort,
    SelectionSort,
    InsertionSort,
    MergeSort,
    QuickSort,
    HeapSort
}

public sealed record BinarySearchInputSortResult(
    BinarySearchInputSortAlgorithm Algorithm,
    int[] SortedValues,
    string ImplementationName);

namespace AlgorithmVisualizer.Client.State;

public static class PracticeRules
{
    /// <summary>
    /// True when every required value occurs in the same order in actual. Extra values
    /// before, between, or after required values are deliberately allowed.
    /// </summary>
    public static bool ContainsOrderedSubsequence(IReadOnlyList<int> actual, IReadOnlyList<int> required)
    {
        if (required.Count == 0) return true;
        var requiredIndex = 0;
        for (var index = 0; index < actual.Count && requiredIndex < required.Count; index++)
        {
            if (actual[index] == required[requiredIndex]) requiredIndex++;
        }
        return requiredIndex == required.Count;
    }

    public static bool IsNonDecreasing(IReadOnlyList<int> values)
    {
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index - 1] > values[index]) return false;
        }
        return true;
    }

    public static bool IsStrictlyDescending(IReadOnlyList<int> values)
    {
        if (values.Count < 2) return false;
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index - 1] <= values[index]) return false;
        }
        return true;
    }

    public static bool HasDuplicate(IReadOnlyList<int> values) => values.GroupBy(value => value).Any(group => group.Count() > 1);

    public static int QuadraticPairCount(int count) => count < 2 ? 0 : count * (count - 1) / 2;

    public static int BinaryHeapAscendingInsertionSwapCount(int count)
    {
        var swaps = 0;
        for (var index = 1; index < count; index++)
        {
            var cursor = index;
            while (cursor > 0)
            {
                swaps++;
                cursor = (cursor - 1) / 2;
            }
        }
        return swaps;
    }

    public static bool IsMaxHeap(IReadOnlyList<int> values)
    {
        for (var parent = 0; parent < values.Count; parent++)
        {
            var left = parent * 2 + 1;
            var right = left + 1;
            if (left < values.Count && values[parent] < values[left]) return false;
            if (right < values.Count && values[parent] < values[right]) return false;
        }
        return true;
    }
}

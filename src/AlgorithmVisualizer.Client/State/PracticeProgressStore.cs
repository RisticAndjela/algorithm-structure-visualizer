using System.Text.Json;

namespace AlgorithmVisualizer.Client.State;

/// <summary>
/// Shared persistence for guided-practice completion and the exact explanation snapshot
/// that was produced by the run/action that satisfied a task.
/// </summary>
public sealed class PracticeProgressStore
{
    private readonly LearningSessionStore _learningStore;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public PracticeProgressStore(LearningSessionStore learningStore)
    {
        _learningStore = learningStore;
    }

    public PracticeModuleProgress Load(string moduleKey, string? legacyCompletedKey, IEnumerable<int> validTaskIds)
    {
        ValidateModuleKey(moduleKey);
        ArgumentNullException.ThrowIfNull(validTaskIds);

        var validTaskIdSet = validTaskIds.ToHashSet();
        var storageKey = GetStorageKey(moduleKey);
        var progress = DeserializeProgress(_learningStore.GetItem(storageKey));

        if (progress is null)
        {
            progress = new PracticeModuleProgress();
            ImportLegacyProgress(progress, legacyCompletedKey, validTaskIdSet);

            if (progress.CompletedTaskIds.Count > 0)
            {
                Persist(moduleKey, progress);
            }
        }

        RemoveStaleEntries(progress, validTaskIdSet);
        return progress;
    }

    public void Complete(
        string moduleKey,
        PracticeModuleProgress progress,
        int taskId,
        PracticeCompletionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(evidence);

        progress.CompletedTaskIds.Add(taskId);
        progress.EvidenceByTask[taskId] = evidence with
        {
            TaskId = taskId,
            CompletedAtUtc = DateTimeOffset.UtcNow
        };

        Persist(moduleKey, progress);
    }

    public PracticeCompletionEvidence? GetEvidence(PracticeModuleProgress progress, int taskId)
    {
        ArgumentNullException.ThrowIfNull(progress);
        return progress.EvidenceByTask.TryGetValue(taskId, out var evidence) ? evidence : null;
    }

    public void Persist(string moduleKey, PracticeModuleProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _learningStore.SetItem(GetStorageKey(moduleKey), JsonSerializer.Serialize(progress, _jsonOptions));
    }

    private PracticeModuleProgress? DeserializeProgress(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PracticeModuleProgress>(raw, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void ImportLegacyProgress(
        PracticeModuleProgress progress,
        string? legacyCompletedKey,
        IReadOnlySet<int> validTaskIds)
    {
        if (string.IsNullOrWhiteSpace(legacyCompletedKey))
        {
            return;
        }

        var legacyTaskIds = DeserializeLegacyTaskIds(_learningStore.GetItem(legacyCompletedKey));
        foreach (var taskId in legacyTaskIds.Where(validTaskIds.Contains))
        {
            progress.CompletedTaskIds.Add(taskId);
        }
    }

    private int[] DeserializeLegacyTaskIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<int[]>(raw, _jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void RemoveStaleEntries(PracticeModuleProgress progress, IReadOnlySet<int> validTaskIds)
    {
        progress.CompletedTaskIds.RemoveWhere(taskId => !validTaskIds.Contains(taskId));

        foreach (var staleTaskId in progress.EvidenceByTask.Keys.Where(taskId => !validTaskIds.Contains(taskId)).ToArray())
        {
            progress.EvidenceByTask.Remove(staleTaskId);
        }
    }

    private static string GetStorageKey(string moduleKey)
    {
        ValidateModuleKey(moduleKey);
        return $"algorithm-visualizer.practice.{moduleKey}.v2";
    }

    private static void ValidateModuleKey(string moduleKey)
    {
        if (string.IsNullOrWhiteSpace(moduleKey))
        {
            throw new ArgumentException("A practice module key is required.", nameof(moduleKey));
        }
    }
}

public sealed class PracticeModuleProgress
{
    public HashSet<int> CompletedTaskIds { get; init; } = [];
    public Dictionary<int, PracticeCompletionEvidence> EvidenceByTask { get; init; } = [];
}

public sealed record PracticeCompletionEvidence
{
    public PracticeCompletionEvidence()
    {
    }

    public PracticeCompletionEvidence(
        int taskId,
        string taskTitle,
        string headline,
        string actionExplanation,
        string memoryExplanation,
        string whyItMatters,
        IReadOnlyList<PracticeEvidenceMetric> metrics,
        string? caseSummary = null,
        DateTimeOffset completedAtUtc = default)
    {
        TaskId = taskId;
        TaskTitle = taskTitle;
        Headline = headline;
        ActionExplanation = actionExplanation;
        MemoryExplanation = memoryExplanation;
        WhyItMatters = whyItMatters;
        Metrics = metrics;
        CaseSummary = caseSummary;
        CompletedAtUtc = completedAtUtc;
    }

    public int TaskId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
    public string Headline { get; init; } = string.Empty;
    public string ActionExplanation { get; init; } = string.Empty;
    public string MemoryExplanation { get; init; } = string.Empty;
    public string WhyItMatters { get; init; } = string.Empty;
    public IReadOnlyList<PracticeEvidenceMetric> Metrics { get; init; } = [];
    public string? CaseSummary { get; init; }
    public DateTimeOffset CompletedAtUtc { get; init; }
}

public sealed record PracticeEvidenceMetric(string Label, string Value);

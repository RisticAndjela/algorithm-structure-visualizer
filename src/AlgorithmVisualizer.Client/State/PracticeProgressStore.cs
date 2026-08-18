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
        var valid = validTaskIds.ToHashSet();
        var storageKey = GetStorageKey(moduleKey);
        var raw = _learningStore.GetItem(storageKey);
        PracticeModuleProgress progress;

        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                progress = JsonSerializer.Deserialize<PracticeModuleProgress>(raw, _jsonOptions) ?? new();
            }
            catch
            {
                progress = new();
            }
        }
        else
        {
            progress = new();
            if (!string.IsNullOrWhiteSpace(legacyCompletedKey))
            {
                try
                {
                    var legacyRaw = _learningStore.GetItem(legacyCompletedKey);
                    var legacy = string.IsNullOrWhiteSpace(legacyRaw)
                        ? []
                        : JsonSerializer.Deserialize<int[]>(legacyRaw, _jsonOptions) ?? [];
                    foreach (var id in legacy.Where(valid.Contains))
                    {
                        progress.CompletedTaskIds.Add(id);
                    }
                }
                catch
                {
                }
            }

            if (progress.CompletedTaskIds.Count > 0)
            {
                Persist(moduleKey, progress);
            }
        }

        progress.CompletedTaskIds.RemoveWhere(id => !valid.Contains(id));
        foreach (var stale in progress.EvidenceByTask.Keys.Where(id => !valid.Contains(id)).ToArray())
        {
            progress.EvidenceByTask.Remove(stale);
        }

        return progress;
    }

    public void Complete(string moduleKey, PracticeModuleProgress progress, int taskId, PracticeCompletionEvidence evidence)
    {
        progress.CompletedTaskIds.Add(taskId);
        progress.EvidenceByTask[taskId] = evidence with { TaskId = taskId, CompletedAtUtc = DateTimeOffset.UtcNow };
        Persist(moduleKey, progress);
    }

    public PracticeCompletionEvidence? GetEvidence(PracticeModuleProgress progress, int taskId) =>
        progress.EvidenceByTask.TryGetValue(taskId, out var evidence) ? evidence : null;

    public void Persist(string moduleKey, PracticeModuleProgress progress)
    {
        _learningStore.SetItem(GetStorageKey(moduleKey), JsonSerializer.Serialize(progress, _jsonOptions));
    }

    private static string GetStorageKey(string moduleKey) =>
        $"algorithm-visualizer.practice.{moduleKey}.v2";
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

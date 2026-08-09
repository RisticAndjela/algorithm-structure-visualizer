namespace AlgorithmVisualizer.Client.State;

/// <summary>
/// UI-facing playback state shared by simulation pages and common controls.
/// Algorithm/data-structure state must not be stored here.
/// </summary>
public sealed class SimulationState
{
    public const int MinDelayMs = 50;
    public const int MaxDelayMs = 2_000;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public int StepDelayMs { get; private set; } = 500;
    public int CurrentStep { get; private set; }
    public int TotalSteps { get; private set; }

    public event Action? Changed;

    public void SetDelay(int milliseconds)
    {
        StepDelayMs = Math.Clamp(milliseconds, MinDelayMs, MaxDelayMs);
        NotifyChanged();
    }

    public void Start(int totalSteps)
    {
        TotalSteps = Math.Max(0, totalSteps);
        CurrentStep = 0;
        IsRunning = true;
        IsPaused = false;
        NotifyChanged();
    }

    public void SetCurrentStep(int step)
    {
        CurrentStep = Math.Clamp(step, 0, TotalSteps);
        NotifyChanged();
    }

    public void Pause()
    {
        if (!IsRunning) return;
        IsPaused = true;
        NotifyChanged();
    }

    public void Resume()
    {
        if (!IsRunning) return;
        IsPaused = false;
        NotifyChanged();
    }

    public void Reset()
    {
        IsRunning = false;
        IsPaused = false;
        CurrentStep = 0;
        TotalSteps = 0;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();
}

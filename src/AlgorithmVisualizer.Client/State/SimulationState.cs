using System.Diagnostics;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Client.State;

/// <summary>
/// Owns playback state for the active visualization.
/// Algorithm and data-structure state must remain outside this service.
/// </summary>
public sealed class SimulationState : ISimulationRuntime, IDisposable
{
    public const int MinDelayMs = 50;
    public const int MaxDelayMs = 2_000;
    public const int DefaultDelayMs = 500;

    private readonly object _sync = new();

    private CancellationTokenSource _simulationCancellation = new();
    private TaskCompletionSource<bool> _controlChanged = CreateControlSignal();
    private bool _singleStepRequested;
    private bool _disposed;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public int StepDelayMs { get; private set; } = DefaultDelayMs;
    public string CurrentStepText { get; private set; } = "Ready.";

    public CancellationToken SimulationCancellationToken
    {
        get
        {
            lock (_sync)
            {
                return _simulationCancellation.Token;
            }
        }
    }

    public event Action? Changed;

    /// <summary>
    /// Starts a new simulation run and cancels any previously active run.
    /// The returned token should be passed through the operation that performs the simulation.
    /// </summary>
    public CancellationToken Start(string initialStepText = "Simulation started.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(initialStepText);
        ThrowIfDisposed();

        CancellationTokenSource previousCancellation;
        CancellationToken currentToken;

        lock (_sync)
        {
            previousCancellation = _simulationCancellation;
            _simulationCancellation = new CancellationTokenSource();
            currentToken = _simulationCancellation.Token;

            IsRunning = true;
            IsPaused = false;
            _singleStepRequested = false;
            CurrentStepText = initialStepText;
        }

        CancelAndDispose(previousCancellation);
        PulseControlChange();
        NotifyChanged();

        return currentToken;
    }

    public void SetDelay(int milliseconds)
    {
        ThrowIfDisposed();

        var normalizedDelay = Math.Clamp(milliseconds, MinDelayMs, MaxDelayMs);
        if (normalizedDelay == StepDelayMs)
        {
            return;
        }

        StepDelayMs = normalizedDelay;
        PulseControlChange();
        NotifyChanged();
    }

    public void Pause()
    {
        ThrowIfDisposed();

        if (!IsRunning || IsPaused)
        {
            return;
        }

        IsPaused = true;
        _singleStepRequested = false;
        PulseControlChange();
        NotifyChanged();
    }

    public void Resume()
    {
        ThrowIfDisposed();

        if (!IsRunning || !IsPaused)
        {
            return;
        }

        IsPaused = false;
        _singleStepRequested = false;
        PulseControlChange();
        NotifyChanged();
    }

    /// <summary>
    /// Switches the active run to paused mode and releases exactly one blocked step.
    /// </summary>
    public void Step()
    {
        ThrowIfDisposed();

        if (!IsRunning)
        {
            return;
        }

        IsPaused = true;
        _singleStepRequested = true;
        PulseControlChange();
        NotifyChanged();
    }

    public void SetCurrentStep(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ThrowIfDisposed();

        CurrentStepText = description;
        NotifyChanged();
    }

    /// <summary>
    /// Marks the active run as complete without clearing the final step description.
    /// </summary>
    public void Complete(string? finalStepText = null)
    {
        ThrowIfDisposed();

        if (!string.IsNullOrWhiteSpace(finalStepText))
        {
            CurrentStepText = finalStepText;
        }

        IsRunning = false;
        IsPaused = false;
        _singleStepRequested = false;
        PulseControlChange();
        NotifyChanged();
    }

    /// <summary>
    /// Cancels the active run and returns playback state to its initial state.
    /// </summary>
    public void Reset()
    {
        ThrowIfDisposed();

        CancellationTokenSource previousCancellation;

        lock (_sync)
        {
            previousCancellation = _simulationCancellation;
            _simulationCancellation = new CancellationTokenSource();

            IsRunning = false;
            IsPaused = false;
            _singleStepRequested = false;
            CurrentStepText = "Ready.";
        }

        CancelAndDispose(previousCancellation);
        PulseControlChange();
        NotifyChanged();
    }

    /// <summary>
    /// Waits according to the current playback mode.
    /// Speed changes and pause requests interrupt the current delay so the new state is applied immediately.
    /// </summary>
    public async Task WaitForNextStepAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        CancellationToken simulationToken;

        lock (_sync)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("A simulation must be started before waiting for the next step.");
            }

            simulationToken = _simulationCancellation.Token;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            simulationToken,
            cancellationToken);

        var token = linkedCancellation.Token;

        while (true)
        {
            token.ThrowIfCancellationRequested();

            if (TryConsumeSingleStep())
            {
                return;
            }

            if (IsPaused)
            {
                await WaitForControlChangeAsync(token);
                continue;
            }

            var delayCompleted = await WaitForDelayAsync(token);
            if (delayCompleted)
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CancelAndDispose(_simulationCancellation);
        PulseControlChange();
    }

    private async Task<bool> WaitForDelayAsync(CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsPaused)
            {
                return false;
            }

            var elapsedMilliseconds = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            var remainingMilliseconds = StepDelayMs - elapsedMilliseconds;

            if (remainingMilliseconds <= 0)
            {
                return true;
            }

            var controlChangedTask = GetControlChangedTask();
            var delayTask = Task.Delay(
                TimeSpan.FromMilliseconds(remainingMilliseconds),
                cancellationToken);

            var completedTask = await Task.WhenAny(delayTask, controlChangedTask);
            if (completedTask == delayTask)
            {
                await delayTask;
                return true;
            }
        }
    }

    private async Task WaitForControlChangeAsync(CancellationToken cancellationToken)
    {
        var controlChangedTask = GetControlChangedTask();
        await controlChangedTask.WaitAsync(cancellationToken);
    }

    private bool TryConsumeSingleStep()
    {
        if (!IsPaused || !_singleStepRequested)
        {
            return false;
        }

        _singleStepRequested = false;
        NotifyChanged();
        return true;
    }

    private Task GetControlChangedTask()
    {
        lock (_sync)
        {
            return _controlChanged.Task;
        }
    }

    private void PulseControlChange()
    {
        TaskCompletionSource<bool> signalToRelease;

        lock (_sync)
        {
            signalToRelease = _controlChanged;
            _controlChanged = CreateControlSignal();
        }

        signalToRelease.TrySetResult(true);
    }

    private static TaskCompletionSource<bool> CreateControlSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void CancelAndDispose(CancellationTokenSource cancellation)
    {
        try
        {
            cancellation.Cancel();
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    private void NotifyChanged() => Changed?.Invoke();

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}

namespace AlgorithmVisualizer.Core.Simulation.Contracts;

/// <summary>
/// Defines the execution boundary used by algorithms and data structures to publish
/// a visual step and wait until the simulation is allowed to continue.
/// </summary>
public interface ISimulationRuntime
{
    /// <summary>
    /// Gets the cancellation token for the currently active simulation run.
    /// </summary>
    CancellationToken SimulationCancellationToken { get; }

    /// <summary>
    /// Publishes the description of the operation currently being visualized.
    /// </summary>
    /// <param name="description">A concise, user-facing description of the current operation.</param>
    void SetCurrentStep(string description);

    /// <summary>
    /// Waits until the current simulation step may advance.
    /// The implementation is responsible for playback speed, pause, single-step, and cancellation.
    /// </summary>
    /// <param name="cancellationToken">Optional caller-owned cancellation token.</param>
    Task WaitForNextStepAsync(CancellationToken cancellationToken = default);
}

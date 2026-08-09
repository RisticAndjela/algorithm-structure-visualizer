using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Simulation;

/// <summary>
/// Optional base class for visual algorithms that need a consistent way to publish
/// a textual operation and wait for the playback controller before continuing.
/// </summary>
public abstract class SimulationAlgorithmBase
{
    protected SimulationAlgorithmBase(ISimulationRuntime simulationRuntime)
    {
        SimulationRuntime = simulationRuntime ?? throw new ArgumentNullException(nameof(simulationRuntime));
    }

    protected ISimulationRuntime SimulationRuntime { get; }

    /// <summary>
    /// Publishes the current operation and waits for the next playback step.
    /// </summary>
    protected Task NextStepAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        SimulationRuntime.SetCurrentStep(description);
        return SimulationRuntime.WaitForNextStepAsync(cancellationToken);
    }
}

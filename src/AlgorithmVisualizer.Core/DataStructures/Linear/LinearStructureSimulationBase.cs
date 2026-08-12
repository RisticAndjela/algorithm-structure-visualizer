using System.Collections.ObjectModel;
using AlgorithmVisualizer.Core.Simulation;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Provides shared synchronization and transient-state cleanup for linear structure simulations.
/// </summary>
public abstract class LinearStructureSimulationBase : SimulationAlgorithmBase
{
    private readonly List<LinearElement> _items = [];
    private readonly ReadOnlyCollection<LinearElement> _readOnlyItems;
    private readonly SemaphoreSlim _operationGate = new(1, 1);

    protected LinearStructureSimulationBase(ISimulationRuntime simulationRuntime)
        : base(simulationRuntime)
    {
        _readOnlyItems = _items.AsReadOnly();
    }

    public IReadOnlyList<LinearElement> Items => _readOnlyItems;
    public int Count => _items.Count;

    protected List<LinearElement> MutableItems => _items;

    public event Action? Changed;

    protected async Task ExecuteExclusiveAsync(
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates();
            await operation();
        }
        finally
        {
            NormalizeVisualStates();
            NotifyChanged();
            _operationGate.Release();
        }
    }

    protected async Task<TResult> ExecuteExclusiveAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _operationGate.WaitAsync(cancellationToken);

        try
        {
            NormalizeVisualStates();
            return await operation();
        }
        finally
        {
            NormalizeVisualStates();
            NotifyChanged();
            _operationGate.Release();
        }
    }

    protected void NormalizeVisualStates()
    {
        foreach (var item in _items)
        {
            item.VisualState = LinearElementVisualState.Normal;
        }
    }

    protected void NotifyChanged() => Changed?.Invoke();
}

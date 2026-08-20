using AlgorithmVisualizer.Core.DataStructures.Matrix;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;
using AlgorithmVisualizer.Core.MachineLearning.GraphMl.SpectralClustering;
using AlgorithmVisualizer.Core.Simulation.Contracts;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.GraphMl;

public sealed class SpectralClusteringSimulationTests
{
    [Fact]
    public void JacobiSolver_ReturnsAscendingEigenvaluesForDiagonalMatrix()
    {
        var matrix = new ManualMatrix(2, 2);
        matrix[0, 0] = 2d;
        matrix[1, 1] = 1d;

        var result = ManualSymmetricEigenSolver.Solve(matrix);

        Assert.Equal(1d, result.Eigenvalues[0], 10);
        Assert.Equal(2d, result.Eigenvalues[1], 10);
    }

    [Fact]
    public async Task TwoCommunities_AreSeparatedByConnectivity()
    {
        var simulation = new SpectralClusteringSimulation(new ImmediateRuntime());
        simulation.Configure(new SpectralClusteringConfiguration(
            Undirected(6, [(0,1),(0,2),(1,2),(2,3),(3,4),(3,5),(4,5)]),
            2));

        var result = await simulation.ExecuteAsync();

        Assert.Equal(result.Assignments[0], result.Assignments[1]);
        Assert.Equal(result.Assignments[1], result.Assignments[2]);
        Assert.Equal(result.Assignments[3], result.Assignments[4]);
        Assert.Equal(result.Assignments[4], result.Assignments[5]);
        Assert.NotEqual(result.Assignments[0], result.Assignments[3]);
        Assert.True(IsAscending(result.Eigenvalues));
        Assert.All(result.Embedding, row => Assert.All(row, value => Assert.True(double.IsFinite(value))));
    }

    [Fact]
    public void Configure_RejectsIsolatedNode()
    {
        var simulation = new SpectralClusteringSimulation(new ImmediateRuntime());
        Assert.Throws<ArgumentException>(() => simulation.Configure(new SpectralClusteringConfiguration(
            Undirected(4, [(0,1),(1,2)]),
            2)));
    }

    private static double[][] Undirected(int n, (int A, int B)[] edges)
    {
        var adjacency = new double[n][];
        for (var row = 0; row < n; row++) adjacency[row] = new double[n];
        foreach (var edge in edges)
        {
            adjacency[edge.A][edge.B] = 1d;
            adjacency[edge.B][edge.A] = 1d;
        }
        return adjacency;
    }

    private static bool IsAscending(double[] values)
    {
        for (var index = 1; index < values.Length; index++)
            if (values[index] < values[index - 1] - 1e-8) return false;
        return true;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

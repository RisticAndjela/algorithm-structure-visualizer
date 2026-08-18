using AlgorithmVisualizer.Core.MachineLearning.Supervised.LogisticRegression;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Supervised;

public sealed class LogisticRegressionSimulationTests
{
    [Fact]
    public async Task CenterSplit_LearnsPositiveWeightAndPerfectTrainingAccuracy()
    {
        var simulation = Create([-3d, -2d, -1d, 1d, 2d, 3d], [0d, 0d, 0d, 1d, 1d, 1d], 0d, 0d, 0.15d, 50, 0.02d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.True(result.FinalWeight > 0d);
        Assert.Equal(1d, result.FinalAccuracy, 10);
    }

    [Fact]
    public async Task ReversedClasses_LearnNegativeWeight()
    {
        var simulation = Create([-3d, -2d, -1d, 1d, 2d, 3d], [1d, 1d, 1d, 0d, 0d, 0d], 0d, 0d, 0.15d, 50, 0.02d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.True(result.FinalWeight < 0d);
        Assert.Equal(1d, result.FinalAccuracy, 10);
    }

    [Fact]
    public async Task ShiftedClasses_MoveDecisionBoundaryAwayFromZero()
    {
        var simulation = Create([-2d, -1d, 0d, 1d, 2d, 3d], [0d, 0d, 0d, 1d, 1d, 1d], 0d, 0d, 0.15d, 60, 0.02d);

        var result = await simulation.ExecuteAsync();
        var boundary = -result.FinalBias / result.FinalWeight;

        Assert.False(result.Diverged);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.InRange(boundary, 0.05d, 1d);
    }

    [Fact]
    public async Task NoisyData_ReducesCrossEntropyWithoutRequiringPerfectAccuracy()
    {
        var simulation = Create([-3d, -2d, -1d, 0d, 1d, 2d, 3d], [0d, 0d, 0d, 1d, 0d, 1d, 1d], 0d, 0d, 0.12d, 80, 0.02d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.InRange(result.FinalAccuracy, 0.70d, 1d);
        Assert.All(result.FinalProbabilities, probability => Assert.InRange(probability, 0d, 1d));
    }

    [Fact]
    public async Task ExtremeScores_KeepSigmoidAndCrossEntropyFinite()
    {
        var simulation = Create([-1000d, 1000d], [0d, 1d], 100d, 0d, 0.1d, 5, 0.001d);

        var result = await simulation.ExecuteAsync();

        Assert.False(result.Diverged);
        Assert.True(double.IsFinite(result.FinalLoss));
        Assert.All(result.FinalProbabilities, probability => Assert.True(double.IsFinite(probability)));
        Assert.All(result.FinalProbabilities, probability => Assert.InRange(probability, 0d, 1d));
    }

    [Fact]
    public void Configure_RejectsLabelsOutsideBinaryZeroOneSet()
    {
        var simulation = new LogisticRegressionSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new LogisticRegressionConfiguration(
            [0d, 1d, 2d], [0d, 2d, 1d], 0d, 0d, 0.1d, 20, 0.01d)));
    }

    [Fact]
    public void Configure_RequiresBothClasses()
    {
        var simulation = new LogisticRegressionSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new LogisticRegressionConfiguration(
            [0d, 1d, 2d], [1d, 1d, 1d], 0d, 0d, 0.1d, 20, 0.01d)));
    }

    private static LogisticRegressionSimulation Create(double[] x, double[] labels, double weight, double bias, double learningRate, int max, double tolerance)
    {
        var simulation = new LogisticRegressionSimulation(new ImmediateRuntime());
        simulation.Configure(new LogisticRegressionConfiguration(x, labels, weight, bias, learningRate, max, tolerance));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

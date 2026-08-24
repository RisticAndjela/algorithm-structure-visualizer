using AlgorithmVisualizer.Core.MachineLearning.Optimization.GradientDescent;
using AlgorithmVisualizer.Core.Simulation.Contracts;
using Xunit;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.Optimization;

public sealed class GradientDescentSimulationTests
{
    [Fact]
    public async Task FixedLearningRate_Converges_OnStableConvexObjective()
    {
        var simulation = CreateSimulation(
            [5d, -3d], [0d, 0d], [1d, 2d], 0.2d, 80, 0.01d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.True(result.Converged);
        Assert.False(result.Diverged);
        Assert.Equal(GradientDescentStopReason.GradientTolerance, result.StopReason);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.True(result.IterationsCompleted > 0);
    }

    [Fact]
    public async Task AlreadyAtTarget_ConvergesWithoutParameterUpdate()
    {
        var simulation = CreateSimulation(
            [2d, -1d], [2d, -1d], [1d, 3d], 0.2d, 20, 0.01d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.True(result.Converged);
        Assert.Equal(0, result.IterationsCompleted);
        Assert.Equal(0d, result.InitialLoss, 10);
        Assert.Equal(0d, result.FinalGradientNorm, 10);
    }

    [Fact]
    public async Task AggressiveLearningRate_TriggersDivergenceGuard()
    {
        var simulation = CreateSimulation(
            [4d], [0d], [4d], 0.7d, 20, 0.01d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.True(result.Diverged);
        Assert.Equal(GradientDescentStopReason.Diverged, result.StopReason);
        Assert.True(result.FinalLoss > result.InitialLoss);
    }

    [Fact]
    public async Task DecayVariant_ReducesEffectiveLearningRate()
    {
        var simulation = CreateSimulation(
            [5d, -3d], [0d, 0d], [1d, 2d], 0.4d, 20, 0.01d, 0.5d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.LearningRateDecay);

        Assert.False(result.Diverged);
        Assert.True(result.FinalLearningRate < result.InitialLearningRate);
        Assert.True(result.FinalLoss < result.InitialLoss);
    }

    [Fact]
    public async Task ThreeParameterObjective_ConvergesWithoutSpecialCaseVectorMath()
    {
        var simulation = CreateSimulation(
            [4d, -3d, 2d], [1d, -1d, 0.5d], [1d, 0.5d, 1.5d], 0.2d, 100, 0.01d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.True(result.Converged);
        Assert.False(result.Diverged);
        Assert.Equal(3, result.FinalParameters.Length);
        Assert.True(result.FinalLoss < result.InitialLoss);
        Assert.All(result.FinalParameters, AssertFinite);
    }

    private static void AssertFinite(double value) => Assert.True(double.IsFinite(value));


    [Fact]
    public async Task OneUpdate_MatchesAnalyticalQuadraticGradientExactly()
    {
        var simulation = CreateSimulation(
            [2d, -1d], [0d, 1d], [1d, 2d], 0.25d, 1, 1e-12d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.False(result.Diverged);
        Assert.Equal(GradientDescentStopReason.MaximumIterations, result.StopReason);
        Assert.Equal(1.5d, result.FinalParameters[0], 10);
        Assert.Equal(0d, result.FinalParameters[1], 10);
        Assert.Equal(6d, result.InitialLoss, 10);
        Assert.Equal(2.125d, result.FinalLoss, 10);
        Assert.Equal(2.5d, result.FinalGradientNorm, 10);
        Assert.Equal(2, result.History.Length);
    }

    [Fact]
    public async Task NonFiniteCandidate_IsReportedAsDivergenceInsteadOfEnteringVectorNorm()
    {
        var simulation = CreateSimulation(
            [1d], [0d], [2d], double.MaxValue, 5, 0.01d, 0.15d);

        var result = await simulation.ExecuteAsync(GradientDescentVariant.FixedLearningRate);

        Assert.True(result.Diverged);
        Assert.Equal(GradientDescentStopReason.Diverged, result.StopReason);
        Assert.All(result.FinalParameters, AssertFinite);
    }

    [Fact]
    public void Configure_RejectsDimensionMismatch()
    {
        var simulation = new GradientDescentSimulation(new ImmediateRuntime());

        Assert.Throws<ArgumentException>(() => simulation.Configure(new GradientDescentConfiguration(
            [1d, 2d], [0d], [1d, 1d], 0.1d, 10, 0.01d, 0.1d)));
    }

    private static GradientDescentSimulation CreateSimulation(
        double[] theta,
        double[] target,
        double[] curvature,
        double learningRate,
        int maxIterations,
        double tolerance,
        double decay)
    {
        var simulation = new GradientDescentSimulation(new ImmediateRuntime());
        simulation.Configure(new GradientDescentConfiguration(
            theta, target, curvature, learningRate, maxIterations, tolerance, decay));
        return simulation;
    }

    private sealed class ImmediateRuntime : ISimulationRuntime
    {
        public CancellationToken SimulationCancellationToken => CancellationToken.None;
        public void SetCurrentStep(string description) { }
        public Task WaitForNextStepAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

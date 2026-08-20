using AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;

namespace AlgorithmVisualizer.Core.Tests.MachineLearning.DeepLearning;

public sealed class ActivationMathTests
{
    [Fact]
    public void ReLU_AndLeakyReLU_DifferOnNegativeInput()
    {
        Assert.Equal(0d, ActivationMath.Apply(ActivationKind.ReLU, -2d), 10);
        Assert.Equal(-0.2d, ActivationMath.Apply(ActivationKind.LeakyReLU, -2d), 10);
        Assert.Equal(0d, ActivationMath.DerivativeFromPreActivation(ActivationKind.ReLU, -2d), 10);
        Assert.Equal(0.1d, ActivationMath.DerivativeFromPreActivation(ActivationKind.LeakyReLU, -2d), 10);
    }

    [Fact]
    public void Sigmoid_IsStableAndHasQuarterDerivativeAtZero()
    {
        Assert.Equal(0.5d, ActivationMath.Apply(ActivationKind.Sigmoid, 0d), 10);
        Assert.Equal(0.25d, ActivationMath.DerivativeFromPreActivation(ActivationKind.Sigmoid, 0d), 10);
        Assert.True(double.IsFinite(ActivationMath.Apply(ActivationKind.Sigmoid, 1000d)));
        Assert.True(double.IsFinite(ActivationMath.Apply(ActivationKind.Sigmoid, -1000d)));
    }
}

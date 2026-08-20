namespace AlgorithmVisualizer.Core.MachineLearning.DeepLearning.Common;

public enum ActivationKind
{
    Linear,
    ReLU,
    Sigmoid,
    Tanh,
    LeakyReLU
}

/// <summary>
/// Small activation helper implemented directly for the deep-learning lessons.
/// It deliberately exposes both the forward value and derivative from the same
/// pre-activation z so Neuron, MLP, and Backpropagation teach one consistent rule.
/// </summary>
public static class ActivationMath
{
    public static double Apply(ActivationKind kind, double z) => kind switch
    {
        ActivationKind.Linear => z,
        ActivationKind.ReLU => z > 0d ? z : 0d,
        ActivationKind.Sigmoid => Sigmoid(z),
        ActivationKind.Tanh => Math.Tanh(z),
        ActivationKind.LeakyReLU => z >= 0d ? z : 0.1d * z,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static double DerivativeFromPreActivation(ActivationKind kind, double z)
    {
        return kind switch
        {
            ActivationKind.Linear => 1d,
            ActivationKind.ReLU => z > 0d ? 1d : 0d,
            ActivationKind.Sigmoid => Sigmoid(z) * (1d - Sigmoid(z)),
            ActivationKind.Tanh => 1d - Math.Pow(Math.Tanh(z), 2d),
            ActivationKind.LeakyReLU => z >= 0d ? 1d : 0.1d,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    public static string DisplayName(ActivationKind kind) => kind switch
    {
        ActivationKind.Linear => "Linear",
        ActivationKind.ReLU => "ReLU",
        ActivationKind.Sigmoid => "Sigmoid",
        ActivationKind.Tanh => "Tanh",
        ActivationKind.LeakyReLU => "Leaky ReLU",
        _ => kind.ToString()
    };

    private static double Sigmoid(double z)
    {
        // Stable two-branch form avoids exp overflow for large |z|.
        if (z >= 0d)
        {
            var e = Math.Exp(-z);
            return 1d / (1d + e);
        }

        var positive = Math.Exp(z);
        return positive / (1d + positive);
    }
}

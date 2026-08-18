namespace AlgorithmVisualizer.Core.DataStructures.Vector;

/// <summary>
/// Mathematical vector storage implemented directly over a contiguous double[].
/// No List, LINQ vector helper, System.Numerics vector type, or numerical library
/// is used for the operations taught by the Vector learning lab.
/// </summary>
public sealed class ManualVector
{
    private double[] _values;

    public ManualVector(int dimension)
    {
        if (dimension < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dimension), "Vector dimension must be at least 1.");
        }

        _values = new double[dimension];
    }

    public int Dimension => _values.Length;

    public double this[int index]
    {
        get
        {
            ValidateIndex(index);
            return _values[index];
        }
        set
        {
            ValidateIndex(index);
            _values[index] = value;
        }
    }

    public void CopyFrom(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count < 1)
        {
            throw new ArgumentException("A vector needs at least one component.", nameof(values));
        }

        if (_values.Length != values.Count)
        {
            _values = new double[values.Count];
        }

        for (var index = 0; index < values.Count; index++)
        {
            _values[index] = values[index];
        }
    }

    public ManualVector Clone()
    {
        var clone = new ManualVector(Dimension);
        for (var index = 0; index < Dimension; index++)
        {
            clone[index] = _values[index];
        }

        return clone;
    }

    public double[] CopyValues()
    {
        var copy = new double[_values.Length];
        for (var index = 0; index < _values.Length; index++)
        {
            copy[index] = _values[index];
        }

        return copy;
    }

    public bool IsZero(double tolerance = 1e-10)
    {
        for (var index = 0; index < _values.Length; index++)
        {
            if (Math.Abs(_values[index]) > tolerance)
            {
                return false;
            }
        }

        return true;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _values.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

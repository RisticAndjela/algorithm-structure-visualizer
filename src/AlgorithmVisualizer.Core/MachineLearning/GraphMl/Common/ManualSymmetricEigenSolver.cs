using AlgorithmVisualizer.Core.DataStructures.Matrix;

namespace AlgorithmVisualizer.Core.MachineLearning.GraphMl.Common;

public sealed record ManualEigenResult(double[] Eigenvalues, double[][] Eigenvectors, int Rotations);

/// <summary>
/// Small symmetric-matrix eigensolver for teaching spectral methods.
/// Uses explicit Jacobi rotations over the project-owned ManualMatrix storage.
/// </summary>
public static class ManualSymmetricEigenSolver
{
    public static ManualEigenResult Solve(ManualMatrix source, int maxRotations = 256, double tolerance = 1e-10)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Rows != source.Columns) throw new ArgumentException("Jacobi eigendecomposition requires a square matrix.", nameof(source));
        if (maxRotations < 1) throw new ArgumentOutOfRangeException(nameof(maxRotations));
        if (tolerance <= 0d || !double.IsFinite(tolerance)) throw new ArgumentOutOfRangeException(nameof(tolerance));

        var size = source.Rows;
        var matrix = source.Clone();
        var vectors = new ManualMatrix(size, size);
        for (var index = 0; index < size; index++) vectors[index, index] = 1d;

        var rotations = 0;
        for (; rotations < maxRotations; rotations++)
        {
            var p = 0;
            var q = 0;
            var largest = 0d;
            for (var row = 0; row < size; row++)
            {
                for (var column = row + 1; column < size; column++)
                {
                    var magnitude = Math.Abs(matrix[row, column]);
                    if (magnitude <= largest) continue;
                    largest = magnitude;
                    p = row;
                    q = column;
                }
            }

            if (largest <= tolerance) break;

            var app = matrix[p, p];
            var aqq = matrix[q, q];
            var apq = matrix[p, q];
            var angle = 0.5d * Math.Atan2(2d * apq, aqq - app);
            var cosine = Math.Cos(angle);
            var sine = Math.Sin(angle);

            for (var index = 0; index < size; index++)
            {
                if (index == p || index == q) continue;
                var aip = matrix[index, p];
                var aiq = matrix[index, q];
                var nextIp = (cosine * aip) - (sine * aiq);
                var nextIq = (sine * aip) + (cosine * aiq);
                matrix[index, p] = nextIp;
                matrix[p, index] = nextIp;
                matrix[index, q] = nextIq;
                matrix[q, index] = nextIq;
            }

            var cc = cosine * cosine;
            var ss = sine * sine;
            var sc = sine * cosine;
            matrix[p, p] = (cc * app) - (2d * sc * apq) + (ss * aqq);
            matrix[q, q] = (ss * app) + (2d * sc * apq) + (cc * aqq);
            matrix[p, q] = 0d;
            matrix[q, p] = 0d;

            for (var row = 0; row < size; row++)
            {
                var vip = vectors[row, p];
                var viq = vectors[row, q];
                vectors[row, p] = (cosine * vip) - (sine * viq);
                vectors[row, q] = (sine * vip) + (cosine * viq);
            }
        }

        var eigenvalues = new double[size];
        var eigenvectors = new double[size][];
        for (var eigenIndex = 0; eigenIndex < size; eigenIndex++)
        {
            eigenvalues[eigenIndex] = matrix[eigenIndex, eigenIndex];
            eigenvectors[eigenIndex] = new double[size];
            for (var component = 0; component < size; component++) eigenvectors[eigenIndex][component] = vectors[component, eigenIndex];
        }

        SortAscending(eigenvalues, eigenvectors);
        return new ManualEigenResult(eigenvalues, eigenvectors, rotations);
    }

    private static void SortAscending(double[] values, double[][] vectors)
    {
        for (var index = 1; index < values.Length; index++)
        {
            var value = values[index];
            var vector = vectors[index];
            var scan = index - 1;
            while (scan >= 0 && values[scan] > value)
            {
                values[scan + 1] = values[scan];
                vectors[scan + 1] = vectors[scan];
                scan--;
            }
            values[scan + 1] = value;
            vectors[scan + 1] = vector;
        }
    }
}

using MultiThreading.Task3.MatrixMultiplier.Matrices;
using System;
using System.Threading.Tasks;

namespace MultiThreading.Task3.MatrixMultiplier.Multipliers
{
    public class MatricesMultiplierParallel : IMatricesMultiplier
    {
        public IMatrix Multiply(IMatrix m1, IMatrix m2)
        {
            var resultMatrix = new Matrix(m1.RowCount, m2.ColCount);

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };
            Parallel.For(0, m1.RowCount, options, i =>
            {
                Parallel.For(0, m2.ColCount, options, j =>
                {
                    long sum = 0;
                    for (byte k = 0; k < m1.ColCount; k++)
                    {
                        sum += m1.GetElement(i, k) * m2.GetElement(k, j);
                    }

                    resultMatrix.SetElement(i, j, sum);
                });
            });

            return resultMatrix;
        }
    }
}

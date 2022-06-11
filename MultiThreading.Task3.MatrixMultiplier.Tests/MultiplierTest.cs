using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiThreading.Task3.MatrixMultiplier.Matrices;
using MultiThreading.Task3.MatrixMultiplier.Multipliers;
using System;
using System.Diagnostics;

namespace MultiThreading.Task3.MatrixMultiplier.Tests
{
    [TestClass]
    public class MultiplierTest
    {
        [TestMethod]
        public void MultiplyMatrix3On3Test()
        {
            TestMatrix3On3(new MatricesMultiplier());
            TestMatrix3On3(new MatricesMultiplierParallel());
        }

        [TestMethod]
        public void ParallelEfficiencyTest()
        {
            var multiplier = new MatricesMultiplier();
            var multiplierParallel = new MatricesMultiplierParallel();

            var sw = new Stopwatch();
            for (int matrixSize = 2; matrixSize <= byte.MaxValue; matrixSize++)
            {
                var matrix = new Matrix(matrixSize, matrixSize, true);
                var multiplyTime = AverageTime(sw, () => multiplier.Multiply(matrix, matrix));
                var multiplyParallelTime = AverageTime(sw, () => multiplierParallel.Multiply(matrix, matrix));

                if (multiplyTime > multiplyParallelTime)
                {
                    Console.WriteLine("Starting from matrix size about {0} use parallel more efficiently.", matrixSize);
                    return;
                }
            }

            Assert.Fail();
        }

        #region private methods

        void TestMatrix3On3(IMatricesMultiplier matrixMultiplier)
        {
            if (matrixMultiplier == null)
            {
                throw new ArgumentNullException(nameof(matrixMultiplier));
            }

            var m1 = new Matrix(3, 3);
            m1.SetElement(0, 0, 34);
            m1.SetElement(0, 1, 2);
            m1.SetElement(0, 2, 6);

            m1.SetElement(1, 0, 5);
            m1.SetElement(1, 1, 4);
            m1.SetElement(1, 2, 54);

            m1.SetElement(2, 0, 2);
            m1.SetElement(2, 1, 9);
            m1.SetElement(2, 2, 8);

            var m2 = new Matrix(3, 3);
            m2.SetElement(0, 0, 12);
            m2.SetElement(0, 1, 52);
            m2.SetElement(0, 2, 85);

            m2.SetElement(1, 0, 5);
            m2.SetElement(1, 1, 5);
            m2.SetElement(1, 2, 54);

            m2.SetElement(2, 0, 5);
            m2.SetElement(2, 1, 8);
            m2.SetElement(2, 2, 9);

            var multiplied = matrixMultiplier.Multiply(m1, m2);
            Assert.AreEqual(448, multiplied.GetElement(0, 0));
            Assert.AreEqual(1826, multiplied.GetElement(0, 1));
            Assert.AreEqual(3052, multiplied.GetElement(0, 2));

            Assert.AreEqual(350, multiplied.GetElement(1, 0));
            Assert.AreEqual(712, multiplied.GetElement(1, 1));
            Assert.AreEqual(1127, multiplied.GetElement(1, 2));

            Assert.AreEqual(109, multiplied.GetElement(2, 0));
            Assert.AreEqual(213, multiplied.GetElement(2, 1));
            Assert.AreEqual(728, multiplied.GetElement(2, 2));
        }


        private static long AverageTime(Stopwatch stopwatch, Action action)
        {
            const int iterations = 10;
            double totalTime = 0;
            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Restart();
                action();
                totalTime += stopwatch.ElapsedMilliseconds;
            }
            return (long)(totalTime / iterations);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;




namespace FANNUnitTests
{
    [TestClass]
    public class BirdAudioAnalysisTests
    {

        private Boolean ArrayEq2D<T>(T[][] one, T[][] two)
        {
            if (one.Length != two.Length)
                return false;
            for (var i = 0; i < one.Length; i++)
            {
                if (one[i].Length != two[i].Length)
                    return false;
                for (var j = 0; j < one[i].Length; j++)
                {
                    if (!one[i][j].Equals(two[i][j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private void Print2D<T>(T[][] one, TextWriter output)
        {
            for (var i = 0; i < one.Length; i++)
            {

                output.Write("[");
                for (var j = 0; j < one[i].Length; j++)
                {
                    output.Write("{0}\t", one[i][j]);
                }
                output.Write("]\n");
            }
            
        }

        [TestMethod]
        public void TestLinqExtension1()
        {
            var testData = new[] { 1, 2, 3, 4, 5 };
            var expectedResult = new int[][]
                                 {
                                     new[] {1, 2},
                                     new[] {2, 3},
                                     new[] {3, 4},
                                     new[] {4, 5}
                                 };
            var result = testData.RollingBuffer(2, 1).ToArray();

            Print2D(result, Console.Out);


            Assert.IsTrue(ArrayEq2D(expectedResult, result));
        }
        [TestMethod]
        public void TestLinqExtension2()
        {
            var testData = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            var expectedResult = new int[][]
                                 {
                                     new[] {1, 2, 3},
                                     new[] {3, 4, 5},
                                     new[] {5, 6, 7},
                                     new[] {7, 8, 9},
                                     new[] {9, 10, 11},
                                 };
            var result = testData.RollingBuffer(3, 2).ToArray();

            Print2D(result, Console.Out);


            Assert.IsTrue(ArrayEq2D(expectedResult, result));
        }
        [TestMethod]
        public void TestLinqExtension3()
        {
            var testData = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var expectedResult = new int[][]
                                 {
                                     new[] {1, 2, 3},
                                     new[] {3, 4, 5},
                                     new[] {5, 6, 7},
                                     new[] {7, 8, 9},
                                     new[] {9, 10, 11},
                                 };
            var result = testData.RollingBuffer(3, 2).ToArray();

            Print2D(result, Console.Out);


            Assert.IsTrue(ArrayEq2D(expectedResult, result));
        }
    }
}

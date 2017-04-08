using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BirdAudioAnalysis;

namespace FANNUnitTests
{
    [TestClass]
    class LinqBufferTest
    {

        private static IEnumerable<Int32> testData = new[] {1, 2, 3, 4, 5};
        [TestMethod]
        public void TestReturn()
        {
            var expectedResult = new int[][]
                                 {
                                     new[] {1, 2},
                                     new[] {2, 3},
                                     new[] {3, 4},
                                     new[] {4, 5}
                                 };
            var result = testData.RollingBuffer(2, 1);

            result.SequenceEqual(expectedResult);
        }
    }
}

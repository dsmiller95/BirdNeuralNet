using System;
using System.CodeDom;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BirdAudioAnalysis;

namespace FANNUnitTests
{
    [TestClass]
    public class AudioAnalyzerTest
    {

        [TestMethod]
        public void FFTForwardBack()
        {

            const int bufferSize = 16;
            

            float[][] inputStream = new float[][]
                                    {
                                        new float[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},
                                        new float[] {10, 11, 12, 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9}
                                    };

            var fft = AudioAnalyzer.FastFourierTransform(inputStream, true, bufferSize);
            

            var reversefft = AudioAnalyzer.FastFourierTransform(fft, false, bufferSize).Select(
                    (arr) => arr.Select((value) => (float)value.Magnitude).ToArray()
                ).ToArray();
            
            BirdAudioAnalysisTests.Print2D(inputStream, Console.Out);
            BirdAudioAnalysisTests.Print2D(reversefft, Console.Out);

            Assert.IsTrue(BirdAudioAnalysisTests.ArrayEq2D(inputStream, reversefft));
        }
    }
}

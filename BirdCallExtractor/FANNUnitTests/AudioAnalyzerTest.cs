using System;
using System.CodeDom;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BirdAudioAnalysis;
using NAudio.Wave;

namespace FANNUnitTests
{
    [TestClass]
    public class AudioAnalyzerTest
    {

        private float[][] FFTAndReverse(float[][] input, int bufferSize)
        {

            var fft = AudioAnalyzer.FastFourierTransform(input, true, bufferSize);


            var reversefft = AudioAnalyzer.FastFourierTransform(fft, false, bufferSize).Select(
                    (arr) => arr.Select((value) => (float) (value.Re)).ToArray()
                ).ToArray();

            return reversefft;
        }

        [TestMethod]
        public void FFTForwardBack()
        {

            const int bufferSize = 16;


            float[][] inputStream = new float[][]
                                    {
                                        new float[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},
                                        new float[] {10, 11, 12, 13, 14, 15, 16, 1, 2, 3, 4, 5, 6, 7, 8, 9}
                                    };
            
            var reversefft = FFTAndReverse(inputStream, bufferSize);

            BirdAudioAnalysisTests.Print2D(inputStream, Console.Out);
            BirdAudioAnalysisTests.Print2D(reversefft, Console.Out);

            Assert.IsTrue(BirdAudioAnalysisTests.ArrayEq2D(inputStream, reversefft));
        }

        [TestMethod]
        public void FFTForwardBackNeg()
        {

            const int bufferSize = 16;


            float[][] inputStream = new float[][]
                                    {
                                        new float[] {1, 2, 3, 4, 5, 4, 3, 2, 1, 0.1F, -1, -2, -3, -4, -5, -4},
                                        new float[] {3, 2, 1, -0.1F, -1, -2, -3, -4, -5, -4, 1, 2, 3, 4, 5, 4}
                                    };

            var reversefft = FFTAndReverse(inputStream, bufferSize);

            BirdAudioAnalysisTests.Print2D(inputStream, Console.Out);
            BirdAudioAnalysisTests.Print2D(reversefft, Console.Out);

            Assert.IsTrue(BirdAudioAnalysisTests.ArrayEq2D(inputStream, reversefft));
        }
    }
}

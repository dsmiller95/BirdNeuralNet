using System.Collections.Generic;
using AForge.Math;

namespace BirdAudioAnalysis
{
    public interface IAudioSplitter
    {

        /// <summary>
        /// Take a buffer of audio FFT data, and filter it so that all that's left is bird calls
        /// </summary>
        /// <param name="originalBuffer">the original fourier transformed data</param>
        /// <returns>a 2D array containing all of the individual chunks of audio</returns>
        IEnumerable<IEnumerable<Complex[]>> SplitAudio(IEnumerable<Complex[]> originalBuffer);
    }
}
using System.Collections.Generic;

namespace BirdAudioAnalysis
{
    public class AudioFFTSplitter
    {
        public AudioFFTSplitter()
        {
            
        }

        /// <summary>
        /// Accepts a stream of FFT results, and returns a new IEnumerable composed of streams of FFT data
        ///     which have been deemed acceptable by the trimmer
        /// </summary>
        /// <param name="FFTData">A stream of FFT data directly from the file</param>
        /// <returns>a new IEnumerable composed of streams of FFT data which have been deemed acceptable by the trimmer</returns>
        public IEnumerable<IEnumerable<float[]>> splitAudioStream(IEnumerable<float[]> FFTData)
        {
            return null;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using AForge.Math;

namespace BirdAudioAnalysis
{
    internal class AverageIntensityAudioSplitter : AudioSplitter
    {
        private int _currentNonPassing;
        private int _sizeOfChunk;

        /// <summary>
        /// Denotes if we should slice here or not
        /// </summary>
        /// <param name="sample">Sample to check</param>
        /// <param name="originalBuffer">Benchmark to use to check if sample is part of signal</param>
        /// <returns>boolean that sample is signal</returns>
        public override bool CutHere(Complex[] sample, List<Complex[]> originalBuffer)
        {
            _sizeOfChunk++;
            //If chunk too small, don't cut yet, no matter what
            if (_sizeOfChunk < 60)//60 chunks is roughly 30,000 samples, which should be a bit less than 1 1/2 seconds
            {
                return false;
            }

            if (sample.Max(value => value.Magnitude) < GetAvgSampleIntensity(originalBuffer))
            {
                _currentNonPassing++;
            }
            else
            {
                _currentNonPassing = 0;
            }

            if (_currentNonPassing < 5) return false;
            _currentNonPassing = 0;
            _sizeOfChunk = 0;
            return true;
        }

        /// <summary>
        /// Denotes if given sample is part of the signal, and not the noise
        /// </summary>
        /// <param name="sample">Sample to check</param>
        /// <param name="originalBuffer">Benchmark to use to check if sample is part of signal</param>
        /// <returns>boolean that sample is signal</returns>
        public override bool IsSignalSample(Complex[] sample, List<Complex[]> originalBuffer) => sample.Max(value => value.Magnitude) > GetAvgSampleIntensity(originalBuffer);

        /// <summary>
        /// Take a buffer of audio FFT data, and filter it so that all that's left is bird calls
        /// </summary>
        /// <param name="file">Input audio file</param>
        /// <returns>a 2D array containing all of the individual chunks of audio</returns>
        public double GetAvgSampleIntensity(IEnumerable<Complex[]> file) => file.Average(sample => sample.Max(complex => complex.Magnitude));
    }
}

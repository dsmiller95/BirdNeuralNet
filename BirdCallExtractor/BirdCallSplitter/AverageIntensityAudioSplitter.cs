using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math;

namespace BirdAudioAnalysis
{
    internal class AverageIntensityAudioSplitter : AudioSplitter
    {
        private int _currentNonPassing;
        private int _sizeOfChunk;
        private double _waterline;

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
            if (_sizeOfChunk < 60) //60 chunks is roughly 30,000 samples, which should be a bit less than 1 1/2 seconds
            {
                return false;
            }

            if (sample.Average(value => value.Magnitude) < _waterline)
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
        public override bool IsSignalSample(Complex[] sample, List<Complex[]> originalBuffer)
            => sample.Average(value => value.Magnitude) > GetAvgSampleIntensity(originalBuffer);

        /// <summary>
        /// Finds a benchmark for us to compare values to
        /// </summary>
        /// <param name="file">Input audio file</param>
        /// <returns>Average of sample's Averages, minus 0.5 std dev</returns>
        public double GetAvgSampleIntensity(IEnumerable<Complex[]> file)
        {
            if (_waterline.Equals(0))
            {
                var complexses = file as IList<Complex[]> ?? file.ToList();
                var average = complexses.Average(sample => sample.Average(complex => complex.Magnitude));
                var sum = complexses.Sum(d => Math.Pow(d.Average(complex => complex.Magnitude) - average, 2)); //Perform the Sum of (value-avg)_2_2      

                _waterline = average - Math.Sqrt(sum / (complexses.Count - 1))/2;
            }

            return _waterline;
            //return file.Average(sample => sample.Average(complex => complex.Magnitude));
        }
    }
}

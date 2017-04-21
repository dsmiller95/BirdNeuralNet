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

        public AverageIntensityAudioSplitter(IEnumerable<Complex[]> buffer) : base(buffer)
        {

            var complexses = origBuffer;

            var averages = complexses.Select(sample => sample.Average(complex => complex.Magnitude));

            var totalAverageMagnitute = averages.Average();

            var variance = averages.Sum(avgMag => Math.Pow(avgMag - totalAverageMagnitute, 2)) / (averages.Count() - 1);
            var standardDeviation = Math.Sqrt(variance);

            _waterline = totalAverageMagnitute;// - standardDeviation;
        }

        /// <summary>
        /// Denotes if we should slice here or not
        /// </summary>
        /// <param name="sample">Sample to check</param>
        /// <param name="originalBuffer">Benchmark to use to check if sample is part of signal</param>
        /// <returns>boolean that sample is signal</returns>
        public override bool CutHere(Complex[] sample)
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
        public override bool IsSignalSample(Complex[] sample)
            => sample.Average(value => value.Magnitude) > _waterline || true;
    }
}

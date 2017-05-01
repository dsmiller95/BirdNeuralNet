using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math;

namespace BirdAudioAnalysis
{
    internal class AverageIntensityAudioSplitter
    {

        private int _currentNonPassing;
        private int _sizeOfChunk;
        private double _waterline;
        private double _avgDeviationOfSpectogram;
        private double _stdDevOfDeviationOfSpectogram;

        private int minChunkNum = 100;

        protected IList<Complex[]> origBuffer;

        public AverageIntensityAudioSplitter(IEnumerable<Complex[]> buffer)
        {

            this.origBuffer = buffer as IList<Complex[]> ?? buffer.ToList();
            var complexses = origBuffer;

            var averages = complexses.Select(sample => sample.Average(complex => complex.Magnitude));

            var totalAverageMagnitute = averages.Average();

            var standardDeviation = GetStdDev(averages);

            _waterline = totalAverageMagnitute - standardDeviation;


            var stdEvs = complexses.Select(sample =>
                GetStdDev(
                    sample.Select(
                        (complex) => complex.Magnitude
                    )
                )
            );

            _avgDeviationOfSpectogram = stdEvs.Average();
            _stdDevOfDeviationOfSpectogram = GetStdDev(stdEvs);
        }

        private double GetStdDev(IEnumerable<double> input)
        {
            var average = input.Average();
            var variance = input.Sum(value => Math.Pow(value - average, 2)) / (input.Count() - 1);
            return Math.Sqrt(variance);
        }

        /// <summary>
        /// Take a buffer of audio FFT data, and filter it so that all that's left is bird calls
        /// </summary>
        /// <param name="originalBuffer">the original fourier transformed data</param>
        /// <returns>a 2D array containing all of the individual chunks of audio</returns>
        public IEnumerable<IEnumerable<Complex[]>> SplitAudio()
        {
            //prevent multiple enumerations of the input IEnumerable
            Console.Out.WriteLine("count: " + origBuffer.Count);


            var isSamples = origBuffer.Select(complexes => IsSignalSample(complexes)).ToList();

            var splitIndexes = GetGroupings(isSamples, 5, 5);

            foreach (var split in splitIndexes)
            {
                var endIndex = split.Item2;
                if(split.Item2 - split.Item1 < minChunkNum)
                {
                    endIndex = minChunkNum + split.Item1;
                }
                yield return origBuffer.Skip(split.Item1).Take(endIndex - split.Item1);
            }

            /*
            var result = new List<IEnumerable<Complex[]>>();
            var enumerator = origBuffer.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (IsSignalSample(enumerator.Current))
                {
                    var tmp = ReadSignals(ref enumerator);
                    Console.Out.WriteLine("lenChunk: " + tmp.Count);
                    result.Add(tmp);
                }
            }

            return result;*/
        }

        /// <summary>
        /// Read in signals from data
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="originalBuffer"></param>
        /// <returns></returns>
        internal List<Complex[]> ReadSignals(ref IEnumerator<Complex[]> enumerator)
        {
            var result = new List<Complex[]>();
            

            do
            {
                if (CutHere(enumerator.Current))
                {
                    return result;
                }
                result.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            return result;
        }

        /// <summary>
        /// Denotes if we should slice here or not
        /// </summary>
        /// <param name="sample">Sample to check</param>
        /// <param name="originalBuffer">Benchmark to use to check if sample is part of signal</param>
        /// <returns>boolean that sample is signal</returns>
        public bool CutHere(Complex[] sample)
        {
            _sizeOfChunk++;
            //If chunk too small, don't cut yet, no matter what
            if (_sizeOfChunk < 60) //60 chunks is roughly 30,000 samples, which should be a bit less than 1 1/2 seconds
            {
                return false;
            }

            if (!(IsSignalSample(sample)))
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
        public bool IsSignalSample(Complex[] sample)
        {
            //look for a sample that is, on average, louder than the average of the file
            var AboveMid = sample.Average(value => value.Magnitude) - _waterline;
            //also look for a sample that has a tighter frequency distribution; that is, a smaller standard deviation
            var AboveStd = GetStdDev(sample.Select(complex => complex.Magnitude)) - (_avgDeviationOfSpectogram + _stdDevOfDeviationOfSpectogram);

            if(AboveMid > 0 && AboveStd < 0)
            {
                return true;
            }
            return false;

            //return sample.Average(value => value.Magnitude) > _waterline;
        }


        /// <summary>
        /// determine where to split a sequence such that the remaining sets have at most maxDistance of false elements between the true elements
        /// </summary>
        /// <param name="toGroup">The list of booleans to determine the split on</param>
        /// <param name="maxDistance">the maximum number of "false" elements between each "true" element, while still allowing it into the in-group</param>
        /// <returns></returns>
        public IEnumerable<Tuple<int, int>> GetGroupings(IList<bool> toGroup, int maxDistance, int minStreak)
        {
            var currentStreak = -1;
            var currentRun = -1;
            var lastTrue = -1;
            //result = [];
            if (toGroup[0])
            {
                currentRun = lastTrue = 0;
                //console.log("current run: " + currentRun);
            }
            for (var i = 0; i < toGroup.Count; i++)
            {
                if (currentRun >= 0)
                {
                    if (!toGroup[i])
                    {
                        if (i - lastTrue >= maxDistance)
                        {
                            
                            //the end of a chunk
                            if(currentStreak >= minStreak)
                                yield return Tuple.Create(currentRun, lastTrue);
                            currentRun = -1;
                            lastTrue = -1;
                        }
                    }
                    else
                    {
                        lastTrue = i;
                        currentStreak += 1;
                    }
                }
                else
                {
                    if (toGroup[i])
                    {
                        currentRun = i;
                        lastTrue = i;
                        currentStreak = 1;
                    }
                }
            }
            if (currentRun >= 0)
            {
                if (toGroup[toGroup.Count - 1])
                    yield return Tuple.Create(currentRun, toGroup.Count - 1);
                else
                    yield return Tuple.Create(currentRun, lastTrue);
            }
        }
    }
}

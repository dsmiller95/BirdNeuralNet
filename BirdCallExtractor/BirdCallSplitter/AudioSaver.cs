﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AForge.Math;
using NAudio.Wave;

namespace BirdAudioAnalysis
{
    public class AudioSaver
    {

        private int _bufferSize;

        public AudioSaver(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Save a stream of FFT data to a file. Will perform the reverse FFT and save it to a file
        /// </summary>
        /// <param name="fileName">The file to save the data to</param>
        /// <param name="data">the FFT data</param>
        /// <param name="format">the WaveFormat of the incoming data</param>
        /// <returns>String pointing to the file which was saved</returns>
        public async Task<string> saveAsAudioFile(string fileName, IEnumerable<Complex[]> data, WaveFormat format)
        {
            try
            {
                using (var writer = new WaveFileWriter(fileName, format))
                {
                    var fftChunks = AudioAnalyzer.FastFourierTransform(data, false, _bufferSize);
                    var samplesComplex = fftChunks.Aggregate(new List<Complex>(), (accumulate, next) =>
                                                         {
                                                             accumulate.AddRange(next);
                                                             return accumulate;
                                                         });
                    var samples = samplesComplex.Select((complex) => (float) complex.Magnitude).ToArray();
                    
                    writer.WriteSamples(samples, 0, samples.Length);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            return "AaaaAaAAAAa";
        }
    }
}
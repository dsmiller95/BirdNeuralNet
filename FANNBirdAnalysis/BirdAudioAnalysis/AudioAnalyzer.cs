using AForge.Math;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if FANN_FIXED
using FANNCSharp.Fixed;
using DataType = System.Int32;
#elif FANN_DOUBLE
using FANNCSharp.Double;
using DataType = System.Double;
#else
using FANNCSharp.Float;
using DataType = System.Single;
#endif


namespace BirdAudioAnalysis
{
    /*
     * A class designed to read the data from an AudioFileReader and output it as floats into a FFT,
     * returning array of numbers representing the frequency amplitudes
     */
    class AudioAnalyzer
    {
        private readonly string _audioFile;
        private readonly int _bufferSize, _sampleRate;
        private readonly bool _trimSilence;
        public AudioAnalyzer(string audioFile, int bufferSize, int sampleRate, bool trimSilence = false)
        {
            _audioFile = audioFile;
            _bufferSize = bufferSize;
            _sampleRate = sampleRate;
            _trimSilence = trimSilence;
        }

        /*
         * Get the corresponding frequency for the given frequency bin
         */
        public int GetFrequencyForBin(int bin)
        {
            return bin * _sampleRate / _bufferSize;
        }

        public float GetFrequencyForBin(float bin)
        {
            return bin * _sampleRate / _bufferSize;
        }

        /**
         * Get the size of the arrays that will be returned from this analyzer
         */
        public int GetDataSize()
        {
            return _bufferSize/2;
        }

        /*
         * Reads in the audio file specified by reader, taking in bufferSize number of samples in each chunk to be analyzed
         * Returns an enumerable composed of lists of the frequency bins
         */
        public IEnumerable<DataType[]> GetFrequencies()
        {

            var reader = new AudioFileReader(_audioFile);
            return (new AudioStreamReader(reader, _bufferSize, _bufferSize / 2, _trimSilence)).Select((floats) =>
            {
                Complex[] complex = new Complex[_bufferSize];
                for (int i = 0; i < floats.Length; i++)
                {
                    complex[i] = new Complex(floats[i], 0);
                }
                return complex;
            }).Select((complex) =>
            {
                FourierTransform.FFT(complex, FourierTransform.Direction.Forward);
                
                return complex.Take(complex.Length / 2).Select((comp) =>(DataType) Math.Abs(comp.Magnitude)).ToArray();
            });
        }
    }
}

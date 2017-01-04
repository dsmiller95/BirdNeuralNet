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
     * A class designed to read the data from an audio file and output an array 
     * of floating points representing the frequency amplitudes; as a result from a FFT
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
         * Get the corresponding frequency in Hz for the given frequency bin
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
                //Cast all of the floating point numbers to Complex numbers in preperation for the FFT
                Complex[] complex = new Complex[_bufferSize];
                for (int i = 0; i < floats.Length; i++)
                {
                    complex[i] = new Complex(floats[i], 0);
                }
                return complex;
            }).Select((complex) =>
            {
                //Perform the FFT and throw away half of the resulting array; then cast to Datatype from Complex
                FourierTransform.FFT(complex, FourierTransform.Direction.Forward);
                
                return complex
                    //throw away half because the FFT is Symmetric when all inputs are real number
                    .Take(complex.Length / 2)
                    .Select((comp) => (DataType) Math.Abs(comp.Magnitude))
                    .ToArray();
            });
        }
    }
}

using AForge.Math;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: explain what all these FANN data types are, they probably don't know. also where are they set anyways? since this is a if/else
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
        // TODO: why is the audioFile a string? where is it getting the file from? another file in the project?
        private readonly string _audioFile;
        // TODO: if we change these, how will it affect the network. ex: smaller buffer size = slower to go through it all? or more accurate? our groupmates will need to understand.
        private readonly int _bufferSize, _sampleRate;
        // TODO: what is trimsilence for
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
        // TODO: what is the "frequency bin"? why did you even call it a bin what does that mean in this context. just a place to put stuff? that kind of bin?
        public int GetFrequencyForBin(int bin)
        {
            return bin * _sampleRate / _bufferSize;
        }
        // TODO: explain why we would need to get the frequency as a float vs why we would want it as an int.
        public float GetFrequencyForBin(float bin)
        {
            return bin * _sampleRate / _bufferSize;
        }

        /**
         * Get the size of the arrays that will be returned from this analyzer
         */
        // TODO: what are "the arrays". arrays of frequencies? arrays of FFT results? why are you dividing it by 2?
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
                // TODO: okay but why do we need Complex numbers for the FFT. I think youre forgetting they dont know all this stuff.
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

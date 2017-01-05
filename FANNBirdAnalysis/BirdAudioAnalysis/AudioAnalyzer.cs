﻿using AForge.Math;
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
        // TODO: why is the audioFile a string? where is it getting the file from? another file in the project?
        //The path to the audio file
        private readonly string _audioFilePath;

        // TODO: if we change these, how will it affect the network. ex: smaller buffer size = slower to go through it all? or more accurate? our groupmates will need to understand.
        // bufferSize is how long of a section of the audio that is analyzed for its frequency at once.
        //  A higher bufferSize might gloss over some rapid changes in frequency, whereas a smaller buffer size
        //  will have more noise in the resulting spectrum and lower resolution in the resulting frequency bands
        private readonly int _bufferSize;

        // Sample rate is an artifact of how the audio file itself was recorded, referring to the number of samples/second
        private readonly int _sampleRate;
        // TODO: what is trimsilence for
        // Silence can be optionally trimmed out of the audio file
        private readonly bool _trimSilence;
        public AudioAnalyzer(string audioFile, int bufferSize, int sampleRate, bool trimSilence = false)
        {
            _audioFilePath = audioFile;
            _bufferSize = bufferSize;
            _sampleRate = sampleRate;
            _trimSilence = trimSilence;
        }

        /**
         * Get the corresponding frequency in Hz for the given frequency bin
         * The frequency bin refers to how the FFT works; when it completes it returns an array of "bins", each of which
         * correspond to a certain frequency and the numeric value in the "bin" corresponds to the intensity of that frequency. 
         * I call them bins since I think of them as analogous to bins we can place things in when doing a histogram of some sort
         */
        // TODO: what is the "frequency bin"? why did you even call it a bin what does that mean in this context. just a place to put stuff? that kind of bin?
        public int GetFrequencyForBin(int bin)
        {
            return bin * _sampleRate / _bufferSize;
        }

        // TODO: explain why we would need to get the frequency as a float vs why we would want it as an int.
        /**
         * Float version of the method provided if higher accuracy is needed
         */
        public float GetFrequencyForBin(float bin)
        {
            return bin * _sampleRate / _bufferSize;
        }

        /**
         * Get the size of the FFT results that will be returned from this analyzer
         */
        // TODO: what are "the arrays". arrays of frequencies? arrays of FFT results? why are you dividing it by 2?
        public int GetDataSize()
        {
            //divided by 2 becaust the FFT is symmetric on inputs of all real numbers; only half of the array carries all of the information
            return _bufferSize/2;
        }

        /*
         * Reads in the audio file specified by reader, taking in bufferSize number of samples in each chunk to be analyzed
         * Returns an enumerable composed of lists of the frequency bins
         */
        public IEnumerable<DataType[]> GetFrequencies()
        {
            
            var reader = new AudioFileReader(_audioFilePath);
            // TODO: When creating the AudioStreamReader, can you replace "_bufferSize / 2" with GetDataSize() 
            // Yes I could, but that would be obscuring the purpose behind that parameter. It would be just as valid
            //  To put bufferSize / 4 in its place
            return (new AudioStreamReader(reader, _bufferSize, _bufferSize / 2, _trimSilence)).Select((floats) =>
            {
                //Cast all of the floating point numbers to Complex numbers in preperation for the FFT
                // TODO: okay but why do we need Complex numbers for the FFT. I think youre forgetting they dont know all this stuff.
                //We need to cast to complex numbers here because this particular library forces us to
                //Technically, the FFT is an operation on complex numbers and returns complex numbers. but in this case
                // we're representing a real number as Complex data type so that the library will accept our data
                Complex[] complex = new Complex[_bufferSize];
                for (int i = 0; i < floats.Length; i++)
                {
                    complex[i] = new Complex(floats[i], 0);
                }
                return complex;
            }).Select((complex) =>
            {
                //Perform the FFT and throw away half of the resulting array; then cast to Datatype from Complex
                // TODO: A little context on what is actually happening here would help. 
                // https://en.wikipedia.org/wiki/Fast_Fourier_transform
                // https://upload.wikimedia.org/wikipedia/commons/5/50/Fourier_transform_time_and_frequency_domains.gif
                // The FFT is an algorith to perform a fourier transformation. this transformation effectively gives us
                //  the amplitudes of sin and cosine waves at specific frequencies that can be used to compose the input sample when added together
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

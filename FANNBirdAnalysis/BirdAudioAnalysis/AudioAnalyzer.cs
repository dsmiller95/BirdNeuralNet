using AForge.Math;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdAudioAnalysis
{
    /*
     * A class designed to read the data from an AudioFileReader and output it as floats into a FFT,
     * returning array of numbers representing the frequency amplitudes
     */
    class AudioAnalyzer
    {
        private string audioFile;
        private int bufferSize, sampleRate;
        public AudioAnalyzer(string audioFile, int bufferSize, int sampleRate)
        {
            this.audioFile = audioFile;
            this.bufferSize = bufferSize;
            this.sampleRate = sampleRate;
        }

        /*
         * Get the corresponding frequency for the given frequency bin
         */
        public int GetFrequencyForBin(int bin)
        {
            return bin * sampleRate / bufferSize;
        }

        public float GetFrequencyForBin(float bin)
        {
            return bin * sampleRate / bufferSize;
        }

        /*
         * Reads in the audio file specified by reader, taking in bufferSize number of samples in each chunk to be analyzed
         * Returns an enumerable composed of lists of the frequency bins
         */
        public IEnumerable<IEnumerable<double>> getFrequencies()
        {

            var reader = new AudioFileReader(audioFile);
            return (new AudioStreamReader(reader, bufferSize, bufferSize / 2)).Select((floats) =>
            {
                Complex[] complex = new Complex[bufferSize];
                for (int i = 0; i < floats.Length; i++)
                {
                    complex[i] = new Complex(floats[i], 0);
                }
                return complex;
            }).Select((complex) =>
            {
                FourierTransform.FFT(complex, FourierTransform.Direction.Forward);
                
                return complex.Take(complex.Length / 2).Select((comp) => Math.Abs(comp.Magnitude));
            });
        }
    }
}

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
	 * A class designed to read the data from an audio file and output an array 
	 * of floating points representing the frequency amplitudes; as a result from a FFT
	 */

    public class AudioAnalyzer
    {
        public const int DefaultSampleRate = 44100;

        //The path to the audio file
        private readonly string _audioFilePath;

        // bufferSize is how long of a section of the audio that is analyzed for its frequency at once.
        //  A higher bufferSize might gloss over some rapid changes in frequency, whereas a smaller buffer size
        //  will have more noise in the resulting spectrum and lower resolution in the resulting frequency bands
        private readonly int _bufferSize;

        // Sample rate is an artifact of how the audio file itself was recorded, referring to the number of samples/second
        private readonly int _sampleRate;

        public AudioAnalyzer(string audioFile, int bufferSize, int sampleRate = DefaultSampleRate)
        {
            _audioFilePath = audioFile;
            _bufferSize = bufferSize;
            _sampleRate = sampleRate;
        }

        /**
		 * Get the corresponding frequency in Hz for the given frequency bin
		 * The frequency bin refers to how the FFT works; when it completes it returns an array of "bins", each of which
		 * correspond to a certain frequency and the numeric value in the "bin" corresponds to the intensity of that frequency. 
		 * I call them bins since I think of them as analogous to bins we can place things in when doing a histogram of some sort
		 */

        public int GetFrequencyForBin(int bin)
        {
            return bin*_sampleRate/_bufferSize;
        }

        /**
		 * Float version of the method provided if higher accuracy is needed
		 */

        public float GetFrequencyForBin(float bin)
        {
            return bin*_sampleRate/_bufferSize;
        }

        /**
		 * Get the size of the FFT results that will be returned from this analyzer
		 */

        public int GetDataSize()
        {
            //divided by 2 becaust the FFT is symmetric on inputs of all real numbers; only half of the array carries all of the information
            return _bufferSize/2;
        }

        public WaveFormat GetWaveFormat()
        {
            var reader = new AudioFileReader(_audioFilePath);
            return reader.WaveFormat;
        }

        /*
		 * Reads in the audio file specified by reader, taking in bufferSize number of samples in each chunk to be analyzed
		 * Returns an enumerable composed of lists of the frequency bins
		 */

        public IEnumerable<Complex[]> GetFrequencies()
        {

            var reader = new AudioFileReader(_audioFilePath);
            //var mp3Reader = new Mp3FileReader(_audioFilePath);

            
            var stream = new AudioStreamReader(reader);

            var bufferedStream = new AudioStreamReader(reader).ChunkBuffer(_bufferSize);//.RollingBuffer(_bufferSize, _bufferSize/1);
            return FastFourierTransform(bufferedStream, true, _bufferSize);
        }

        /*public IEnumerable<IEnumerable<float[]>> GetTrainingFrequencies()
		{
			AudioSplitter audioSplitter = new AudioSplitter(1F);
			var reader = new AudioFileReader(_audioFilePath);
			var listBufferedStream = audioSplitter.SplitAudio(new AudioStreamReader(reader));//.RollingBuffer(_bufferSize, _bufferSize / 2);
			return audioSplitter.SplitAudio(new AudioStreamReader(reader)).Select((sample) =>
			{
				var bufferedStream = sample.RollingBuffer(_bufferSize, _bufferSize / 1);
				return FastFourierTransform(bufferedStream);
			});
		}*/

        public static IEnumerable<Complex[]> FastFourierTransform(IEnumerable<float[]> bufferedStream, bool forward, int bufferSize)
        {
            return (bufferedStream).Select((floats) =>
                {
                    //Cast all of the floating point numbers to Complex numbers in preperation for the FFT
                    //We need to cast to complex numbers here because this particular library forces us to
                    //Technically, the FFT is an operation on complex numbers and returns complex numbers. but in this case
                    // we're representing a real number as Complex data type so that the library will accept our data
                    Complex[] complex = new Complex[bufferSize];
                    for (int i = 0; i < floats.Length; i++)
                    {
                        complex[i] = new Complex(floats[i], 0);
                    }
                    return complex;
                }).Select((complex) =>
                    {
                        //Perform the FFT and throw away half of the resulting array; then cast to Datatype from Complex

                        // https://en.wikipedia.org/wiki/Fast_Fourier_transform
                        // https://upload.wikimedia.org/wikipedia/commons/5/50/Fourier_transform_time_and_frequency_domains.gif
                        // The FFT is an algorith to perform a fourier transformation. this transformation effectively gives us
                        //  the amplitudes of sin and cosine waves at specific frequencies that can be used to compose the input sample when added together
                        FourierTransform.FFT(complex, forward ? FourierTransform.Direction.Forward : FourierTransform.Direction.Backward);
                        
                        return complex
                            ////throw away half because the FFT is Symmetric when all inputs are real number
                            //.Take(complex.Length / 2)
                            //.Select((comp) => (float)Math.Abs(comp.Magnitude))
                            .ToArray();
                    });
        }

        public static IEnumerable<Complex[]> FastFourierTransform(IEnumerable<Complex[]> bufferedStream, bool forward, int bufferSize)
        {
            return (bufferedStream).Select((complex) =>
                    {
                        //Perform the FFT

                        // https://en.wikipedia.org/wiki/Fast_Fourier_transform
                        // https://upload.wikimedia.org/wikipedia/commons/5/50/Fourier_transform_time_and_frequency_domains.gif
                        // The FFT is an algorith to perform a fourier transformation. this transformation effectively gives us
                        //  the amplitudes of sin and cosine waves at specific frequencies that can be used to compose the input sample when added together
                        FourierTransform.FFT(complex, forward ? FourierTransform.Direction.Forward : FourierTransform.Direction.Backward);

                        return complex
                            //.Select((comp) => (float)Math.Abs(comp.Magnitude))
                            .ToArray();
                    });
        }
    }
}

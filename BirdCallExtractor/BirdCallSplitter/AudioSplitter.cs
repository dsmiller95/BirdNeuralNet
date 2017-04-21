using System;
using System.Linq;
using System.Collections.Generic;
using AForge.Math;


namespace BirdAudioAnalysis
{
	abstract class AudioSplitter : IAudioSplitter
	{
        protected IList<Complex[]> origBuffer;

        public AudioSplitter(IEnumerable<Complex[]> originalBuffer)
        {
            this.origBuffer = originalBuffer as IList<Complex[]> ?? originalBuffer.ToList();
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
            
            return result;
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

        public abstract bool CutHere(Complex[] sample);
	    public abstract bool IsSignalSample(Complex[] sample);
	}
}

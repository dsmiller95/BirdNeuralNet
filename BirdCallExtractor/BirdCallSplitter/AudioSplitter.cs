using System;
using System.Linq;
using System.Collections.Generic;
using AForge.Math;


namespace BirdAudioAnalysis
{
	abstract class AudioSplitter : IAudioSplitter
	{
        /// <summary>
        /// Take a buffer of audio FFT data, and filter it so that all that's left is bird calls
        /// </summary>
        /// <param name="originalBuffer">the original fourier transformed data</param>
        /// <returns>a 2D array containing all of the individual chunks of audio</returns>
        public IEnumerable<IEnumerable<Complex[]>> SplitAudio(IEnumerable<Complex[]> originalBuffer)
        {
            //prevent multiple enumerations of the input IEnumerable
            var origBuff = originalBuffer.ToList();
            Console.Out.WriteLine("count: " + origBuff.Count);
            
            var result = new List<IEnumerable<Complex[]>>();
            var enumerator = origBuff.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (IsSignalSample(enumerator.Current, origBuff))
                {
                    var tmp = ReadSignals(ref enumerator, origBuff);
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
        internal List<Complex[]> ReadSignals(ref List<Complex[]>.Enumerator enumerator, List<Complex[]> originalBuffer)
        {
            var result = new List<Complex[]>();

            do
            {
                if (CutHere(enumerator.Current, originalBuffer))
                {
                    return result;
                }
                result.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            return result;
        }

        public abstract bool CutHere(Complex[] sample, List<Complex[]> originalBuffer);
	    public abstract bool IsSignalSample(Complex[] sample, List<Complex[]> originalBuffer);
	}
}

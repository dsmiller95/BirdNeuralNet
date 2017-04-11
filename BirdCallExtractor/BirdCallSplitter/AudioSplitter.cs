using System;
using System.Linq;
using NAudio.Wave;
using System.IO;
using System.Collections.Generic;
using AForge.Math;


namespace BirdAudioAnalysis
{
	class AudioSplitter
	{
        public AudioSplitter()
        {
            
        }

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

            var avgSampleIntensity = GetAvgSampleIntensity(origBuff);
            Console.Out.WriteLine("avgIntensity: " + origBuff.Count);

            var result = new List<IEnumerable<Complex[]>>();
            var enumerator = origBuff.GetEnumerator();
            
            while (enumerator.MoveNext())
            {
                if (IsSignalSample(enumerator.Current, avgSampleIntensity))
                {
                    //Console.Out.WriteLine("len: " + origBuff.Count);
                    var tmp = ReadSignals(ref enumerator, avgSampleIntensity);
                    Console.Out.WriteLine("lenChunk: " + tmp.Count);
                    result.Add(tmp);
                }
            }// while (enumerator.MoveNex\t());
            
            
            
            return result;
		}

	    private List<Complex[]> ReadSignals(ref List<Complex[]>.Enumerator enumerator, double avgIntensity)
	    {
	        var result = new List<Complex[]>();
	        int currentNonPassing = 0;
	        do
	        {
                if (!IsSignalSample(enumerator.Current, avgIntensity))
                {
                    currentNonPassing += 1;
                    if (currentNonPassing >= 10)
                    {
                        return result;
                    }
                }
                else
                {
                    currentNonPassing = 0;
                }
                result.Add(enumerator.Current);
	        } while (enumerator.MoveNext());
	        return result;
	    }

	    public double GetAvgSampleIntensity(IEnumerable<Complex[]> file)
	    {
	        return file.Average((sample) => sample.Average((complex) => complex.Magnitude));
	    }

	    public bool IsSignalSample(Complex[] sample, double avgSampleIntensity)
	    {
	        var avgIntesity = sample.Average((value) => value.Magnitude);

	        return (avgIntesity > avgSampleIntensity);
	    }
	}
}

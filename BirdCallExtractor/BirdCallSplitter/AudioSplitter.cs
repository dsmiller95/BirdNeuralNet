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
	    private readonly int _chunkSize = 500;

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
		    var origBuff = originalBuffer.ToArray();
            yield return origBuff;
		}
	}
}

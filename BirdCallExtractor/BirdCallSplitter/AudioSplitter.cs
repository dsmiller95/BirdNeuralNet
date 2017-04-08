using System;
using System.Linq;
using NAudio.Wave;
using System.IO;
using System.Collections.Generic;


namespace BirdAudioAnalysis
{
	class AudioSplitter
	{
		private readonly int _chunkSize = AudioAnalyzer.DefaultSampleRate / 2; //1/2 of a second of samples at the default sample rate

        public AudioSplitter(float chunkSizeSeconds)
        {
            _chunkSize = (int)(AudioAnalyzer.DefaultSampleRate * chunkSizeSeconds);
        }
        public AudioSplitter(int chunkSize)
        {
            _chunkSize = chunkSize;
        }

        /*
		 * This method takes in an audio file and copies sections of the audio.
		 * The copied sections are the length of the chunkSize starting at randomly
		 * selected locations of the audio file. The method will create x number of 
		 * copied audio snippets, where x is how many times the chunkSize fits into
		 * the length of the audio file.
		 * Returns: a 2D array of floats containing the randomly copied audio
		 */
        public IEnumerable<IEnumerable<float>> SplitAudio(IEnumerable<float> originalBuffer)
		{
            //prevent multiple enumerations of the input IEnumerable
		    var origBuff = originalBuffer.ToList();
            Console.Out.Write(origBuff.Count + " ");

			var length = origBuff.Count;


            if (length <= _chunkSize)
            {
                //if the chunk size is bigger than the sample itself, we probably shouldn't split it up into smaller pieces
                yield return origBuff;
                yield break;
            }

            var numGenFiles = (length / _chunkSize) * 4 + 1;
            Console.Out.Write(numGenFiles + ": ");
            
            var random = new Random();
            for (int i = 0; i < numGenFiles; i++)
			{
                //try to keep the sizes of the samples consistent
				var randOffset = random.Next(length - _chunkSize);

				var tempBufferIEnumerable = origBuff.Skip(randOffset).Take(_chunkSize);
				//var tempBuffer = tempBufferIEnumerable.ToArray();

				yield return tempBufferIEnumerable;
			}
		}
	}
}

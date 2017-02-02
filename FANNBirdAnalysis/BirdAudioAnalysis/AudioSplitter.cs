using System;
using System.Linq;
using NAudio.Wave;
using System.IO;
using System.Collections.Generic;


namespace BirdAudioAnalysis
{
	class AudioSplitter
	{
		private long chunkSize = 90000; //approximately 1/3rd of a 1.5s audio file

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
			var length = originalBuffer.Count();
			var numGenFiles = (int)(length / chunkSize);

			for (int i = 0; i < numGenFiles; i++)
			{
				Random random = new Random();
				var randOffset = random.Next((int)length);

				var tempBufferIEnumerable = originalBuffer.Skip(randOffset).Take((int)(chunkSize));
				//var tempBuffer = tempBufferIEnumerable.ToArray();

				yield return tempBufferIEnumerable;
			}
		}
	}
}

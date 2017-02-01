using System;
using System.Linq;
using NAudio.Wave;
using System.IO;

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
		public float[][] SplitAudio(string audioFilePath)
		{
			var reader = new AudioFileReader(audioFilePath);
			var length = reader.Length;
			var numGenFiles = (int)(length / chunkSize);

			float[][] result = new float[numGenFiles + 1][];

			byte[] buffer = new byte[length];
			reader.Read(buffer, 0, buffer.Length);

			for (int i = 0; i < numGenFiles; i++)
			{
				Random random = new Random();
				var randOffset = random.Next((int)length);

				var tempBufferIenumerable = buffer.Skip(randOffset).Take((int)(chunkSize));
				var tempBuffer = tempBufferIenumerable.ToArray();

				float[] transferBuffer = new float[chunkSize];

				// Copy the newly read data to the transfer buffer and convert to floats
				for (int x = 0; x < randOffset; x++)
				{
					transferBuffer[x] = BitConverter.ToSingle(tempBuffer, x);
				}

				result[i] = transferBuffer;

				//This can be removed at some point.
				var tempLocation = "C:\\Users\\heinzer.AD\\Desktop\\" + i + ".wav";
				File.WriteAllBytes(tempLocation, tempBuffer);
			}

			return result;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace BirdAudioAnalysis
{
	public static class StreamBuffer
	{
		/// <summary>
		/// and provide a rolling window over the data.
		/// This will give a window of width ChunkSize and each next sample will be offset by Offset samples
		/// EX:
		/// ChunkSize of 4 and Offset of 2. The number |-#-| indicates a unique window of samples, numbered in order that they are returned
		/// Samples:
		///      |--2---||--4---||--6---|
		///  |---1--||--3---||--5---||--7---|
		///  * * * * * * * * * * * * * * * * *
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input">the Enumerable to be buffered</param>
		/// <param name="chunksize">Size of each buffer returned</param>
		/// <param name="offset">Number of samples skipped over between each buffer</param>
		/// <returns></returns>
		public static IEnumerable<T[]> RollingBuffer<T>(this IEnumerable<T> input, int chunksize, int offset)
		{
			//buffer to hold all the sample data points
			T[] mainBuffer = new T[chunksize];

			var enumer = input.GetEnumerator();

			//read in a full buffer of data to start and return it
			/*for (var i = 0; i < chunksize; i++)
			{
				if (enumer.MoveNext())
				{
					mainBuffer[i] = enumer.Current;
				}
				else
				{
					goto exitLoop;
				}
			}
			yield return mainBuffer;*/

			while (true)
			{
				//use a tmpBuffer to avoid overwriting the same data on the same pointer
				var tmpBuffer = new T[chunksize];
				//shift the buffer back by offset
				Array.Copy(mainBuffer, offset, tmpBuffer, 0, chunksize - offset);

				//read in a bunch of shit and put it into the main buffer
				for (var i = 0; i < offset; i++)
				{
					if (enumer.MoveNext())
					{
						tmpBuffer[chunksize - offset + i] = enumer.Current;
					}
					else
					{
						goto exitLoop;
					}
				}
				yield return tmpBuffer;
				mainBuffer = tmpBuffer;
			}
			exitLoop:

			yield break;
		}
	}
}
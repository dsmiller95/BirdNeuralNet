﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace BirdAudioAnalysis
{
	/**
	* This class will read in the raw floating point samples from an audio file, and return an IEnumberable representing all the floating point samples
	*/
	class AudioStreamReader : IEnumerable<float>
	{
		private readonly AudioFileReader _reader;


		public AudioStreamReader(AudioFileReader reader)
		{
			_reader = reader;
		}

		public IEnumerator<float> GetEnumerator()
		{
			//buffer to hold all the sample data points
			//float[] mainBuffer = new float[_chunksize];

			//buffer to read in <offset> # of samples from the raw byte stream
			//will be converted from bytes to floats
			byte[] buffer = new byte[4];
			while (_reader.Read(buffer, 0, buffer.Length) > 0)
			{
				float result = BitConverter.ToSingle(buffer, 0);
				yield return result;
			}
			yield break;
		}

		// This is necessary fully implement the IEnumerable interface
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
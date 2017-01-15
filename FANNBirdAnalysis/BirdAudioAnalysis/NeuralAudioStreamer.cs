using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if FANN_FIXED
using FANNCSharp.Fixed;
using DataType = System.Int32;
#elif FANN_DOUBLE
using FANNCSharp.Double;
using DataType = System.Double;
#else
using FANNCSharp.Float;
using DataType = System.Single;
#endif

namespace BirdAudioAnalysis
{

	/**
	 * Class built to stream an audio file through a neural network. Ideally, the network should have been trained first
	 */
	class NeuralAudioStreamer
	{

		private readonly AudioAnalyzer _analyzer;
		private string _fileName;
		private readonly NeuralNet _network;

		/// <summary>
		/// Initialize a new streamer
		/// </summary>
		/// <param name="network">The neural network to stream data through</param>
		/// <param name="fileName">The name of the audio file</param>
		/// <param name="bufferSize">The size in samples of the buffer. 4096 was chosen as default mostly arbitrarily, any other chosen number should be divisible by 2</param>
		/// <param name="sampleRate">The sample rate of the file. Default is 44100 because it is a common sampling rate and audacity exports in this format</param>
		public NeuralAudioStreamer(NeuralNet network, string fileName, int bufferSize = 4096, int sampleRate = 44100)
		{
			_fileName = fileName;
			_analyzer = new AudioAnalyzer(fileName, bufferSize, sampleRate);
			_network = network;
		}

		/**
		 * Returns a stream of results from the output layer of the neural network
		 */
		public IEnumerable<DataType[]> GetResultStream()
		{

			//The input length is used to determine the size of the rolling buffer that we will need when streaming data into the network
			var inputLength = _network.InputCount;
			//the frequencySize represents the size of each chunk of data we'll get from the analyzer, using this we can know how many more
			// elements that we'll have to fit into the rolling buffer each loop
			int frequencySize = _analyzer.GetDataSize();
			if(inputLength % frequencySize != 0)
			{
				//if the data chunks from the analyzer can't fit smoothly into the neural network inputs, we should probably not continue
				//This isn't a hard-and-fast rule, it could be changed. it just worked ok for me
				Console.WriteLine("Incompatable lengths!!");
				yield break;
			}

			//An array to hold all the data for the input layer of the neural network
			var rollingWindow = new DataType[inputLength];
			var frequencies = _analyzer.GetFrequencies();

			foreach (var current in frequencies)
			{
				/*
				 * Every loop,
				 *  take some more data from the analyzer
				 *  Shift over the current data to make room for the new data, and copy the new data in
				 *  Yeild the result of running the current data through the neural network
				 */

				DataType[] transferBuffer = new DataType[inputLength];
				//copy current values into the transfer buffer
				Array.Copy(current, 0, transferBuffer, 0, frequencySize);

				//copy the rest of the past data over into the transfer buffer at a higher index
				Array.Copy(rollingWindow, 0, transferBuffer, frequencySize, inputLength - frequencySize);

				rollingWindow = transferBuffer;
				
				//yield the output result of the neural network
				yield return _network.Run(rollingWindow);
			}
		}
	}
}

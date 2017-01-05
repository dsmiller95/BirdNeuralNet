using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: like before, maybe explain what these data types are and how they are chosen
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
        // TODO: why are these numbers the ones you chose and if someone is changing the neural network, which of these numbers should be changed?
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
            // TODO: why are you getting the input length and data size in this context and what are you doing with it
            var inputLength = _network.InputCount;
            int frequencySize = _analyzer.GetDataSize();
            if(inputLength % frequencySize != 0)
            {
                Console.WriteLine("Incompatable lengths!!");
                yield break;
            }

            //An array to hold all the data for the input layer of the neural network
            var rollingWindow = new DataType[inputLength];
            var frequencies = _analyzer.GetFrequencies();
            // TODO: for this whole thing you explain each line but not really why youre doing it. so include that.
            foreach (var current in frequencies)
            {
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

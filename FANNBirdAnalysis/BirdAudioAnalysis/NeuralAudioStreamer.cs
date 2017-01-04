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
     * Class built to stream an audio file through a neural network; after it has been trained ideally
     */
    class NeuralAudioStreamer
    {

        private readonly AudioAnalyzer _analyzer;
        private string _fileName;
        private readonly NeuralNet _network;

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
            foreach (var current in frequencies)
            {
                DataType[] transferBuffer = new DataType[inputLength];
                //copy current values into the transfer buffer
                Array.Copy(current, 0, transferBuffer, 0, frequencySize);

                //copy the rest of the past data over into the transfer buffer at a higher index
                Array.Copy(rollingWindow, 0, transferBuffer, frequencySize, inputLength - frequencySize);

                rollingWindow = transferBuffer;
                
                //yeild the output result of the neural network
                yield return _network.Run(rollingWindow);
            }
        }
    }
}

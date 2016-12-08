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
    class NeuralAudioStreamer
    {

        private AudioAnalyzer _analyzer;
        private string _fileName;
        private NeuralNet _network;

        public NeuralAudioStreamer(NeuralNet network, string fileName, int bufferSize = 4096, int sampleRate = 44100)
        {
            _fileName = fileName;
            _analyzer = new AudioAnalyzer(fileName, bufferSize, sampleRate);
            _network = network;
        }

        public IEnumerable<DataType[]> getResultStream()
        {
            var inputLength = _network.InputCount;
            int frequencySize = _analyzer.getDataSize();
            if(inputLength % frequencySize != 0)
            {
                Console.WriteLine("Incompatable lengths!!");
                yield break;
            }

            var rollingWindow = new DataType[inputLength];
            var frequencies = _analyzer.getFrequencies();
            foreach (var current in frequencies)
            {
                DataType[] transferBuffer = new DataType[inputLength];
                //copy values
                Array.Copy(current, 0, transferBuffer, 0, frequencySize);

                //shift over all the values
                Array.Copy(rollingWindow, 0, transferBuffer, frequencySize, inputLength - frequencySize);

                rollingWindow = transferBuffer;
                

                yield return _network.Run(rollingWindow);
            }
        }
    }
}

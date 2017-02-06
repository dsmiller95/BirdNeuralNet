using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


using FANNCSharp;

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
    class TrainingDataBuilder
    {
        private List<DataType[]> _inputs;
        private List<DataType[]> _expectedOutputs;
        

        public TrainingDataBuilder()
        {
            _inputs = new List<DataType[]>();
            _expectedOutputs = new List<DataType[]>();
        }

        /// <summary>
        /// Adds a training point. One set of inputs is expected to produce the given set of outputs
        /// </summary>
        /// <param name="inputs">The set of data that is input into the neural network</param>
        /// <param name="outputs">The set of data that is expected to be produced by the neural network given the input</param>
        public void AddPair(DataType[] inputs, DataType[] outputs)
        {
            _inputs.Add(inputs);
            _expectedOutputs.Add(outputs);
        }

        public TrainingData GetTrainingData()
        {
            var res = new TrainingData();
            res.SetTrainData(_inputs.ToArray(), _expectedOutputs.ToArray());
            return res;
        }

        public void PadInputsUpTo(int length)
        {
            PadAllWith0(_inputs, length);
        }

        public void PrintSize()
        {
            Console.WriteLine("Training data: ({0}x{1})", _inputs.Count, _inputs[0].Length);
            Console.WriteLine("Expected data: ({0}x{1})", _expectedOutputs.Count, _expectedOutputs[0].Length);
        }


        private void PadAllWith0<T>(List<T[]> array, int maxLength)
        {
            for (int i = 0; i < array.Count; i++)
            {
                var tmp = new T[maxLength];
                Array.Copy(array[i], tmp, array[i].Length);
                array[i] = tmp;
            }
        }
    }
}

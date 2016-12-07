using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using FANNCSharp;
using System.Reactive.Linq;

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
    class NeuralAudioTrainer
    {
        private AudioAnalyzer analyzer;
        private List<IEnumerable<double>> frequencyTrainingData;


        public NeuralAudioTrainer(AudioAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            frequencyTrainingData = this.analyzer.getFrequencies().ToList();
        }

        public void TrainTheNetwork()
        {
            const uint num_layers = 3;
            const uint num_neurons_hidden = 96;
            const float desired_error = 0.001F;

            /*using (NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, trainData.InputCount, num_neurons_hidden, trainData.OutputCount))
            {
                net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

                net.LearningMomentum = 0.5F;

                net.TrainOnData(trainData, 20000, 5000, desired_error);

                Console.WriteLine("MSE error on train data: {0}", net.TestData(trainData));
                Console.WriteLine("MSE error on test data: {0}", net.TestData(testData));
            }*/
        }
    }
}

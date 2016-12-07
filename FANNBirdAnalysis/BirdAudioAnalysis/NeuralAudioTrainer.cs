using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using FANNCSharp;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
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
        private AudioAnalyzer _analyzer;
        private List<IEnumerable<double>> frequencyTrainingData;


        private readonly string _rootFile;
        private int _numFiles;
        private const int DefaultBufferSize = 4096;

        public NeuralAudioTrainer(string rootFolder, int numFiles)
        {
            //this.analyzer = analyzer;
            _rootFile = rootFolder;
            _numFiles = numFiles;
            //frequencyTrainingData = this._analyzer.getFrequencies().ToList();
        }

        private AudioAnalyzer GetAnalyzerForFile(int file, int bufferSize = DefaultBufferSize)
        {
            file += 1;
            return new AudioAnalyzer(_rootFile + file.ToString("D2") + ".mp3", bufferSize, 44100);
        }

        public void TrainTheNetwork()
        {
            double[][] trainingData = new double[_numFiles][];
            for (int i = 0; i < _numFiles; i++)
            {
                Console.WriteLine("\nAnalyzing file {0}", i);
                var analyzer = GetAnalyzerForFile(i);
                var theData = analyzer.getFrequencies().ToArray();
                int sampleWindow = theData.Length;
                int dataSize = analyzer.getDataSize();
                trainingData[i] = new double[sampleWindow * dataSize];
                for (int j = 0; j < sampleWindow; j++)
                {
                    Array.Copy(theData[j], 0, trainingData[i], j*dataSize, dataSize);
                }



                /*analyzer.getFrequencies().ToObservable()
                    .Subscribe((frequencies) =>
                    {
                        double max = -10000;
                        int maxloc = -1;
                        int j = 0;
                        frequencies.ToList().ForEach((magnitude) =>
                        {
                            if (magnitude > max)
                            {
                                max = magnitude;
                                maxloc = j;
                            }
                            j++;
                        });
                        Console.WriteLine("Maximum frequency of {0} with amplitute {1}; bin {2}", analyzer.GetFrequencyForBin(maxloc), max, maxloc);
                    });*/
            }

            Console.WriteLine("Training data: ({0}x{1})", trainingData.Length, trainingData[0].Length);
            /*const uint num_layers = 3;
            const uint num_neurons_hidden = 96;
            const float desired_error = 0.001F;*/

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

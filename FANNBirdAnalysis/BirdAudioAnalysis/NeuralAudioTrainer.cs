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


        private readonly string[] _rootFolders;
        private int _numFiles;
        private int _defaultBufferSize;
        private bool trimSilence;

        public NeuralAudioTrainer(string[] rootFolders, int numFiles, int bufferSize = 4096, bool trimSilence = false)
        {
            //this.analyzer = analyzer;
            _rootFolders = rootFolders;
            _numFiles = numFiles;
            _defaultBufferSize = bufferSize;
            this.trimSilence = trimSilence;
        }

        private AudioAnalyzer GetAnalyzerForFile(int dataset, int file, int bufferSize)
        {
            file += 1;
            return new AudioAnalyzer(_rootFolders[dataset] + file.ToString("D2") + ".mp3", bufferSize, 44100, trimSilence);
        }

        private DataType[] GetExpectedResultForDataset(int dataset)
        {
            var result = new DataType[_rootFolders.Length];
            for(int i = 0; i < result.Length; i++)
            {
                result[i] = (float) -0.01;
            }
            result[dataset] = 1;
            return result;
        }

        private TrainingData training;
        private TrainingData testing;

        private void GetTrainingData(int numToTrain, int numToTest)
        {

            int trainingIndex = 0;
            DataType[][] trainingData = new DataType[numToTrain * _rootFolders.Length][];
            DataType[][] trainingResultsExpected = new DataType[numToTrain * _rootFolders.Length][];

            int testingIndex = 0;
            DataType[][] testingData = new DataType[numToTest * _rootFolders.Length][];
            DataType[][] testingResultsExpected = new DataType[numToTest * _rootFolders.Length][];

            //maximum length out of all the samples; so that the rest can be padded with 0 to match
            int maxLength = 0;
            for (int dataset = 0; dataset < _rootFolders.Length; dataset++)
            {
                for (int file = 0; file < _numFiles; file++)
                {
                    Console.WriteLine("\nAnalyzing file {0}", file);
                    var analyzer = GetAnalyzerForFile(dataset, file, _defaultBufferSize);
                    var theData = analyzer.getFrequencies().ToArray();
                    int sampleWindow = theData.Length;
                    int dataSize = analyzer.getDataSize();

                    if (sampleWindow * dataSize > maxLength)
                    {
                        maxLength = sampleWindow * dataSize;
                    }

                    if (file < numToTrain)
                    {
                        //collect training data
                        trainingData[trainingIndex] = new DataType[sampleWindow * dataSize];
                        for (int j = 0; j < sampleWindow; j++)
                        {
                            Array.Copy(theData[j], 0, trainingData[trainingIndex], j * dataSize, dataSize);
                        }
                        trainingResultsExpected[trainingIndex] = GetExpectedResultForDataset(dataset);
                        trainingIndex++;
                    }
                    else
                    {
                        //collect testing data
                        testingData[testingIndex] = new DataType[sampleWindow * dataSize];
                        for (int j = 0; j < sampleWindow; j++)
                        {
                            Array.Copy(theData[j], 0, testingData[testingIndex], j * dataSize, dataSize);
                        }
                        testingResultsExpected[testingIndex] = GetExpectedResultForDataset(dataset);
                        testingIndex++;
                    }
                }
            }

            this.PadAllWith0(trainingData, maxLength);
            //this.PadAllWith0(trainingResultsExpected, maxLength);
            this.PadAllWith0(testingData, maxLength);
            //this.PadAllWith0(testingResultsExpected, maxLength);


            Console.WriteLine("Training data: ({0}x{1})", trainingData.Length, trainingData[0].Length);
            Console.WriteLine("Expected data: ({0}x{1})", trainingResultsExpected.Length, trainingResultsExpected[0].Length);
            Console.WriteLine("Testing data: ({0}x{1})", testingData.Length, testingData[0].Length);
            Console.WriteLine("Expected data: ({0}x{1})", testingResultsExpected.Length, testingResultsExpected[0].Length);

            

            training = new TrainingData();
            training.SetTrainData(trainingData, trainingResultsExpected);

            testing = new TrainingData();
            testing.SetTrainData(testingData, testingResultsExpected);
        }

        public NeuralNet TrainTheNetwork()
        {
            int numToTest = 3;
            int numToTrain = _numFiles - numToTest;
            GetTrainingData(numToTrain, numToTest);
            

            const uint num_layers = 3;
            uint num_input = training.InputCount;
            const uint num_neurons_hidden = 100;
            uint num_output = training.OutputCount;
            const float desired_error = 0.001F;

            NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, num_input, num_neurons_hidden, num_output);
            /*net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

            net.LearningMomentum = 0.5F;*/
            Console.WriteLine("MSE error on train data: {0}", net.TestData(training));
            Console.WriteLine("MSE error on test data:  {0}", net.TestData(testing));
            
            net.TrainOnData(training, 20000, 5, desired_error);

            Console.WriteLine("MSE error on train data: {0}", net.TestData(training));
            Console.WriteLine("MSE error on test data:  {0}", net.TestData(testing));

            var testData = testing.Input;
            foreach(var testSet in testData)
            {
                var result = net.Run(testSet);
                Console.WriteLine(string.Join(", ", result));
            }


            return net;
        }

        private void PadAllWith0<T>(T[][] array, int maxLength)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var tmp = new T[maxLength];
                Array.Copy(array[i], tmp, array[i].Length);
                array[i] = tmp;
            }
        }
    }
}

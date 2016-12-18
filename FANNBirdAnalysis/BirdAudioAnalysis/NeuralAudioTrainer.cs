using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    class NeuralAudioTrainer
    {
        private readonly string[] _rootFolders;
        private readonly int _numFiles;
        private readonly int _defaultBufferSize;
        private readonly bool _trimSilence;

        public NeuralAudioTrainer(string[] rootFolders, int numFiles, int bufferSize = 4096, bool trimSilence = false)
        {
            _rootFolders = rootFolders;
            _numFiles = numFiles;
            _defaultBufferSize = bufferSize;
            _trimSilence = trimSilence;
        }


        private AudioAnalyzer GetAnalyzerForFile(int dataset, int file, int bufferSize)
        {
            file += 1;
            return new AudioAnalyzer(_rootFolders[dataset] + file.ToString("D2") + ".mp3", bufferSize, 44100, _trimSilence);
        }

        /**
         * Get a set of what we should expect the neural network to output for a specific data classification.
         * All array entries except the target classification will be -0.01; the target will be 1
         */
        private DataType[] GetExpectedResultForDataset(int dataset)
        {
            var result = new DataType[_rootFolders.Length];
            for(int i = 0; i < result.Length; i++)
            {
                result[i] = -0.01F;
            }
            result[dataset] = 1;
            return result;
        }


        private TrainingData _training;
        private TrainingData _testing;

        private void GetTrainingData(int numToTrain, int numToTest)
        {

            int trainingIndex = 0;
            DataType[][] trainingData = new DataType[numToTrain * _rootFolders.Length][];
            DataType[][] trainingResultsExpected = new DataType[numToTrain * _rootFolders.Length][];

            int testingIndex = 0;
            DataType[][] testingData = new DataType[numToTest * _rootFolders.Length][];
            DataType[][] testingResultsExpected = new DataType[numToTest * _rootFolders.Length][];

            //maximum length out of all the samples; so that the rest can be padded with 0 to match the same size
            int maxLength = 0;
            for (int dataset = 0; dataset < _rootFolders.Length; dataset++)
            {
                for (int file = 0; file < _numFiles; file++)
                {
                    Console.WriteLine("\nAnalyzing file {0}", file);

                    var analyzer = GetAnalyzerForFile(dataset, file, _defaultBufferSize);
                    var theData = analyzer.GetFrequencies().ToArray();

                    int sampleWindow = theData.Length;
                    int dataSize = analyzer.GetDataSize();

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

            //pad the 2D arrays with 0's to make them all at least maxLength in length
            //ensures the arrays are rectangular and not jagged
            PadAllWith0(trainingData, maxLength);
            PadAllWith0(testingData, maxLength);


            Console.WriteLine("Training data: ({0}x{1})", trainingData.Length, trainingData[0].Length);
            Console.WriteLine("Expected data: ({0}x{1})", trainingResultsExpected.Length, trainingResultsExpected[0].Length);
            Console.WriteLine("Testing data: ({0}x{1})", testingData.Length, testingData[0].Length);
            Console.WriteLine("Expected data: ({0}x{1})", testingResultsExpected.Length, testingResultsExpected[0].Length);

            
            //create the actual training and testing data sets
            _training = new TrainingData();
            _training.SetTrainData(trainingData, trainingResultsExpected);

            _testing = new TrainingData();
            _testing.SetTrainData(testingData, testingResultsExpected);
        }

        /**
         * Get a neural network that has been trained on the previously specified datasets
         */
        public NeuralNet TrainTheNetwork()
        {
            int numToTest = 3;
            int numToTrain = _numFiles - numToTest;
            GetTrainingData(numToTrain, numToTest);
            

            const uint numLayers = 3;
            uint numInput = _training.InputCount;
            const uint numNeuronsHidden = 100;
            uint numOutput = _training.OutputCount;
            const float desiredError = 0.001F;

            NeuralNet net = new NeuralNet(NetworkType.LAYER, numLayers, numInput, numNeuronsHidden, numOutput);
            /*net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

            net.LearningMomentum = 0.5F;*/

            Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
            Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));
            
            net.TrainOnData(_training, 20000, 5, desiredError);

            Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
            Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));

            //output the raw results from the test data
            var testData = _testing.Input;
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

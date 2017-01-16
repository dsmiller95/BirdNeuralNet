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
		//An array of folder roots, each root folder containing its own data set parallel to the others
		private readonly string[] _rootFolders;

		//The number of files in every data set. Note: all data sets have the same number of files like this
		private readonly int _numFiles;

		//The default size of the audio sampling, as set in the constructor
		private readonly int _defaultBufferSize;

		//Whether or not silence should be trimmed from the input audio files in the audio file reader
		private readonly bool _trimSilence;

		public NeuralAudioTrainer(string[] rootFolders, int numFiles, int bufferSize = 4096, bool trimSilence = false)
		{
			_rootFolders = rootFolders;
			_numFiles = numFiles;
			_defaultBufferSize = bufferSize;
			_trimSilence = trimSilence;
		}   

		/// <summary>
		/// Get an audio analyzer for one file
		/// </summary>
		/// <param name="dataset">the number (index) of the dataset to get the file from</param>
		/// <param name="file">the number or index of the file within the dataset to grab</param>
		/// <param name="bufferSize">the size of the buffer to use in the audio analyzer</param>
		/// <returns></returns>
		private AudioAnalyzer GetAnalyzerForFile(int dataset, int file, int bufferSize)
		{
			//Add one to file name so that they can start numbering at 1
			file += 1;
			return new AudioAnalyzer(_rootFolders[dataset] + file.ToString("D2") + ".mp3", bufferSize, 44100, _trimSilence);
		}

		/**
		 * Get a set of what we should expect the neural network to output for a specific data classification.
		 * All array entries except the target classification will be -0.01; the target will be 1 to represent true
		 * The outcomes that are expected to be false are set to -0.01 to create a higher motivation for the
		 *  neural network trainer to cause those specific outputs to be low-valued. When they are set to 0,
		 *  the trainer doesn't always seem to care if they entries are actually that close to 0
		 */
		private DataType[] GetExpectedResultForDataset(int dataset)
		{
			var result = new DataType[_rootFolders.Length + 1];
			for(int i = 0; i < result.Length; i++)
			{
				result[i] = -0.01F;
			}
			result[dataset] = 1;
			return result;
		}


		private TrainingData _training;
		private TrainingData _testing;

		/// <summary>
		/// Get the training data for 
		/// </summary>
		/// <param name="numToTrain">the number of data points to use as training data</param>
		/// <param name="numToTest">the number of data points to use as testing data</param>
		private void GetTrainingData(int numToTrain, int numToTest)
		{
			//Training data is seperated from testing data in order to prevent overfitting
			// Overfitting is when the neural network fits all of our data we train it on, but in doing so
			// misses the mark on other similar cases because of too much specific optimization
			// So, we keep some of the data seperate to test the network on when we're done. if it trained well,
			// then the network will perform on the test data just about as well as it does on the training data
			// if it overfit the training data, then it will perform much worse on the testing data than on the training

			// These arrays hold all of the training and testing data
			// They are sized to how many data points they need to hold. The training data takes
			//  <numToTrain> data points from each data set (Each data set is represented by one entry in _rootFolders)
			//  And the same is done for the testing data
			int trainingIndex = 0;
			DataType[][] trainingData = new DataType[numToTrain * _rootFolders.Length + 1][];
			DataType[][] trainingResultsExpected = new DataType[numToTrain * _rootFolders.Length + 1][];

		    trainingData[trainingData.Length - 1] = new DataType[] {0};
		    trainingResultsExpected[trainingResultsExpected.Length - 1] = GetExpectedResultForDataset(_rootFolders.Length);
            

			int testingIndex = 0;
			DataType[][] testingData = new DataType[numToTest * _rootFolders.Length][];
			DataType[][] testingResultsExpected = new DataType[numToTest * _rootFolders.Length][];

			//maximum length out of all the samples; so that the rest can be padded with 0 to match the same size
			//The samples need to be of the same length
			int maxLength = 0;
			for (int dataset = 0; dataset < _rootFolders.Length; dataset++)
			{
				for (int file = 0; file < _numFiles; file++)
				{
					Console.WriteLine("\nAnalyzing file {0}", file);

					var analyzer = GetAnalyzerForFile(dataset, file, _defaultBufferSize);
					var frequencies = analyzer.GetFrequencies().ToArray();

					int sampleWindow = frequencies.Length;
					int dataSize = analyzer.GetDataSize();

					//find the max length of any of the samples
					if (sampleWindow * dataSize > maxLength)
					{
						maxLength = sampleWindow * dataSize;
					}

					//if our file counter is less than the number of data points we want in our training data set
					if (file < numToTrain)
					{
						//collect training data
						trainingData[trainingIndex] = new DataType[sampleWindow * dataSize];
						for (int j = 0; j < sampleWindow; j++)
						{
							Array.Copy(frequencies[j], 0, trainingData[trainingIndex], j * dataSize, dataSize);
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
							Array.Copy(frequencies[j], 0, testingData[testingIndex], j * dataSize, dataSize);
						}
						testingResultsExpected[testingIndex] = GetExpectedResultForDataset(dataset);
						testingIndex++;
					}
				}
			}

			//pad the 2D arrays with 0's to make them all at least maxLength in length
			//ensures the arrays are rectangular and not jagged
			//This is necessary because the neural network is currently configured to take in a fixed length of data
			// and it will throw an error if it gets anything else. There is probably a better way to get samples of all
			// the same size, but this was the easiest at the time.
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


		/// <summary>
		/// Get a neural network that has been trained on the previously specified datasets
		/// </summary>
		/// <param name="testCases">The number of data points that should be used as test cases rather than training cases</param>
		/// <returns></returns>
		public NeuralNet TrainTheNetwork(int testCases)
		{
			// How many data points out of each data set to use for testing
			int numToTest = testCases;
			int numToTrain = _numFiles - numToTest;
			GetTrainingData(numToTrain, numToTest);
			
			//Neural network setup
			//the number of layers of neurons in the neural network
			const uint numLayers = 3;
			//The number of input neurons the neural network (how many data points it can process all at once)
			uint numInput = _training.InputCount;
			//The number of hidden nuerons. these are in the hidden layer. 100 was picked arbitrarily, with this implementation it could
			// conceivably be as high as 10000. but 100 worked for this test application
			const uint numNeuronsHidden = 200;
			const uint numNeuronsHidden2 = 50;
			//The number of output neurons. Each output neuron represents one classification
			// I.E. if you wanted to tell the difference between a robin, a sparrow, and a humingbird, you'd have 3 output neurons
			uint numOutput = _training.OutputCount;
			//The amount of MSE error looked for in the network, the network will stop training once it reaches this amount of error or lower
			

			Console.WriteLine("Creating neural network with: \nNumber of Layers: {0} \nNumber of Inputs: {1} \n Number of Outputs: {2} \nNumber of Hidden Neurons: {3}", numLayers, numInput, numOutput, numNeuronsHidden);

			NeuralNet net = new NeuralNet(NetworkType.LAYER, numLayers, numInput, numNeuronsHidden, numOutput);
			//NeuralNet net = new NeuralNet("birdneuralnet.net");

			net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

            float desiredError = 0.1F;
            net.LearningMomentum = 0.7F;

			Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
			Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));
			
			net.TrainOnData(_training, 20000, 5, desiredError);

			Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
			Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));

            desiredError = 0.01F;
            net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_QUICKPROP;
            net.TrainOnData(_training, 5000, 5, desiredError);

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

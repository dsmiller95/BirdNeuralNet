using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


using FANNCSharp;
using System.IO;

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

		//The target level or error on the final neural network
		private readonly float _finalError;


        private TrainingData _training;
        private TrainingData _testing;

        public NeuralAudioTrainer(string[] rootFolders, int numFiles, int bufferSize = 4096, bool trimSilence = false, float desiredError = 0.01F)
		{
			_rootFolders = rootFolders;
			_numFiles = numFiles;
			_defaultBufferSize = bufferSize;
			_trimSilence = trimSilence;
			_finalError = desiredError;
		}

	    private void GetTrainingData(float percentTest)
	    {
            //First, get a full 2D list of all files to be processed
            // This array does not have to be rectangular; that is, each data set
            //  does not have to have the same number of samples
	        var allFiles = new List<string[]>();

	        foreach (var folder in _rootFolders)
	        {
	            /*var files = new string[_numFiles];
	            for (int i = 0; i < _numFiles; i++)
	            {
	                files[i] = folder + "\\" + (i + 1).ToString("D2") + ".mp3";
	            }
                allFiles.Add(files);*/
                allFiles.Add(Directory.GetFiles(folder));
	        }

            var processor = new TrainingFileProcessor(allFiles.ToArray(), percentTest, bufferSize: _defaultBufferSize);
	        _training = processor.GetTrainingData();
	        _testing = processor.GetTestingData();
	    }


		/// <summary>
		/// Get a neural network that has been trained on the previously specified datasets
		/// </summary>
		/// <param name="percentTests">The percent of data points that should be used as test cases rather than training cases</param>
		/// <returns></returns>
		public NeuralNet TrainTheNetwork(float percentTests)
		{
			// How many data points out of each data set to use for testing
			//int numToTest = testCases;
			//int numToTrain = _numFiles - numToTest;
			GetTrainingData(percentTests);
			
			//Neural network setup
			//the number of layers of neurons in the neural network
			const uint numLayers = 3;
			//The number of input neurons the neural network (how many data points it can process all at once)
			uint numInput = _training.InputCount;
			//The number of hidden nuerons. these are in the hidden layer. 100 was picked arbitrarily, with this implementation it could
			// conceivably be as high as 10000. but 100 worked for this test application
			const uint numNeuronsHidden = 50;
			const uint numNeuronsHidden2 = 50;
			//The number of output neurons. Each output neuron represents one classification
			// I.E. if you wanted to tell the difference between a robin, a sparrow, and a humingbird, you'd have 3 output neurons
			uint numOutput = _training.OutputCount;
			//The amount of MSE error looked for in the network, the network will stop training once it reaches this amount of error or lower
			

			Console.WriteLine("Creating neural network with: \nNumber of Layers: {0} \nNumber of Inputs: {1} \n Number of Outputs: {2} \nNumber of Hidden Neurons: {3}", numLayers, numInput, numOutput, numNeuronsHidden);

			var net = new NeuralNet(NetworkType.LAYER, numLayers, numInput, numNeuronsHidden, numOutput);
			//NeuralNet net = new NeuralNet("birdneuralnet.net");

			net.TrainingAlgorithm = TrainingAlgorithm.TRAIN_INCREMENTAL;

            float desiredError = 0.03F;
			net.LearningMomentum = 0.7F;
            net.TrainOnData(_training, 10, 5, 0.0001F);

            Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
			Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));
			
			net.TrainOnData(_training, 20000, 5, desiredError);

			Console.WriteLine("MSE error on train data: {0}", net.TestData(_training));
			Console.WriteLine("MSE error on test data:  {0}", net.TestData(_testing));

			desiredError = _finalError;
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

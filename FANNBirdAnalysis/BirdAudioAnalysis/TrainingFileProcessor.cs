using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    class TrainingFileProcessor
    {
        private string[][] _files;
        private float _percentTest;
        private int _bufferSize;

        private TrainingDataBuilder _training, _testing;

        public TrainingFileProcessor(string[][] files, float percentToTest, int bufferSize = 4096)
        {
            _files = files;
            _percentTest = percentToTest;
            _bufferSize = bufferSize;

            ProcessFiles();
        }


        /// <summary>
        /// Get the training data for 
        /// </summary>
        private void ProcessFiles()
        {
            //Training data is seperated from testing data in order to prevent overfitting
            // Overfitting is when the neural network fits all of our data we train it on, but in doing so
            // misses the mark on other similar cases because of too much specific optimization
            // So, we keep some of the data seperate to test the network on when we're done. if it trained well,
            // then the network will perform on the test data just about as well as it does on the training data
            // if it overfit the training data, then it will perform much worse on the testing data than on the training
            
            _training = new TrainingDataBuilder();

            _testing = new TrainingDataBuilder();

            //maximum length out of all the samples; so that the rest can be padded with 0 to match the same size
            //The samples need to be of the same length
            int maxLength = 0;

            for (int datasetNum = 0; datasetNum < _files.Length; datasetNum++)
            {
                for (int fileNum = 0; fileNum < _files[datasetNum].Length; fileNum++)
                {
                    Console.WriteLine("\nAnalyzing file {0}", fileNum);

                    var analyzer = new AudioAnalyzer(_files[datasetNum][fileNum], _bufferSize, 44100);
                    var frequencies = analyzer.GetFrequencies().ToArray();

                    int sampleWindow = frequencies.Length;
                    int dataSize = analyzer.GetDataSize();


                    var aggregateFrequencies = new DataType[sampleWindow * dataSize];
                    for (int j = 0; j < sampleWindow; j++)
                    {
                        Array.Copy(frequencies[j], 0, aggregateFrequencies, j * dataSize, dataSize);
                    }

                    //find the max length of all of the samples
                    if (aggregateFrequencies.Length > maxLength)
                    {
                        maxLength = aggregateFrequencies.Length;
                    }

                    //if our file counter is less than the number of data points we want in our training data set
                    if (fileNum < _percentTest * _files[datasetNum].Length)
                    {
                        //collect testing data
                        _testing.AddPair(aggregateFrequencies, GetExpectedResultForIndex(datasetNum));
                    }
                    else
                    {
                        //collect training data
                        _training.AddPair(aggregateFrequencies, GetExpectedResultForIndex(datasetNum));
                    }
                }
            }

            //Add in a all-padding item to train on, to help prevent audio tracks with more or less
            //  padding due to length bing identified based on length
            _training.AddPair(new DataType[] { 0 }, GetExpectedResultForIndex(_files.Length));

            //pad the 2D arrays with 0's to make them all at least maxLength in length
            //ensures the arrays are rectangular and not jagged
            //This is necessary because the neural network is currently configured to take in a fixed length of data
            // and it will throw an error if it gets anything else. There is probably a better way to get samples of all
            // the same size, but this was the easiest at the time.
            _training.PadInputsUpTo(maxLength);
            _testing.PadInputsUpTo(maxLength);

            _training.PrintSize();
            _testing.PrintSize();
        }
        

        /**
		 * Get a set of what we should expect the neural network to output for a specific data classification.
		 * All array entries except the target classification will be -0.01; the target will be 1 to represent true
		 * The outcomes that are expected to be false are set to -0.01 to create a higher motivation for the
		 *  neural network trainer to cause those specific outputs to be low-valued. When they are set to 0,
		 *  the trainer doesn't always seem to care if they entries are actually that close to 0
		 */
        private DataType[] GetExpectedResultForIndex(int dataset)
        {
            var result = new DataType[_files.Length + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = -0.01F;
            }
            result[dataset] = 1;
            return result;
        }

        public TrainingData GetTestingData()
        {
            return _testing.GetTrainingData();
        }
        public TrainingData GetTrainingData()
        {
            return _training.GetTrainingData();
        }


    }
}

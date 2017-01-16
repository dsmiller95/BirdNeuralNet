using System;
using System.Collections.Generic;
using System.Linq;


//This statement is used to allow for compilation of the code using a 
// integer, double, or float neural network. that means that the FANN network
// would use this data type for all of its computations. this could be useful for 
// testing how the performace varies when the compilation flags are changed.
//Currently it should be configured to compile with DataType resolving to System.Single; a Float
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

	class NeuralAudioTest
	{
		private static void Main(string[] args)
		{
			#region
			//Set up arrays to hold the paths to the folders which contain all the training data sets
			/*string toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\Samples\\";
			//This is the array used to point to each sample set
			string[] fileSetRoots =
			{
				toneRoot + "Sin\\440Hz\\",
				toneRoot + "Square\\440Hz\\",
				toneRoot + "Sawtooth\\440Hz\\"
			};
			
			//File paths for continuous tones to do a rough test against
			toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\";
			string[] streamingRoots = { toneRoot + "440Hz_Sine_Noise.mp3", toneRoot + "440Hz_Square_Noise.mp3", toneRoot + "440Hz_Sawtooth_Noise.mp3" };
			 */
			#endregion

			//Set up arrays to hold the paths to the folders which contain all the training data sets
			string toneRoot = "..\\..\\..\\DataSets\\Audio\\Birds\\";
			//This is the array used to point to each sample set
			string[] fileSetRoots =
			{
				toneRoot + "BlackCappedChickadee\\",
				toneRoot + "AmericanCrow\\"
				//toneRoot + "WildTurkey\\"
			};

			//get our trained neural network!
			var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10, trimSilence: false);
			var network = audioTrainer.TrainTheNetwork(7);

			Console.WriteLine("Loading files to test against");

			toneRoot = "..\\..\\..\\DataSets\\Audio\\Birds\\";
			string[] streamingRoots = { toneRoot + "chickadee17.mp3", toneRoot + "crow13.mp3"}; //, toneRoot + "wildturkey12.mp3" 

			for (var i = 0; i < streamingRoots.Length; i++)
			{
			    var avgResult = new float[3];

				Console.WriteLine("Testing against Stream {0}", i);

				var filename = streamingRoots[i].Split('\\');
				Console.WriteLine("File tested: {0}", filename[6]);

				//stream the audio data through the neural network, and print out the first 50 results
				var stream = new NeuralAudioStreamer(network, streamingRoots[i]);
			    var len = stream.GetResultStream().Take(50).Select((results) =>
			        {
						for (var j = 0; j < results.Length; j++)
						{
							Console.Write("{0:0.000}  ", results[j]);
						    avgResult[j] += results[j];
						}
			            Console.WriteLine("");
			            return 0;
			        }).ToList().Count;

				Console.WriteLine("The guess is for audio file {0} is: {1} chickadee, {2} crow", filename[6], (avgResult[0] / len), (avgResult[1] / len), (avgResult[2] / len));
			}

			//Console.WriteLine("\nPrinting Network Connections");
			//network.PrintConnections();

			Console.WriteLine("Press the any key to exit");
			Console.ReadKey();

			//save network
			//network.Save("birdneuralnet.net");
			//destroy the network?
			network.Dispose();
		}
	}
}

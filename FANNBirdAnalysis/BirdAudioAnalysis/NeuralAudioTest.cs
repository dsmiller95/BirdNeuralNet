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
				toneRoot + "AmericanCrow\\",
				toneRoot + "WildTurkey\\"
			};

		    int buffersize = 1024;

			//get our trained neural network!
			var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10, trimSilence: false, bufferSize: buffersize, desiredError: 0.02F);
			var network = audioTrainer.TrainTheNetwork(0.3F);

			Console.WriteLine("Loading files to test against");

			toneRoot = "..\\..\\..\\DataSets\\Audio\\Birds\\";
			string[] streamingRoots = { toneRoot + "chickadee17.mp3", toneRoot + "crow13.mp3", toneRoot + "wildturkey12.mp3" };

		    string printableResults = String.Format("{0, 16} {1, 10}\t {2, 5}\t {3, 5}\t {4, 5}\n", "File", "Chickadee", "Crow", "Turkey", "None");

			for (var i = 0; i < streamingRoots.Length; i++)
			{
			    //var avgResult = new float[3];
				var avgResult = new float[4]; 
				Console.WriteLine("Testing against Stream {0}", i);

				var filename = streamingRoots[i].Split('\\');
				Console.WriteLine("File tested: {0}", filename[6]);

				//stream the audio data through the neural network, and print out the first 50 results

				var stream = new NeuralAudioStreamer(network, streamingRoots[i], bufferSize: buffersize);
			    var len = stream.GetResultStream().Select((results) =>
			        {
						for (var j = 0; j < results.Length; j++)
						{
							Console.Write("{0:0.000}  ", results[j]);
						    avgResult[j] += results[j];
						}
			            Console.WriteLine("");
			            return 0;
			        }).ToList().Count;
				Console.WriteLine("Length: " + len);
                //Console.WriteLine("The guess is for audio file {0} is: {1} chickadee, {2} crow, none: {3}", filename[6], (avgResult[0] / len), (avgResult[1] / len), (avgResult[2] / len));

                printableResults += string.Format("{0, 16} {1, 10:0.000}\t {2, 5:0.000}\t {3, 5:0.000}\t{4, 5:0.000}\n", filename[6], (avgResult[0]/len), (avgResult[1]/len), (avgResult[2]/len), (avgResult[3]/len));
				Console.WriteLine("The guess is for audio file {0} is: {1} chickadee, {2} crow, {3} wild turkey, {4} none", filename[6], (avgResult[0] / len), (avgResult[1] / len), (avgResult[2] / len), (avgResult[3] / len));
			}
		    Console.WriteLine(printableResults);
			Console.WriteLine("Press the any key to exit");
			var key = Console.ReadKey();

		    if (key.KeyChar == 's')
		    {
                //save network
                network.Save("birdneuralnet.net");
            }
			//free up the used memory
			network.Dispose();
		}
	}
}

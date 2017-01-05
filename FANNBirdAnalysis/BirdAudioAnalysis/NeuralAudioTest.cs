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
    // TODO: this is a really vague name for this file. can we change this to make more sense?
    class NeuralAudioTest
    {
        private static void Main(string[] args)
        {
            
            //Set up arrays to hold the paths to the folders which contain all the training data sets
            // TODO: this is worded really weirdly... file roots??? arrays to hold it? the next line doesnt even have an array, what are you referring to?
            string toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\Samples\\";
            //This is the array used to point to each sample set
            string[] fileSetRoots =
            {
                toneRoot + "Sin\\440Hz\\",
                toneRoot + "Square\\440Hz\\",
                toneRoot + "Sawtooth\\440Hz\\"
            };
            

            //get our trained neural network!
            var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10, trimSilence: true);
            var network = audioTrainer.TrainTheNetwork(3);

            Console.WriteLine("Loading files to test against");
            
            //File paths for continuous tones to do a rough test against
            toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\";
            string[] streamingRoots = { toneRoot + "440Hz_Sine_Noise.mp3", toneRoot + "440Hz_Square_Noise.mp3", toneRoot + "440Hz_Sawtooth_Noise.mp3" };

            for(var i = 0; i < streamingRoots.Length; i++)
            {
                Console.WriteLine("Testing against Stream {0}", i);

                //stream the audio data through the neural network, and print out the first 50 results
                var stream = new NeuralAudioStreamer(network, streamingRoots[i]);
                stream.GetResultStream().Take(50).Select((results) =>
                    {
                        foreach (var result in results)
                        {
                            Console.Write("{0:0.000}  ", result);
                        }
                        Console.WriteLine("");
                        return 0;
                    }).ToList();
            }

            Console.WriteLine("Press the any key to exit");
            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.IO;

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
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("loading file");


            int bufferSize = 4096;
            string toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\Samples\\";
            string[] fileSetRoots = { toneRoot + "Sin\\440Hz\\", toneRoot + "Square\\440Hz\\", toneRoot + "Sawtooth\\440Hz\\" };
            string speechRoot = "..\\..\\..\\DataSets\\Audio\\Speech\\";
            string[] speechSetRoots = { speechRoot + "Cold\\Erin\\Set_1\\", speechRoot + "Winter\\Erin\\Set_1\\" };

            var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10, trimSilence: true);
            var network = audioTrainer.TrainTheNetwork();


            Console.WriteLine("Loading files to test against");
            toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\";
            string[] streamingRoots = { toneRoot + "440Hz_Sine_Noise.mp3", toneRoot + "440Hz_Square_Noise.mp3", toneRoot + "440Hz_Sawtooth_Noise.mp3" };

            for(var i = 0; i < streamingRoots.Length; i++)
            {
                Console.WriteLine("Testing against Stream {0}", i);
                var stream = new NeuralAudioStreamer(network, streamingRoots[i]);
                stream.getResultStream().Take(50).Select((results) =>
                    {
                        foreach (var result in results)
                        {
                            Console.Write("{0:0.000}  ", result);
                        }
                        Console.WriteLine("");
                        return 0;
                    }).ToList();

            }

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;


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
        private static void Main(string[] args)
        {
            
            //Set up arrays to hold the file roots of the training data sets
            string toneRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\Samples\\";
            //This is the array used to point to each sample set
            string[] fileSetRoots = { toneRoot + "Sin\\440Hz\\", toneRoot + "Square\\440Hz\\", toneRoot + "Sawtooth\\440Hz\\" };


            string speechRoot = "..\\..\\..\\DataSets\\Audio\\Speech\\";
            string[] speechSetRoots = { speechRoot + "Cold\\Erin\\Set_1\\", speechRoot + "Winter\\Erin\\Set_1\\" };

            //get our trained neural network!
            var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10, trimSilence: true);
            var network = audioTrainer.TrainTheNetwork();

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
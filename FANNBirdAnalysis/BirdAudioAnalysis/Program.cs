using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.IO;


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


using NAudio.Wave;
using NAudio;
using AForge.Math;

using System.Reactive.Linq;

namespace BirdAudioAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("loading file");


            int bufferSize = 4096;
            string rootRoot = "..\\..\\..\\DataSets\\Audio\\Tones\\Samples\\";
            string[] fileSetRoots = { rootRoot + "Sin\\440Hz\\", rootRoot + "Square\\440Hz\\", rootRoot + "Sawtooth\\440Hz\\" };

            var audioTrainer = new NeuralAudioTrainer(fileSetRoots, 10);
            var network = audioTrainer.TrainTheNetwork();
            

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
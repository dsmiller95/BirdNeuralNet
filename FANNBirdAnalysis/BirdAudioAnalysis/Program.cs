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
            var myReader = new AudioFileReader("..\\..\\..\\DataSets\\Audio\\Tones\\440Hz_Sawtooth_Noise.mp3");

            int bufferSize = 4096;
            var analyzer = new AudioAnalyzer(myReader, bufferSize, 44100);
            var frequencyData = (analyzer).getFrequencies().ToObservable();
            frequencyData
                .Subscribe((frequencies) =>
               {
                   double max = -10000;
                   int maxloc = -1;
                   int i = 0;
                   frequencies.ToList().ForEach((magnitude) =>
                   {
                       if (magnitude > max)
                       {
                           max = magnitude;
                           maxloc = i;
                       }
                       i++;
                   });
                   Console.WriteLine("Maximum frequency of {0} with amplitute {1}; bin {2}", analyzer.GetFrequencyForBin(maxloc), max, maxloc);
               });
            
            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
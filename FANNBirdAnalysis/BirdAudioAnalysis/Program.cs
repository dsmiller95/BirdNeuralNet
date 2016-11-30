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

namespace BirdAudioAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("loading file");
            var myReader = new AudioFileReader("..\\..\\..\\DataSets\\Audio\\Tones\\440Hz_Sawtooth_Noise.mp3");

            ReadWithLinq(myReader, 1024);
            //ReadWithoutLinq(myReader, 2048);
            

            Console.WriteLine("Status={0}", "Alll good, my friend");


            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }

        public static void ReadWithLinq(AudioFileReader reader, int bufferSize)
        {
            var theData = (new AudioStreamReader(reader, bufferSize, bufferSize/2));

            var theResult = theData.Select((floats) =>
               {
                   Complex[] complex = new Complex[bufferSize];
                   for (int i = 0; i < floats.Length; i++)
                   {
                       complex[i] = new Complex(floats[i], 0);
                       //Console.Write("{0: 0.0;-0.0} ", floats[i]);

                   }
                   Console.WriteLine();
                   return complex;
               }).Select((complex) =>
                {
                    FourierTransform.FFT(complex, FourierTransform.Direction.Forward);
                    return complex;
                }).Select((fouriest) =>
                {
                    double max = -10000;
                    int maxloc = -1;
                    for (int i = bufferSize/2; i < fouriest.Length; i++)
                    {
                        double value = Math.Abs(fouriest[i].Magnitude);
                        if (value > max)
                        {
                            max = value;
                            maxloc = i;
                        }
                    }
                    var bin = bufferSize - maxloc;//maxloc - bufferSize / 2;
                    Console.WriteLine("Maximum frequency of {0} with amplitute {1}; bin {2}", bin * 44100 / (bufferSize) , max, bin);
                    return 1;
                });

            //force the enumerable to iterate
            theResult.ToList();
        }

        public static void ReadWithoutLinq(AudioFileReader reader, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            Complex[] otherBuffer = new Complex[2048];
            //var offset = 0;
            while(reader.Read(buffer, 0, buffer.Length) > 0)
            {
                for(int i = 0; i < buffer.Length; i++)
                {
                    otherBuffer[i] = new Complex(buffer[i], 0);
                }
                FourierTransform.FFT(otherBuffer, FourierTransform.Direction.Forward);
                
                double max = -10000;
                int maxloc = -1;
                for(int i = 10; i < otherBuffer.Length; i++)
                {
                    if(otherBuffer[i].Re > max)
                    {
                        max = otherBuffer[i].Re;
                        maxloc = i;
                    }
                }
                Console.WriteLine("Maximum frequency of {0} with amplitute {1}", maxloc, max);
            }
        }
    }
}

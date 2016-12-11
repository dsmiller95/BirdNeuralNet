using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace BirdAudioAnalysis
{
    class AudioStreamReader : IEnumerable<float[]>
    {
        private int chunksize, offset;
        private AudioFileReader reader;

        private enum ReadingState
        {
            Silence,
            Reading,
            Permaread
        }

        private ReadingState state;

        public AudioStreamReader(AudioFileReader reader, int chunksize, int offset, bool trimSilence = false)
        {
            this.chunksize = chunksize;
            this.reader = reader;
            this.offset = offset;
            state = (trimSilence) ? ReadingState.Silence : ReadingState.Permaread;
        }

        private const float threshold = 0.1F;

        public IEnumerator<float[]> GetEnumerator()
        {
            float[] mainBuffer = new float[chunksize];
            byte[] buffer = new byte[offset * 4];
            while (reader.Read(buffer, 0, buffer.Length) > 0)
            {
                float[] transferBuffer = new float[chunksize];
                //copy and convert to floats
                for(int i = 0; i < offset; i++)
                {
                    transferBuffer[(chunksize - offset) + i] = System.BitConverter.ToSingle(buffer, i * 4);
                }
                
                switch (state)
                {
                    case ReadingState.Silence:
                        float avgSampleIntensity = transferBuffer.Skip(chunksize - offset).Average(sample => Math.Abs(sample));
                        Console.WriteLine(avgSampleIntensity);
                        if (avgSampleIntensity < threshold)
                        {
                            break;
                        }
                        Console.WriteLine("Going to read!");
                        state = ReadingState.Reading;
                        goto case ReadingState.Reading;

                    case ReadingState.Reading:

                        Array.Copy(mainBuffer, offset, transferBuffer, 0, chunksize - offset);
                        mainBuffer = transferBuffer;
                        float avgBufferIntensity = transferBuffer.Average(sample => Math.Abs(sample));
                        Console.WriteLine(avgBufferIntensity);
                        if (avgBufferIntensity < threshold/2)
                        {
                            state = ReadingState.Silence;
                            Console.WriteLine("Going to silence");
                        }

                        break;

                    case ReadingState.Permaread:

                        Array.Copy(mainBuffer, offset, transferBuffer, 0, chunksize - offset);
                        mainBuffer = transferBuffer;
                        break;


                    default:
                        break;
                }
                
                
                

                //shift over all the values

                //float avgBufferIntensity = transferBuffer.Average(sample => Math.Abs(sample));



                yield return transferBuffer;
                
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

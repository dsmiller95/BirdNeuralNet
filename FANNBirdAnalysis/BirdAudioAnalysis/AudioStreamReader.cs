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

        public AudioStreamReader(AudioFileReader reader, int chunksize, int offset)
        {
            this.chunksize = chunksize;
            
            this.offset = offset;
            this.reader = reader;
        }

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

                //shift over all the values
                Array.Copy(mainBuffer, offset, transferBuffer, 0, chunksize - offset);
                mainBuffer = transferBuffer;

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

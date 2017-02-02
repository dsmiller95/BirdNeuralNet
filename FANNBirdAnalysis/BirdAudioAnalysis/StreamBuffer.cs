using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace BirdAudioAnalysis
{
    public static class StreamBuffer
    {

        public static IEnumerable<T[]> RollingBuffer<T>(this IEnumerable<T> input, int chunksize, int offset)
        {
            //buffer to hold all the sample data points
            T[] mainBuffer = new T[chunksize];

            var enumer = input.GetEnumerator();

            //read in a full buffer of data to start and return it
            for (var i = 0; i < chunksize; i++)
            {
                if (enumer.MoveNext())
                {
                    mainBuffer[i] = enumer.Current;
                }
                else
                {
                    goto exitLoop;
                }
            }
            yield return mainBuffer;

            while (true)
            {
                //use a tmpBuffer to avoid overwriting the same data on the same pointer
                var tmpBuffer = new T[chunksize];
                //shift the buffer back by offset
                Array.Copy(mainBuffer, offset, tmpBuffer, 0, chunksize - offset);


                //read in a bunch of shit and put it into the main buffer
                for (var i = 0; i < offset; i++)
                {
                    if (enumer.MoveNext())
                    {
                        tmpBuffer[chunksize - offset + i] = enumer.Current;
                    }
                    else
                    {
                        goto exitLoop;
                    }
                }
                yield return tmpBuffer;
                mainBuffer = tmpBuffer;
            }
            exitLoop:

            yield break;
        }
    }
}
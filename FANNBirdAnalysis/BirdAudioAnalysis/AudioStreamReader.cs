using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace BirdAudioAnalysis
{
    /**
     * This class will read in the raw floating point samples from an audio file, and provide a rolling window over the data.
     * This will give a window of width ChunkSize and each next sample will be offset by Offset samples
     * EX:
     * ChunkSize of 4 and Offset of 2. The number |-#-| indicates a unique window of samples, numbered in order that they are returned
     * Samples:
     *      |--2---||--4---||--6---|
     *  |---1--||--3---||--5---||--7---|
     *  * * * * * * * * * * * * * * * * *
     */
    class AudioStreamReader : IEnumerable<float[]>
    {
        private readonly int _chunksize, _offset;
        private readonly AudioFileReader _reader;

    // TODO: ???? should probably explain when these would be used, especially "permaread"
        private enum ReadingState
        {
            Silence,
            Reading,
            Permaread
        }

        //Manages the current state of the reader for silence trimming
        private ReadingState _state;
        // TODO: what is this threshold used for. what does it mean
        private const float SilenceThreshold = 0.1F;

        public AudioStreamReader(AudioFileReader reader, int chunksize, int offset, bool trimSilence = false)
        {
            _chunksize = chunksize;
            _reader = reader;
            _offset = offset;
            // TODO: explain what you are doing here & why
            _state = trimSilence ? ReadingState.Silence : ReadingState.Permaread;
        }


        public IEnumerator<float[]> GetEnumerator()
        {
            //buffer to hold all the sample data points
            float[] mainBuffer = new float[_chunksize];

            //buffer to read in <offset> # of samples from the raw byte stream
            //will be converted from bytes to floats
            byte[] buffer = new byte[_offset * 4];
            while (_reader.Read(buffer, 0, buffer.Length) > 0)
            {
                float[] transferBuffer = new float[_chunksize];

                //copy the newly read data to the transfer buffer and convert to floats
                // TODO: why do you need a transfer buffer. where is it being transferred to?
                for(int i = 0; i < _offset; i++)
                {
                    transferBuffer[(_chunksize - _offset) + i] = BitConverter.ToSingle(buffer, i * 4);
                }
                
                switch (_state)
                {
                    case ReadingState.Silence:
                        //if we're reading silence; check to see if the average amplitude is high enough to count as non-silence
                        float avgSampleIntensity = transferBuffer.Skip(_chunksize - _offset).Average(sample => Math.Abs(sample));
                        
                        //if it's still quiet, we're done here
                        if (avgSampleIntensity < SilenceThreshold)
                        {
                            break;
                        }
                        //Console.WriteLine("Going to read!");
                        _state = ReadingState.Reading;
                        goto case ReadingState.Reading;

                    case ReadingState.Reading:
                        //if we're reading noise, check to make sure it hasn't gotten quiet enough to count as silence
                        // TODO: so... why do we care if its silent vs if its not? maybe say that in the silence portion? or here?
                        //copy the old buffer data in the mainBuffer over into the transfer buffer; at an offset of its original position
                        //TODO: should this be done after the silence threshold is checked?
                        Array.Copy(mainBuffer, _offset, transferBuffer, 0, _chunksize - _offset);
                        mainBuffer = transferBuffer;

                        //get the average amplitude of the buffer
                        float avgBufferIntensity = transferBuffer.Average(sample => Math.Abs(sample));

                        //if the average intensity is too low, we're reading silence and should go into the silence state
                        // TODO: Why is this different from the conditional in the ReadingState.Silence switch block? ie: "avgBufferIntensity < SilenceThreshold/2" vs "avgBufferIntensity < SilenceThreshold"
                        if (avgBufferIntensity < SilenceThreshold/2)
                        {
                            _state = ReadingState.Silence;
                            break;
                        }

                        //yield the next buffer
                        // TODO: what does this even mean. to yield it. you basically wrote a comment that is identical to
                        // the actual code
                        yield return transferBuffer;
                        break;

                    case ReadingState.Permaread:
                        //Just read the data and pass it on, no noise or silence filtering
                        Array.Copy(mainBuffer, _offset, transferBuffer, 0, _chunksize - _offset);
                        mainBuffer = transferBuffer;

                        yield return transferBuffer;
                        break;
                }
                // TODO: should this be removed since its commented out
                //yield return transferBuffer;
            }
            yield break;
        }
        // TODO: what is this for
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

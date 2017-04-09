using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirdAudioAnalysis
{
    public class AudioFileSplitAndTrim : IAudioFileSplitAndTrim
    {


        public const int BufferSize = 512;


        /// <summary>
        /// Takes in all the file paths given and does a batch splitting and trimming of each file, putting
        /// the resulting split files into the targetFolder
        /// Returns a task which returns a string array of all the resulting filepaths to the split audio
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        public async Task<string[]> ProcessTheseFiles(string[] filePaths, string targetFolder)
        {
            var i = 0;

            var files = new List<string>();

            foreach (var file in filePaths)
            {
                var analyzer = new AudioAnalyzer(file, BufferSize);
                var fftStream = analyzer.GetFrequencies();
                var newFile = await (new AudioSaver(BufferSize)).saveAsAudioFile(
                    "..\\..\\..\\DataSets\\AudioToSplit\\Split\\" + i + ".wav",
                    fftStream,
                    analyzer.GetWaveFormat());
                Console.Out.WriteLine(newFile);
                files.Add(newFile);
                i += 1;
            }
            

            return files.ToArray();
        }
    }
}
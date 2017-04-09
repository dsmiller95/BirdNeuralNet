using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirdAudioAnalysis
{
    public class AudioFileSplitAndTrim : IAudioFileSplitAndTrim
    {
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
            int bufferSize = 512;

            var i = 0;

            var files = new List<string>();

            foreach (var file in filePaths)
            {
                var analyzer = new AudioAnalyzer(file, bufferSize);
                var fftStream = analyzer.GetFrequencies();
                var newFile = await (new AudioSaver()).saveAsAudioFile("..\\..\\..\\DataSets\\AudioToSplit\\Split\\" + i + ".mp3", fftStream);
                Console.Out.WriteLine(newFile);
                files.Add(newFile);
            }
            

            return files.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio.Wave;

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
            /**
              * 1.Take in all file names
              * 2.   Load in file
              * 3.   Split file into multiple transforms
              * 4.   Identify leading and trailing silences
              * 5.   Save all non-silence files to new files
              * 6.   Compile filenames and return up
              * 7.Return all file names
              **/
            AudioAnalyzer[] sourceFiles = {};
            const int bufferSize = 512;
            var files = new List<string>();

            for (var i = 0; i < filePaths.Length; i++)
                sourceFiles[i] = new AudioAnalyzer(filePaths[i], bufferSize);

            Parallel.ForEach(sourceFiles, async _ => 
                            {
                                var fftStream = _.GetFrequencies();


                                var newFile = await (new AudioSaver()).saveAsAudioFile("..\\..\\..\\DataSets\\AudioToSplit\\Split\\" + files.Count + ".mp3", fftStream);
                                Console.Out.WriteLine(newFile);
                                files.Add(newFile);
                            });
            
            return files.ToArray();
        }
    }
}
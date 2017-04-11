using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

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
            /**
              * 1.Take in all file names
              * 2.   Load in file
              * 3.   Split file into multiple transforms
              * 4.   Identify leading and trailing silences
              * 5.   Save all non-silence files to new files
              * 6.   Compile filenames and return up
              * 7.Return all file names
              **/
            var factory = new TaskFactory();

           

            var waiters = filePaths.Select((file, i) =>
                {
                    return analyzeFile(file, i);
                });
            
            var result2D = await Task.WhenAll(waiters);

            var result = result2D.Aggregate(new List<string>(), (aggregate, value) =>
                            {
                                aggregate.AddRange(value);
                                return aggregate;
                            }).ToArray();

            return result;
        }

        private async Task<string[]> analyzeFile(string file, int i)
        {
            try
            {
                var analyzer = new AudioAnalyzer(file, BufferSize);
                var fftStream = analyzer.GetFrequencies();
                var splitter = new AudioSplitter();
                var splitAudio = splitter.SplitAudio(fftStream);

                var toWait = splitAudio.Select((splitPiece, index) =>
                            {
                                return (new AudioSaver(BufferSize)).SaveAsAudioFile(
                                    "..\\..\\..\\DataSets\\AudioToSplit\\Split\\" + i.ToString("D2") + "-" + index.ToString("D2") + ".wav",
                                    splitPiece,
                                    analyzer.GetWaveFormat());
                            });
                var savedFiles = await Task.WhenAll(toWait);
                return savedFiles;

                /*var newFile = await (new AudioSaver(BufferSize)).SaveAsAudioFile(
                    "..\\..\\..\\DataSets\\AudioToSplit\\Split\\" + i + ".wav",
                    fftStream,
                    analyzer.GetWaveFormat());
                Console.Out.WriteLine(newFile);
                return new string[] {newFile};*/
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return new string[] {e.Message};
            }
        }
    }
}
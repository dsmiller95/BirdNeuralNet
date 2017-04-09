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
            return null;
        }

    }
}
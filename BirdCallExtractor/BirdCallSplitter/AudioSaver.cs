
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirdAudioAnalysis
{
    public class AudioSaver
    {
        public AudioSaver()
        {
            
        }

        /// <summary>
        /// Save a stream of FFT data to a file. Will perform the reverse FFT and save it to a file
        /// </summary>
        /// <param name="fileName">The file to save the data to</param>
        /// <param name="data">the FFT data</param>
        /// <returns>String pointing to the file which was saved</returns>
        public async Task<string> saveAsAudioFile(string fileName, IEnumerable<float[]> data)
        {
            return "Aaaa";
        }
    }
}
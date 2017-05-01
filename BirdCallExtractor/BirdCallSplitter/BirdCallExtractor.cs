using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Threading.Tasks;


namespace BirdAudioAnalysis
{

	class BirdCallExtractor
	{

        private static string[] testBirdNames = {   "AmericanCrow",};

        private static AudioDatabaseDownloader downloader;

		private static void Main(string[] args)
		{

			Console.WriteLine("Press S to split up the audio, or D to download some bird, or A to download some birds and split their audio up");


            
		    string splittingRoot = "..\\..\\..\\DataSets\\AudioToSplit\\";

            var key = Console.ReadKey();

            switch (key.KeyChar.ToString().ToLower())
            {
                case "s":
                    var splitter = new AudioFileSplitAndTrim(splittingRoot + "Split\\");
                    var resultFiles = splitter.ProcessTheseFiles(new string[] {
                        splittingRoot + "01.mp3",
                        splittingRoot + "02.mp3",
                        splittingRoot + "03.mp3",
                        splittingRoot + "04.mp3",
                        splittingRoot + "05.mp3"
                    });
                    resultFiles.GetAwaiter().OnCompleted(() =>
                    {
                        Console.Out.WriteLine("complete");
                        foreach (var file in resultFiles.Result)
                        {
                            Console.Out.WriteLine(file);
                        }
                    });
                    break;
                case "d":
                    using (AudioDatabaseDownloader test = new AudioDatabaseDownloader("G1tR3kt123"))
                    {
                        var theDownloaded = test.DownloadAudioForBird("Turdus rufiventris");
                        theDownloaded.GetAwaiter().OnCompleted(() =>
                        {
                            Console.Out.WriteLine("download complete");
                            foreach (var file in theDownloaded.Result)
                            {
                                Console.Out.WriteLine("File: " + file);
                            }
                        });
                    }
                    break;
                case "a":
                    using (downloader = new AudioDatabaseDownloader("G1tR3kt123"))
                    {
                        var waitingList = testBirdNames.Select((sciName) => ProcessWholeBird(sciName)).ToArray();
                        Task.WaitAll(waitingList);
                    }
                    Console.Out.WriteLine("Done processing Birdos");
                    break;
            }
            
            Console.Out.WriteLine("Press the any key to exit");
			key = Console.ReadKey();
        }


        private static async Task<string[]> ProcessWholeBird(string scientificName)
        {
            string[] resultFiles = null;
            try
            {
                string[] downloadedFiles;
                //using (var localDownloader = new AudioDatabaseDownloader("G1tR3kt123"))
                //{
                //downloadedFiles = await downloader.DownloadAudioForBird(scientificName);

                downloadedFiles = Directory.GetFiles(downloader.GetPathForBird(scientificName));

                var saveDirectory = downloader.GetPathForBird(scientificName) + "..\\";
                var splitter = new AudioFileSplitAndTrim(saveDirectory);
                Directory.CreateDirectory(saveDirectory);
                resultFiles = await splitter.ProcessTheseFiles(downloadedFiles);
                //}

                Console.Out.WriteLine("Done processing " + scientificName);
            }catch(Exception e)
            {
                Console.Out.WriteLine(e.Message + "\n" + e.StackTrace);
            }
            return resultFiles;
        }
	}
}

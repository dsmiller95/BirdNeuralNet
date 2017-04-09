using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;



namespace BirdAudioAnalysis
{

	class BirdCallExtractor
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Press S to split up the audio");


            
		    string splittingRoot = "..\\..\\..\\DataSets\\AudioToSplit\\";

            var key = Console.ReadKey();
		    if (key.KeyChar == 'S' || key.KeyChar == 's'){
		        var splitter = new AudioFileSplitAndTrim();
		        var resultFiles = splitter.ProcessTheseFiles(new string[] {
                        splittingRoot + "01.mp3",
                        splittingRoot + "02.mp3",
                        splittingRoot + "03.mp3",
                        splittingRoot + "04.mp3",
                        splittingRoot + "05.mp3"
                    }, splittingRoot + "\\Split");
                resultFiles.GetAwaiter().OnCompleted(() =>
                                                     {
                                                         foreach (var file in resultFiles.Result)
                                                         {
                                                             Console.Out.WriteLine(file);
                                                         }
                                                     });
		    }

            Console.WriteLine("Press the any key to exit");
			key = Console.ReadKey();
        }
	}
}

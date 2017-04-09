using System;
using System.Collections.Generic;
using System.Linq;



namespace BirdAudioAnalysis
{

	class BirdCallExtractor
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Press S to split up the audio");
            
            var key = Console.ReadKey();
		    if (key.KeyChar == 'S' || key.KeyChar == 's')
		    {
		        
		    }

            Console.WriteLine("Press the any key to exit");
			key = Console.ReadKey();
        }
	}
}

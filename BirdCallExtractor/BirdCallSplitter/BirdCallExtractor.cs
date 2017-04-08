using System;
using System.Collections.Generic;
using System.Linq;


//This statement is used to allow for compilation of the code using a 
// integer, double, or float neural network. that means that the FANN network
// would use this data type for all of its computations. this could be useful for 
// testing how the performace varies when the compilation flags are changed.
//Currently it should be configured to compile with DataType resolving to System.Single; a Float
#if FANN_FIXED
using FANNCSharp.Fixed;
using DataType = System.Int32;
#elif FANN_DOUBLE
using FANNCSharp.Double;
using DataType = System.Double;
#else
using FANNCSharp.Float;
using DataType = System.Single;
#endif


namespace BirdAudioAnalysis
{

	class BirdCallExtractor
	{
		private static void Main(string[] args)
		{
            
			Console.WriteLine("Loading files to test against");
            
		    
			Console.WriteLine("Press the any key to exit");
			var key = Console.ReadKey();
            
		}
	}
}

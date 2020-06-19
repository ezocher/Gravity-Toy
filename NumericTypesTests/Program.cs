using System;
using System.Drawing;


namespace NumericTypesTests
{
    class Program
    {
        static void Main(string[] args)
        {
            const long Million = 1000000;

            FloatingPointNotAssociative.Demonstration();
            Console.WriteLine("\n---------------\n");
            FloatingPointAdditionPrecision.Investigations();
            Console.WriteLine("\n---------------\n");
            FloatingPointUtilPerformanceTests.RunTest(100 * Million);
        }
    }
}

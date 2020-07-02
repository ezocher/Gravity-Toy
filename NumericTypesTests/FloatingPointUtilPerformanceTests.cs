using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class FloatingPointUtilPerformanceTests
{
    public static void RunTest(long iterations)
    {
        Stopwatch stopwatch = new Stopwatch();

        long loopTime, randomGenerationTime, additionTime, checkPrecisionTimeOriginal, checkPrecisionTimeOptimized, checkPrecisionTimeV3;

        stopwatch.Start();
        for (long i = 0; i < iterations;)
        {
            i++;
        }
        loopTime = stopwatch.ElapsedMilliseconds;

        Double[] randomA = new Double[iterations];
        Double[] randomB = new Double[iterations];
        Random random = new Random();
        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            randomA[i] = random.NextDouble();
            randomB[i] = random.NextDouble();
        }
        randomGenerationTime = stopwatch.ElapsedMilliseconds;

        double sum;
        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            sum = randomA[i] + randomB[i];
        }
        additionTime = stopwatch.ElapsedMilliseconds;

        bool result;
        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            result = FloatingPointUtil.CheckAdditionPrecisionOriginal(randomA[i], randomB[i]);
        }
        checkPrecisionTimeOriginal = stopwatch.ElapsedMilliseconds;


        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            result = FloatingPointUtil.CheckAdditionPrecision(randomA[i], randomB[i]);
        }
        checkPrecisionTimeOptimized = stopwatch.ElapsedMilliseconds;

        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            result = FloatingPointUtil.CheckAdditionPrecisionV3(randomA[i], randomB[i]);
        }
        checkPrecisionTimeV3 = stopwatch.ElapsedMilliseconds;


        Console.WriteLine("Performance Test of CheckAdditionPrecision() - running {0:N0} iterations", iterations);
        Console.WriteLine("Times in ms:");
        Console.WriteLine("   Empty loop: {0:N0}", loopTime);
        Console.WriteLine("   Random number generation: {0:N0}, net: {1:N0}", randomGenerationTime, randomGenerationTime - loopTime);
        Console.WriteLine("   Addition: {0:N0}, net: {1:N0}", additionTime, additionTime - loopTime);
        Console.WriteLine("   CAP Original():           {0:N0}, net: {1:N0}", checkPrecisionTimeOriginal, checkPrecisionTimeOriginal - loopTime);
        Console.WriteLine("   CheckAdditionPrecision(): {0:N0}, net: {1:N0}", checkPrecisionTimeOptimized, checkPrecisionTimeOptimized - loopTime);
        Console.WriteLine("   CAP V3():                 {0:N0}, net: {1:N0}", checkPrecisionTimeV3, checkPrecisionTimeV3 - loopTime);
        Console.WriteLine("CheckAdditionPrecision() takes {0:N2} times longer than addition", (checkPrecisionTimeOptimized - loopTime) / (additionTime - loopTime));
        Console.WriteLine("CheckAdditionPrecision() runs in {0:N2}% time of original version",
          ((float)(checkPrecisionTimeOptimized - loopTime) / (float)(checkPrecisionTimeOriginal - loopTime)) * 100.0f);

    }
    /*
        Performance Test of CheckAdditionPrecision() - running 100,000,000 iterations
        Times in ms:
           Empty loop: 225
           Random number generation: 3,700, net: 3,475
           Addition:                   387, net:   162
           CAP Original():           8,074, net: 7,849
           CheckAdditionPrecision(): 6,842, net: 6,617
        CheckAdditionPrecision() takes 40.00 times longer than addition
        CheckAdditionPrecision() runs in 84.30% time of original version
     */

}

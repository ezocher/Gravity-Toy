using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class FloatingPointUtilPerformanceTests
{
    public static void RunTest(long iterations)
    {
        Stopwatch stopwatch = new Stopwatch();

        long loopTime, randomGenerationTime, additionTime, checkPrecisionTimeV1, checkPrecisionTimeV2, checkPrecisionTimeV3;

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
            result = FloatingPointUtil.CheckAdditionPrecisionV1(randomA[i], randomB[i]);
        }
        checkPrecisionTimeV1 = stopwatch.ElapsedMilliseconds;


        stopwatch.Reset(); stopwatch.Start();
        for (long i = 0; i < iterations; i++)
        {
            result = FloatingPointUtil.CheckAdditionPrecisionV2(randomA[i], randomB[i]);
        }
        checkPrecisionTimeV2 = stopwatch.ElapsedMilliseconds;

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
        Console.WriteLine("   CAP V1():   {0:N0}, net: {1:N0}", checkPrecisionTimeV1, checkPrecisionTimeV1 - loopTime);
        Console.WriteLine("   CAP V2():   {0:N0}, net: {1:N0}", checkPrecisionTimeV2, checkPrecisionTimeV2 - loopTime);
        Console.WriteLine("   CAP V3():   {0:N0}, net: {1:N0}", checkPrecisionTimeV3, checkPrecisionTimeV3 - loopTime);
        Console.WriteLine("CheckAdditionPrecisionV3() takes {0:N2} times longer than addition", (checkPrecisionTimeV3 - loopTime) / (additionTime - loopTime));
        Console.WriteLine("CheckAdditionPrecisionV3() runs in {0:N2}% time of original version",
          ((float)(checkPrecisionTimeV3 - loopTime) / (float)(checkPrecisionTimeV1 - loopTime)) * 100.0f);

    }
    /*
        Performance Test of CheckAdditionPrecision() - running 100,000,000 iterations
        Times in ms:
           Empty loop: 224
           Random number generation: 3,830, net: 3,606
           Addition: 389, net: 165
           CAP V1():   9,735, net: 9,511
           CAP V2():   7,382, net: 7,158
           CAP V3():   3,435, net: 3,211
        CheckAdditionPrecisionV3() takes 19.00 times longer than addition
        CheckAdditionPrecisionV3() runs in 33.76% time of original version
    */

}

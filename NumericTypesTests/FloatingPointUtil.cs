using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;

public class FloatingPointUtil
{
    public const int DoubleSignificantDigits = 17;
    public const int MinimumDigitsPrecision = 5;
    public const int MaxAllowedMagnitudeDifferenceDouble = DoubleSignificantDigits - MinimumDigitsPrecision;

    // Returns true when the number of digits actually used in a calculation falls below MinimumDigitsPrecision
    // Specific example:
    //  This limit allows any quantity at or above 1/10 of a millimeter to be added to 42,000 km (geosync orbit radius)
    //
    // Performance: This check takes about 40 times as long as the addition it's checking on (see perf results at end of this class)
    //
    // See below for original version with calculation steps spelled out
    public static bool CheckAdditionPrecision(double a, double b)
    {
        return ((int)Math.Round(Math.Abs(Math.Log10(Math.Abs(a)) - Math.Log10(Math.Abs(b))), MidpointRounding.AwayFromZero) > MaxAllowedMagnitudeDifferenceDouble);
    }

    // Original more understandable version of CheckAdditionPrecision()
    //
    // This is about 15% slower than the final optimized version above (see perf results at end of this class)
    public static bool CheckAdditionPrecisionOriginal(double a, double b)
    {
        int magnitudeDifference = AdditionMagnitudeDifference(a, b);
        return (magnitudeDifference > MaxAllowedMagnitudeDifferenceDouble);
    }

    // Calculates the difference in magnitude for two operands to be added (or subtracted)
    // As the difference gets near the number of significant digits in the representation we have fewer and fewer bits
    //  being used, losing precision
    // When the difference exceeds the number of significant digits the addition has no effect
    public static int AdditionMagnitudeDifference(double a, double b)
    {
        double magnitudeA, magnitudeB;

        magnitudeA = Math.Log10(Math.Abs(a));
        magnitudeB = Math.Log10(Math.Abs(b));

        return (int)Math.Round(Math.Abs(magnitudeA - magnitudeB), MidpointRounding.AwayFromZero);
    }

    /* Run of FloatingPointUtilPerformanceTests.cs:
     
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

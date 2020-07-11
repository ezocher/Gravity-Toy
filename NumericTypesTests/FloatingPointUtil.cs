using NumericTypesTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;

public class FloatingPointUtil
{
    public const int DoubleSignificantDecimalDigits = 17;      // Sometimes 16 in practice, e.g. 3 (digits in calc.) + 13 (digits of mag. difference)
    public const int MinimumDigitsPrecision = 3;
    public const int MaxAllowedMagnitudeDifferenceDouble = DoubleSignificantDecimalDigits - MinimumDigitsPrecision - 1;

    private const int DoubleMantissaBinaryDigits = 53;
    private const int BinaryBase = 2;
    private static readonly double DoubleMachinePrecision = Math.Pow(BinaryBase, -(DoubleMantissaBinaryDigits - 1));
    private const int DecimalBase = 10;
    public static readonly double MinAllowedRatioDouble = DoubleMachinePrecision * Math.Pow(DecimalBase, MinimumDigitsPrecision - 1);

    public static bool CheckAdditionPrecision(double a, double b) => CheckAdditionPrecisionV3(a, b);

    // Returns true when the number of digits actually used in an addition falls below MinimumDigitsPrecision
    // Specific example:
    //  This limit allows any quantity around or above 1/100 of a millimeter to be repeatedly added to 42,000 km (geosync orbit radius)
    //  without losing much accuracy. E.g. 1/300 of a millimeter added 10,000 times should equal about 1/30 of a meter
    //
    // Output from FloatingPointAdditionPrecision.CAP_SpecificExample() in NumericTypesTests project:
    //      Large value = 42,000, small value = 3.333333333333333E-09, addition iterations = 10,000
    //      CheckAdditionPrecision(42,000, 3.333333333333333E-09) = False
    //      Result = 42000.000033323886
    //
    // Performance: This check takes about 20 times as long as the addition it's checking on (see perf results output at end of this class)
    public static bool CheckAdditionPrecisionV3(double a, double b)
    {
        if ((a == 0.0) || (b == 0.0)) return false;

        double larger, smaller;

        if (Math.Abs(a) > Math.Abs(b))
        { larger = a; smaller = b; }
        else
        { larger = b; smaller = a; }

        return (Math.Abs(smaller / larger) < MinAllowedRatioDouble);
    }

    // Returns true when the number of digits actually used in a calculation falls below MinimumDigitsPrecision
    // Specific example:
    //  This limit allows any quantity at or above 1/10 of a millimeter to be added to 42,000 km (geosync orbit radius)
    //
    // Performance: This check takes about 40 times as long as the addition it's checking on (see perf results at end of this class)
    //
    // See below for original version with calculation steps spelled out
    public static bool CheckAdditionPrecisionV2(double a, double b)
    {
        return ((a == 0.0) || (b == 0.0)) ? false : ((int)Math.Round(Math.Abs(Math.Log10(Math.Abs(a)) - Math.Log10(Math.Abs(b))), MidpointRounding.AwayFromZero) > MaxAllowedMagnitudeDifferenceDouble);
    }

    // Original more understandable version of CheckAdditionPrecisionV2()
    //
    // This is about 15% slower than the final optimized version above (see perf results at end of this class)
    public static bool CheckAdditionPrecisionV1(double a, double b)
    {
        if ((a == 0.0) || (b == 0.0)) return false;
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
           Empty loop: 227
           Random number generation: 3,935, net: 3,708
           Addition: 392, net: 165
           CAP V1():   9,179, net: 8,952
           CAP V2():   7,251, net: 7,024
           CAP V3():   3,816, net: 3,589
        CheckAdditionPrecisionV3() takes 21.00 times longer than addition
        CheckAdditionPrecisionV3() runs in 40.09% time of original version
     */
}

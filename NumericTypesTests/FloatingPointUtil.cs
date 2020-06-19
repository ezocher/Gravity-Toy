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
    // locReference is for finding problematic calculations in source (without allocating strings)
    public static bool CheckAdditionPrecision(double a, double b, int locReference)
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
}

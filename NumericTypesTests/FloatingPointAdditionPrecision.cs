using System;
using System.Collections.Generic;
using System.Text;

namespace NumericTypesTests
{
    public class FloatingPointAdditionPrecision
    {
        public static void Investigations()
        {
            double geosyncOrbitR = 42000.0;     // Real example, approximate geosynchronous orbit in km
            double oneMillimeter = 1.0 / 1000.0 / 1000.0;      // One mm in kilometers

            Console.WriteLine("=========== Geosync Orbit plus 1/3 millimeter (and smaller) in kilometers ==========");
            Console.WriteLine("   Objective is to have fractions of a millimeter correctly accumulate over many additions\n");
            double large = geosyncOrbitR;
            double small = oneMillimeter / 3.0;
            Console.WriteLine("large: {0}, log10(large): {1:G5}\n", large, Math.Log10(Math.Abs(large)));

            for (int i = 0; i < 200; i++)
            {
                Console.WriteLine("large + small ({0,22:G17}) = {1,22:G17} - sig digits in calc: {2}, log10(small): {3,7:G5}, logs difference: {4,6:G5} ({5}) (CAP V2: {6}, V3: {7})",
                    small, large + small, DecimalAdditionPrecision(large, small), Math.Log10(Math.Abs(small)), 
                    Math.Log10(Math.Abs(large)) - Math.Log10(Math.Abs(small)), FloatingPointUtil.AdditionMagnitudeDifference(large, small), 
                    FloatingPointUtil.CheckAdditionPrecisionV2(large, small), FloatingPointUtil.CheckAdditionPrecisionV3(large, small));
                if (large + small == large) break;
                small *= 0.1;
            }
        }

        /*
            =========== Geosync Orbit plus 1/3 millimeter (and smaller) in kilometers ==========
               Objective is to have fractions of a millimeter correctly accumulate over many additions

            large: 42000, log10(large): 4.6232

            large + small ( 3.333333333333333E-07) =     42000.000000333333 - sig digits in calc: 5, log10(small): -6.4771, logs difference:   11.1 (11) (CAP V2: False, V3: False)
            large + small (3.3333333333333334E-08) =     42000.000000033331 - sig digits in calc: 4, log10(small): -7.4771, logs difference:   12.1 (12) (CAP V2: False, V3: False)
            large + small (3.3333333333333334E-09) =     42000.000000003332 - sig digits in calc: 3, log10(small): -8.4771, logs difference:   13.1 (13) (CAP V2: True, V3: True)
            large + small (3.3333333333333337E-10) =     42000.000000000335 - sig digits in calc: 2, log10(small): -9.4771, logs difference:   14.1 (14) (CAP V2: True, V3: True)
            large + small (3.3333333333333341E-11) =     42000.000000000036 - sig digits in calc: 1, log10(small): -10.477, logs difference:   15.1 (15) (CAP V2: True, V3: True)
            large + small (3.3333333333333343E-12) =                  42000 - sig digits in calc: 0, log10(small): -11.477, logs difference:   16.1 (16) (CAP V2: True, V3: True)
        */


        // Replaced these original looping versions with the Log-based version in FloatingPointUtil.cs
        //
        // This was V0 of what became FloatingPointUtil.CheckAdditionPrecision()
        static int DecimalAdditionPrecision(double a, double b)
        {
            double larger, smaller;

            if (Math.Abs(a) > Math.Abs(b))
                { larger = a; smaller = b; }
            else
                { larger = b; smaller = a; }

            int significantDigits = 0;
            while ((larger + smaller) != larger)
            {
                smaller *= 0.1;
                significantDigits++;
            }
            return (significantDigits);
        }

        // Created to run in the debugger to study details of constants
        public static void CheckConstants()
        {
            const int DoubleSignificantDigits = 17;
            const int MinimumDigitsPrecision = 4;
            const int MaxAllowedMagnitudeDifferenceDouble = DoubleSignificantDigits - MinimumDigitsPrecision;

            const int DoubleMantissaBinaryDigits = 53;
            const int BinaryBase = 2;
            double DoubleMachinePrecision = Math.Pow(BinaryBase, -(DoubleMantissaBinaryDigits - 1));
            const int DecimalBase = 10;
            double MinAllowedRatioDouble = DoubleMachinePrecision * Math.Pow(DecimalBase, MinimumDigitsPrecision);

            double larger = 42000.0, smaller = 1.0e-6;
            for (int i = 0; i < 15; i++)
            {
                double ratio = smaller / larger;
                bool check = ratio < MinAllowedRatioDouble;

                smaller *= 0.1;
            }
        }

        public static void CAP_SpecificExample()
        {
            // Run and display calculation described by this comment from CheckAdditionPrecision():
                // Specific example:
                //  This limit allows any quantity around or above 1/100 of a millimeter to be repeatedly added to 42,000 km (geosync orbit radius)
                //  without losing too much accuracy. E.g. 1/300 of a mm added 10,000 times should about equal 1/30 of a meter

            double geosyncOrbitR = 42000.0;     // Real example, approximate geosynchronous orbit in km
            double oneMillimeter = 1.0 / 1000.0 / 1000.0;      // One mm in kilometers

            double smallDelta = oneMillimeter / 300.0;
            int numAdditions = 10000;

            Console.WriteLine("Large value = {0:N0}, small value = {1:G17}, addition iterations = {2:N0}", geosyncOrbitR, smallDelta, numAdditions);
            Console.WriteLine("CheckAdditionPrecision({0:N0}, {1}) = {2}", geosyncOrbitR, smallDelta, FloatingPointUtil.CheckAdditionPrecision(geosyncOrbitR, smallDelta));

            for (int i = 0; i < numAdditions; i++)
                geosyncOrbitR += smallDelta;

            Console.WriteLine("Result = {0:G17}", geosyncOrbitR);
        }
        /* Output:
            Large value = 42,000, small value = 3.333333333333333E-09, addition iterations = 10,000
            CheckAdditionPrecision(42,000, 3.333333333333333E-09) = False
            Result = 42000.000033323886
         */


    }
}

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
            double oneMillimeterDelta = 1.0 / 1000.0 / 1000.0;      // One mm in kms


            Console.WriteLine("=========== double ==========");
            double large = geosyncOrbitR;
            double small = oneMillimeterDelta;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("large ({0:G}) + small ({1:G}) = {2:G17} - sig digits in calc: {3}, log10()s: {4:G}, {5:G}, logs difference: {6}",
                    large, small, large + small, DetectAdditionPrecisionIssue(large, small), Math.Log10(Math.Abs(large)), Math.Log10(Math.Abs(small)),
                    FloatingPointUtil.AdditionMagnitudeDifference(large, small));
                small *= 0.1;
            }

            /* OUTPUT:
                =========== double ==========
                large(42000) + small(1E-06) = 42000.000001 - sig digits in calc: 6, log10()s: 4.623249290397901, -6, logs difference: 11
                large(42000) + small(1E-07) = 42000.000000100001 - sig digits in calc: 5, log10()s: 4.623249290397901, -7, logs difference: 12
                large(42000) + small(1E-08) = 42000.000000009997 - sig digits in calc: 4, log10()s: 4.623249290397901, -8, logs difference: 13
                large(42000) + small(1E-09) = 42000.000000000997 - sig digits in calc: 3, log10()s: 4.623249290397901, -9, logs difference: 14
                large(42000) + small(1.0000000000000002E-10) = 42000.000000000102 - sig digits in calc: 2, log10()s: 4.623249290397901, -10, logs difference: 15
                large(42000) + small(1.0000000000000003E-11) = 42000.000000000007 - sig digits in calc: 1, log10()s: 4.623249290397901, -11, logs difference: 16
                large(42000) + small(1.0000000000000004E-12) = 42000 - sig digits in calc: 0, log10()s: 4.623249290397901, -12, logs difference: 17
                large(42000) + small(1.0000000000000004E-13) = 42000 - sig digits in calc: 0, log10()s: 4.623249290397901, -13, logs difference: 18
                large(42000) + small(1.0000000000000005E-14) = 42000 - sig digits in calc: 0, log10()s: 4.623249290397901, -14, logs difference: 19
                large(42000) + small(1.0000000000000005E-15) = 42000 - sig digits in calc: 0, log10()s: 4.623249290397901, -15, logs difference: 20
             */

            //Console.WriteLine("\n\n=========== decimal ==========");
            //decimal dpos = -42157.0m;
            //decimal dsmall = 0.1m;
            //for (int i = 0; i < 30; i++)
            //{
            //    Console.WriteLine("pos ({0:G}) + small ({1:E}) = {2:G} - significant digits in calculation: {3}", dpos, dsmall, dpos + dsmall, DetectAdditionPrecisionIssue(dpos, dsmall));
            //    dsmall *= 0.1m;
            //}
        }
     
        // Replaced these original looping versions with the Log-based version in FloatingPointUtil.cs
        static int DetectAdditionPrecisionIssue(double a, double b)
        {
            double larger, smaller;

            if (Math.Abs(a) > Math.Abs(b))
            {
                larger = a; smaller = b;
            }
            else
            {
                larger = b; smaller = a;
            }

            int significantDigits = 0;
            while ((larger + smaller) != larger)
            {
                smaller *= 0.1;
                significantDigits++;
            }
            return (significantDigits);
        }

        static int DetectAdditionPrecisionIssue(decimal larger, decimal smaller)
        {
            int significantDigits = 0;

            while ((larger + smaller) != larger)
            {
                smaller *= 0.1m;
                significantDigits++;
            }
            return (significantDigits);
        }
    }
}

using System;
using System.Drawing;


namespace NumericTypesTests
{
    struct Point
    {
        public double X;
        public double Y;
    }

    class Program
    {
        static void Main(string[] args)
        {
            double diagonals = 0.56568926572799683;
            double across = 0.40000274777412415;
            double center = 1.6000109910964966;
            
            float facross = 0.400002747f;
            double dfacross = (double)facross;

            float fdiagonals = 0.565689266f;
            double dfdiagonals = (double)fdiagonals;

            double incorrectSumAfterTwo = 0.96569204330444336;

            Point[] accelerations = new Point[5];
            Point accel;

            accelerations[0].X = 0.0;

            accel.X = diagonals;
            accelerations[0].X += accel.X;

            accel.X = dfacross;
            accelerations[0].X += accel.X;

            float s = facross + fdiagonals;
            double ds = (double)s;

            Console.WriteLine("accelerations[0].X = {0:G17}", accelerations[0].X);

            Console.WriteLine("Error delta = {0:G17}", incorrectSumAfterTwo - accelerations[0].X);

            /*
            double zeroX = 0.0, oneY = 0.0, twoX = 0.0;

            Console.WriteLine("zeroX = {0:G17}", zeroX);
            zeroX = diagonals;
            Console.WriteLine("zeroX = {0:G17}", zeroX);
            zeroX += across;
            Console.WriteLine("zeroX = {0:G17}", zeroX);
            zeroX += diagonals;
            Console.WriteLine("zeroX = {0:G17}", zeroX); 
            zeroX += center;
            Console.WriteLine("zeroX = {0:G17}", zeroX);

            Console.WriteLine("oneY = {0:G17}", oneY);
            oneY = -diagonals;
            Console.WriteLine("oneY = {0:G17}", oneY);
            oneY += -diagonals;
            Console.WriteLine("oneY = {0:G17}", oneY);
            oneY += -across;
            Console.WriteLine("oneY = {0:G17}", oneY);
            oneY += -center;
            Console.WriteLine("oneY = {0:G17}", oneY);

            twoX = -across;
            twoX += -diagonals;
            twoX += -diagonals;
            twoX += -center;


            Console.WriteLine("zero = {0:G17}\none  = {1:G17}\ntwo  = {2:R}", zeroX, oneY, twoX);
            */



            /*
            double large = -42157.0;
            //double deltaT = 0.0000333333333333333;
            //double v = 0.8077192;
            //Console.WriteLine("pos = {0:G17} v = {1:G17} deltaT = {2:G17}", large, v, deltaT);

            //double dist = deltaT * v / 2.0;
            //large += dist;

            //Console.WriteLine("dist = {0:G17} pos = {1:G17}", dist, large);

            Console.WriteLine("=========== double ==========");
            large = -42157.0;
            double small = 0.1;
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("pos ({0:G17}) + small ({1:G17}) = {2:R} - significant digits in calculation: {3}, log10s: {4:R}, {5:R}, log difference: {6}", 
                    large, small, large + small, DetectAdditionPrecisionIssue_Original(large, small), Math.Log10(Math.Abs(small)), Math.Log10(Math.Abs(large)), 
                    DetectAdditionPrecisionIssue_LogVersion(large, small));
                small *= 0.1;
            }

            Console.WriteLine("\n\n=========== decimal ==========");
            decimal dpos = -42157.0m;
            decimal dsmall = 0.1m;
            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine("pos ({0:G}) + small ({1:E}) = {2:G} - significant digits in calculation: {3}", dpos, dsmall, dpos + dsmall, DetectAdditionPrecisionIssue_Original(dpos, dsmall));
                dsmall *= 0.1m;
            }
            */
        }


        // Consider adding this to Gravity-Sandbox numeric investigation branch (with a Debug message or Debug.Break() on 0 or <=1 or <= 2 ??)
        //                                                          -or- put a condional breakpoint on return (significantDigits);
        // Using this for cumulative error detection??        
        static int DetectAdditionPrecisionIssue_Original(double larger, double smaller)
        {
            int significantDigits = 0;

            while ((larger + smaller) != larger)
            {
                smaller *= 0.1;
                significantDigits++;
            }
            return (significantDigits);
        }

        // New version with built in ordering of inputs and stopping when good precision is found
        const int GoodPrecision = 6;    // If precision is this or higher, consider it good
                                        //  6 is a guess, need more experience and investigation to set properly
        
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
                if (significantDigits == GoodPrecision)
                    break;
            }
            return (significantDigits);
        }

        static int DetectAdditionPrecisionIssue_LogVersion(double a, double b)
        {
            double magnitudeA, magnitudeB;

            magnitudeA = Math.Log10(Math.Abs(a));
            magnitudeB = Math.Log10(Math.Abs(b));

            return (int)Math.Round(Math.Abs(magnitudeA - magnitudeB), MidpointRounding.AwayFromZero);
        }



        static int DetectAdditionPrecisionIssue_Original(decimal larger, decimal smaller)
        {
            int significantDigits = 0;

            while ((larger + smaller) != larger)
            {
                smaller *= 0.1m;
                significantDigits++;
            }
            return (significantDigits);
        }


        /* ++++++++++ OUTPUT ++++++++++
         * 
                =========== double ==========
                pos (-42157) + small (0.1) = -42156.9 - significant digits in calculation: 11
                pos (-42157) + small (0.010000000000000002) = -42156.99 - significant digits in calculation: 10
                pos (-42157) + small (0.0010000000000000002) = -42156.999 - significant digits in calculation: 9
                pos (-42157) + small (0.00010000000000000003) = -42156.9999 - significant digits in calculation: 8
                pos (-42157) + small (1.0000000000000004E-05) = -42156.99999 - significant digits in calculation: 7
                pos (-42157) + small (1.0000000000000004E-06) = -42156.999999 - significant digits in calculation: 6
                pos (-42157) + small (1.0000000000000005E-07) = -42156.9999999 - significant digits in calculation: 5
                pos (-42157) + small (1.0000000000000005E-08) = -42156.99999999 - significant digits in calculation: 4
                pos (-42157) + small (1.0000000000000005E-09) = -42156.999999999 - significant digits in calculation: 3
                pos (-42157) + small (1.0000000000000006E-10) = -42156.9999999999 - significant digits in calculation: 2
                pos (-42157) + small (1.0000000000000006E-11) = -42156.99999999999 - significant digits in calculation: 1
                pos (-42157) + small (1.0000000000000006E-12) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.0000000000000007E-13) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.0000000000000008E-14) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.0000000000000009E-15) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.000000000000001E-16) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.000000000000001E-17) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.000000000000001E-18) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.000000000000001E-19) = -42157 - significant digits in calculation: 0
                pos (-42157) + small (1.0000000000000011E-20) = -42157 - significant digits in calculation: 0

                >>> The e-11 line is definately an issue for building up cumulative errors and the e-10 up through e-8 lines may also be as well

                =========== decimal ==========
                pos (-42157.0) + small (1.000000E-001) = -42156.9 - significant digits in calculation: 24
                pos (-42157.0) + small (1.000000E-002) = -42156.99 - significant digits in calculation: 23
                pos (-42157.0) + small (1.000000E-003) = -42156.999 - significant digits in calculation: 22
                pos (-42157.0) + small (1.000000E-004) = -42156.9999 - significant digits in calculation: 21
                pos (-42157.0) + small (1.000000E-005) = -42156.99999 - significant digits in calculation: 20
                pos (-42157.0) + small (1.000000E-006) = -42156.999999 - significant digits in calculation: 19
                pos (-42157.0) + small (1.000000E-007) = -42156.9999999 - significant digits in calculation: 18
                pos (-42157.0) + small (1.000000E-008) = -42156.99999999 - significant digits in calculation: 17
                pos (-42157.0) + small (1.000000E-009) = -42156.999999999 - significant digits in calculation: 16
                pos (-42157.0) + small (1.000000E-010) = -42156.9999999999 - significant digits in calculation: 15
                pos (-42157.0) + small (1.000000E-011) = -42156.99999999999 - significant digits in calculation: 14
                pos (-42157.0) + small (1.000000E-012) = -42156.999999999999 - significant digits in calculation: 13
                pos (-42157.0) + small (1.000000E-013) = -42156.9999999999999 - significant digits in calculation: 12
                pos (-42157.0) + small (1.000000E-014) = -42156.99999999999999 - significant digits in calculation: 11
                pos (-42157.0) + small (1.000000E-015) = -42156.999999999999999 - significant digits in calculation: 10
                pos (-42157.0) + small (1.000000E-016) = -42156.9999999999999999 - significant digits in calculation: 9
                pos (-42157.0) + small (1.000000E-017) = -42156.99999999999999999 - significant digits in calculation: 8
                pos (-42157.0) + small (1.000000E-018) = -42156.999999999999999999 - significant digits in calculation: 7
                pos (-42157.0) + small (1.000000E-019) = -42156.9999999999999999999 - significant digits in calculation: 6
                pos (-42157.0) + small (1.000000E-020) = -42156.99999999999999999999 - significant digits in calculation: 5
                pos (-42157.0) + small (1.000000E-021) = -42156.999999999999999999999 - significant digits in calculation: 4
                pos (-42157.0) + small (1.000000E-022) = -42156.9999999999999999999999 - significant digits in calculation: 3
                pos (-42157.0) + small (1.000000E-023) = -42156.99999999999999999999999 - significant digits in calculation: 2
                pos (-42157.0) + small (1.000000E-024) = -42156.999999999999999999999999 - significant digits in calculation: 1
                pos (-42157.0) + small (1.000000E-025) = -42157.000000000000000000000000 - significant digits in calculation: 0
                pos (-42157.0) + small (1.000000E-026) = -42157.000000000000000000000000 - significant digits in calculation: 0
                pos (-42157.0) + small (1.000000E-027) = -42157.000000000000000000000000 - significant digits in calculation: 0
                pos (-42157.0) + small (1.000000E-028) = -42157.000000000000000000000000 - significant digits in calculation: 0
                pos (-42157.0) + small (0.000000E+000) = -42157.000000000000000000000000 - significant digits in calculation: 0
                pos (-42157.0) + small (0.000000E+000) = -42157.000000000000000000000000 - significant digits in calculation: 0
        */
    }
}

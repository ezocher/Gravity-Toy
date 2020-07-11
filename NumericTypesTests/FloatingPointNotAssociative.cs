using System;
using System.Collections.Generic;
using System.Text;

namespace NumericTypesTests
{
    public class FloatingPointNotAssociative
    {
        /*
         * Demonstration of (barely) non-associative floating point addition using actual values from the 5 Body Cross scenario in GravitySandbox.
         * This caused body #1 to drift out of symetry with the others, eventually causing the central body to move. This problem is
         * sidestepped by rounding accelerations to 10 significant digits.
         *
         */

        public static void Demonstration()
        {
            double diagonal = -0.56569329780727118;
            double across   = -0.40000556695130257;
            // double center   = -1.6000222678052103;

            double bodyZero = diagonal + diagonal + across;
            double bodyOne  = diagonal + across + diagonal;

            Console.WriteLine("Floating point addition is not associative:");
            Console.WriteLine("bodyZero (a + a + b) = {0:G17}", bodyZero);
            Console.WriteLine("bodyOne  (a + b + a) = {0:G17}", bodyOne);
            Console.WriteLine("bodyZero == bodyOne? {0}", bodyZero == bodyOne);
        }
    }
}

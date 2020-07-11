using System;
using System.Collections.Generic;
using System.Text;

namespace NumericTypesTests
{
    class GoldenRatioNumericIssueDemo
    {
        // Numeric Recipes 3e - page 11 - Stability
        // Powers of psi (golden ratio)

        // With floats, subtractions are off by > 1% at N = 16
        // With doubles, subtractions are off by > 1% at N = 36

        public static void RunPsiCalculations(int n)
        {
            PsiFloatCalculations(n);
            PsiDoubleCalculations(n * 2);
        }

        public static void PsiFloatCalculations(int n)
        {
            float psi = ((float)Math.Sqrt(5.0) - 1.0f) / 2.0f;
            const float psiSub0 = 1.0f;

            Console.WriteLine("\nGolden Ratio (Psi) by multiplication vs. subtraction - floats");
            float multiplyPsi = psiSub0;
            float subtractPsi = psiSub0;
            float PsiSubN = subtractPsi;
            float PsiSubNMinus1 = subtractPsi + psi;

            for (int i = 0; i <= n; i++)
            {
                Console.WriteLine("n = {0,3:D}, mult Psi = {1,15:G9}, sub Psi = {2,15:G9}, diff factor = {3}", i, multiplyPsi, subtractPsi, subtractPsi / multiplyPsi);
                multiplyPsi *= psi;

                subtractPsi = PsiSubNMinus1 - PsiSubN;
                PsiSubNMinus1 = PsiSubN;
                PsiSubN = subtractPsi;
            }
        }

        public static void PsiDoubleCalculations(int n)
        {
            double psi = (Math.Sqrt(5.0) - 1.0) / 2.0;
            const double psiSub0 = 1.0;

            Console.WriteLine("\nGolden Ratio (Psi) by multiplication vs. subtraction - doubles");
            double multiplyPsi = psiSub0;
            double subtractPsi = psiSub0;
            double PsiSubN = subtractPsi;
            double PsiSubNMinus1 = subtractPsi + psi;

            for (int i = 0; i <= n; i++)
            {
                Console.WriteLine("n = {0,3:D}, mult Psi = {1,23:G17}, sub Psi = {2,23:G17}, diff factor = {3}", i, multiplyPsi, subtractPsi, subtractPsi / multiplyPsi);
                multiplyPsi *= psi;

                subtractPsi = PsiSubNMinus1 - PsiSubN;
                PsiSubNMinus1 = PsiSubN;
                PsiSubN = subtractPsi;
            }
        }


    }
}

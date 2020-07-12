using System;
using System.Diagnostics;

namespace GravitySandboxUWP
{
    public static class TimeDisplay
    {
        public enum BaseUnits { Seconds, Minutes, Hours, Days, Years };
        const int lastUnitsIndex = (int)BaseUnits.Years;

        static readonly string[] unitAbbreviations = { "sec.", "min.", "hrs.", "days", "years" };
        static readonly string[] formatSpecifiers = { "N1", "N2", "N2", "N3", "N3" };
        static readonly double[] unitsPerUnit = { 60.0, 60.0, 24.0, 365.25 };
        static readonly double[] cutoverPoint = { 90.0, 90.0, 24.0, 365.25 };  // Number of smaller units accumulated before cutting over to next larger unit

        public static string FormatElapsedTime(double elapsedTime, BaseUnits baseUnits)
        {
            int unitsIndex = (int)baseUnits;

            while ((elapsedTime > cutoverPoint[unitsIndex]) && (unitsIndex < lastUnitsIndex))
            {
                elapsedTime /= unitsPerUnit[unitsIndex++];
            }
            return String.Format("{0:" + formatSpecifiers[unitsIndex] +"} {1}", elapsedTime, unitAbbreviations[unitsIndex]);
        }

        public static void UseCase()
        {
            double[] testValues = { 30.0, 100.0, 15.0, 200.0, 40000.0, 20.0, 72.0 };
            BaseUnits[] testUnits = {
                BaseUnits.Seconds, BaseUnits.Seconds, BaseUnits.Minutes, BaseUnits.Minutes, BaseUnits.Minutes,
                BaseUnits.Hours, BaseUnits.Hours
            };

            for (int i = 0; i < testValues.Length; i++)
                Debug.WriteLine("{0} {1} -> {2}", testValues[i], unitAbbreviations[(int)testUnits[i]],
                    FormatElapsedTime(testValues[i], testUnits[i]) );
        }
    }
}
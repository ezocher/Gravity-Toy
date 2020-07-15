using GravitySandboxUWP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NumericTypesTests
{
    // See GitHub issue #37
    public class ISSOrbitScenario
    {
        // From SimulationSpace.cs
        public const double BigG_M3PerKgSec2 = 6.6743E-11; // m^3/kg*sec^2
        public const double KmPerMeter = 1.0 / 1000.0;
        public const double MinutesPerHour = 60.0;
        public const double SecondsPerMinute = 60.0;
        public const double SecondsPerHour = SecondsPerMinute * MinutesPerHour;

        public double BigG { get; private set; }
        public string MassUnitsAbbr { get; private set; }

        public string DistanceUnitsAbbr { get; private set; }

        public TimeDisplay.BaseUnits TimeUnits { get; private set; }

        public double TimeUnitsPerUISecond { get; private set; }

        public string VelocityUnitsAbbr { get; private set; }

        // Factor for deriving velocity when either distance or time units in velocity are
        //  different from base time or base distance units
        public double VelocityConnversionFactor { get; private set; }

        // From GravitySim.cs
        private double simElapsedTime;
        //private bool checkSim;
        //private int simRounding;
        //private bool accelerationLimitOn;
        //private double accelerationLimit;
        public static double minimumSeparationSquared;

        private SimPoint position;
        private SimPoint velocity;
        private SimPoint acceleration;

        // TBD: Orbit completion detection (actually half-orbit completion)
        //      Half orbit complete when y <= 0.0 then look for
        //      Full orbit complete whrn y >= 0.0 then cycle


        // ----------------------------------------------
        public ISSOrbitScenario()
        {
            MassUnitsAbbr = "kg";
            DistanceUnitsAbbr = "km";
            TimeUnits = TimeDisplay.BaseUnits.Minutes;
            VelocityUnitsAbbr = "km/h";
            // Internal velocities are in km/min., multiply by this to get km/h
            VelocityConnversionFactor = MinutesPerHour;

            // Needs to be in kg, km, and minutes
            //      km^3/kg*min^2
            BigG = BigG_M3PerKgSec2 *
                KmPerMeter * KmPerMeter * KmPerMeter * SecondsPerMinute * SecondsPerMinute;

            // Time base: 1 min / second real
            //TimeUnitsPerUISecond = 1.0;

            //minimumSeparationSquared = 1.0;
        }

        double startingVelocityMagnitude;
        double startingOrbitRadius;

        //int calculationCyclesPerFrame;
        //bool checkAdditionPrecision;

        //const int originalCalculationCyclesPerFrame = 200;

        // Calculation Cycles
        //
        //  The original scenario at base speed ran at one Minute of simulation per second of wall clock, with 60 frames per second and 200 iterations per frame.
        //  So the original scenario did 200 iterations per simuilation second or 12,000 iterations per siumlation minute
        //
        //  The SpeedFactor in the orginal scenario changed the number of minutes simulated per second of wall clock. E.g. a speed factor of 2.0 ran at 2 minutes per second,
        //  with the same number of frames per second (always 60 fps), so at speed factor 2.0 there are half as many iterations per simulation minute
        //
        public void RunISSOrbits()
        {
            int[] iterationsPerSimMinuteTrials = { 1, 6, 10, 60, 100, 600, 1000, 6_000, 10_000, 12_000, 60_000, 100_000, 120_000, 600_000, 1_200_000, 3_000_000 };
            //sim.SetCalculationSettings(new CalculationSettings(200, false, true));
            // calculationCyclesPerFrame = originalCalculationCyclesPerFrame;

            foreach (int iterationsPerSimMinute in iterationsPerSimMinuteTrials)
            {
                startingOrbitRadius = SolarSystem.ISS_OrbitRadiusKm;
                startingVelocityMagnitude = CircularOrbitVelocity(SolarSystem.EarthMassKg, SolarSystem.ISS_OrbitRadiusKm);

                position = new SimPoint(-startingOrbitRadius, 0.0);
                velocity = new SimPoint(0.0, startingVelocityMagnitude / VelocityConnversionFactor);
                acceleration = new SimPoint(0.0, 0.0);

                simElapsedTime = 0.0;

                Run(iterationsPerSimMinute, false, 10);
            }

        }

        public double CircularOrbitVelocity(double centralMass, double orbitRadius)
        {
            return Math.Sqrt((BigG * centralMass) / orbitRadius) * VelocityConnversionFactor;
        }


        public void Run(int iterationsPerSimMinute, bool checkAdditionPrecision, long numberOfOrbits)
        {
            OrbitalPhysics orbitalPhysics = new OrbitalPhysics(BigG, SolarSystem.EarthMassKg);

            //double timeInterval = 1.0 / 60.0;
            ////Stopwatch perfStopwatch = new Stopwatch();

            //double scaledTimeInterval = timeInterval * SpeedFactor;
            //double timeIntervalPerCycle = scaledTimeInterval / (double)iterationsPerSimMinute;

            double timeIntervalPerCycle = 1.0 / iterationsPerSimMinute;

            checkingForHalfOrbit = true;

            double startingR = position.Magnitude();
            double startingV = (velocity * VelocityConnversionFactor).Magnitude();

            double lastR = startingR;
            double lastV = startingV;

            long orbitsCompleted = 0;
            Console.WriteLine("\n\nStarting run with {0:N0} iterations per simulation minute. Starting R = {1} km, V = {2} km/h",
                iterationsPerSimMinute, startingR, startingV);

            // Use completed orbits to exit inner loop
            for (long iteration = 0; true /* calcCycle < calculationCyclesPerFrame */; iteration++)
            {
                orbitalPhysics.Accelerate(position, ref acceleration);

                // Update positon and velocity

                //if (checkAdditionPrecision)
                //    orbitalPhysics.MoveWithPrecisionCheck(ref position, ref velocity, acceleration, timeIntervalPerCycle);
                //else
                
                orbitalPhysics.Move(ref position, ref velocity, acceleration, timeIntervalPerCycle);

                simElapsedTime += timeIntervalPerCycle;

                //if ((calcCycle % (60 * calculationCyclesPerFrame)) == 0)
                //    Console.WriteLine("Time {0} in {1:N0} calc cycles. R = {2} km, V = {3} km/h",
                //        TimeDisplay.FormatElapsedTime(simElapsedTime, TimeDisplay.BaseUnits.Minutes), calcCycle, position.Magnitude(),
                //        (velocity * VelocityConnversionFactor).Magnitude());

                //if ((calcCycle % (600 * calculationCyclesPerFrame)) == 0)
                //    Debug.WriteLine(calcCycle);
                if (CheckForCompletedOrbit(position.Y))
                {
                    orbitsCompleted++;
                    Console.Write(".");
                    if ((orbitsCompleted % 10) == 0)
                    {
                        double currentR = position.Magnitude();
                        Console.WriteLine("\nOrbit {0} completed at time {1} in {2:N0} calc cycles. R = {3} km, V = {4} km/h",
                            orbitsCompleted, TimeDisplay.FormatElapsedTime(simElapsedTime, TimeDisplay.BaseUnits.Minutes), iteration, currentR,
                            (velocity * VelocityConnversionFactor).Magnitude());
                        double rGrowthPerOrbit = (currentR - lastR) / 10.0;
                        Console.WriteLine("R grew by {0:N5} km per orbit = {1}% per orbit", rGrowthPerOrbit, rGrowthPerOrbit / lastR * 100.0);
                        lastR = currentR;
                    }
                    if (orbitsCompleted == numberOfOrbits)
                    {
                        break;
                    }
                }
            }
        }

        private static bool checkingForHalfOrbit = true;

        /// <summary>
        /// Returns true when we've just completed an orbit.
        /// Assumes orbit starts at -orbitRadius, 0.0 and runs clockwise
        /// </summary>
        /// <param name="yCoordinate"></param>
        /// <returns></returns>
        public bool CheckForCompletedOrbit(double yCoordinate)
        {
            if (checkingForHalfOrbit ? yCoordinate <= 0.0 : yCoordinate >= 0.0)
            {
                // We just crossed zero, if we just found a half orbit return false, found a full orbit return true
                // In all cases toggle checkingForHalfOrbit
                checkingForHalfOrbit = !checkingForHalfOrbit;
                return checkingForHalfOrbit;
            }
            else
                return false;
        }
    }
}

/* OUTPUT

Starting run with 1 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 2.829 days in 4,073 calc cycles. R = 25386.316319853915 km, V = 14562.754334695377 km/h
R grew by 1,859.05963 km per orbit = 27.356330631417887% per orbit


Starting run with 6 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 23.74 hrs. in 8,544 calc cycles. R = 11007.741455304269 km, V = 21666.459359737968 km/h
R grew by 421.20215 km per orbit = 6.198050324769515% per orbit


Starting run with 10 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 20.43 hrs. in 12,257 calc cycles. R = 9439.78086898936 km, V = 23394.218716967112 km/h
R grew by 264.40609 km per orbit = 3.8907737060817107% per orbit


Starting run with 60 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 16.31 hrs. in 58,713 calc cycles. R = 7268.784868622231 km, V = 26658.869235246028 km/h
R grew by 47.30649 km per orbit = 0.6961217775632769% per orbit


Starting run with 100 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.98 hrs. in 95,881 calc cycles. R = 7081.460818656588 km, V = 27009.163664885305 km/h
R grew by 28.57408 km per orbit = 0.42047173611712674% per orbit


Starting run with 600 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.57 hrs. in 560,484 calc cycles. R = 6843.755935111363 km, V = 27474.21418617924 km/h
R grew by 4.80359 km per orbit = 0.07068557137634122% per orbit


Starting run with 1,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.54 hrs. in 932,166 calc cycles. R = 6824.56185391815 km, V = 27512.822578772986 km/h
R grew by 2.88419 km per orbit = 0.042441204049238866% per orbit


Starting run with 6,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 5,578,194 calc cycles. R = 6800.53122105398 km, V = 27561.389892149153 km/h
R grew by 0.48112 km per orbit = 0.007079781176945712% per orbit


Starting run with 10,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 9,295,016 calc cycles. R = 6798.606936926805 km, V = 27565.29011609443 km/h
R grew by 0.28869 km per orbit = 0.004248169328349302% per orbit


Starting run with 12,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 11,153,427 calc cycles. R = 6798.12582334126 km, V = 27566.26551720244 km/h
R grew by 0.24058 km per orbit = 0.0035402037477408536% per orbit


Starting run with 60,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 55,755,294 calc cycles. R = 6796.201198736204 km, V = 27570.168503101817 km/h
R grew by 0.04812 km per orbit = 0.0007080908810304477% per orbit


Starting run with 100,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 92,923,516 calc cycles. R = 6796.008721300697 km, V = 27570.558923260054 km/h
R grew by 0.02887 km per orbit = 0.0004248575584294735% per orbit


Starting run with 120,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 111,507,627 calc cycles. R = 6795.960601482221 km, V = 27570.656531844914 km/h
R grew by 0.02406 km per orbit = 0.000354048551471929% per orbit


Starting run with 600,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 557,526,296 calc cycles. R = 6795.768120667118 km, V = 27571.046979568495 km/h
R grew by 0.00481 km per orbit = 7.081025574548864E-05% per orbit


Starting run with 1,200,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 1,115,049,632 calc cycles. R = 6795.744060335589 km, V = 27571.0957872025 km/h
R grew by 0.00241 km per orbit = 3.540513085990708E-05% per orbit


Starting run with 3,000,000 iterations per simulation minute. Starting R = 6795.72 km, V = 27571.144595064063 km/h
..........
Orbit 10 completed at time 15.49 hrs. in 2,787,619,638 calc cycles. R = 6795.729624135704 km, V = 27571.125071887815 km/h
R grew by 0.00096 km per orbit = 1.41620545051055E-05% per orbit

    */

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

        // simulation speed factor, 1.0 = 100% of original scenario speed
        public double SpeedFactor { get; private set; }


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
            TimeUnitsPerUISecond = 1.0;

            simElapsedTime = 0.0;
            minimumSeparationSquared = 1.0;
            SpeedFactor = 1.0;
        }

        private const double speedIncrement = 1.25992105;    // Cube root of 2 -> three steps doubles or halves simulation speed

        public void RunFaster() { SpeedFactor *= speedIncrement; }

        public void RunSlower() { SpeedFactor *= 1.0 / speedIncrement; }

        double startingVelocityMagnitude;
        double startingOrbitRadius;

        int calculationCyclesPerFrame;
        bool checkAdditionPrecision;

        const int originalCalculationCyclesPerFrame = 200;

        public void RunISSOrbits()
        {
            //sim.SetCalculationSettings(new CalculationSettings(200, false, true));
            // calculationCyclesPerFrame = originalCalculationCyclesPerFrame;
            calculationCyclesPerFrame = 1000;
            checkAdditionPrecision = true;

            startingOrbitRadius = SolarSystem.ISS_OrbitRadiusKm;
            startingVelocityMagnitude = CircularOrbitVelocity(SolarSystem.EarthMassKg, SolarSystem.ISS_OrbitRadiusKm);

            position = new SimPoint(-startingOrbitRadius, 0.0);
            velocity = new SimPoint(0.0, startingVelocityMagnitude / VelocityConnversionFactor);

            Run(calculationCyclesPerFrame, false, 100);
        }

        public double CircularOrbitVelocity(double centralMass, double orbitRadius)
        {
            return Math.Sqrt((BigG * centralMass) / orbitRadius) * VelocityConnversionFactor;
        }


        public void Run(int calculationCyclesPerFrame, bool checkAdditionPrecision, long numberOfOrbits)
        {
            OrbitalPhysics orbitalPhysics = new OrbitalPhysics(BigG, SolarSystem.EarthMassKg);

            double timeInterval = 1.0 / 60.0;
            //Stopwatch perfStopwatch = new Stopwatch();

            double scaledTimeInterval = timeInterval * SpeedFactor;
            double timeIntervalPerCycle = scaledTimeInterval / (double)calculationCyclesPerFrame;

            checkingForHalfOrbit = true;
            long orbitsCompleted = 0;

            // Use completed orbits to exit inner loop
            for (long calcCycle = 0; true /* calcCycle < calculationCyclesPerFrame */; calcCycle++)
            {
                SimPoint acceleration = orbitalPhysics.Accelerate(position);

                // Update positon and velocity
                if (checkAdditionPrecision)
                    orbitalPhysics.MoveWithPrecisionCheck(ref position, ref velocity, acceleration, timeIntervalPerCycle);
                else
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
                    if ((orbitsCompleted % 10) == 0)
                        Console.WriteLine("Orbit {0} completed at time {1} in {2:N0} calc cycles. R = {3} km, V = {4} km/h",
                            orbitsCompleted, TimeDisplay.FormatElapsedTime(simElapsedTime, TimeDisplay.BaseUnits.Minutes), calcCycle, position.Magnitude(), 
                            (velocity * VelocityConnversionFactor).Magnitude());
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

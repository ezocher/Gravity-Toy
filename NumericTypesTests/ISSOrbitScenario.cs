using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NumericTypesTests
{
    // See GitHub issue #37
    public class ISSOrbitScenario
    {
        // From SimulationSpace.cs
        public const double BigG_M3PerKgSec2 = 6.6743E-11; // m^3/kg*sec^2
        public const double EarthMassKg = 5.97220E+24;
        public const double KmPerMeter = 1.0 / 1000.0;
        public const double EarthRadiusKm = 6371.0;         // https://en.wikipedia.org/wiki/Earth_radius
        public const double MinutesPerHour = 60.0;
        public const double SecondsPerMinute = 60.0;
        public const double SecondsPerHour = SecondsPerMinute * MinutesPerHour;

        // Averages across one orbit from https://spotthestation.nasa.gov/tracking_map.cfm on 6/19/2020 at 22:25:00 GMT
        public const double ISS_OrbitRadiusKm = 424.72 + EarthRadiusKm;
        public const double ISS_OrbitVelocityKmH = 27570.2;

        public double BigG { get; private set; }
        public string MassUnitsAbbr { get; private set; }

        public string DistanceUnitsAbbr { get; private set; }

        public double SimBoxHeightAndWidth { get; private set; }

        public TimeDisplay.BaseUnits TimeUnits { get; private set; }

        public double TimeUnitsPerUISecond { get; private set; }

        public string VelocityUnitsAbbr { get; private set; }

        // Factor for deriving velocity when either distance or time units in velocity are
        //  different from base time or base distance units
        public double VelocityConnversionFactor { get; private set; }

        // From GravitySim.cs
        private double simElapsedTime;
        private bool checkSim;
        private int simRounding;
        private bool accelerationLimitOn;
        private double accelerationLimit;
        public static double minimumSeparationSquared;

        private SimPoint acceleration;
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
            checkSim = false;
            simRounding = 0;
            accelerationLimitOn = false;
            minimumSeparationSquared = 1.0;
            SpeedFactor = 1.0;
        }

        private const double speedIncrement = 1.25992105;    // Cube root of 2 -> three steps doubles or halves simulation speed

        public void RunFaster() { SpeedFactor *= speedIncrement; }

        public void RunSlower() { SpeedFactor *= 1.0 / speedIncrement; }

        public static void RunISSOrbits(ISSOrbitScenario scenario)
        {
            //SetScenarioName(sim, "Low Earth Orbit (ISS + 4 Starlink satellites) Scenario");

            //sim.ClearSim();
            //sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.GEO));      // LEO or GEO Space -> Km, minutes, Kg, Km/h
            //sim.SetCalculationSettings(new CalculationSettings(200, false, true));
            //sim.SetSimRounding(0);      // Must remain at zero to have circular orbits

            //// === EARTH ===
            //sim.AddBodyActual(SimulationSpace.EarthMassKg, true, SimulationSpace.EarthRadiusKm * 2.0, 3, new SimPoint(0.0, 0.0), new SimPoint(0.0, 0.0));

            ////// === ISS ===
            //sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.5, 1,
            //    new SimPoint(-SimulationSpace.ISS_OrbitRadiusKm, 0.0), new SimPoint(0.0, SimulationSpace.ISS_OrbitVelocityKmH));

            //sim.SetAccelerationLimits(false, 0.0, 0.0);

        }




        public static void Step(double timeInterval, bool simRunning)
        {
            Stopwatch perfStopwatch = new Stopwatch();
            long perfIntervalTicks = 0L;
            bool simStepping = !simRunning;

            double scaledTimeInterval = timeInterval * SpeedFactor;
            SetTimeForTrailMark(simElapsedTime);

            if (simStepping)
            {
                Debug.WriteLine("Elapsed times for {0} bodies:", bodies.Count());
                perfStopwatch.Start();
            }

            if (accelerations == null)
                accelerations = new SimPoint[bodies.Count()];

            if (checkSim)
            {
                if (positions == null)
                    positions = new SimPoint[bodies.Count()];
                if (velocities == null)
                    velocities = new SimPoint[bodies.Count()];

            }

            double timeIntervalPerCycle = scaledTimeInterval / (double)simCalcSettings.CalculationCyclesPerFrame;

            List<SimPoint> otherPositions = new List<SimPoint>();
            List<SimPoint> otherAccelerations = new List<SimPoint>();

            if (checkSim)
            {
                for (int i = 0; i < bodies.Count(); i++)
                {
                    positions[i] = bodies[i].Position;
                    velocities[i] = bodies[i].Velocity;
                }
                Validate5BodyCross(positions, "Positions Before Update");
                Validate5BodyCross(velocities, "Velocities Before Update");
            }

            for (int calcCycle = 0; calcCycle < simCalcSettings.CalculationCyclesPerFrame; calcCycle++)
            {
                // Calculate NBody acceleration
                if (simCalcSettings.CheckAllAdditionPrecision)
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        accelerations[i].X = 0.0;
                        accelerations[i].Y = 0.0;

                        for (int j = 0; j < bodies.Count(); j++)
                            if ((i != j) && bodies[j].IsGravitySource)
                            {
                                SimPoint accel = bodies[i].BodyToBodyAccelerate(bodies[j]);
                                if (simRounding > 0)
                                {
                                    accel.X += Math.Round(accel.X, simRounding, MidpointRounding.AwayFromZero);
                                    accel.Y += Math.Round(accel.Y, simRounding, MidpointRounding.AwayFromZero);
                                }
                                if (FloatingPointUtil.CheckAdditionPrecision(accelerations[i].X, accel.X))
                                    Body.DisplayPrecisionIssue(accelerations[i].X, accel.X, "Accumulating Accel.X", i);
                                accelerations[i].X += accel.X;
                                if (FloatingPointUtil.CheckAdditionPrecision(accelerations[i].Y, accel.Y))
                                    Body.DisplayPrecisionIssue(accelerations[i].Y, accel.Y, "Accumulating Accel.Y", i);
                                accelerations[i].Y += accel.Y;
                            }
                    }
                }
                else if (simRounding > 0)
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        accelerations[i].X = 0.0;
                        accelerations[i].Y = 0.0;

                        for (int j = 0; j < bodies.Count(); j++)
                            if ((i != j) && bodies[j].IsGravitySource)
                            {
                                SimPoint accel = bodies[i].BodyToBodyAccelerate(bodies[j]);
                                accelerations[i].X += Math.Round(accel.X, simRounding, MidpointRounding.AwayFromZero);
                                accelerations[i].Y += Math.Round(accel.Y, simRounding, MidpointRounding.AwayFromZero);
                            }
                    }

                }
                else
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        accelerations[i].X = 0.0;
                        accelerations[i].Y = 0.0;

                        for (int j = 0; j < bodies.Count(); j++)
                            if ((i != j) && bodies[j].IsGravitySource)
                            {
                                SimPoint accel = bodies[i].BodyToBodyAccelerate(bodies[j]);
                                accelerations[i].X += accel.X;
                                accelerations[i].Y += accel.Y;
                            }
                    }
                }

                //if (checkSim) Validate5BodyCross(accelerations, "Accelerations Before Limit and Rounding");

                if (accelerationLimitOn)
                    EnforceAccelerationLimit(accelerations, accelerationLimit);

                if (simRounding > 0)
                    RoundAccelerations(accelerations, simRounding);

                if (checkSim) Validate5BodyCross(accelerations, "Accelerations After Limit and Rounding");

                // Update positons and velocities
                if (simCalcSettings.CheckAllAdditionPrecision)
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        bodies[i].MoveWithPrecisionCheck(accelerations[i], timeIntervalPerCycle);
                    }
                }
                else
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        bodies[i].Move(accelerations[i], timeIntervalPerCycle);
                    }
                }

                if (checkSim)
                {
                    for (int i = 0; i < bodies.Count(); i++)
                    {
                        positions[i] = bodies[i].Position;
                        velocities[i] = bodies[i].Velocity;
                    }
                    Validate5BodyCross(positions, "Positions After Update");
                    Validate5BodyCross(velocities, "Velocities After Update");
                }

                simElapsedTime += timeIntervalPerCycle;

                if ((MainPage.trailsEnabled) && TimeForTrailsMark(simElapsedTime))
                    DrawTrails();
            }
            if (simStepping) perfIntervalTicks = DisplayPerfIntervalElapsed(perfStopwatch, perfIntervalTicks,
                String.Format("Compute N-body accelerations, update positions & velocities ({0} iterations)", simCalcSettings.CalculationCyclesPerFrame));


            // Update rendering
            renderer.BodiesMoved(bodies);
            simPage.UpdateMonitoredValues(bodies[monitoredBody], simElapsedTime);

            if (simStepping) perfIntervalTicks = DisplayPerfIntervalElapsed(perfStopwatch, perfIntervalTicks, "Update transforms of XAML shapes & monitored values");

            if (simStepping)
            {
                Debug.WriteLine("Total elapsed time = {0:F2} ms", (double)perfIntervalTicks / (double)(Stopwatch.Frequency / 1000L));
                Debug.WriteLine("");
            }

            // stepRunning = false;
        }

        // Treats a SimPoint as a vector and calculates its magnitude
        public static double Magnitude(SimPoint v)
        {
            return (Math.Sqrt((v.X * v.X) + (v.Y * v.Y)));
        }

    }
}

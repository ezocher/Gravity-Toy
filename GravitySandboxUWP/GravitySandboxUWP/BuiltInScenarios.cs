﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace GravitySandboxUWP
{
    static class BuiltInScenarios
    {
        private const double toySpaceScenariosDefaultAccelerationLimit = 10.0;
        private const double toySpaceScenariosDeaultMinimumSeparation = 10.0;

        public static void LoadFiveBodiesScenario(GravitySim sim, bool diagonals)
        {
            const double baseSize = 30.0;
            const double baseMass = 100000.0;

            if (diagonals)
                SetScenarioName(sim, "5 Bodies Scenario - Diagonals");
            else
                SetScenarioName(sim, "5 Bodies Scenario - Cross");

            sim.ClearSim();

            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));
            sim.SetAccelerationLimits(true, toySpaceScenariosDefaultAccelerationLimit, toySpaceScenariosDeaultMinimumSeparation);
            sim.SetCalculationSettings(new CalculationSettings(10, false, false));
            sim.SetSimRounding(0);

            if (diagonals)
            {
                sim.AddBody(baseMass, baseSize, 4, GravitySim.BodyStartPosition.StageTopLeft);
                sim.AddBody(baseMass, baseSize, 5, GravitySim.BodyStartPosition.StageTopRight);
                sim.AddBody(baseMass, baseSize, 6, GravitySim.BodyStartPosition.StageBottomRight);
                sim.AddBody(baseMass, baseSize, 8, GravitySim.BodyStartPosition.StageBottomLeft);
            }
            else
            {
                sim.AddBody(baseMass, baseSize, 3, GravitySim.BodyStartPosition.StageLeft);
                sim.AddBody(baseMass, baseSize, 2, GravitySim.BodyStartPosition.StageTop);
                sim.AddBody(baseMass, baseSize, 1, GravitySim.BodyStartPosition.StageRight);
                sim.AddBody(baseMass, baseSize, 7, GravitySim.BodyStartPosition.StageBottom);
            }
            sim.AddBody(baseMass, baseSize, 9, GravitySim.BodyStartPosition.CenterOfTheUniverse);
            sim.SetMonitoredBody(4);   // center = 4
            sim.SetMonitoredValues();
            sim.SetCheckSim(false);
            //sim.SetCheckSim(!diagonals); // Sim checking is only implemented for the Cross version
        }

        public static void LoadFourBodiesScenario(GravitySim sim)
        {
            const double baseMass = 100000.0;

            SetScenarioName(sim, "4 Bodies Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));
            sim.AddBody(baseMass, 4.0, 3, GravitySim.BodyStartPosition.StageLeft);
            sim.AddBody(baseMass, 4.0, 2, GravitySim.BodyStartPosition.StageTop);
            sim.AddBody(baseMass, 4.0, 1, GravitySim.BodyStartPosition.StageRight);
            sim.AddBody(baseMass, 4.0, 7, GravitySim.BodyStartPosition.StageBottom);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            // sim.SetSimRounding(3);
            sim.SetAccelerationLimits(true, toySpaceScenariosDefaultAccelerationLimit, toySpaceScenariosDeaultMinimumSeparation);
        }

        public static void LoadNineBodiesScenario(GravitySim sim)
        {
            const double baseSize = 30.0;
            const double baseMass = 100000.0;

            SetScenarioName(sim, "9 Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));
            sim.SetAccelerationLimits(true, toySpaceScenariosDefaultAccelerationLimit, toySpaceScenariosDeaultMinimumSeparation);
            sim.SetCalculationSettings(new CalculationSettings(10, false, false));
            sim.SetSimRounding(8);      // 0 -> Symmetry breaks down at ~75 sec.
                                // 8 -> Works well

            sim.SetCheckSim(false);

            sim.AddBody(baseMass, baseSize, 3, GravitySim.BodyStartPosition.StageLeft);
            sim.AddBody(baseMass, baseSize, 2, GravitySim.BodyStartPosition.StageTop);
            sim.AddBody(baseMass, baseSize, 1, GravitySim.BodyStartPosition.StageRight);
            sim.AddBody(baseMass, baseSize, 7, GravitySim.BodyStartPosition.StageBottom);
            sim.AddBody(baseMass, baseSize, 4, GravitySim.BodyStartPosition.StageTopLeft);
            sim.AddBody(baseMass, baseSize, 5, GravitySim.BodyStartPosition.StageTopRight);
            sim.AddBody(baseMass, baseSize, 6, GravitySim.BodyStartPosition.StageBottomRight);
            sim.AddBody(baseMass, baseSize, 8, GravitySim.BodyStartPosition.StageBottomLeft);
            sim.AddBody(baseMass, baseSize, 9, GravitySim.BodyStartPosition.CenterOfTheUniverse);

            sim.SetMonitoredBody(8);   // center = 8
            sim.SetMonitoredValues();
        }

        public static void LoadToyOrbitingBodiesScenario(GravitySim sim)
        {
            const double baseMass = 100000.0;

            SetScenarioName(sim, "Orbiting Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));
            sim.SetCalculationSettings(new CalculationSettings(10, false, false));       // To produce braided pattern of trails change calcsPerFrame to 1
                                                          // calcsPerFrame = 10000 is good for one body orbiting
            
            // sim.AddBody(baseMass, 2.0, 1, GravitySim.bodyStartPosition.stageLeft, new SimPoint(0.0, 50.5), false);
            //sim.AddBody(baseMass, 2.0, 2, GravitySim.bodyStartPosition.stageLeft, new SimPoint(0.0, 40.0), false);
            //sim.AddBody(baseMass, 2.0, 4, GravitySim.bodyStartPosition.stageLeft, new SimPoint(0.0, 30.0), false);
            //sim.AddBody(baseMass, 2.0, 6, GravitySim.bodyStartPosition.stageLeft, new SimPoint(0.0, 20.0), false);
            //sim.AddBody(baseMass, 2.0, 3, GravitySim.bodyStartPosition.stageLeft, new SimPoint(0.0, 60.0), false);
            sim.AddBody(0.1 * baseMass, 40.0, 5, GravitySim.BodyStartPosition.StageTopLeft, new SimPoint(-38.9, -38.9), true);
            sim.AddBody(10.0 * baseMass, 80.0, 8, GravitySim.BodyStartPosition.CenterOfTheUniverse, new SimPoint(0.389, 0.380), true);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimits(false, 0.0, 0.0);
        }

        public static void LoadXRandomBodies(GravitySim sim, int numBodies, Renderer.ColorScheme colorScheme)
        {
            const double baseMass = 100000.0;

            SetScenarioName(sim, String.Format("{0} Random Bodies Scenario ({1})", numBodies, colorScheme));
            
            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 4.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size * baseMass, size * 10.0, RandomColor(colorScheme, rand), GravitySim.BodyStartPosition.RandomScreenPosition);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimits(true, toySpaceScenariosDefaultAccelerationLimit, toySpaceScenariosDeaultMinimumSeparation);
        }

        // Use GravitySim.BodyStartPosition.RandomDenseCenterCircularCluster OR GravitySim.BodyStartPosition.RandomUniformDensityCircularCluster
        // sizeFactor 1.0 = tiny, sizeFactor 5.0 = "normal"
        public static void LoadXBodiesCircularCluster(GravitySim sim, int numBodies, double sizeFactor, Renderer.ColorScheme colorScheme, GravitySim.BodyStartPosition bodyStartPosition)
        {
            const double baseMass = 100000.0;

            SetScenarioName(sim, String.Format("{0} Bodies Circular Cluster Scenario ({1})", numBodies, colorScheme));

            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimulationSpace(SimulationSpace.Space.Toy));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 3.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size * baseMass, size * sizeFactor, RandomColor(colorScheme, rand), bodyStartPosition);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimits(true, toySpaceScenariosDefaultAccelerationLimit, toySpaceScenariosDeaultMinimumSeparation);
        }

        public static void LoadLowEarthOrbit(GravitySim sim)
        {
            // To make a braided spiral with ISS trails use calc setting of 2 and then speed up by 800%
            //  4 and 1600% are also interesting

            SetScenarioName(sim, "Low Earth Orbit (ISS + 4 Starlink satellites) Scenario");

            sim.ClearSim();
            SimulationSpace simulationSpace = new SimulationSpace(SimulationSpace.Space.LEO);   // LEO or GEO Space -> Km, minutes, Kg, Km/h
            sim.SetSimSpace(simulationSpace);      
            sim.SetCalculationSettings(new CalculationSettings(100, false, false));
            sim.SetSimRounding(0);      // Must remain at zero to have circular orbits

            // === EARTH ===
            sim.AddBodyActual(SolarSystem.EarthMassKg, true, SolarSystem.EarthRadiusKm * 2.0, 3, new SimPoint(0.0, 0.0), new SimPoint(0.0, 0.0));

            //// === ISS ===
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.5, 1,
                new SimPoint(-SolarSystem.ISS_OrbitRadiusKm, 0.0), new SimPoint(0.0, simulationSpace.CircularOrbitVelocity(SolarSystem.EarthMassKg, SolarSystem.ISS_OrbitRadiusKm)));

            // Satellites - Starlink

            // 12 Starlinks
            //double[] startingAngles = { 30.0, 45.0, 60.0, 120.0, 135.0, 150.0, 210.0, 225.0, 240.0, 300.0, 315.0, 330.0 };
            //foreach (double angle in startingAngles)

            // 36 Starlinks
            for (double angle = 0.0; angle < 360.0; angle += 10.0)
            {
                SimPoint position, velocity;
                simulationSpace.InitializeCircularOrbit(angle, SolarSystem.StarlinkOrbitRadiusKm, SolarSystem.EarthMassKg,
                    out position, out velocity);
                sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx, (int)Renderer.ColorNumber.BodyColorMedGrey,
                    position, velocity);
            }

            // GPS
            //sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 5.0, 4,
            //    new SimPoint(-SolarSystem.GPS_OrbitRadiusKm, 0.0), new SimPoint(0.0, simulationSpace.CircularOrbitVelocity(SolarSystem.EarthMassKg, SolarSystem.GPS_OrbitRadiusKm)));

            // Geosynchronus orbit
            //sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.0, 5,
            //    new SimPoint(-SolarSystem.GeosynchronousOrbitRadiusKm, 0.0), new SimPoint(0.0, simulationSpace.CircularOrbitVelocity(SolarSystem.EarthMassKg, SolarSystem.GeosynchronousOrbitRadiusKm)));

            sim.SetMonitoredBody(1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimits(false, 0.0, 0.0);
        }

        private static void SetScenarioName(GravitySim sim, string name)
        {
            Debug.WriteLine("Loaded " + name);
            sim.SetMessage("Running " + name);
            sim.ScenarioName = name;
        }

        private static int RandomColor(Renderer.ColorScheme colorScheme, Random random)
        {
            switch (colorScheme)
            {
                case Renderer.ColorScheme.AllColors:
                    return random.Next(Renderer.firstColorIndex, Renderer.lastColorIndex);    // Decided to exclude the darkest gray value from this, thus the missing "+1"
                case Renderer.ColorScheme.GrayColors:
                    return random.Next(Renderer.firstMonochromeColorIndex, Renderer.lastColorIndex + 1);
                case Renderer.ColorScheme.PastelColors:
                    return random.Next(Renderer.firstColorIndex, Renderer.lastPastelColorIndex + 1);
            }
            return 1; // Added to make the compiler happy, not reachable
        }
    }
}

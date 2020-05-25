using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace GravitySandboxUWP
{
    class BuiltInScenarios
    {

        public static void LoadFiveBodiesScenario(GravitySim sim)
        {
            const double baseMass = 100000.0;

            Debug.WriteLine("Loaded 5 bodies scenario");
            sim.SetMessage("Running 5 Bodies Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.AddBody(baseMass, 3.0, 3, GravitySim.BodyStartPosition.StageLeft);
            sim.AddBody(baseMass, 3.0, 2, GravitySim.BodyStartPosition.StageTop);
            sim.AddBody(baseMass, 3.0, 1, GravitySim.BodyStartPosition.StageRight);
            sim.AddBody(baseMass, 3.0, 7, GravitySim.BodyStartPosition.StageBottom);
            sim.AddBody(baseMass, 10.0, 6, GravitySim.BodyStartPosition.CenterOfTheUniverse);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }


        public static void LoadFourBodiesScenario(GravitySim sim)
        {
            const double baseMass = 100000.0;

            Debug.WriteLine("Loaded 4 bodies scenario");
            sim.SetMessage("Running 4 Bodies Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.AddBody(baseMass, 4.0, 3, GravitySim.BodyStartPosition.StageLeft);
            sim.AddBody(baseMass, 4.0, 2, GravitySim.BodyStartPosition.StageTop);
            sim.AddBody(baseMass, 4.0, 1, GravitySim.BodyStartPosition.StageRight);
            sim.AddBody(baseMass, 4.0, 7, GravitySim.BodyStartPosition.StageBottom);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            // sim.SetSimRounding(3);
        }

        public static void LoadNineBodiesScenario(GravitySim sim)
        {
            const double baseSize = 30.0;
            const double baseMass = 100000.0;

            Debug.WriteLine("Loaded 9 bodies scenario");
            sim.SetMessage("Running 9 Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.SetCalculationSettings(new CalculationSettings(100, false));

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
            // sim.SetCheckSim(true);
            sim.SetSimRounding(2);
            sim.SetAccelerationLimit(true);
        }

        public static void LoadOrbitingBodiesScenario(GravitySim sim)
        {
            const double baseMass = 100000.0;

            Debug.WriteLine("Loaded orbiting bodies scenario");
            sim.SetMessage("Running Orbiting Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.SetCalculationSettings(new CalculationSettings(5000, false));       // To produce braided pattern of trails change calcsPerFrame to 1
                                                                                    // calcsPerFrame = 10000 is good for one body orbiting
            
            // sim.AddBody(baseMass, 2.0, 1, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 50.5), false);
            //sim.AddBody(baseMass, 2.0, 2, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 40.0), false);
            //sim.AddBody(baseMass, 2.0, 4, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 30.0), false);
            //sim.AddBody(baseMass, 2.0, 6, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 20.0), false);
            //sim.AddBody(baseMass, 2.0, 3, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 60.0), false);
            sim.AddBody(0.1 * baseMass, 40.0, 5, GravitySim.BodyStartPosition.StageTopLeft, new Point(-38.9, -38.9), true);
            sim.AddBody(10.0 * baseMass, 80.0, 8, GravitySim.BodyStartPosition.CenterOfTheUniverse, new Point(0.389, 0.380), true);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(false);
        }

        public static void LoadXRandomBodies(GravitySim sim, int numBodies, SimRenderer.ColorScheme colorScheme)
        {
            const double baseMass = 100000.0;

            Debug.WriteLine(String.Format("Loading XRandomBodies scenario with {0} bodies", numBodies));
            sim.SetMessage(String.Format("Running {0} Random Bodies Scenario", numBodies));
            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 4.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size * baseMass, size * 10.0, RandomColor(colorScheme, rand), GravitySim.BodyStartPosition.RandomScreenPosition);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(true);
        }

        // Use GravitySim.BodyStartPosition.RandomDenseCenterCircularCluster OR GravitySim.BodyStartPosition.RandomUniformDensityCircularCluster
        // sizeFactor 1.0 = tiny, sizeFactor 5.0 = "normal"
        public static void LoadXBodiesCircularCluster(GravitySim sim, int numBodies, double sizeFactor, SimRenderer.ColorScheme colorScheme, GravitySim.BodyStartPosition bodyStartPosition)
        {
            const double baseMass = 100000.0;

            Debug.WriteLine(String.Format("Loading XBodiesCircularCluster scenario with {0} bodies", numBodies));
            sim.SetMessage(String.Format("Running {0} Bodies Circular Cluster Scenario ({1})", numBodies, colorScheme));
            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 3.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size * baseMass, size * sizeFactor, RandomColor(colorScheme, rand), bodyStartPosition);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(false);
        }

        public static void LoadLowEarthOrbit(GravitySim sim)
        {
            Debug.WriteLine("Loaded Low Earth Orbit (ISS + 4 Starlink) scenario");
            sim.SetMessage("Running Low Earth Orbit (ISS + 4 Starlink) Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.LEOSpace));      // LEO Space -> Km, minutes, Kg, Km/h
            sim.SetCalculationSettings(new CalculationSettings(500, false));

            // === EARTH ===
            sim.AddBodyActual(SimSpace.EarthMassKg, true, SimSpace.EarthRadiusKm * 2.0, 3, new Point(0.0, 0.0), new Point(0.0, 0.0));

            // === ISS ===
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.5, 1,
                new Point(-SimSpace.ISS_OrbitRadiusKm, 0.0), new Point(0.0, SimSpace.ISS_OrbitVelocityKmH));

            // Satellites - Starlink times 4, GPS, Geosynchronous
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx, (int)SimRenderer.ColorNumber.BodyColorWhite,
                new Point(0.0, SimSpace.StarlinkOrbitRadiusKm), new Point(SimSpace.StarlinkOrbitVelocityKmH, 0.0));
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx, (int)SimRenderer.ColorNumber.BodyColorWhite,
                new Point(-SimSpace.StarlinkOrbitRadiusKm, 0.0), new Point(0.0, SimSpace.StarlinkOrbitVelocityKmH));
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx, (int)SimRenderer.ColorNumber.BodyColorWhite,
                new Point(0.0, -SimSpace.StarlinkOrbitRadiusKm), new Point(-SimSpace.StarlinkOrbitVelocityKmH, 0.0));
            sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx, (int)SimRenderer.ColorNumber.BodyColorWhite,
                new Point(SimSpace.StarlinkOrbitRadiusKm, 0.0), new Point(0.0, -SimSpace.StarlinkOrbitVelocityKmH));

            //sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.5, 4,
            //    new Point(-SimSpace.GPS_OrbitRadiusKm, 0.0), new Point(0.0, SimSpace.GPS_OrbitVelocityKmH));

            ////sim.AddBodyActual(0.0, false, sim.simSpace.SmallestBodySizePx * 2.5, 5,
            //new Point(-SimSpace.GeosynchronousOrbitRadiusKm, 0.0), new Point(0.0, SimSpace.GeosynchronousOrbitVelocityKmH));

            sim.SetMonitoredBody(2);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(false);
        }

        private static int RandomColor(SimRenderer.ColorScheme colorScheme, Random random)
        {
            switch (colorScheme)
            {
                case SimRenderer.ColorScheme.AllColors:
                    return random.Next(SimRenderer.firstColorIndex, SimRenderer.lastColorIndex);    // Decided to exclude the darkest gray value from this, thus the missing "+1"
                case SimRenderer.ColorScheme.GrayColors:
                    return random.Next(SimRenderer.firstMonochromeColorIndex, SimRenderer.lastColorIndex + 1);
                case SimRenderer.ColorScheme.PastelColors:
                    return random.Next(SimRenderer.firstColorIndex, SimRenderer.lastPastelColorIndex + 1);
            }
            return 1; // Added to make the compiler happy, not reachable
        }
    }
}

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
            Debug.WriteLine("Loaded 5 bodies scenario");
            sim.SetMessage("Running 5 Bodies Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.AddBody(1.0, 3.0, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, 3.0, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, 3.0, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(1.0, 3.0, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.AddBody(1.0, 10.0, 6, GravitySim.bodyStartPosition.centerOfTheUniverse);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }


        public static void LoadFourBodiesScenario(GravitySim sim)
        {
            Debug.WriteLine("Loaded 4 bodies scenario");
            sim.SetMessage("Running 4 Bodies Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.AddBody(1.0, 4.0, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, 4.0, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, 4.0, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(1.0, 4.0, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            // sim.SetSimRounding(3);
        }

        public static void LoadNineBodiesScenario(GravitySim sim)
        {
            const double baseSize = 30.0;

            Debug.WriteLine("Loaded 9 bodies scenario");
            sim.SetMessage("Running 9 Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            sim.AddBody(1.0, baseSize, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, baseSize, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, baseSize, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(1.0, baseSize, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.AddBody(1.0, baseSize, 4, GravitySim.bodyStartPosition.stageTopLeft);
            sim.AddBody(1.0, baseSize, 5, GravitySim.bodyStartPosition.stageTopRight);
            sim.AddBody(1.0, baseSize, 6, GravitySim.bodyStartPosition.stageBottomRight);
            sim.AddBody(1.0, baseSize, 8, GravitySim.bodyStartPosition.stageBottomLeft);
            sim.AddBody(1.0, baseSize, 9, GravitySim.bodyStartPosition.centerOfTheUniverse);
            sim.SetMonitoredBody(0);   // center = 8
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            sim.SetSimRounding(2);
            sim.SetAccelerationLimit(true);
        }

        public static void LoadOrbitingBodiesScenario(GravitySim sim)
        {
            Debug.WriteLine("Loaded orbiting bodies scenario");
            sim.SetMessage("Running Orbiting Bodies Scenario");

            sim.ClearSim();

            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));
            // sim.AddBody(1.0, 2.0, 1, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 50.5), false);
            //sim.AddBody(1.0, 2.0, 2, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 40.0), false);
            //sim.AddBody(1.0, 2.0, 4, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 30.0), false);
            //sim.AddBody(1.0, 2.0, 6, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 20.0), false);
            //sim.AddBody(1.0, 2.0, 3, GravitySim.bodyStartPosition.stageLeft, new Point(0.0, 60.0), false);
            sim.AddBody(0.1, 40.0, 5, GravitySim.bodyStartPosition.stageTopLeft, new Point(-38.9, -38.9), true);
            sim.AddBody(10.0, 80.0, 8, GravitySim.bodyStartPosition.centerOfTheUniverse, new Point(0.389, 0.380), true);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }

        public static void LoadXRandomBodies(GravitySim sim, int numBodies, SimRenderer.ColorScheme colorScheme)
        {
            Debug.WriteLine(String.Format("Loading XRandomBodies scenario with {0} bodies", numBodies));
            sim.SetMessage(String.Format("Running {0} Random Bodies Scenario", numBodies));
            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 4.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size, size * 10.0, RandomColor(colorScheme, rand), GravitySim.bodyStartPosition.randomScreenPosition);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(true);
        }

        public static void LoadXBodiesCircularCluster(GravitySim sim, int numBodies, SimRenderer.ColorScheme colorScheme)
        {
            Debug.WriteLine(String.Format("Loading XBodiesCircularCluster scenario with {0} bodies", numBodies));
            sim.SetMessage(String.Format("Running {0} Bodies Circular Cluster Scenario ({1})", numBodies, colorScheme));
            Random rand = new Random();

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.ToySpace));

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 3.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size, 2.0 * size, RandomColor(colorScheme, rand), GravitySim.bodyStartPosition.randomCircularCluster);
            }

            sim.SetMonitoredBody(numBodies - 1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(false);
        }

        public static void LoadLowEarthOrbit(GravitySim sim)
        {
            Debug.WriteLine("Loaded Low Earth Orbit scenario");
            sim.SetMessage("Running Low Earth Orbit Scenario");

            sim.ClearSim();
            sim.SetSimSpace(new SimSpace(SimSpace.DefinedSpace.LEOSpace));

            // EARTH
            sim.AddBodyActual(5.97220E+24, true, SimSpace.EarthRadiusKm * 2.0, 3, new Point(0.0, 0.0), new Point(0.0, 0.0));

            // Satellites, space station, etc.
            // Calculated for circular orbit of 200 km it is 7.79 km/s  (28,000 km/h)
            sim.AddBodyActual(0.0, false, 150.0, 1, new Point(-(SimSpace.EarthRadiusKm + 200.0), 0.0), new Point(0.0, 28000.0));


            sim.SetMonitoredBody(1);
            sim.SetMonitoredValues();
            sim.SetAccelerationLimit(false);
        }

        private static int RandomColor(SimRenderer.ColorScheme colorScheme, Random random)
        {
            switch (colorScheme)
            {
                case SimRenderer.ColorScheme.allColors:
                    return random.Next(SimRenderer.firstColorIndex, SimRenderer.lastColorIndex);    // Decided to exclude the darkest gray value from this, thus the missing "+1"
                case SimRenderer.ColorScheme.grayColors:
                    return random.Next(SimRenderer.firstMonochromeColorIndex, SimRenderer.lastColorIndex + 1);
                case SimRenderer.ColorScheme.pastelColors:
                    return random.Next(SimRenderer.firstColorIndex, SimRenderer.lastPastelColorIndex + 1);
            }
            return 1; // Added to make the compiler happy, not reachable
        }
    }
}

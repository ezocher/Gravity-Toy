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
            sim.ClearSim();
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
            sim.ClearSim();
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
            const double baseSize = 3.0d;

            sim.ClearSim();
            sim.AddBody(1.0, baseSize, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, baseSize, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, baseSize, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(1.0, baseSize, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.AddBody(1.0, baseSize, 4, GravitySim.bodyStartPosition.stageTopLeft);
            sim.AddBody(1.0, baseSize, 5, GravitySim.bodyStartPosition.stageTopRight);
            sim.AddBody(1.0, baseSize, 6, GravitySim.bodyStartPosition.stageBottomRight);
            sim.AddBody(1.0, baseSize, 8, GravitySim.bodyStartPosition.stageBottomLeft);
            sim.AddBody(1.0, baseSize, 9, GravitySim.bodyStartPosition.centerOfTheUniverse);
            sim.SetMonitoredBody(8);
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            sim.SetSimRounding(2);
            Debug.WriteLine("Loaded 9 bodies scenario");
        }


        public static void LoadTwoBodiesScenario(GravitySim sim)
        {
            sim.ClearSim();
            sim.AddBody(1.0, 1.0, 3, GravitySim.bodyStartPosition.stageLeft, new Point(-10000.0, 10000.0));
            sim.AddBody(333000.0, 20.0, 1, GravitySim.bodyStartPosition.stageRight);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }


        public static void LoadFakeEarthScenario(GravitySim sim)
        {
            sim.ClearSim();
            sim.AddBody(1.0, 2.0, 1, GravitySim.bodyStartPosition.screenTop);
            sim.AddBody(1000.0, 20.0, 7, GravitySim.bodyStartPosition.screenBottom);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }

        public static void LoadXRandomBodies(GravitySim sim, int numBodies, SimRender.ColorScheme colorScheme)
        {
            Debug.WriteLine(String.Format("Loading XRandomBodies scenario with {0} bodies", numBodies));
            Random rand = new Random();

            sim.ClearSim();

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 4.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size, size, RandomColor(colorScheme, rand), GravitySim.bodyStartPosition.randomScreenPosition);
            }

            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }

        public static void LoadXBodiesCircularCluster(GravitySim sim, int numBodies, SimRender.ColorScheme colorScheme)
        {
            Debug.WriteLine(String.Format("Loading XBodiesCircularCluster scenario with {0} bodies", numBodies));
            Random rand = new Random();

            sim.ClearSim();

            for (int i = 0; i < numBodies; i++)
            {
                double size = rand.NextDouble() * 3.0 + 1.0;   // Mass as square of size
                sim.AddBody(size * size, 0.5d, RandomColor(colorScheme, rand), GravitySim.bodyStartPosition.randomCircularCluster);
            }

            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }

        private static int RandomColor(SimRender.ColorScheme colorScheme, Random random)
        {
            switch (colorScheme)
            {
                case SimRender.ColorScheme.allColors:
                    return random.Next(SimRender.firstColorIndex, SimRender.lastColorIndex);    // Decided to exclude the darkest gray value from this, thus the missing "+1"
                case SimRender.ColorScheme.grayColors:
                    return random.Next(SimRender.firstMonochromeColorIndex, SimRender.lastColorIndex + 1);
                case SimRender.ColorScheme.pastelColors:
                    return random.Next(SimRender.firstColorIndex, SimRender.lastPastelColorIndex + 1);
            }
            return 1; // Added to make the compiler happy, not reachable
        }
    }
}

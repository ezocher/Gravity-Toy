using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace GravitySandboxUWP
{
    class BuiltInScenarios
    {
        /*
        public static void LoadFiveBodiesScenario(GravitySim sim)
        {
            sim.ClearSim();
            sim.AddBody(1.0, 3.0, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, 3.0, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, 3.0, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(1.0, 3.0, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.AddBody(1.0, 5.0, 6, GravitySim.bodyStartPosition.centerOfTheUniverse);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }
         * */

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
            sim.SetSimRounding(3);
        }

        public static void LoadNineBodiesScenario(GravitySim sim)
        {
            const double baseSize = 3.0d;

            sim.ClearSim();
            sim.AddBody(1.0, baseSize, 3, GravitySim.bodyStartPosition.stageLeft);
            sim.AddBody(1.0, baseSize, 2, GravitySim.bodyStartPosition.stageTop);
            sim.AddBody(1.0, baseSize, 1, GravitySim.bodyStartPosition.stageRight);
            sim.AddBody(9.0, baseSize * 3.0, 7, GravitySim.bodyStartPosition.stageBottom);
            sim.AddBody(1.0, baseSize, 4, GravitySim.bodyStartPosition.stageTopLeft);
            sim.AddBody(1.0, baseSize, 5, GravitySim.bodyStartPosition.stageTopRight);
            sim.AddBody(1.0, baseSize, 6, GravitySim.bodyStartPosition.stageBottomRight);
            sim.AddBody(1.0, baseSize, 8, GravitySim.bodyStartPosition.stageBottomLeft);
            sim.AddBody(4.0, baseSize * 2.0, 9, GravitySim.bodyStartPosition.centerOfTheUniverse);
            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
            // sim.SetCheckSim(true);
            // sim.SetSimRounding(2);
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

        public static void LoadThreeHundredRandomBodies(GravitySim sim)
        {
            Random rand = new Random();

            sim.ClearSim();

            for (int i = 0; i < 200; i++)
            {
                sim.AddBody(1.0, rand.NextDouble() * 4.0 + 1.0, rand.Next(SimRender.firstColorIndex, SimRender.lastColorIndex), GravitySim.bodyStartPosition.randomScreenPosition);
            }

            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }

        public static void LoadOneHundredBodiesCircularCluster(GravitySim sim)
        {
            Random rand = new Random();


            sim.ClearSim();

            for (int i = 0; i < 100; i++)
            {
                double size = rand.NextDouble() * 3.0 + 1.0;   // TBD: Try mass as square of size
                if (size > 3.0)
                    size += (size - 3.0) * 2.0;
                sim.AddBody(size, /* rand.NextDouble() * 4.0 + 1.0*/ size, rand.Next(SimRender.firstColorIndex, SimRender.lastColorIndex), GravitySim.bodyStartPosition.randomCircularCluster);
            }

            sim.SetMonitoredBody(0);
            sim.SetMonitoredValues();
        }


    }
}

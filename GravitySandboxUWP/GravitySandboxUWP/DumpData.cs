using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace GravitySandboxUWP
{
    class DumpData
    {
        public static bool loggingOn = false;
        public static bool collectingData = false;

        public static string scenarioName;
        public static int monitoredBody;

        public static List<double> times;
        public static List<double> timeIntervals;
        public static List<SimPoint> prePositions;
        public static List<SimPoint> preVelocities;
        public static List<List<SimPoint>> otherBodyPositions;
        public static List<List<SimPoint>> otherBodyAccelerations;
        public static List<SimPoint> totalAccelerations;
        public static List<SimPoint> afterAccLimitAccelerations; 
        public static List<SimPoint> afterRoundingAccelerations;
        public static List<SimPoint> postPositions;
        public static List<SimPoint> postVelocities;

        // Start by logging everything about the monitored body
        public static void BeginAccumulatingData(GravitySim sim)
        {
            // Create in-memory structures to hold accumulated data
            scenarioName = GravitySim.currentScenarioName;
            monitoredBody = sim.monitoredBody;

            ClearAccumulatedData();
            sim.simPage.SetMessageText("*** RECORDING IN PROGRESS ***");
        }

        private static void ClearAccumulatedData()
        {
            times = new List<double>();
            timeIntervals = new List<double>();
            prePositions = new List<SimPoint>();
            preVelocities = new List<SimPoint>();
            otherBodyPositions = new List<List<SimPoint>>();
            otherBodyAccelerations = new List<List<SimPoint>>();
            totalAccelerations = new List<SimPoint>();
            afterAccLimitAccelerations = new List<SimPoint>();
            afterRoundingAccelerations = new List<SimPoint>();
            postPositions = new List<SimPoint>();
            postVelocities = new List<SimPoint>();
        }

        

        public async static void DumpAccumulatedData(GravitySim sim)
        {
            sim.simPage.SetMessageText("*** DUMPING DATA...");

            const string recordHeaderPart1 = "Num\tTime\tTime Int.\tPos Before\t\tVel Before\t\t";
            string recordHeaderPart2 = "Other Positions\t\t";
            string recordHeaderPart3 = "Accelerations from Other Bodies\t\t";
            const string recordHeaderPart4 = "Total Acc\t\tAfter Acc Limit\t\tAfter Round\t\tPos After\t\tVel After";
            int numberOfOtherBodies;

            if (otherBodyPositions.Count > 0)
                numberOfOtherBodies = otherBodyPositions[0].Count;
            else
            {
                Debugger.Break();
                numberOfOtherBodies = 0;
            }

            if ((numberOfOtherBodies - 1) > 0)
            {
                string additionalTabs = new string('\t', (numberOfOtherBodies - 1) * 2);
                recordHeaderPart2 += additionalTabs;
                recordHeaderPart3 += additionalTabs;
            }

            const string recordFieldsFormatPart1 = "{0}\t{1:R}\t{2:R}\t{3:R}\t{4:R}\t{5:R}\t{6:R}\t";
            const string recordFieldsFormatPart2 = "{0:R}\t{1:R}\t";
            const string recordFieldsFormatPart3 = recordFieldsFormatPart2;
            const string recordFieldsFormatPart4 = "{0:R}\t{1:R}\t{2:R}\t{3:R}\t{4:R}\t{5:R}\t{6:R}\t{7:R}\t{8:R}\t{9:R}";

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // string dumpFileName = FileUtil.GetUniqueFileName(scenarioName + ".txt");
            string dumpFileName = scenarioName + ".txt";

            StorageFile dumpFile = await storageFolder.CreateFileAsync(dumpFileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await dumpFile.OpenStreamForWriteAsync())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("{0} Dump of body # {1}\n", scenarioName, monitoredBody);
                    writer.WriteLine("{0}{1}{2}{3}", recordHeaderPart1, recordHeaderPart2, recordHeaderPart3, recordHeaderPart4);

                    for (int i = 0; i < times.Count; i++)
                    {
                        writer.Write(recordFieldsFormatPart1, i + 1, times[i], timeIntervals[i], prePositions[i].X, prePositions[i].Y, preVelocities[i].X, preVelocities[i].Y);
                        foreach (SimPoint simPoint in otherBodyPositions[i])
                            writer.Write(recordFieldsFormatPart2, simPoint.X, simPoint.Y);
                        foreach (SimPoint simPoint in otherBodyAccelerations[i])
                            writer.Write(recordFieldsFormatPart3, simPoint.X, simPoint.Y);
                        writer.WriteLine(recordFieldsFormatPart4, totalAccelerations[i].X, totalAccelerations[i].Y, afterAccLimitAccelerations[i].X, afterAccLimitAccelerations[i].Y, 
                          afterRoundingAccelerations[i].X, afterRoundingAccelerations[i].Y, postPositions[i].X, postPositions[i].Y, postVelocities[i].X, postVelocities[i].Y);
                    }
                }
            }

            ClearAccumulatedData();

            sim.simPage.AppendMessageText("  ...Data Dump completed");
        }

    }

}
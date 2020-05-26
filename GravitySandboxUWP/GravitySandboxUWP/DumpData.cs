using System;
using System.Collections.Generic;
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
        public static List<Point> prePositions;
        public static List<Point> preVelocities;
        public static List<List<Point>> otherBodyPositions;
        public static List<List<Point>> otherBodyAccelerations;
        public static List<Point> totalAccelerations;
        public static List<Point> afterAccLimitAccelerations; 
        public static List<Point> afterRoundingAccelerations;
        public static List<Point> postPositions;
        public static List<Point> postVelocities;

        // Start by logging everything about the monitored body
        public static void BeginAccumulatingData(GravitySim sim)
        {
            // Create in-memory structures to hold accumulated data
            scenarioName = GravitySim.currentScenarioName;
            monitoredBody = sim.monitoredBody;

            ClearAccumulatedData();
        }

        private static void ClearAccumulatedData()
        {
            times = new List<double>();
            timeIntervals = new List<double>();
            prePositions = new List<Point>();
            preVelocities = new List<Point>();
            otherBodyPositions = new List<List<Point>>();
            otherBodyAccelerations = new List<List<Point>>();
            totalAccelerations = new List<Point>();
            afterAccLimitAccelerations = new List<Point>();
            afterRoundingAccelerations = new List<Point>();
            postPositions = new List<Point>();
            postVelocities = new List<Point>();
        }


        public async static void DumpAccumulatedData()
        {
            const string dumpFileHeader = "Num\tTime\tTime Int.\tPos Before\tVel Before\tOther Positions\tAccelerations\tTotal Acc\tAfter Acc Limit\tAfter Round\tPos After\tVel After";
            const string firstFourFieldsFormat = "{0}\t{1}\t{2}\t{3}\t{4}";
            const string lastFiveFieldsFormat = "\t{0}\t{1}\t{2}\t{3}\t{4}";

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            // string dumpFileName = FileUtil.GetUniqueFileName(scenarioName + ".txt");
            string dumpFileName = scenarioName + ".txt";

            StorageFile dumpFile = await storageFolder.CreateFileAsync(dumpFileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await dumpFile.OpenStreamForWriteAsync())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("{0} Dump of body # {1}\n", scenarioName, monitoredBody);
                    writer.WriteLine(dumpFileHeader);

                    for (int i = 0; i < times.Count; i++)
                    {
                        writer.Write(firstFourFieldsFormat, i + 1, times[i], timeIntervals[i], prePositions[i], preVelocities[i]);
                        foreach (Point point in otherBodyPositions[i])
                            writer.Write("\t{0}", point);
                        foreach (Point point in otherBodyAccelerations[i])
                            writer.Write("\t{0}", point);
                        writer.WriteLine(lastFiveFieldsFormat, totalAccelerations[i], afterAccLimitAccelerations[i], afterRoundingAccelerations[i], postPositions[i], postVelocities[i]);
                    }
                }
            }

            ClearAccumulatedData();
        }

    }

}
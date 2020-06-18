using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace GravitySandboxUWP
{
    // The simulation owns the bodies in the simulation space and holds references to their graphics incarnations (which are animated by the SimRender class)
    class GravitySim
    {
        
        public enum BodyStartPosition
        {
            StageLeft, StageRight, StageTop, StageBottom, StageTopLeft, StageTopRight, StageBottomRight, StageBottomLeft,
            ScreenTop, ScreenLeft, ScreenRight, ScreenBottom,
            CenterOfTheUniverse,
            RandomStagePosition, RandomScreenPosition, 
            RandomDenseCenterCircularCluster, RandomUniformDensityCircularCluster
        };

        public SimSpace simSpace;
        public CalculationSettings simCalcSettings;
        public SimRenderer renderer;
        private List<Body> bodies;
        private Canvas simCanvas;
        public MainPage simPage;
        public int monitoredBody = 0;
        private double simElapsedTime;
        private bool checkSim;
        private int simRounding;
        private bool accelerationLimitOn;
        private double accelerationLimit;
        public static double minimumSeparationSquared;
        public static string currentScenarioName;
        private Point[] accelerations;
        private Point[] positions;          // Used for checkSim only
        private Point[] velocities;         // Used for checkSim only

        private double speedFactor;          // simulation speed factor, 1.0 = 100% of original scenario speed

        public GravitySim(Canvas simulationCanvas, MainPage simulationPage, CoreDispatcher dispatcher)
        {
            bodies = new List<Body>();
            simCanvas = simulationCanvas;
            simPage = simulationPage;
            simSpace = new SimSpace(SimSpace.DefinedSpace.NullSpace);
            renderer = new SimRenderer(simSpace, simCanvas, dispatcher, simPage);
            // accelerations default to null, they're newed when they're needed
            simCalcSettings = new CalculationSettings();
            simElapsedTime = 0.0;
            checkSim = false; 
            simRounding = 0;
            accelerationLimitOn = false;
            minimumSeparationSquared = 1.0;
            speedFactor = 1.0;
        }

        public void ClearSim()
        {
            bodies.Clear();
            // simCanvas never changes
            // simPage never changes
            SetSimSpace(new SimSpace(SimSpace.DefinedSpace.NullSpace));
            renderer.ClearSim();
            accelerations = null;
            simCalcSettings = new CalculationSettings();
            simElapsedTime = 0.0;
            checkSim = false;
            simRounding = 0;
            accelerationLimitOn = false;
            minimumSeparationSquared = 1.0;
            speedFactor = 1.0;
        }

        public void SetSimSpace(SimSpace space)
        {
            this.simSpace = space;
            renderer.simSpace = space;
            renderer.SetSimulationTransform(simCanvas.ActualWidth, simCanvas.ActualHeight);
        }

        public void SetCalculationSettings(CalculationSettings calculationSettings) => simCalcSettings = calculationSettings;

        public void AddBody(double mass, double size, int color, BodyStartPosition startPosition)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, BodyStartPosition startPosition, Point startVelocity)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), startVelocity, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, BodyStartPosition startPosition, Point startVelocity,
            bool isGravitySource)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), startVelocity, isGravitySource, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBodyActual(double mass, bool isGravitySource, double size, int color, Point startPosition, Point startVelocity)
        {
            var velocity = new Point(startVelocity.X / simSpace.VelocityConnversionFactor, startVelocity.Y / simSpace.VelocityConnversionFactor);
            bodies.Add(new Body(mass, size, startPosition, velocity, isGravitySource, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void SetCheckSim(bool check)
        {
            checkSim = check;
        }

        public void SetSimRounding(int roundingDigits)
        {
            simRounding = roundingDigits;
        }

        // We are not simulating collisions so we need to have a minimum body separation > 0 
        //  to avoid dividing by 0 in acceleration calculations
        public void SetAccelerationLimits(bool limitOn, double limit, double minimumSeparation)
        {
            accelerationLimitOn = limitOn;
            accelerationLimit = limit;

            if (minimumSeparation == 0.0)
                minimumSeparation = 1.0;
            minimumSeparationSquared = minimumSeparation * minimumSeparation;
        }


        // The scale, origin, or layout changed so we need to re-transform all of the rendered bodies
        public void TransformChanged()
        {
            renderer.TransformChanged(bodies);
        }

        public void ZoomPlus() => renderer.ZoomIn();

        public void ZoomMinus() => renderer.ZoomOut();


        private const double speedIncrement = 1.25992105;    // Cube root of 2 -> three steps doubles or halves simulation speed

        public void RunFaster()
        {
            speedFactor *= speedIncrement;
        }

        public void RunSlower()
        {
            speedFactor *= 1.0 / speedIncrement;
        }


        //  simRunning - true if sim is auto-running
        //               false if sim is single stepping
        public void Step(double timeInterval, bool simRunning)
        {
            Stopwatch perfStopwatch = new Stopwatch();
            long perfIntervalTicks = 0L;
            bool simStepping = !simRunning;

            double scaledTimeInterval = timeInterval * speedFactor;
            SetTimeForTrailMark(simElapsedTime);

            if (simStepping)
            {
                Debug.WriteLine("Elapsed times for {0} bodies:", bodies.Count());
                perfStopwatch.Start();
            }

            if (accelerations == null)
                accelerations = new Point[bodies.Count()];

            if (checkSim)
            {
                if (positions == null)
                    positions = new Point[bodies.Count()];
                if (velocities == null)
                    velocities = new Point[bodies.Count()];

            }

            double timeIntervalPerCycle = scaledTimeInterval / (double)simCalcSettings.CalculationCyclesPerFrame;

            List<Point> otherPositions = new List<Point>();
            List<Point> otherAccelerations = new List<Point>();

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
                if (DumpData.collectingData)
                {
                    DumpData.times.Add(simElapsedTime);
                    DumpData.timeIntervals.Add(timeIntervalPerCycle);
                    DumpData.prePositions.Add(bodies[monitoredBody].Position);
                    DumpData.preVelocities.Add(bodies[monitoredBody].Velocity);

                    otherPositions = new List<Point>();
                    otherAccelerations = new List<Point>();
                }

                // Calculate NBody acceleration
                for (int i = 0; i < bodies.Count(); i++)
                {
                    accelerations[i].X = 0.0;
                    accelerations[i].Y = 0.0;

                    for (int j = 0; j < bodies.Count(); j++)
                        if ((i != j) && bodies[j].IsGravitySource)
                        {
                            Point accel = bodies[i].BodyToBodyAccelerate(bodies[j]);
                            accelerations[i].X = accelerations[i].X + accel.X;
                            accelerations[i].Y += accel.Y;

                            if ((i == monitoredBody) && (DumpData.collectingData))
                            {
                                otherPositions.Add(bodies[j].Position);
                                otherAccelerations.Add(accel);
                            }
                        }
                }

                if (checkSim) Validate5BodyCross(accelerations, "Accelerations Before Limit and Rounding");


                if (DumpData.collectingData)
                {
                    DumpData.otherBodyPositions.Add(otherPositions);
                    DumpData.otherBodyAccelerations.Add(otherAccelerations);

                    DumpData.totalAccelerations.Add(accelerations[monitoredBody]);
                }

                if (accelerationLimitOn)
                    EnforceAccelerationLimit(accelerations, accelerationLimit);

                if (DumpData.collectingData)
                    DumpData.afterAccLimitAccelerations.Add(accelerations[monitoredBody]);

                if (simRounding > 0)
                    RoundAccelerations(accelerations, simRounding);

                if (DumpData.collectingData)
                    DumpData.afterRoundingAccelerations.Add(accelerations[monitoredBody]);

                if (checkSim) Validate5BodyCross(accelerations, "Accelerations After Limit and Rounding");

                // Update positons and velocities
                for (int i = 0; i < bodies.Count(); i++)
                {
                    bodies[i].Move(accelerations[i], timeIntervalPerCycle);
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

                if (DumpData.collectingData)
                {
                    DumpData.postPositions.Add(bodies[monitoredBody].Position);
                    DumpData.postVelocities.Add(bodies[monitoredBody].Velocity);
                }

                simElapsedTime += timeIntervalPerCycle;

                if ((MainPage.trailsEnabled) && TimeForTrailsMark(simElapsedTime))
                    DrawTrails();
            }
            if (simStepping) perfIntervalTicks = DisplayPerfIntervalElapsed(perfStopwatch, perfIntervalTicks, 
                String.Format("Compute N-body accelerations, update positions & velocities ({0} iterations)", simCalcSettings.CalculationCyclesPerFrame) );


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

        private const double trailMarksPerSimulationTimeUnit = 2.0;
        private static int previousTimeUnit;

        private void SetTimeForTrailMark(double currentSimTime)
        {
            previousTimeUnit = (int) Math.Truncate(currentSimTime * trailMarksPerSimulationTimeUnit);
        }

        // Triggers drawing of a trail mark every time we cross a sim time unit boundary
        //  as defined by trailMarksPerSimulationTimeUnit
        // It's now possible to draw more than one trail mark per frame (versus drawing one every 30 frames before)
        private bool TimeForTrailsMark(double currentSimTime)
        {
            int newTimeUnit = (int)Math.Truncate(currentSimTime * trailMarksPerSimulationTimeUnit);

            if (newTimeUnit > previousTimeUnit)
            {
                previousTimeUnit = newTimeUnit;
                return true;
            }
            else
                return false;
        }

        private void DrawTrails()
        {
            renderer.DrawTrails(bodies[monitoredBody]);
        }

        private static long DisplayPerfIntervalElapsed(Stopwatch stopwatch, long previousIntervalStartTicks, string workDescription)
        {
            long elapsedTicks = stopwatch.ElapsedTicks;
            long ticksThisInterval = elapsedTicks - previousIntervalStartTicks;

            long ticksPerMillisecond = Stopwatch.Frequency / 1000L;

            Debug.WriteLine("   {0} took {1:F2} ms", workDescription, (double)ticksThisInterval / (double)ticksPerMillisecond);

            return elapsedTicks;
        }

        public void EnforceAccelerationLimit(Point[] accelerations, double limit)
        {
            for (int i = 0; i < accelerations.Length; i++)
            {
                double m = Magnitude(accelerations[i]);
                if (m > limit)
                {
                    double scale = limit / m;
                    accelerations[i].X = accelerations[i].X * scale;
                    accelerations[i].Y = accelerations[i].Y * scale;
                }
            }
        }


        #region Validation Checks

        // Validates accelerations and positions match where they should
        // Validates values that should always be zero
        private void Validate5BodyCross(Point[] points, string when)
        {
            if (!FourWayMatch(points[0].X, -points[1].Y, -points[2].X, points[3].Y))
                Debugger.Break();
            if (!AllAreZero(points[0].Y, points[1].X, points[2].Y, points[3].X))
                Debugger.Break();
            if (!AllAreZero(points[4].X, points[4].Y))
                Debugger.Break();
        }

        private bool FourWayMatch(double a, double b, double c, double d)
        {
            bool result = ((a == b) && (b == c) && (c == d));
            if (!result)
            {
                Debugger.Break();
                bool test;
                test = (a == b);
                test = (b == c);
                test = (c == d);
            }
            return result;
        }
        
        private bool AllAreZero(double a, double b, double c, double d)
        {
            bool result = ((a == 0.0) && (b == 0.0) && (c == 0.0) && (d == 0.0));
            if (!result)
            {
                Debugger.Break();
                bool test;
                test = (a == 0.0);
                test = (b == 0.0);
                test = (c == 0.0);
                test = (d == 0.0);
            }
            return result;
        }

        private bool AllAreZero(double a, double b)
        {
            bool result = ((a == 0.0) && (b == 0.0));
            if (!result)
            {
                Debugger.Break();
                bool test;
                test = (a == 0.0);
                test = (b == 0.0);
            }
            return result;
        }


        //private static bool CheckMirrorAndZeroY(Point a, Point b)
        //{
        //    if ((a.Y != 0.0) || (b.Y != 0.0) || (a.X != -b.X))
        //        return false;
        //    else
        //        return true;
        //}

        //private static bool CheckMirrorAndZeroX(Point a, Point b)
        //{
        //    if ((a.X != 0.0) || (b.X != 0.0) || (a.Y != -b.Y))
        //        return false;
        //    else
        //        return true;
        //}

        //private static bool CrossCheckXY(Point a, Point b)
        //{
        //    if ((a.X != b.Y) || (a.Y != b.X))
        //        return false;
        //    else
        //        return true;
        //}

        #endregion

        private void RoundAccelerations(Point[] accelerations, int roundingDigits)
        {
            for (int i = 0; i < accelerations.Length; i++)
            {
                accelerations[i].X = Math.Round(accelerations[i].X, roundingDigits);
                accelerations[i].Y = Math.Round(accelerations[i].Y, roundingDigits);
            }
        }

        // const double epsilon = 0.001;  // Horizontal, vertical or 45 degree diagonal acceleration will start to drift because of cumulative
        //  rounding error. For differences less than epsilon, snap the values back to zero or the 
        //  perfect diagonal.

        /*// Clean X & Y to eliminate cumulative rounding errors
        double absX = Math.Abs(accelerations[i].X);
        double absY = Math.Abs(accelerations[i].Y);

        if (Math.Abs(absX - absY) < epsilon)
        {
            double avgDiagonalAccel = (absX + absY) / 2.0;
            accelerations[i].X = Math.Sign(accelerations[i].X) * avgDiagonalAccel;
            accelerations[i].Y = Math.Sign(accelerations[i].Y) * avgDiagonalAccel;
        }
        else
        {
                
            if ((absX > 0.0) && (absX < epsilon))
                accelerations[i].X = 0.0;
            if ((absY > 0.0) && (absY < epsilon))
                accelerations[i].Y = 0.0;
        **/

        // Treats a point as a vector and calculates its magnitude
        public static double Magnitude(Point v)
        {
            return (Math.Sqrt((v.X * v.X) + (v.Y * v.Y)));
        }

        public void SetMonitoredValues()
        {
            simPage.UpdateMonitoredValues(bodies[monitoredBody], simElapsedTime);
        }

        public void SetMonitoredBody(int monitorBody)
        {
            monitoredBody = monitorBody;
            renderer.SetMonitoredColor(monitoredBody);
        }

        public void SetMessage(string message)
        {
            simPage.SetMessageText(message);
        }
    }
}

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
        
        public enum bodyStartPosition
        {
            stageLeft, stageRight, stageTop, stageBottom, stageTopLeft, stageTopRight, stageBottomRight, stageBottomLeft,
            screenTop, screenLeft, screenRight, screenBottom,
            centerOfTheUniverse,
            randomStagePosition, randomScreenPosition, randomCircularCluster
        };

        public SimSpace simSpace;
        public CalculationSettings simCalcSettings;
        public SimRenderer renderer;
        private List<Body> bodies;
        private Canvas simCanvas;
        private MainPage simPage;
        private int monitoredBody = 0;
        private double simElapsedTime;
        private bool checkSim;
        private int simRounding;
        private bool accelerationLimit;
        private Point[] accelerations;

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
            accelerationLimit = false;
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
            accelerationLimit = false;
            speedFactor = 1.0;
        }

        public void SetSimSpace(SimSpace space)
        {
            this.simSpace = space;
            renderer.simSpace = space;
            renderer.SetSimulationTransform(simCanvas.ActualWidth, simCanvas.ActualHeight);
        }

        public void SetCalculationSettings(CalculationSettings calculationSettings) => simCalcSettings = calculationSettings;

        public void AddBody(double mass, double size, int color, bodyStartPosition startPosition)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, bodyStartPosition startPosition, Point startVelocity)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), startVelocity, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, bodyStartPosition startPosition, Point startVelocity,
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

        public void SetAccelerationLimit(bool limitOn)
        {
            accelerationLimit = limitOn;
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
            // TBD: factor out to SimSpace
            const double defaultAccelerationLimit = 10.0; // SimSpaceUnits per second^2

            Stopwatch perfStopwatch = new Stopwatch();
            long perfIntervalTicks = 0L;
            bool simStepping = !simRunning;

            /*
            if (stepRunning)
            {
                Debug.WriteLine("> Previous step still running, skipping this time interval");
                return;
            }
            stepRunning = true;
            */

            double scaledTimeInterval = timeInterval * speedFactor;
            SetTimeForTrailMark(simElapsedTime);

            if (simStepping)
            {
                Debug.WriteLine("Elapsed times for {0} bodies:", bodies.Count());
                perfStopwatch.Start();
            }

            if (accelerations == null)
                accelerations = new Point[bodies.Count()];

            double timeIntervalPerCycle = scaledTimeInterval / (double)simCalcSettings.CalculationCyclesPerFrame;

            for (int calcCycle = 0; calcCycle < simCalcSettings.CalculationCyclesPerFrame; calcCycle++)
            {
                // Calculate NBody acceleration
                for (int i = 0; i < bodies.Count(); i++)
                {
                    accelerations[i].X = 0.0;
                    accelerations[i].Y = 0.0;
                    for (int j = 0; j < bodies.Count(); j++)
                        if ((i != j) && bodies[j].IsGravitySource)
                        {
                            Point accel = bodies[i].BodyToBodyAccelerate(bodies[j]);
                            accelerations[i].X += accel.X;
                            accelerations[i].Y += accel.Y;
                        }
                }

                if (accelerationLimit)
                    EnforceAccelerationLimit(accelerations, defaultAccelerationLimit);

                if (simRounding > 0)
                    RoundAccelerations(accelerations, simRounding);

                // Update positons and velocities
                for (int i = 0; i < bodies.Count(); i++)
                {
                    bodies[i].Move(accelerations[i], timeIntervalPerCycle);
                }

                simElapsedTime += timeIntervalPerCycle;

                if (TimeForTrailsMark(simElapsedTime))
                    DrawTrails();
            }
            if (simStepping) perfIntervalTicks = DisplayPerfIntervalElapsed(perfStopwatch, perfIntervalTicks, 
                String.Format("Compute N-body accelerations, update positions & velocities ({0} iterations)", simCalcSettings.CalculationCyclesPerFrame) );

            //if (checkSim)
            //{
            //    ValidateState(accelerations);
            //    if (simStepping) perfIntervalTicks = DisplayPerfIntervalElapsed(perfStopwatch, perfIntervalTicks, "Validate state of accelerations");
            //}

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
        private void ValidateState(Point[] accelerations)
        {
            bool invalid;

            invalid = false;
            if (!CheckMirrorAndZeroY(accelerations[0], accelerations[2]))
                invalid = true;
            if (!CheckMirrorAndZeroY(bodies[0].Position, bodies[2].Position))
                invalid = true;
            if (!CheckMirrorAndZeroY(bodies[0].Velocity, bodies[2].Velocity))
                invalid = true;
            if (!CheckMirrorAndZeroX(accelerations[1], accelerations[3]))
                invalid = true;
            if (!CheckMirrorAndZeroX(bodies[1].Position, bodies[3].Position))
                invalid = true;
            if (!CheckMirrorAndZeroX(bodies[1].Velocity, bodies[3].Velocity))
                invalid = true;
            if (!CrossCheckXY(accelerations[0], accelerations[3]))
                invalid = true;
            if (!CrossCheckXY(bodies[0].Position, bodies[3].Position))
                invalid = true;
            if (!CrossCheckXY(bodies[0].Velocity, bodies[3].Velocity))
                invalid = true;
            if (!CrossCheckXY(accelerations[1], accelerations[2]))
                invalid = true;
            if (!CrossCheckXY(bodies[1].Position, bodies[2].Position))
                invalid = true;
            if (!CrossCheckXY(bodies[1].Velocity, bodies[2].Velocity))
                invalid = true;

            if (invalid)
                invalid = true;
        }

        private static bool CheckMirrorAndZeroY(Point a, Point b)
        {
            if ((a.Y != 0.0) || (b.Y != 0.0) || (a.X != -b.X))
                return false;
            else
                return true;
        }

        private static bool CheckMirrorAndZeroX(Point a, Point b)
        {
            if ((a.X != 0.0) || (b.X != 0.0) || (a.Y != -b.Y))
                return false;
            else
                return true;
        }

        private static bool CrossCheckXY(Point a, Point b)
        {
            if ((a.X != b.Y) || (a.Y != b.X))
                return false;
            else
                return true;
        }

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

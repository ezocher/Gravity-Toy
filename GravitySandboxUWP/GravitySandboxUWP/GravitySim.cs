﻿using System;
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
    // The simulation owns the bodies in the simulation space and holds references to their graphics incarnations (which are animated by the Renderer class)
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

        public SimulationSpace simSpace;
        public CalculationSettings simCalcSettings;
        public Renderer renderer;
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
        public string ScenarioName { get; set; }
        private SimPoint[] accelerations;
        private SimPoint[] positions;          // Used for checkSim only
        private SimPoint[] velocities;         // Used for checkSim only

        // simulation speed factor, 1.0 = 100% of original scenario speed
        public double SpeedFactor { get; private set; }

        public GravitySim(Canvas simulationCanvas, MainPage simulationPage, CoreDispatcher dispatcher)
        {
            bodies = new List<Body>();
            simCanvas = simulationCanvas;
            simPage = simulationPage;
            simSpace = new SimulationSpace(SimulationSpace.Space.Null);
            renderer = new Renderer(simSpace, simCanvas, dispatcher, simPage);
            // accelerations default to null, they're newed when they're needed
            simCalcSettings = new CalculationSettings();
            simElapsedTime = 0.0;
            checkSim = false; 
            simRounding = 0;
            accelerationLimitOn = false;
            minimumSeparationSquared = 1.0;
            SpeedFactor = 1.0;
        }

        public void ClearSim()
        {
            bodies.Clear();
            Body.ResetBodyCount();
            // simCanvas never changes
            // simPage never changes
            SetSimSpace(new SimulationSpace(SimulationSpace.Space.Null));
            renderer.ClearSim();
            accelerations = null;
            simCalcSettings = new CalculationSettings();
            simElapsedTime = 0.0;
            checkSim = false;
            simRounding = 0;
            accelerationLimitOn = false;
            minimumSeparationSquared = 1.0;
            SpeedFactor = 1.0;
        }

        public void SetSimSpace(SimulationSpace space)
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

        public void AddBody(double mass, double size, int color, BodyStartPosition startPosition, SimPoint startVelocity)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), startVelocity, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, BodyStartPosition startPosition, SimPoint startVelocity,
            bool isGravitySource)
        {
            bodies.Add(new Body(mass, size, renderer.GetStartingPosition(startPosition), startVelocity, isGravitySource, simSpace));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBodyActual(double mass, bool isGravitySource, double diameter, int color, SimPoint startPosition, SimPoint startVelocity)
        {
            var velocity = new SimPoint(startVelocity.X / simSpace.VelocityConnversionFactor, startVelocity.Y / simSpace.VelocityConnversionFactor);
            bodies.Add(new Body(mass, diameter, startPosition, velocity, isGravitySource, simSpace));
            renderer.Add(diameter, color, bodies.Last());
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
        public void TransformChanged() => renderer.TransformChanged(bodies);

        public void ZoomPlus() => renderer.ZoomIn();

        public void ZoomMinus() => renderer.ZoomOut();

        public double GetZoomFactor() => renderer.ZoomFactor;

        private const double speedIncrement = 1.25992105;    // Cube root of 2 -> three steps doubles or halves simulation speed

        public void RunFaster() { SpeedFactor *= speedIncrement; }

        public void RunSlower() { SpeedFactor *= 1.0 / speedIncrement; }

        //  simRunning - true if sim is auto-running
        //               false if sim is single stepping
        public void Step(double timeInterval, bool simRunning)
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

        public void EnforceAccelerationLimit(SimPoint[] accelerations, double limit)
        {
            for (int i = 0; i < accelerations.Length; i++)
            {
                double m = accelerations[i].Magnitude();
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
        private void Validate5BodyCross(SimPoint[] points, string when)
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


        //private static bool CheckMirrorAndZeroY(SimPoint a, SimPoint b)
        //{
        //    if ((a.Y != 0.0) || (b.Y != 0.0) || (a.X != -b.X))
        //        return false;
        //    else
        //        return true;
        //}

        //private static bool CheckMirrorAndZeroX(SimPoint a, SimPoint b)
        //{
        //    if ((a.X != 0.0) || (b.X != 0.0) || (a.Y != -b.Y))
        //        return false;
        //    else
        //        return true;
        //}

        //private static bool CrossCheckXY(SimPoint a, SimPoint b)
        //{
        //    if ((a.X != b.Y) || (a.Y != b.X))
        //        return false;
        //    else
        //        return true;
        //}

        #endregion

        private void RoundAccelerations(SimPoint[] accelerations, int roundingDigits)
        {
            for (int i = 0; i < accelerations.Length; i++)
            {
                accelerations[i].X = Math.Round(accelerations[i].X, roundingDigits, MidpointRounding.AwayFromZero);
                accelerations[i].Y = Math.Round(accelerations[i].Y, roundingDigits, MidpointRounding.AwayFromZero);
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

        public void SetMonitoredValues()
        {
            simPage.UpdateMonitoredValues(bodies[monitoredBody], simElapsedTime);
        }

        public void SetMonitoredBody(int monitorBody)
        {
            monitoredBody = monitorBody;
            renderer.SetMonitoredColor(monitoredBody);
        }

        public void SetMessage(string message) => simPage.SetMessageText(message);
    }
}

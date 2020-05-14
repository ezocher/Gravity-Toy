using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace GravitySandbox10
{
    // The simulation owns the bodies in the simulation space and holds references to their graphics incarnations (which are animated by the SimRender class)
    class GravitySim
    {
        public enum simulationType { spaceSimulation, earthSimulation };
        public enum bodyStartPosition
        {
            stageLeft, stageRight, stageTop, stageBottom, stageTopLeft, stageTopRight, stageBottomRight, stageBottomLeft,
            screenTop, screenLeft, screenRight, screenBottom,
            centerOfTheUniverse, 
            randomStagePosition, randomScreenPosition, randomCircularCluster
        };

        private simulationType simType;
        public SimRender renderer;
        private List<Flatbody> bodies;
        private Canvas simCanvas;
        private MainPage simPage;
        private int monitoredBody = 0;
        private double simElapsedTime;
        private bool checkSim;
        private int simRounding;
        private Point[] accelerations;

        public GravitySim(Canvas simulationCanvas, MainPage simulationPage)
        {
            simType = simulationType.spaceSimulation;
            simCanvas = simulationCanvas;
            simPage = simulationPage;
            renderer = new SimRender(simType, simCanvas);
            simElapsedTime = 0.0;
            bodies = new List<Flatbody>();
            checkSim = false;
            simRounding = 0;
        }

        public void AddBody(double mass, double size, int color, bodyStartPosition startPosition)
        {
            bodies.Add(new Flatbody(mass, size, renderer.GetStartingPosition(startPosition)));
            renderer.Add(size, color, bodies.Last());
        }

        public void AddBody(double mass, double size, int color, bodyStartPosition startPosition, Point startVelocity)
        {
            bodies.Add(new Flatbody(mass, size, renderer.GetStartingPosition(startPosition), startVelocity));
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

        public void ClearSim()
        {
            bodies.Clear();
            renderer.ClearSim();
            simElapsedTime = 0.0;
            checkSim = false;
            simRounding = 0;
            accelerations = null;
        }

        // The scale, origin, or layout changed so we need to re-transform all of the rendered bodies
        public void TransformChanged()
        {
            renderer.TransformChanged(bodies);
        }


        // TBD: add + operator to Point class ??
        public void Step(double timeInterval)
        {
            const double defaultAccelerationLimit = 10.0; // SimSpaceUnits per second^2

            simElapsedTime += timeInterval;
            if (accelerations == null)
                accelerations = new Point[bodies.Count()];

            // Calculate NBody acceleration
            for (int i = 0; i < bodies.Count(); i++)
            {
                accelerations[i].X = 0.0;
                accelerations[i].Y = 0.0;
                for (int j = 0; j < bodies.Count(); j++)
                    if (i != j)
                    {
                        Point a = bodies[i].BodyToBodyAccelerate(bodies[j]);
                        accelerations[i].X += a.X;
                        accelerations[i].Y += a.Y;
                    }
            }

            // if (accelerationLimit)
                EnforceAccelerationLimit(accelerations, defaultAccelerationLimit);

            if (simRounding > 0)
                RoundAccelerations(accelerations, simRounding);

            // Update positons and velocity
            for (int i = 0; i < bodies.Count(); i++)
            {
                bodies[i].Move(accelerations[i], timeInterval);
                if (i == monitoredBody)
                {
                    simPage.UpdateMonitoredValues(bodies[i], simElapsedTime);
                }
            }

            if (checkSim)
                ValidateState(accelerations);

            // Update rendering
            renderer.BodiesMoved(bodies);
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

    }
}

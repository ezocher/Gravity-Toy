using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using Windows.UI;
using System.Diagnostics;
using Windows.UI.Notifications;

namespace GravitySandboxUWP
{
    class SimRenderer
    {

        // The simulation box is a square with it's own coordinate space
        //      For simulations in space the origin is in the center of the square
        //      For simulations on earth, the earth surface is a flat strip across the bottom of the screen and the origin is horizontally centered with y = 0 at ground level
        //
        // The simulation box will be centered on the display and will be the maximum size it can be while remaining completely on the screen
        //
        const double simBoxHeightAndWidth = 1000.0;
        const double simBoxMaxXY = simBoxHeightAndWidth / 2.0;

        const double earthStripHeight = 20.0;

        const double circleSizeFactor = 10.0;  // the smallest body will have a simulaton size of 1.0, render it with this diameter in simulation space


        private Point screenDimensions;     // screen dimensions in rendering space
        private double scaleFactor;         // use scaleFactor for x, use -scaleFactor for y - incorporates any zoom factor
        private double zoomFactor;          // zoom factor, 1.0 = 100%
        private Point simulationCenterTranslation;
        private Point screenSimulationDimensions;    // screen width and height in simulation space

        private Canvas simCanvas;

        private Random rand;

        // Mapping point in simulation space to rendering space:
        //  simPt * scaleFactor + simulationCenterTranslation + circleCenterTranslation -> renderingOffset

        private SimSpace simSpace;

        private List<Ellipse> circles;
        private List<Point> trailsPositions;        // Positions of trails dots in simulation space

        private CoreDispatcher dispatcher;

        private MainPage mainPage;

        public enum ColorNumber { bodyColorRed = 1, bodyColorGreen, bodyColorBlue, bodyColorLtGrey = 9, bodyColorMedGrey, bodyColorDarkGrey };

        const string circleColorResourceBaseString = "bodyColor";
        const string circleColorMonitoredColorResourceName = "monitoredBodyColor";

        public const int firstColorIndex = 1;
        public const int lastPastelColorIndex = 8;
        public const int firstMonochromeColorIndex = 9;
        public const int lastColorIndex = 11;

        private static SolidColorBrush trailsBrush;

        public enum ColorScheme { pastelColors, grayColors, allColors};

        public SimRenderer(SimSpace space, Canvas simulationCanvas, CoreDispatcher dispatcher, MainPage mainPage)
        {
            this.simSpace = space;
            this.circles = new List<Ellipse>();
            this.trailsPositions = new List<Point>();
            this.rand = new Random();
            this.simCanvas = simulationCanvas;
            this.zoomFactor = 1.0;
            this.dispatcher = dispatcher;
            this.mainPage = mainPage;
            trailsBrush = new SolidColorBrush(Colors.Yellow);
        }

        public void Add(double size, int colorNumber, Flatbody body)
        {
            Ellipse newCircle = new Ellipse();
            newCircle.Width = newCircle.Height = CircleSize(size);
            newCircle.Fill = (SolidColorBrush)Application.Current.Resources[circleColorResourceBaseString + colorNumber.ToString()];
            newCircle.RenderTransform = CircleTransform(body);
            simCanvas.Children.Add(newCircle);
            circles.Add(newCircle);
        }

        private const double dotSize = 2.0;
        private Point previousTrailPosition;

        public void PlotTrailDot(Point position)
        {
            // Always plot the first point
            // Don't plot subsequent points iff they're repeats of the previous point
            if ((trailsPositions.Count == 0) || !position.Equals(previousTrailPosition))
            {
                previousTrailPosition = position;

                trailsPositions.Add(position);

                Rectangle dot = new Rectangle();
                dot.Width = dot.Height = dotSize;
                dot.Fill = trailsBrush;
                dot.RenderTransform = CircleTransform(position, dotSize);
                simCanvas.Children.Add(dot);
            }
        }

        public void SetMonitoredColor(int monitoredCircleIndex)
        {
            circles[monitoredCircleIndex].Fill = (SolidColorBrush)Application.Current.Resources[circleColorMonitoredColorResourceName];
        }

        public void ClearSim()
        {
            // TBD: Would be nice to get an exlusive lock that ensures no rendering is in progress

            simCanvas.Children.Clear();
            circles.Clear();
            trailsPositions.Clear();
            scaleFactor = scaleFactor / zoomFactor;
            zoomFactor = 1.0;
        }

        public void TransformChanged(List<Flatbody> bodies)
        {
            int i = 0;
            Ellipse circle;
            foreach (Flatbody body in bodies)
            {
                circle = circles[i++];
                circle.Width = circle.Height = CircleSize(body.Size);
                /* if (CircleOnscreen(circle))
                    circle.Visibility = Visibility.Visible;
                else
                    circle.Visibility = Visibility.Collapsed; */
                circle.RenderTransform = CircleTransform(body);
            }

            if (simCanvas.Children.Count > bodies.Count)
            {
                // Trails have be drawn, move them too
                for (int j = bodies.Count; j < simCanvas.Children.Count; j++ )
                {
                    simCanvas.Children[j].RenderTransform = CircleTransform(trailsPositions[j - bodies.Count], dotSize);
                }
            }
        }

        /* Original BodiesMoved()
                public void BodiesMoved(List<Flatbody> bodies)
                {
                    int i = 0;
                    Ellipse circle;
                    foreach (Flatbody body in bodies)
                    {
                        circle = circles[i++];
                        circle.RenderTransform = CircleTransform(body);

                        if (CircleOnscreen(circle))
                            circle.Visibility = Visibility.Visible;
                        else
                            circle.Visibility = Visibility.Collapsed;
                    }
                }
        */

        // Updated to be marshalled onto the UI thread
        public void BodiesMoved(List<Flatbody> bodies)
        {
            if (mainPage.UI_UpdatesStopped()) return;   // Stop UI updates while app is suspended or changing scenarios

            var ignore = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    circles[i].RenderTransform = CircleTransform(bodies[i]);
                }
            });
        }

        public void DrawTrails(Flatbody body)
        {
            if (mainPage.UI_UpdatesStopped()) return;   // Stop UI updates while app is suspended or changing scenarios

            var ignore = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlotTrailDot(body.Position);
            });
        }

        // Calculates the mapping from simulation coordinates to XAML coordinates
        //  Called when the simulation is loaded and whenever the window is resized
        public void SetSimulationTransform(double screenWidth, double screenHeight)
        {
            screenDimensions = new Point(screenWidth, screenHeight);
            double minDimension = Math.Min(screenWidth, screenHeight);

            
            //if (simType == SimulationType.spaceSimulation)
            //{
                scaleFactor = minDimension / simBoxHeightAndWidth;
                simulationCenterTranslation = new Point(screenWidth / 2, screenHeight / 2);
                screenSimulationDimensions = new Point(screenWidth / scaleFactor, screenHeight / scaleFactor);
            //}
            //else // unimplemented simType
            //{
            //    throw new ArgumentException("Unimplemented simulation type:" + simType.ToString());
            //}

            scaleFactor = scaleFactor * zoomFactor;
        }


        private double CircleSize(double bodySize)
        {
            return (bodySize * circleSizeFactor * scaleFactor);
        }


        // Mapping point in simulation space to rendering space:
        //  simPt * scaleFactor + simulationCenterTranslation + circleCenterTranslation -> renderingOffset
        public TranslateTransform CircleTransform(Flatbody body)
        {
            TranslateTransform t = new TranslateTransform();
            double circleCenterTranslation = -CircleSize(body.Size) / 2.0;

            t.X = body.Position.X * scaleFactor + simulationCenterTranslation.X + circleCenterTranslation;
            t.Y = body.Position.Y * -scaleFactor + simulationCenterTranslation.Y + circleCenterTranslation;

            return t;
        }


        public TranslateTransform CircleTransform(Point position, double size)
        {
            TranslateTransform t = new TranslateTransform();
            double circleCenterTranslation = -size / 2.0;

            t.X = position.X * scaleFactor + simulationCenterTranslation.X + circleCenterTranslation;
            t.Y = position.Y * -scaleFactor + simulationCenterTranslation.Y + circleCenterTranslation;

            return t;
        }



        // Returns starting position for a body in simulation space coordinates
        public Point GetStartingPosition(GravitySim.bodyStartPosition startPos)
        {
            const double stagePosition = 0.5; // For the "stage" positions - proportion of the way from the center of the stage to the edge
                                              //    in all directions
            double stageXY = simBoxMaxXY * stagePosition;

            double screenMaxX = screenSimulationDimensions.X / 2.0;
            double screenMaxY = screenSimulationDimensions.Y / 2.0;

            switch (startPos)
            {
                case GravitySim.bodyStartPosition.stageLeft:
                    return new Point(-stageXY, 0.0);
                case GravitySim.bodyStartPosition.stageRight:
                    return new Point(stageXY, 0.0);
                case GravitySim.bodyStartPosition.stageTop:
                    return new Point(0.0, stageXY);
                case GravitySim.bodyStartPosition.stageBottom:
                    return new Point(0.0, -stageXY);

                case GravitySim.bodyStartPosition.stageTopLeft:
                    return new Point(-stageXY, stageXY);
                case GravitySim.bodyStartPosition.stageTopRight:
                    return new Point(stageXY, stageXY);
                case GravitySim.bodyStartPosition.stageBottomLeft:
                    return new Point(-stageXY, -stageXY);
                case GravitySim.bodyStartPosition.stageBottomRight:
                    return new Point(stageXY, -stageXY);

                case GravitySim.bodyStartPosition.screenLeft:
                    return new Point(-screenMaxX, 0.0);
                case GravitySim.bodyStartPosition.screenRight:
                    return new Point(screenMaxX, 0.0);
                case GravitySim.bodyStartPosition.screenTop:
                    return new Point(0.0, screenMaxY);
                case GravitySim.bodyStartPosition.screenBottom:
                    return new Point(0.0, -screenMaxY);

                case GravitySim.bodyStartPosition.centerOfTheUniverse:
                    return new Point(0.0, 0.0);

                case GravitySim.bodyStartPosition.randomStagePosition:
                    return new Point(rand.Next((int)-simBoxMaxXY, (int)simBoxMaxXY),
                                      rand.Next((int)-simBoxMaxXY, (int)simBoxMaxXY));
                case GravitySim.bodyStartPosition.randomScreenPosition:
                    return new Point(rand.Next((int)-screenMaxX, (int)screenMaxX),
                                      rand.Next((int)-screenMaxY, (int)screenMaxY));
                case GravitySim.bodyStartPosition.randomCircularCluster:
                    double length = rand.NextDouble() * simBoxMaxXY * 0.9;
                    double angle = rand.NextDouble() * Math.PI * 2.0; // radians
                    return new Point(length * Math.Cos(angle), length * Math.Sin(angle));
                // This approach gives us higher density toward the center of the circle
                // TBD: Add random circular cluster with uniform density: pick random locations in a square and discard those outside the circle


                default:
                    return new Point(0.0, 0.0);
            }
        }
    }
}

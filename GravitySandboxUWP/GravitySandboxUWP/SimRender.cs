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

namespace GravitySandboxUWP
{
    class SimRender
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

        private GravitySim.simulationType simType;

        private List<Ellipse> circles;

        private CoreDispatcher dispatcher;

        public enum ColorNumber { bodyColorRed = 1, bodyColorGreen, bodyColorBlue, bodyColorLtGrey = 9, bodyColorMedGrey, bodyColorDarkGrey };

        const string circleColorResourceBaseString = "bodyColor";
        const string circleColorMonitoredColorResourceName = "monitoredBodyColor";

        public const int firstColorIndex = 1;
        public const int lastPastelColorIndex = 8;
        public const int firstMonochromeColorIndex = 9;
        public const int lastColorIndex = 11;

        public enum ColorScheme { pastelColors, grayColors, allColors};

        public SimRender(GravitySim.simulationType type, Canvas simulationCanvas, CoreDispatcher dispatcher)
        {
            this.simType = type;
            this.circles = new List<Ellipse>();
            this.rand = new Random();
            this.simCanvas = simulationCanvas;
            this.zoomFactor = 1.0;
            this.dispatcher = dispatcher;
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

        public void SetMonitoredColor(int monitoredCircleIndex)
        {
            circles[monitoredCircleIndex].Fill = (SolidColorBrush)Application.Current.Resources[circleColorMonitoredColorResourceName];
        }

        public void ClearSim()
        {
            simCanvas.Children.Clear();
            circles.Clear();
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
            if (MainPage.appSuspended) return;   // Stop UI updates while app is suspended

            var ignore = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    circles[i].RenderTransform = CircleTransform(bodies[i]);

                    /* if (CircleOnscreen(circles[i]))
                        circles[i].Visibility = Visibility.Visible;
                    else
                        circles[i].Visibility = Visibility.Collapsed; */
                }
            });
        }

        // TBD: test if the bounding rectangle of the circle intersects the screen rectangle
        public bool CircleOnscreen(Ellipse circle)
        {
            return true;
        }


        // The simulation space is a 
        public void SetSimulationTransform(double screenWidth, double screenHeight)
        {
            screenDimensions = new Point(screenWidth, screenHeight);
            double minDimension = Math.Min(screenWidth, screenHeight);

            if (simType == GravitySim.simulationType.spaceSimulation)
            {
                scaleFactor = minDimension / simBoxHeightAndWidth;
                simulationCenterTranslation = new Point(screenWidth / 2, screenHeight / 2);
                screenSimulationDimensions = new Point(screenWidth / scaleFactor, screenHeight / scaleFactor);
            }
            else // unimplemented simType
            {
                throw new ArgumentException("Unimplemented simulation type:" + simType.ToString());
            }
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

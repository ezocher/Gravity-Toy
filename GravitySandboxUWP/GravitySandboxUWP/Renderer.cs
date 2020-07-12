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
using Windows.Devices.Bluetooth.Background;

namespace GravitySandboxUWP
{
    class Renderer
    {

        // The simulation box is a square with it's own coordinate space
        //      For simulations in space the origin is in the center of the square
        //      For simulations on earth, the earth surface is a flat strip across the bottom of the screen and the origin is horizontally centered with y = 0 at ground level
        //
        // The simulation box will be centered on the display and will be the maximum size it can be while remaining completely on the screen
        //
        const double earthStripHeight = 20.0;

        private Point screenDimensions;     // screen dimensions in rendering space
        private double scaleFactor;         // use scaleFactor for x, use -scaleFactor for y - incorporates any zoom factor

        // zoom factor, 1.0 = 100%
        public double ZoomFactor { get; private set; }

        private Point simulationCenterTranslation;
        private SimPoint screenSimulationDimensions;    // screen width and height in simulation space

        private Canvas simCanvas;

        private Random rand;

        // Mapping point in simulation space to rendering space:
        //  simPt * scaleFactor + simulationCenterTranslation + circleCenterTranslation -> renderingOffset

        public SimulationSpace simSpace;

        private List<Ellipse> circles;
        private List<SimPoint> trailsPositions;        // Positions of trails dots in simulation space

        private CoreDispatcher dispatcher;

        private MainPage mainPage;

        public enum ColorNumber { BodyColorRed = 1, BodyColorGreen, BodyColorBlue, BodyColorLtGrey = 9, BodyColorMedGrey, BodyColorDarkGrey, BodyColorWhite };

        const string circleColorResourceBaseString = "bodyColor";
        const string circleColorMonitoredColorResourceName = "monitoredBodyColor";

        public const int firstColorIndex = 1;
        public const int lastPastelColorIndex = 8;
        public const int firstMonochromeColorIndex = 9;
        public const int lastColorIndex = 11;

        private static SolidColorBrush trailsBrush;

        public enum ColorScheme { PastelColors, GrayColors, AllColors};

        public Renderer(SimulationSpace space, Canvas simulationCanvas, CoreDispatcher dispatcher, MainPage mainPage)
        {
            this.simSpace = space;
            this.circles = new List<Ellipse>();
            this.trailsPositions = new List<SimPoint>();
            this.rand = new Random();
            this.simCanvas = simulationCanvas;
            this.ZoomFactor = 1.0;
            this.dispatcher = dispatcher;
            this.mainPage = mainPage;
            trailsBrush = new SolidColorBrush(Colors.Yellow);
        }

        public void Add(double size, int colorNumber, Body body)
        {
            Ellipse newCircle = new Ellipse();
            newCircle.Width = newCircle.Height = CircleSize(size);
            newCircle.Fill = (SolidColorBrush)Application.Current.Resources[circleColorResourceBaseString + colorNumber.ToString()];
            newCircle.RenderTransform = CircleTransform(body);
            simCanvas.Children.Add(newCircle);
            circles.Add(newCircle);
        }

        private const double dotSize = 2.0;
        private SimPoint previousTrailPosition;

        public void PlotTrailDot(SimPoint position)
        {
            // Always plot the first dot
            // Don't plot subsequent dots iff they're in the same position as the previous dot
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
            // Issue #29: Would be nice to have a clean way to ensure that no rendering is in progress (exclusive lock?)

            simCanvas.Children.Clear();
            circles.Clear();
            trailsPositions.Clear();
            scaleFactor = scaleFactor / ZoomFactor;
            ZoomFactor = 1.0;
        }

        private const double zoomIncrement = 1.25992105;    // Cube root of 2 -> three zooms doubles or halves view

        public void ZoomIn() { ZoomFactor *= zoomIncrement; }

        public void ZoomOut() { ZoomFactor *= 1.0 / zoomIncrement; }

        public void TransformChanged(List<Body> bodies)
        {
            int i = 0;
            Ellipse circle;
            foreach (Body body in bodies)
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


        // Updated to be marshalled onto the UI thread
        public void BodiesMoved(List<Body> bodies)
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

        public void DrawTrails(Body body)
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


            // This can be called before any scenarios have been loaded, provide a placeholder value in this case
            double simBoxDimensions;
            if (simSpace == null)
                simBoxDimensions = 1.0;
            else
                simBoxDimensions = simSpace.SimBoxHeightAndWidth;

            scaleFactor = minDimension / simBoxDimensions;
            simulationCenterTranslation = new Point(screenWidth / 2, screenHeight / 2);
            screenSimulationDimensions = new SimPoint(screenWidth / scaleFactor, screenHeight / scaleFactor);

            scaleFactor = scaleFactor * ZoomFactor;
        }


        private double CircleSize(double bodySize)
        {
            return (bodySize * scaleFactor);
        }


        // Mapping point in simulation space to rendering space:
        //  simPt * scaleFactor + simulationCenterTranslation + circleCenterTranslation -> renderingOffset
        public TranslateTransform CircleTransform(Body body)
        {
            TranslateTransform t = new TranslateTransform();
            double circleCenterTranslation = -CircleSize(body.Size) / 2.0;

            t.X = body.Position.X * scaleFactor + simulationCenterTranslation.X + circleCenterTranslation;
            t.Y = body.Position.Y * -scaleFactor + simulationCenterTranslation.Y + circleCenterTranslation;

            return t;
        }


        public TranslateTransform CircleTransform(SimPoint position, double size)
        {
            TranslateTransform t = new TranslateTransform();
            double circleCenterTranslation = -size / 2.0;

            t.X = position.X * scaleFactor + simulationCenterTranslation.X + circleCenterTranslation;
            t.Y = position.Y * -scaleFactor + simulationCenterTranslation.Y + circleCenterTranslation;

            return t;
        }

        // Returns starting position for a body in simulation space coordinates
        public SimPoint GetStartingPosition(GravitySim.BodyStartPosition startPos)
        {
            const double stagePosition = 0.5; // For the "stage" positions - proportion of the way from the center of the stage to the edge
                                              //    in all directions

            double simBoxMaxXY = simSpace.SimBoxHeightAndWidth / 2.0;
            double stageXY = simBoxMaxXY * stagePosition;

            double screenMaxX = screenSimulationDimensions.X / 2.0;
            double screenMaxY = screenSimulationDimensions.Y / 2.0;

            switch (startPos)
            {
                case GravitySim.BodyStartPosition.StageLeft:
                    return new SimPoint(-stageXY, 0.0);
                case GravitySim.BodyStartPosition.StageRight:
                    return new SimPoint(stageXY, 0.0);
                case GravitySim.BodyStartPosition.StageTop:
                    return new SimPoint(0.0, stageXY);
                case GravitySim.BodyStartPosition.StageBottom:
                    return new SimPoint(0.0, -stageXY);

                case GravitySim.BodyStartPosition.StageTopLeft:
                    return new SimPoint(-stageXY, stageXY);
                case GravitySim.BodyStartPosition.StageTopRight:
                    return new SimPoint(stageXY, stageXY);
                case GravitySim.BodyStartPosition.StageBottomLeft:
                    return new SimPoint(-stageXY, -stageXY);
                case GravitySim.BodyStartPosition.StageBottomRight:
                    return new SimPoint(stageXY, -stageXY);

                case GravitySim.BodyStartPosition.ScreenLeft:
                    return new SimPoint(-screenMaxX, 0.0);
                case GravitySim.BodyStartPosition.ScreenRight:
                    return new SimPoint(screenMaxX, 0.0);
                case GravitySim.BodyStartPosition.ScreenTop:
                    return new SimPoint(0.0, screenMaxY);
                case GravitySim.BodyStartPosition.ScreenBottom:
                    return new SimPoint(0.0, -screenMaxY);

                case GravitySim.BodyStartPosition.CenterOfTheUniverse:
                    return new SimPoint(0.0, 0.0);

                case GravitySim.BodyStartPosition.RandomStagePosition:
                    return new SimPoint(rand.Next((int)-simBoxMaxXY, (int)simBoxMaxXY),
                                      rand.Next((int)-simBoxMaxXY, (int)simBoxMaxXY));
                case GravitySim.BodyStartPosition.RandomScreenPosition:
                    return new SimPoint(rand.Next((int)-screenMaxX, (int)screenMaxX),
                                      rand.Next((int)-screenMaxY, (int)screenMaxY));

                // This approach gives us higher density toward the center of the circle
                case GravitySim.BodyStartPosition.RandomDenseCenterCircularCluster:
                    double length = rand.NextDouble() * simBoxMaxXY * 0.9;
                    double angle = rand.NextDouble() * Math.PI * 2.0; // radians
                    return new SimPoint(length * Math.Cos(angle), length * Math.Sin(angle));
                
                // This approach gives us uniform density throughout the circle
                case GravitySim.BodyStartPosition.RandomUniformDensityCircularCluster:
                    SimPoint newBodyPosition;
                    double limitXY = 0.9 * simBoxMaxXY;
                    do
                    {
                        newBodyPosition = new SimPoint(rand.Next((int)-limitXY, (int)limitXY),
                                      rand.Next((int)-limitXY, (int)limitXY));
                    }
                    while (MainPage.Hypotenuse(newBodyPosition) > limitXY);
                    return (newBodyPosition);

                default:
                    return new SimPoint(0.0, 0.0);
            }
        }
    }
}

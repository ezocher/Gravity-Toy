using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GravitySandboxUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GravitySim sim;
        ThreadPoolTimer timer;
        CoreDispatcher dispatcher;
        Random rand;
        bool simRunning;
        bool firstRun;

        const double ticksPerSecond = 32.0;             // powers of two for numerical stability (divisions)
        const double defaultStepInterval = 1.0 / 8.0;   // powers of two for numerical stability (divisions)

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            sim = new GravitySim(backgroundCanvas, this);
            rand = new Random();
            simRunning = false;
            SetRunPauseButton(!simRunning);
            firstRun = true;

            // The inital Scenario is loaded by BackgroundGrid_SizeChanged(), which is fired when the app's window size is set initially 
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in accel Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void BackgroundGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            if (e.OriginalSource == backgroundGrid)        // Ignore the tap if it was routed from another control
            {
                sim.Step(defaultStepInterval);
            }
             * */
        }



        public void RunSimTick(ThreadPoolTimer tpt)
        {
            // TBD: run calculations on a background thread and UI updates on UI thread
            var ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                const double tick = 1.0 / ticksPerSecond; // seconds
                sim.Step(tick);
            });
        }




        // Needs to be marshalled onto the UI thread
        public void UpdateMonitoredValues(Flatbody body, double simElapsedTime)
        {
            positionTextBlock.Text = "position = " + FormatPointToString(body.Position);
            velocityTextBlock.Text = "velocity = " + FormatPointToString(body.Velocity);
            timeTextBlock.Text = String.Format("time = {0:F3}", simElapsedTime);
        }

        static string FormatPointToString(Point p)
        {
            return String.Format("{0:F3}, {1:F3}", p.X, p.Y);
        }


        private void BackgroundGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Pause the running simulation
            if (simRunning)
                timer.Cancel();

            sim.renderer.SetSimulationTransform(backgroundCanvas.ActualWidth, backgroundCanvas.ActualHeight);
            // var res = DisplayProperties.ResolutionScale;  // don't need this (yet)
            if (!firstRun)
                sim.TransformChanged();

            // TBD: restart the running simulation
            if (simRunning)
                timer = ThreadPoolTimer.CreatePeriodicTimer(RunSimTick, new TimeSpan(0, 0, 0, 0, 1000 / (int)ticksPerSecond));

            // TBD: Figure out which event to hook to initialize the initially loaded scenario (or maybe we have to do it this way since we need the initial layout to occur before loading the 
            //          starting scenario)
            if (firstRun)
            {
                firstRun = false;
                ScenarioChanging();
                // BuiltInScenarios.LoadTwoBodiesScenario(sim);

                var ellipse1 = new Ellipse();
                ellipse1.Fill = new SolidColorBrush(Windows.UI.Colors.Yellow);
                ellipse1.Width = 150;
                ellipse1.Height = 150;
                backgroundCanvas.Children.Add(ellipse1);


                var rect1 = new Rectangle();
                rect1.Fill = new SolidColorBrush(Windows.UI.Colors.BlueViolet);
                rect1.Width = 222;
                rect1.Height = 111;
                backgroundCanvas.Children.Add(rect1);

                setpositions();
            }
        }

        int translateFactor = 0;

        private void setpositions()
        {
            var tt = new TranslateTransform();
            tt.X = translateFactor * 40.0;
            tt.Y = translateFactor * 75.0;

            var uu = new TranslateTransform();
            uu.X = 1000.0 - (translateFactor * 33.33);
            uu.Y = 600.0 - (translateFactor * 10.0);

            translateFactor++;

            backgroundCanvas.Children[0].RenderTransform = tt;
            backgroundCanvas.Children[1].RenderTransform = uu;
        }

        private void ScenarioChanging()
        {
            if (timer != null) timer.Cancel(); // If previous simulation is still running, stop it
            simRunning = false;
            SetRunPauseButton(true);
        }


        private void Button_Click_Scenario1(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadTwoBodiesScenario(sim);
        }

        private void Button_Click_Scenario2(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadNineBodiesScenario(sim);
        }

        private void Button_Click_Scenario3(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadThreeHundredRandomBodies(sim);
        }

        private void Button_Click_Scenario4(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            // BuiltInScenarios.LoadFiveBodiesScenario(sim);
            BuiltInScenarios.LoadThreeHundredRandomBodies(sim);
        }

        private void stepButton_Click(object sender, RoutedEventArgs e)
        {
            setpositions();
            // sim.Step(defaultStepInterval);
        }

        private void SetRunPauseButton(bool setToRun)
        {
            if (setToRun)
                runPauseButton.Content = "Run";
            else
                runPauseButton.Content = "Pause";
            stepButton.IsEnabled = setToRun;      // Step is available when Run is available
        }

        private void runPauseButton_Click(object sender, RoutedEventArgs e)
        {
            SetRunPauseButton(simRunning);

            if (simRunning)
            {
                // Pause button clicked
                timer.Cancel();
            }
            else
            {
                // Run button clicked
                timer = ThreadPoolTimer.CreatePeriodicTimer(RunSimTick, new TimeSpan(0, 0, 0, 0, 1000 / (int)ticksPerSecond));
            }
            simRunning = !simRunning;
        }

        #region tests
        private void testCoordinateMapping()
        {
            TranslateTransform t = new TranslateTransform();

            Flatbody a = new Flatbody(new Point(0, 0));
            t = sim.renderer.CircleTransform(a);
            Flatbody b = new Flatbody(new Point(-500, 0));
            t = sim.renderer.CircleTransform(b);
            Flatbody c = new Flatbody(new Point(500, 0));
            t = sim.renderer.CircleTransform(c);
            Flatbody d = new Flatbody(new Point(0, 500));
            t = sim.renderer.CircleTransform(d);
            Flatbody e = new Flatbody(new Point(0, -500));
            t = sim.renderer.CircleTransform(e);
        }
        #endregion

        private void backgroundGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {}

        private void backgroundGrid_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {}

        private void backgroundGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {}

    }
}

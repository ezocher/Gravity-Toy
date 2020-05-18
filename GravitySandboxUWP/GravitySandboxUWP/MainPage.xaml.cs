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
using System.Diagnostics;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GravitySandboxUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GravitySim sim;
        ThreadPoolTimer frameTimer;
        CoreDispatcher dispatcher;
        Random rand;
        bool simRunning;
        bool firstRun;

        const double ticksPerSecond = 32.0;             // powers of two for numerical stability (divisions)
        const double defaultStepInterval = 1.0 / 8.0;   // powers of two for numerical stability (divisions)

        private static bool frameInProgress = false;
        private static long framesRendered = 0;
        private static long framesDropped = 0;
        private static long totalFrameDelay = 0;

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            sim = new GravitySim(backgroundCanvas, this, dispatcher);
            rand = new Random();
            simRunning = false;
            SetRunPauseButton(!simRunning);
            firstRun = true;
            frameInProgress = false;

            // The inital Scenario is loaded by BackgroundGrid_SizeChanged(), which is fired when the app's window size is set initially

            DisplayTimerProperties();
        }

        public static void DisplayTimerProperties()
        {
            // Display the timer frequency and resolution.
            if (Stopwatch.IsHighResolution)
            {
                Debug.WriteLine("Operations timed using the system's high-resolution performance counter.");
            }
            else
            {
                Debug.WriteLine("Operations timed using the DateTime class.");
            }

            long frequency = Stopwatch.Frequency;
            Debug.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            Debug.WriteLine("  Timer frequency in ticks per millisecond = {0}", frequency / 1000L);
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
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

        //  Updated to do all calculations in this worker thread and to do UI updates that happen inside of this on the UI thread
        public void RunSimTick(ThreadPoolTimer tpt)
        {
            const double tick = 1.0 / ticksPerSecond; // seconds
            const int reportingInterval = 5 * (int)ticksPerSecond;

            // Added check to see if the previous frame is still calculating/rendering when this method gets called by the timer
            // Sufficiently large scenarios (size varies depending on the PC) can take longer than a frame tick to run
            //  When this happens a frame is dropped and we wait until the next tick to check again

            // Waiting an entire frame also gives the XAML/UWP rendering tasks time to finish before they are fired again
            // The amount of rendering work is proportional to the amount of calculation work, so this all works out

            if (frameInProgress)
            {
                framesDropped++;
            }
            else
            {
                frameInProgress = true;
                sim.Step(tick, simRunning);
                frameInProgress = false;
                framesRendered++;

                if ((framesRendered % reportingInterval) == 0)
                    Debug.WriteLine("Frames rendered = {0}, dropped = {1}, dropped pct = {2:F2}", framesRendered, framesDropped,
                        100.0 * (float)framesDropped / (float)(framesRendered + framesDropped));
            }   
        }

        // Updated to be marshalled onto the UI thread
        public void UpdateMonitoredValues(Flatbody body, double simElapsedTime)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                positionTextBlock.Text = "position = " + FormatPointToString(body.Position);
                velocityTextBlock.Text = "velocity = " + FormatPointToString(body.Velocity);
                timeTextBlock.Text = String.Format("time = {0:F3}", simElapsedTime);
            });
        }

        static string FormatPointToString(Point p)
        {
            return String.Format("{0:F3}, {1:F3}", p.X, p.Y);
        }

        private void BackgroundGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Pause the running simulation
            if (simRunning)
                frameTimer.Cancel();

            sim.renderer.SetSimulationTransform(backgroundCanvas.ActualWidth, backgroundCanvas.ActualHeight);
            // var res = DisplayProperties.ResolutionScale;  // don't need this (yet)
            if (!firstRun)
                sim.TransformChanged();

            // TBD: restart the running simulation
            if (simRunning)
                frameTimer = ThreadPoolTimer.CreatePeriodicTimer(RunSimTick, new TimeSpan(0, 0, 0, 0, 1000 / (int)ticksPerSecond));

            // TBD: Figure out which event to hook to initialize the initially loaded scenario (or probably we have to do it this way since we need the initial layout to occur before loading the 
            //          starting scenario)
            if (firstRun)
            {
                firstRun = false;
                Button_Click_Scenario1(null, null);
            }
        }

        private void ScenarioChanging()
        {
            if (frameTimer != null) frameTimer.Cancel(); // If previous simulation is still running, stop it
            simRunning = false;
            SetRunPauseButton(true);
            framesRendered = framesDropped = totalFrameDelay = 0L;
            frameInProgress = false;

            // Wait 1 simulation tick for any frames in progress to finish
            Task.Delay(1000 / (int)ticksPerSecond).Wait();
        }

        private void Button_Click_Scenario1(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadNineBodiesScenario(sim);
        }

        private void Button_Click_Scenario2(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadXRandomBodies(sim, 300, SimRender.ColorScheme.allColors);
        }

        private void Button_Click_Scenario3(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadXBodiesCircularCluster(sim, 500, SimRender.ColorScheme.pastelColors);
        }

        private void Button_Click_Scenario4(object sender, RoutedEventArgs e)
        {
            ScenarioChanging();
            BuiltInScenarios.LoadXBodiesCircularCluster(sim, 418, SimRender.ColorScheme.grayColors);
            // BuiltInScenarios.LoadFourBodiesScenario(sim);
        }

        private void stepButton_Click(object sender, RoutedEventArgs e)
        {
            sim.Step(defaultStepInterval, simRunning);
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
                frameTimer.Cancel();
            }
            else
            {
                // Run button clicked
                frameTimer = ThreadPoolTimer.CreatePeriodicTimer(RunSimTick, new TimeSpan(0, 0, 0, 0, 1000 / (int)ticksPerSecond));
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

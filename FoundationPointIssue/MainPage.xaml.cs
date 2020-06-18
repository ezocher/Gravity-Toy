using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FoundationPointIssue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ShowWindowsFoundationPointIsFloatInternally();
        }


        // This is a demonstration that in UWP Windows.Foundation.Point is publicly (double, double) but is (float, float) internally.
        // Microsoft Docs: https://docs.microsoft.com/en-us/dotnet/api/windows.foundation.point?view=dotnet-plat-ext-3.1 
        // Created Issue #35 to create a Docs PR
        //
        //  Debug Output:
        //      p.X = 0.3333333432674408
        //      f =   0.3333333432674408
        //      d =   0.33333333333333331
        //      p.X == b? False

        private static void ShowWindowsFoundationPointIsFloatInternally()
        {
            Point p = new Windows.Foundation.Point(1.0/3.0, 0.0);
            float f = 1.0f / 3.0f;
            double d = 1.0 / 3.0;

            Debug.WriteLine("p.X = {0:G17}", p.X);
            Debug.WriteLine("f =   {0:G17}", (double)f);
            Debug.WriteLine("d =   {0:G17}", d);
            Debug.WriteLine("p.X == d? {0}", p.X == d);
        }
    }
}

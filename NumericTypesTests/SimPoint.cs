using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

// Lifted verbatim from GravitySandboxUWP
namespace NumericTypesTests
{
    // Was using Windows.Foundation.Point for positions, accelerations, and velocities, but discovered that it is (float, float) internally.
    //  This is a true (double, double) point struct to use for simulation values.

    public struct SimPoint : IEquatable<SimPoint>
    {
        public double X { get; set; }
        public double Y { get; set; }

        public SimPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return String.Format("({0:G17}, {1:G17})", X, Y);
        }

        public bool Equals(SimPoint other)
        {
            return ((X == other.X) && (Y == other.Y));
        }

        public double Magnitude()
        {
            return (Math.Sqrt((X * X) + (Y * Y)));
        }

        public static SimPoint operator +(SimPoint point1, SimPoint point2) =>
            new SimPoint(point1.X + point2.X, point1.Y + point2.Y);

        public static SimPoint operator *(SimPoint point, double factor) =>
            new SimPoint(point.X * factor, point.Y * factor);
    }
}

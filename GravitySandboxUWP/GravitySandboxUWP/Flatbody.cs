using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GravitySandboxUWP
{
    public class Flatbody
    {
        // TBD: refactor to SimSpace
        private const double earthSurfaceGravity = 9.80665; // m/sec^2

        // Mass example ratios:
        //   Sol is 1047.57 Jupiter masses and 333,000 Earth masses
        //   Sagittarius A* is 2.6 ± 0.2 million Solar masses

        // TBD: refactor to SimSpace
        // Mass is in abstract units and is designed to make interesting accelerations happen at the scale and speed of the simulation
        //      Requested masses are scaled by the massFactor, so a requested mass of 1.0 is 100,000 mass
        private const double massFactor = 100000.0; // TBD: refactor to SimSpace
        private const double defaultMass = 1.0;
        
        double mass;
        public double Mass
        {
            get { return mass; }
            private set { mass = value * massFactor; }
        }

        // Size is in abstract units, with 1.0 the smallest size normally rendered
        private const double defaultSize = 2.0; // TBD: refactor to SimSpace
        double size;
        public double Size
        {
            get { return size; }
            private set { size = value; }
        }

        Point position;
        public Point Position
        {
            get { return position; }
            private set { position = value; }
        }

        private Point defaultStartingVelocity = new Point(0.0, 0.0);
        Point velocity;
        public Point Velocity
        {
            get { return velocity; }
            private set { velocity = value; }
        }

        public Flatbody(Point bodyStartingPosition)
        {
            Mass = defaultMass;
            Size = defaultSize;
            Position = bodyStartingPosition;
            Velocity = defaultStartingVelocity;
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition)
        {
            Mass = bodyMass;
            Size = bodySize;
            Position = bodyStartingPosition;
            Velocity = defaultStartingVelocity;
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition, Point bodyStartingVelocity)
        {
            Mass = bodyMass;
            Size = bodySize;
            Position = bodyStartingPosition;
            Velocity = bodyStartingVelocity;
        }

        /*
        public Point EarthSurfaceAccelerate()
        {
            return (new Point(0.0, -earthSurfaceGravity));  // "Earth" along the bottom of the screen
        }
        */

        // TBD: Look holistically at distance minimums and acceleration limits and clean up and centralize
        // Need to keep at least some minimum value for r to avoid divide by zero
        public Point BodyToBodyAccelerate(Flatbody otherBody)
        {
            const double rMinimum = 10.0;   // we are not simulating collisions so don't let accelerations run away as bodies
                                           //  approach 0.0 separation
            const double rMinSquared = rMinimum * rMinimum;

            double rX = otherBody.position.X - position.X;
            double rY = otherBody.position.Y - position.Y;

            double rSquared = (rX * rX) + (rY * rY);
            rSquared = Math.Max(rSquared, rMinSquared); // enforce minimum value of r
            double r = Math.Sqrt(rSquared);

            // TBD: refactor to SimSpace (currently omits g)
            // F = m1 * a, g = m1 * m2 / rSquared, m1's cancel out so a = m2 / rSquared
            double a = otherBody.mass / rSquared;

            return (new Point(a * rX / r, a * rY / r));
        }

        public void Move(Point accel, double deltaT)
        {
            // Applying linear acceleration during the time interval

            double newVelocityX = velocity.X + (accel.X * deltaT);
            position.X += (velocity.X + newVelocityX) / 2 * deltaT;
            velocity.X = newVelocityX;

            double newVelocityY = velocity.Y + (accel.Y * deltaT);
            position.Y += (velocity.Y + newVelocityY) / 2 * deltaT;
            velocity.Y = newVelocityY;
        }
    }
}

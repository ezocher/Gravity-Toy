using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GravitySandboxUWP
{
    public struct Flatbody
    {
        private const double earthSurfaceGravity = 9.80665; // m/sec^2

        // Mass is in abstract units and is designed to make interesting accelerations happen at the scale of the simulation
        //      Requested masses are scaled by the massFactor, so a requested mass of 1.0 is 100,000 mass
        private const double massFactor = 100000.0;
        private const double defaultMass = 1.0;
        double mass;
        public double Mass
        {
            get { return mass; }
            private set { mass = value * massFactor; }
        }

        // Size is in abstract units, with 1.0 the smallest size normally rendered
        private const double defaultSize = 2.0;
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

        Point velocity;
        public Point Velocity
        {
            get { return velocity; }
            private set { velocity = value; }
        }

        public Flatbody(Point bodyStartingPosition)
        {
            mass = defaultMass;
            size = defaultSize;
            position = bodyStartingPosition;
            velocity = new Point(0.0, 0.0);
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition)
        {
            mass = bodyMass;
            size = bodySize;
            position = bodyStartingPosition;
            velocity = new Point(0.0, 0.0);
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition, Point bodyStartingVelocity)
        {
            mass = bodyMass;
            size = bodySize;
            position = bodyStartingPosition;
            velocity = bodyStartingVelocity;
        }

        public Point EarthSurfaceAccelerate()
        {
            return (new Point(0.0, -earthSurfaceGravity));  // "earth" along the bottom of the screen
        }

        public Point BodyToBodyAccelerate(Flatbody otherBody)
        {
            const double rMinimum = 10.0;   // we are not simulating collisions so don't let accelerations run away as bodies
                                            //  approach 0.0 separation
            const double rMinSquared = rMinimum * rMinimum;

            double rX = otherBody.position.X - this.position.X;
            double rY = otherBody.position.Y - this.position.Y;

            double rSquared = (rX * rX) + (rY * rY);
            rSquared = Math.Max(rSquared, rMinSquared); // enforce minimum value of r
            double r = Math.Sqrt(rSquared);
            double a = otherBody.mass / rSquared;

            return (new Point(a * rX / r, a * rY / r));
        }

        public void Move(Point accel, double deltaT)
        {
            Point newVelocity = new Point(0.0, 0.0);

            newVelocity.X = velocity.X + (accel.X * deltaT);
            position.X += (velocity.X + newVelocity.X) / 2 * deltaT;
            velocity.X = newVelocity.X;

            newVelocity.Y = velocity.Y + (accel.Y * deltaT);
            position.Y += (velocity.Y + newVelocity.Y) / 2 * deltaT;
            velocity.Y = newVelocity.Y;
        }
    }
}

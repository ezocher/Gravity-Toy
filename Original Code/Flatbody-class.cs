using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;



namespace Gravity_Sandbox
{
    public class Flatbody
    {
        private const double earthSurfaceGravity = 9.80665; // m/sec^2

        private Point defaultStartingVelocity = new Point(0.0, 0.0);

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
            Mass = defaultMass;
            size = defaultSize;
            position = bodyStartingPosition;
            velocity = defaultStartingVelocity;
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition)
        {
            Mass = bodyMass;
            size = bodySize;
            position = bodyStartingPosition;
            velocity = defaultStartingVelocity;
        }

        public Flatbody(double bodyMass, double bodySize, Point bodyStartingPosition, Point bodyStartingVelocity)
        {
            Mass = bodyMass;
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
                                            // was 10.0
            const double rMinimum = 0.1;    // we are not simulating collisions so don't let accelerations run away as bodies
                                            //  approach 0.0 separation
            const double rMinSquared = rMinimum * rMinimum;

            double dX = otherBody.position.X - this.position.X;
            double dY = otherBody.position.Y - this.position.Y;

            double rSquared = (dX * dX) + (dY * dY);
            rSquared = Math.Max(rSquared, rMinSquared); // enforce minimum value of r
            double a = otherBody.mass/rSquared; //F = ma, g = m * m2 / rSquared, m's cancel out so a = m2 / rSquared

            double r = Math.Sqrt(rSquared);

            // return aX, aY
            return (new Point(a * dX/r, a * dY/r));
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

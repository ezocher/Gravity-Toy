using System;
using System.Diagnostics;

namespace GravitySandboxUWP
{
    public class Body
    {
        // Mass, size, position, and velocity are in the units defined by the SimSpace used by the scenario

        private const double defaultValue = 1.0;

        public double Mass { get; private set; }

        public double Size { get; private set; }

        SimPoint position;
        public SimPoint Position
        {
            get { return position; }
            private set { position = value; }
        }

        private SimPoint defaultStartingVelocity = new SimPoint(0.0, 0.0);
        private SimPoint velocity;
        public SimPoint Velocity
        {
            get { return velocity; }
            private set { velocity = value; }
        }

        // GravitySource is true for bodies that exert gravity on all other bodies and false for those that don't
        //    exert gravity on any other bodies
        //
        // Usage: For scenarios with many tiny bodies orbiting a large one (e.g. satellites in earth orbit), 
        //   have only the larger body exert gravity. This will eliminate unnecessary calculations and also leave the large
        //   body undisturbed by all the tiny things flying around it
        private const bool defaultGravitySource = true;
        public bool IsGravitySource { get; private set; }

        private SimSpace simSpace;

        // To identify a Body during debugging. Automatically numbered in the order they're created when a scenario
        //   gets initialized
        private int bodyNumber;
        private static int currentBodyNumber = 0;

        // Limit total number of debug messages about precision issues
        private const int DebugMessageCountLimit = 500;
        private static int debugMessageCount = 0;

        public static void ResetBodyCount()
        {
            currentBodyNumber = 0;
            debugMessageCount = 0;
        }


        #region Constructors
        public Body(SimPoint bodyStartingPosition, SimSpace space)
        {
            Mass = defaultValue;
            Size = defaultValue;
            Position = bodyStartingPosition;
            Velocity = defaultStartingVelocity;
            IsGravitySource = defaultGravitySource;
            simSpace = space;
            bodyNumber = currentBodyNumber++;
        }

        public Body(double bodyMass, double bodySize, SimPoint bodyStartingPosition, SimSpace space)
        {
            Mass = bodyMass;
            Size = bodySize;
            Position = bodyStartingPosition;
            Velocity = defaultStartingVelocity;
            IsGravitySource = defaultGravitySource;
            simSpace = space;
            bodyNumber = currentBodyNumber++;
        }

        public Body(double bodyMass, double bodySize, SimPoint bodyStartingPosition, SimPoint bodyStartingVelocity, SimSpace space)
        {
            Mass = bodyMass;
            Size = bodySize;
            Position = bodyStartingPosition;
            Velocity = bodyStartingVelocity;
            IsGravitySource = defaultGravitySource;
            simSpace = space;
            bodyNumber = currentBodyNumber++;
        }

        public Body(double bodyMass, double bodySize, SimPoint bodyStartingPosition, SimPoint bodyStartingVelocity,
            bool isGravitySource, SimSpace space)
        {
            Mass = bodyMass;
            Size = bodySize;
            Position = bodyStartingPosition;
            Velocity = bodyStartingVelocity;
            IsGravitySource = isGravitySource;
            simSpace = space;
            bodyNumber = currentBodyNumber++;
        }
        #endregion

        #region 2D Physics

        public SimPoint BodyToBodyAccelerate(Body otherBody)
        {
            double rX = otherBody.position.X - position.X;
            double rY = otherBody.position.Y - position.Y;

            double rSquared = (rX * rX) + (rY * rY);
            rSquared = Math.Max(rSquared, GravitySim.minimumSeparationSquared); // enforce minimum value of r
            double r = Math.Sqrt(rSquared);

            // F = m1 * a, g = G * m1 * m2 / rSquared, m1's cancel out so a = G * m2 / rSquared
            double a = simSpace.BigG * otherBody.Mass / rSquared;

            return (new SimPoint(a * rX / r, a * rY / r));
        }

        public void Move(SimPoint accel, double deltaT)
        {
            if (!((accel.X == 0.0) && (accel.Y == 0.0)))
            {
                // Apply linear acceleration during the time interval

                double newVelocityX = velocity.X + (accel.X * deltaT);
                position.X += (velocity.X + newVelocityX) / 2 * deltaT;
                velocity.X = newVelocityX;

                double newVelocityY = velocity.Y + (accel.Y * deltaT);
                position.Y += (velocity.Y + newVelocityY) / 2 * deltaT;
                velocity.Y = newVelocityY;
            }
        }

        public void MoveWithPrecisionCheck(SimPoint accel, double deltaT)
        {
            if (!((accel.X == 0.0) && (accel.Y == 0.0)))
            {
                // Apply linear acceleration during the time interval

                double deltaVX = accel.X * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(velocity.X, deltaVX))
                    DisplayPrecisionIssue(velocity.X, deltaVX, "Adding DeltaV to VX", bodyNumber);
                double newVelocityX = velocity.X + deltaVX;

                double deltaPX = (velocity.X + newVelocityX) / 2 * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(position.X, deltaPX))
                    DisplayPrecisionIssue(position.X, deltaPX, "Adding to Position.X", bodyNumber);
                position.X += deltaPX;

                velocity.X = newVelocityX;


                double deltaVY = accel.Y * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(velocity.Y, deltaVY))
                    DisplayPrecisionIssue(velocity.Y, deltaVY, "Adding DeltaV to VY", bodyNumber);
                double newVelocityY = velocity.Y + deltaVY;

                double deltaPY = (velocity.Y + newVelocityY) / 2 * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(position.Y, deltaPY))
                    DisplayPrecisionIssue(position.Y, deltaPY, "Adding to Position.Y", bodyNumber);
                position.Y += deltaPY;

                velocity.Y = newVelocityY;
            }
        }

        public static void DisplayPrecisionIssue(double a, double b, string whichCalculation, int bodyNumber)
        {
            if (debugMessageCount++ < DebugMessageCountLimit)
                Debug.WriteLine("Body #{4} {3}: a = {0:G17}, b = {1:G17}, mag diff = {2}", a, b, FloatingPointUtil.AdditionMagnitudeDifference(a, b), whichCalculation, bodyNumber);
        }
        #endregion
    }
}

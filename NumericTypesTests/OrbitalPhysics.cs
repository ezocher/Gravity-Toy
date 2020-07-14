using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NumericTypesTests
{
    public class OrbitalPhysics
    {
        private double BigG { get; set; }
        private double CentralBodyMass { get; set; }

        private const double MinimumSeparationSquared = 1.0;

        /// <summary>
        /// Central body is at 0.0, 0.0
        /// </summary>
        /// <param name="bigG"></param>
        /// <param name="centralBodyMass"></param>
        public OrbitalPhysics(double bigG, double centralBodyMass)
        {
            BigG = bigG;
            CentralBodyMass = centralBodyMass;
        }

        public SimPoint Accelerate(SimPoint position)
        {
            double rX = -position.X;
            double rY = -position.Y;

            double rSquared = (rX * rX) + (rY * rY);
            rSquared = Math.Max(rSquared, MinimumSeparationSquared); // enforce minimum value of r
            double r = Math.Sqrt(rSquared);

            // F = m1 * a, g = G * m1 * m2 / rSquared, m1's cancel out so a = G * m2 / rSquared
            double a = BigG * CentralBodyMass / rSquared;

            return (new SimPoint(a * rX / r, a * rY / r));
        }

        public void Move(ref SimPoint position, ref SimPoint velocity, SimPoint acceleration, double deltaT)
        {
            if (!((acceleration.X == 0.0) && (acceleration.Y == 0.0)))
            {
                // Apply linear acceleration during the time interval

                double newVelocityX = velocity.X + (acceleration.X * deltaT);
                position.X += (velocity.X + newVelocityX) / 2 * deltaT;
                velocity.X = newVelocityX;

                double newVelocityY = velocity.Y + (acceleration.Y * deltaT);
                position.Y += (velocity.Y + newVelocityY) / 2 * deltaT;
                velocity.Y = newVelocityY;
            }
        }

        public void MoveWithPrecisionCheck(ref SimPoint position, ref SimPoint velocity, SimPoint acceleration, double deltaT)
        {
            if (!((acceleration.X == 0.0) && (acceleration.Y == 0.0)))
            {
                // Apply linear acceleration during the time interval

                double deltaVX = acceleration.X * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(velocity.X, deltaVX))
                    DisplayPrecisionIssue(velocity.X, deltaVX, "Adding DeltaV to VX", 0);
                double newVelocityX = velocity.X + deltaVX;

                double deltaPX = (velocity.X + newVelocityX) / 2 * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(position.X, deltaPX))
                    DisplayPrecisionIssue(position.X, deltaPX, "Adding to Position.X", 0);
                position.X += deltaPX;

                velocity.X = newVelocityX;

                double deltaVY = acceleration.Y * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(velocity.Y, deltaVY))
                    DisplayPrecisionIssue(velocity.Y, deltaVY, "Adding DeltaV to VY", 0);
                double newVelocityY = velocity.Y + deltaVY;

                double deltaPY = (velocity.Y + newVelocityY) / 2 * deltaT;
                if (FloatingPointUtil.CheckAdditionPrecision(position.Y, deltaPY))
                    DisplayPrecisionIssue(position.Y, deltaPY, "Adding to Position.Y", 0);
                position.Y += deltaPY;

                velocity.Y = newVelocityY;
            }
        }

        private static int debugMessageCount = 0;
        private const int DebugMessageCountLimit = 1000;

        public static void DisplayPrecisionIssue(double a, double b, string whichCalculation, int bodyNumber)
        {
            if (debugMessageCount++ < DebugMessageCountLimit)
                Debug.WriteLine("Body #{4} {3}: a = {0:G17}, b = {1:G17}, mag diff = {2}", a, b, FloatingPointUtil.AdditionMagnitudeDifference(a, b), whichCalculation, bodyNumber);
            if (debugMessageCount == DebugMessageCountLimit)
                Debug.WriteLine(">>> Reached limit of {0:N0} precision warning messages. No more will be displayed.", DebugMessageCountLimit);
        }

    }
}

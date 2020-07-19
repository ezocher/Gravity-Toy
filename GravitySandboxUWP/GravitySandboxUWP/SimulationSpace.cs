using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace GravitySandboxUWP
{
    /* Definitions of dimensions and units in space, mass, and time for simulation spaces
     * 
     * Screen space is always in UWP XAML units and seconds
     * 
     */
    public class SimulationSpace
    {
        public enum Space {
            Null, Toy,
            LEO, GEO, EarthMoon, SolarSystem,
            BinaryOrTrinarySystem,
            StarCluster, MicroGalaxy
                };

        // Null space has no distance units and is 100 x 100 in extent

        // Original simulation space: toySpace
        // Dimensions   Mass   Time    Velocity

        // Spaces:
        //      Toy Space
        //      Earth Orbit Space
        //      Earth Moon System Space
        //      Solar System Space
        //      Star System Space (Binaries and trinaries)
        //      Star Cluster Space
        //      Faux Galaxy Space (for "colliding galaxies with central black holes")
        //      Galaxy Space

        #region Physical Constants

        // ========== GRAVITY and MASS ==========
        public const double BigG_M3PerKgSec2 = 6.6743E-11; // m^3/kg*sec^2

        // ========== DISTANCE ==========
        public const double KmPerMeter = 1.0 / 1000.0;

        // ========== TIME ==========
        public const double MinutesPerHour = 60.0;
        public const double SecondsPerMinute = 60.0;
        public const double SecondsPerHour = SecondsPerMinute * MinutesPerHour;

        // ========== SCREEN UNITS ==========
        private const double SmallestBodySizeAsPortionOfStartingScreenSize = (1.0 / 100.0) * 0.5;      // = 1/2 of a %

        #endregion

        #region Properties and Fields

        // ========== GRAVITY and MASS ==========
        public double BigG { get; private set; }

        public string MassUnitsAbbr { get; private set; }

        // ========== DISTANCE ==========
        public string DistanceUnitsAbbr { get; private set; }

        public double SimBoxHeightAndWidth { get; private set; }

        public double DistanceOffset { get; private set; }


        // ========== TIME ==========
        public TimeDisplay.BaseUnits TimeUnits { get; private set; }

        public double TimeUnitsPerUISecond { get; private set; }


        // ========== VELOCITY ==========
        public string VelocityUnitsAbbr { get; private set; }

        // Factor for deriving velocity when either distance or time units in velocity are
        //  different from base time or base distance units
        public double VelocityConnversionFactor { get; private set; }


        // ========== SCREEN UNITS ==========
        public double SmallestBodySizePx { get; private set; }

        #endregion

        #region Constructor
        public SimulationSpace(Space space)
        {
            if (space == Space.Toy)
            {
                BigG = 1.0;
                MassUnitsAbbr = "simass";

                DistanceUnitsAbbr = "simunits";
                SimBoxHeightAndWidth = 1000.0;
                DistanceOffset = 0.0;

                TimeUnits = TimeDisplay.BaseUnits.Seconds;
                TimeUnitsPerUISecond = 1.0;

                VelocityUnitsAbbr = "simunits/sec.";
                VelocityConnversionFactor = 1.0;

                SmallestBodySizePx = SimBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
            else if (space == Space.Null)
            {
                BigG = 1.0;
                MassUnitsAbbr = "";

                DistanceUnitsAbbr = "";
                SimBoxHeightAndWidth = 100.0;
                DistanceOffset = 0.0;

                TimeUnits = TimeDisplay.BaseUnits.Seconds;
                TimeUnitsPerUISecond = 1.0;

                VelocityUnitsAbbr = "";
                VelocityConnversionFactor = 1.0;

                SmallestBodySizePx = SimBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
            else if ((space == Space.LEO) || (space == Space.GEO))
            // Earth orbit scenarios
            //  Kilometers - Minutes - Kilograms - Kilometers/Minute
            {
                MassUnitsAbbr = "kg";

                DistanceUnitsAbbr = "km";
                if (space == Space.LEO)
                    SimBoxHeightAndWidth = 4.0 * SolarSystem.EarthRadiusKm;
                else // Space.GEO
                    SimBoxHeightAndWidth = 2.5 * SolarSystem.GeosynchronousOrbitRadiusKm;
                DistanceOffset = SolarSystem.EarthRadiusKm;

                TimeUnits = TimeDisplay.BaseUnits.Minutes;

                VelocityUnitsAbbr = "km/h";
                // Internal velocities are in km/min., multiply by this to get km/h
                VelocityConnversionFactor = MinutesPerHour;

                // Needs to be in kg, km, and minutes
                //      km^3/kg*min^2
                BigG = BigG_M3PerKgSec2 *
                    KmPerMeter * KmPerMeter * KmPerMeter * SecondsPerMinute * SecondsPerMinute;

                // Time base: 1 min / second real
                TimeUnitsPerUISecond = 1.0;

                SmallestBodySizePx = SimBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
            else
            {
                throw new NotImplementedException("SimulationSpace not implemented: " + space.ToString());
            }
        }
        #endregion

        // https://www.physicsclassroom.com/class/circles/Lesson-4/Mathematics-of-Satellite-Motion#:~:text=The%20orbital%20speed%20can%20be,speed%20of%207676%20m%2Fs.
        public double CircularOrbitVelocity(double centralMass, double orbitRadius)
        {
            return Math.Sqrt((BigG * centralMass) / orbitRadius) * VelocityConnversionFactor;
        }

        /// <summary>
        /// Puts a body into a clockwise circular orbit
        /// </summary>
        /// <param name="startingPosition">Angle, measured in degrees, with 0 degrees at top of orbit</param>
        /// <param name="orbitRadius"></param>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        public void InitializeCircularOrbit(double startingPosition, double orbitRadius, double centralMass, out SimPoint position, out SimPoint velocity)
        {
            double startingPositionRadians = Math.PI * 2.0 * (startingPosition / 360.0);
            position = new SimPoint(orbitRadius * Math.Sin(startingPositionRadians), orbitRadius * Math.Cos(startingPositionRadians));

            double orbitVelocity = CircularOrbitVelocity(centralMass, orbitRadius);
            velocity = new SimPoint(orbitVelocity * Math.Cos(startingPositionRadians), -orbitVelocity * Math.Sin(startingPositionRadians));
        }
    }
}

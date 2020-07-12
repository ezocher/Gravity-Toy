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

        public const double EarthSurfaceAccelerationMPerSec2 = 9.80665; // m/sec^2

        public const double EarthMassKg = 5.97220E+24;

        // Mass example ratios:
        //   Sol is 1047.57 Jupiter masses and 333,000 Earth masses
        //   Sagittarius A* is 2.6 ± 0.2 million Solar masses


        // ========== SPACE ==========
        public const double KmPerMeter = 1.0 / 1000.0;
        public const double EarthRadiusKm = 6371.0;
        public const double LEO_OrbitMaxAltitudeKm = 2000.0;


        // ========== TIME ==========
        public const double MinutesPerHour = 60.0;
        public const double SecondsPerMinute = 60.0;
        public const double SecondsPerHour = SecondsPerMinute * MinutesPerHour;


        // ========== VELOCITY ==========

        // Calculated for circular orbit of 200 km it is 7.79 km/s  (28,000 km/h)
        public const double EarthLEO200kmCircularVelocityKmH = 28000.0;


        // ========== SCREEN UNITS ==========
        private const double SmallestBodySizeAsPortionOfStartingScreenSize = 0.005;      // = 1/2 of a %

        #endregion

        #region Spacecraft

        // ========== ISS ==========
        // Averages across one orbit from https://spotthestation.nasa.gov/tracking_map.cfm on 6/19/2020 at 22:25:00 GMT
        public const double ISS_OrbitRadiusKm = 424.72 + EarthRadiusKm;
        public const double ISS_OrbitVelocityKmH = 27570.2;

        // Other spacecraft
        public const double StarlinkOrbitRadiusKm = 550.0 + EarthRadiusKm;
        public const double StarlinkOrbitVelocityKmH = 27320.0;

        public const double GPS_OrbitRadiusKm = 20180.0 + EarthRadiusKm;
        public const double GPS_OrbitVelocityKmH = 13949.0;

        public const double GeosynchronousOrbitRadiusKm = 42164.2;
        //public const double GeosynchronousOrbitRadiusKm = 35786.0 + EarthRadiusKm;
        public const double GeosynchronousOrbitVelocityKmH = 11070.0;


        #endregion


        #region Properties and Fields

        // ========== GRAVITY and MASS ==========
        public double BigG { get; private set; }

        public string MassUnitsAbbr { get; private set; }

        // ========== SPACE ==========
        public string DistanceUnitsAbbr { get; private set; }

        public double SimBoxHeightAndWidth { get; private set; }

        public double DistanceOffset { get; private set; }


        // ========== TIME ==========
        public string TimeUnitsAbbr { get; private set; }

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

                TimeUnitsAbbr = "sec.";
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

                TimeUnitsAbbr = "sec.";
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
                    SimBoxHeightAndWidth = 4.0 * EarthRadiusKm;
                else // Space.GEO
                    SimBoxHeightAndWidth = 10.0 * EarthRadiusKm;
                DistanceOffset = EarthRadiusKm;

                TimeUnitsAbbr = "min.";
                TimeUnitsPerUISecond = 1.0;

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
    }
}

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
    public class SimSpace
    {
        public enum DefinedSpace { NullSpace, ToySpace, LEOSpace   // , SolarSystemSpace
                };

        // Null space has no distance units and is 100 x 100 in extent

        // Original simulation space: toySpace
        // Dimensions   Mass   Time    Velocity


        // Spaces:
        //      Toy Space
        //      Earth Orbit Space
        //      Solar System Space
        //      Star System Space (Binaries and trinaries)
        //      Globular Cluster Space
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
        // From https://spotthestation.nasa.gov/tracking_map.cfm on 5/25/2020 at 00:30:00 GMT
        public const double ISS_OrbitAltitudeKm = 420.0;
        public const double ISS_OrbitVelocityKmH = 27583.0;

        public const double StarlinkOrbitAltitudeKm = 550.0;
        public const double StarlinkOrbitVelocityKmH = 27320.0;

        public const double GPS_OrbitAltitudeKm = 20180.0;
        public const double GPS_OrbitVelocityKmH = 13949.0;

        public const double GeosynchronousOrbitAltitudeKm = 35786.0;
        public const double GeosynchronousOrbitVelocityKmH = 11070.0;


        #endregion


        #region Fields

        // ========== GRAVITY and MASS ==========
        readonly double bigG;
        public double BigG { get { return bigG;  } }

        readonly string massUnitsAbbr;
        public string MassUnitsAbbr { get { return massUnitsAbbr; } }


        // ========== SPACE ==========
        readonly string distanceUnitsAbbr;
        public string DistanceUnitsAbbr { get { return distanceUnitsAbbr; } }

        readonly double simBoxHeightAndWidth;
        public double SimBoxHeightAndWidth {  get { return simBoxHeightAndWidth; } }

        readonly double distanceOffset;
        public double DistanceOffset {  get { return distanceOffset; } }


        // ========== TIME ==========
        readonly string timeUnitsAbbr;
        public string TimeUnitsAbbr { get { return timeUnitsAbbr; } }

        readonly double timeUnitsPerUISecond;
        public double TimeUnitsPerUISecond {  get { return timeUnitsPerUISecond; } }


        // ========== VELOCITY ==========
        readonly string velocityUnitsAbbr;
        public string VelocityUnitsAbbr { get { return velocityUnitsAbbr; } }

        // Factor for deriving velocity when either distance or time units in velocity are
        //  different from base time or distance units
        readonly double velocityConversionFactor;
        public double VelocityConnversionFactor { get { return velocityConversionFactor; } }


        // ========== SCREEN UNITS ==========
        readonly double smallestBodySizePx;
        public double SmallestBodySizePx { get { return smallestBodySizePx; } }

        #endregion


        #region Constructor
        public SimSpace(DefinedSpace space)
        {
            if (space == DefinedSpace.ToySpace)
            {
                bigG = 1.0;
                massUnitsAbbr = "simass";

                distanceUnitsAbbr = "simunits";
                simBoxHeightAndWidth = 1000.0;
                distanceOffset = 0.0;

                timeUnitsAbbr = "sec.";
                timeUnitsPerUISecond = 1.0;

                velocityUnitsAbbr = "simunits/sec.";
                velocityConversionFactor = 1.0;

                smallestBodySizePx = simBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
            else if (space == DefinedSpace.NullSpace)
            {
                bigG = 1.0;
                massUnitsAbbr = "";

                distanceUnitsAbbr = "";
                simBoxHeightAndWidth = 100.0;
                distanceOffset = 0.0;

                timeUnitsAbbr = "sec.";
                timeUnitsPerUISecond = 1.0;

                velocityUnitsAbbr = "";
                velocityConversionFactor = 1.0;

                smallestBodySizePx = simBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
            else if (space == DefinedSpace.LEOSpace)
            {
                bigG = 1.0;
                massUnitsAbbr = "kg";

                distanceUnitsAbbr = "km";
                simBoxHeightAndWidth = 4.0 * EarthRadiusKm;
                distanceOffset = EarthRadiusKm;

                timeUnitsAbbr = "min.";
                timeUnitsPerUISecond = 1.0;

                velocityUnitsAbbr = "km/h";
                // Internal velocities are in km/min., multiply by this to get km/h
                velocityConversionFactor = MinutesPerHour;

                // Needs to be in kg, km, and minutes
                //      km^3/kg*min^2
                bigG = BigG_M3PerKgSec2 *
                    KmPerMeter * KmPerMeter * KmPerMeter * SecondsPerMinute * SecondsPerMinute;

                // Time base: 1 min / second real
                timeUnitsPerUISecond = 1.0;

                smallestBodySizePx = simBoxHeightAndWidth * SmallestBodySizeAsPortionOfStartingScreenSize;
            }
        }
        #endregion
    }
}

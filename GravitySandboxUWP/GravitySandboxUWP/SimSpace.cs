using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GravitySandboxUWP
{
    /* Definitions of dimensions and units in space, mass, and time for simulation spaces
     * 
     * Screen space is always in UWP XAML units and seconds
     * 
     */
    class SimSpace
    {
        public enum DefinedSpace { NullSpace, ToySpace, LEOSpace   // , SolarSystemSpace
                };

        // Null space has no units and is 1 x 1 in extent

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

        public const double EarthRadiusKm = 6371.0;
        public const double LEO_OrbitMaxAltitudeKm = 2000.0;

        readonly string velocityUnitsAbbr;
        public string VelocityUnitsAbbr { get { return velocityUnitsAbbr; } }

        readonly string distanceUnitsAbbr;
        public string DistanceUnitsAbbr { get { return distanceUnitsAbbr; } }

        readonly string timeUnitsAbbr;
        public string TimeUnitsAbbr { get { return timeUnitsAbbr; } }

        readonly double simBoxHeightAndWidth;
        public double SimBoxHeightAndWidth {  get { return simBoxHeightAndWidth; } }

        readonly double distanceOffset;
        public double DistanceOffset {  get { return distanceOffset; } }

        public SimSpace(DefinedSpace space)
        {
            if (space == DefinedSpace.ToySpace)
            {
                this.velocityUnitsAbbr = "";
                this.distanceUnitsAbbr = "";
                this.timeUnitsAbbr = "sec.";

                this.simBoxHeightAndWidth = 1000.0;
                this.distanceOffset = 0.0;
            }
            else if (space == DefinedSpace.NullSpace)
            {
                this.velocityUnitsAbbr = "";
                this.distanceUnitsAbbr = "";
                this.timeUnitsAbbr = "";

                this.simBoxHeightAndWidth = 500.0;
                this.distanceOffset = 0.0;
            }
            else if (space == DefinedSpace.LEOSpace)
            {
                this.velocityUnitsAbbr = "km/h";
                this.distanceUnitsAbbr = "km";
                this.timeUnitsAbbr = "min.";

                this.simBoxHeightAndWidth = 4.0 * EarthRadiusKm;
                this.distanceOffset = EarthRadiusKm;
            }
        }
    }
}

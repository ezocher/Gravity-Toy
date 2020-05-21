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
        public enum DefinedSpace { NullSpace, ToySpace //, EarthOrbitSpace, SolarSystemSpace
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


        readonly string velocityUnitsAbbr;
        public string VelocityUnitsAbbr { get { return velocityUnitsAbbr; } }

        readonly string distanceUnitsAbbr;
        public string DistanceUnitsAbbr { get { return distanceUnitsAbbr; } }

        readonly string timeUnitsAbbr;
        public string TimeUnitsAbbr { get { return timeUnitsAbbr; } }

        public SimSpace(DefinedSpace space)
        {
            if (space == DefinedSpace.ToySpace)
            {
                this.velocityUnitsAbbr = "";
                this.distanceUnitsAbbr = "";
                this.timeUnitsAbbr = "sec.";
            }
            else if (space == DefinedSpace.NullSpace)
            {
                this.velocityUnitsAbbr = "";
                this.distanceUnitsAbbr = "";
                this.timeUnitsAbbr = "";
            }
        }
    }
}

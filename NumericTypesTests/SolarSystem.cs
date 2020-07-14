using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravitySandboxUWP
{
    public static class SolarSystem
    {
        // ========== EARTH ==========
        public const double EarthSurfaceAccelerationMPerSec2 = 9.80665; // m/sec^2
        public const double EarthMassKg = 5.97220E+24;
        public const double EarthRadiusKm = 6371.0;         // https://en.wikipedia.org/wiki/Earth_radius
        public const double LEO_OrbitMaxAltitudeKm = 2000.0;

        #region Spacecraft

        // ========== ISS ==========
        // Averages across one orbit from https://spotthestation.nasa.gov/tracking_map.cfm on 6/19/2020 at 22:25:00 GMT
        public const double ISS_OrbitRadiusKm = 424.72 + EarthRadiusKm;

        // ========== Other earth-orbiting spacecraft ========== 
        public const double StarlinkOrbitRadiusKm = 550.0 + EarthRadiusKm;
        public const double GPS_OrbitRadiusKm = 20180.0 + EarthRadiusKm;
        public const double GeosynchronousOrbitRadiusKm = 35786.0 + EarthRadiusKm;

        #endregion
    }
}

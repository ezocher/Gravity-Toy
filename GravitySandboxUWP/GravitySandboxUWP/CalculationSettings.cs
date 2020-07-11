using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravitySandboxUWP
{
    // Settings for calculations in the physics engine
    //
    //  Set by every scenario to adapt to its scale and to suit its accuracy requirements

    class CalculationSettings
    {
        public int CalculationCyclesPerFrame { get; private set; }

        // Figure out how to slice the parallel work dynamically by looking at size of problem and available threads
        public bool UseParallelCalculations { get; private set; }

        public bool CheckAllAdditionPrecision { get; private set; }

        public CalculationSettings(int calculationCyclesPerFrame, bool useParallelCalculations, bool checkAllAdditionPrecision)
        {
            CalculationCyclesPerFrame = calculationCyclesPerFrame;
            UseParallelCalculations = useParallelCalculations;
            CheckAllAdditionPrecision = checkAllAdditionPrecision;
        }

        // Use default settings
        public CalculationSettings()
        {
            CalculationCyclesPerFrame = 1;
            UseParallelCalculations = false;
            CheckAllAdditionPrecision = false;
        }
    }
}

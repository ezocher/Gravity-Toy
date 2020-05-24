using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravitySandboxUWP
{
    // Settings for calculations in the physics engine
    //
    //  Set by every scenario to adapt to its scale andto suit its accuracy requirements

    class CalculationSettings
    {
        readonly int calculationCyclesPerFrame;
        public int CalculationCyclesPerFrame { get { return calculationCyclesPerFrame; } }

        // Figure out how to slice the parallel work dynamically by looking at size of problem and available threads
        readonly bool useParallelCalculations;
        public bool UseParallelCalculations { get { return useParallelCalculations; } }

        public CalculationSettings(int calculationCyclesPerFrame, bool useParallelCalculations)
        {
            this.calculationCyclesPerFrame = calculationCyclesPerFrame;
            this.useParallelCalculations = useParallelCalculations;
        }

        // Returns default settings
        public CalculationSettings()
        {
            this.calculationCyclesPerFrame = 1;
            this.useParallelCalculations = false;
        }
    }
}

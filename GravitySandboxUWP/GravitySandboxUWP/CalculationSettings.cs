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
        readonly int calculationCyclesPerFrame;
        public int CalculationCyclesPerFrame { get { return calculationCyclesPerFrame; } }

        // Figure out how to slice the parallel work dynamically by looking at size of problem and available threads
        readonly bool useParallelCalculations;
        public bool UseParallelCalculations { get { return useParallelCalculations; } }

        readonly bool checkAllAdditionPrecision;
        public bool CheckAllAdditionPrecision {  get { return checkAllAdditionPrecision; } }

        public CalculationSettings(int calculationCyclesPerFrame, bool useParallelCalculations, bool checkAllAdditionPrecision)
        {
            this.calculationCyclesPerFrame = calculationCyclesPerFrame;
            this.useParallelCalculations = useParallelCalculations;
            this.checkAllAdditionPrecision = checkAllAdditionPrecision;
        }

        // Returns default settings
        public CalculationSettings()
        {
            this.calculationCyclesPerFrame = 1;
            this.useParallelCalculations = false;
            this.checkAllAdditionPrecision = false;
        }
    }
}

using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation processors
    /// </summary>
    public abstract class SimulationControl : BaseControl
    {
        protected void SetBaseParameters(BaseConfiguration baseConfiguration, ProcessingContext context)
        {
            if (context.SimulationConfiguration.Gmin.HasValue)
            {
                baseConfiguration.Gmin = context.SimulationConfiguration.Gmin.Value;
            }

            if (context.SimulationConfiguration.AbsoluteTolerance.HasValue)
            {
                baseConfiguration.AbsoluteTolerance = context.SimulationConfiguration.AbsoluteTolerance.Value;
            }

            if (context.SimulationConfiguration.RelTolerance.HasValue)
            {
                baseConfiguration.RelativeTolerance = context.SimulationConfiguration.RelTolerance.Value;
            }

            if (context.SimulationConfiguration.DCMaxIterations.HasValue)
            {
                baseConfiguration.DcMaxIterations = context.SimulationConfiguration.DCMaxIterations.Value;
            }
        }
    }
}

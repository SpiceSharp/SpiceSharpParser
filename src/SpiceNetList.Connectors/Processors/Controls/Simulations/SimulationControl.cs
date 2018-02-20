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
            if (context.GlobalConfiguration.Gmin.HasValue)
            {
                baseConfiguration.Gmin = context.GlobalConfiguration.Gmin.Value;
            }

            if (context.GlobalConfiguration.AbsoluteTolerance.HasValue)
            {
                baseConfiguration.AbsoluteTolerance = context.GlobalConfiguration.AbsoluteTolerance.Value;
            }

            if (context.GlobalConfiguration.RelTolerance.HasValue)
            {
                baseConfiguration.RelativeTolerance = context.GlobalConfiguration.RelTolerance.Value;
            }

            if (context.GlobalConfiguration.DCMaxIterations.HasValue)
            {
                baseConfiguration.DCMaxIterations = context.GlobalConfiguration.DCMaxIterations.Value;
            }
        }
    }
}

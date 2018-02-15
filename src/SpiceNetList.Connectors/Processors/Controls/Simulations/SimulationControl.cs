using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    public abstract class SimulationControl : SingleControlProcessor
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

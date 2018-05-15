using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using System;

namespace SpiceSharpParser.Connector.Processors.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation processors.
    /// </summary>
    public abstract class SimulationControl : BaseControl
    {
        /// <summary>
        /// Sets the temperatures of the simulation.
        /// </summary>
        /// <param name="context">The processing context.</param>
        /// <param name="simulation">The simulation to set.</param>
        protected static void SetCircuitTemperatures(IProcessingContext context, BaseSimulation simulation)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    if (context.Result.SimulationConfiguration.TemperatureInKelvins.HasValue)
                    {
                        rs.Temperature = context.Result.SimulationConfiguration.TemperatureInKelvins.Value;
                    }

                    if (context.Result.SimulationConfiguration.NominalTemperatureInKelvins.HasValue)
                    {
                        rs.NominalTemperature = context.Result.SimulationConfiguration.NominalTemperatureInKelvins.Value;
                    }
                }

                //TODO: What to do with complex state?
            };

            simulation.OnBeforeTemperatureCalculations += setState;
        }

        protected void SetBaseParameters(BaseConfiguration baseConfiguration, IProcessingContext context)
        {
            if (context.Result.SimulationConfiguration.Gmin.HasValue)
            {
                baseConfiguration.Gmin = context.Result.SimulationConfiguration.Gmin.Value;
            }

            if (context.Result.SimulationConfiguration.AbsoluteTolerance.HasValue)
            {
                baseConfiguration.AbsoluteTolerance = context.Result.SimulationConfiguration.AbsoluteTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.RelTolerance.HasValue)
            {
                baseConfiguration.RelativeTolerance = context.Result.SimulationConfiguration.RelTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.DCMaxIterations.HasValue)
            {
                baseConfiguration.DcMaxIterations = context.Result.SimulationConfiguration.DCMaxIterations.Value;
            }
        }
    }
}

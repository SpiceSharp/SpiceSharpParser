using System;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;

namespace SpiceSharpParser.Connector.Processors.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation processors.
    /// </summary>
    public abstract class SimulationControl : BaseControl
    {
        protected void SetTempVariable(IProcessingContext context, double? operatingTemperatureInKelvins, BaseSimulation sim)
        {
            double temp = 0;
            if (operatingTemperatureInKelvins.HasValue)
            {
                temp = operatingTemperatureInKelvins.Value - Circuit.CelsiusKelvin;
            }
            else
            {
                temp = Circuit.ReferenceTemperature - Circuit.CelsiusKelvin;
            }

            sim.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs e) =>
            {
                context.Evaluator.SetParameter("TEMP", temp);
            };
        }

        protected string GetSimulationName(IProcessingContext context, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("{0} - {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count()+1, TypeName, temperatureInKelvin.Value, temperatureInKelvin.Value - SpiceSharp.Circuit.CelsiusKelvin);
            }

            return string.Format("{0} - {1}", context.Result.Simulations.Count() + 1, TypeName);
        }

        protected static void SetTemperatures(BaseSimulation simulation, double? operatingTemperatureInKelvins, double? nominalTemperatureInKelvins)
        {
            if (operatingTemperatureInKelvins.HasValue)
            {
                SetCircuitTemperature(simulation, operatingTemperatureInKelvins.Value);
            }

            if (nominalTemperatureInKelvins.HasValue)
            {
                SetCircuitNominalTemperature(simulation, nominalTemperatureInKelvins.Value);
            }
        }

        /// <summary>
        /// Sets the nominal temperature of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to set.</param>
        /// <param name="nominalTemperatureInKelvins">Nominal temperature</param>
        protected static void SetCircuitNominalTemperature(BaseSimulation simulation, double nominalTemperatureInKelvins)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    rs.NominalTemperature = nominalTemperatureInKelvins;

                }
                //TODO: What to do with complex state?
            };

            simulation.OnBeforeTemperatureCalculations += setState;
        }

        /// <summary>
        /// Sets the temperature of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to set.</param>
        /// <param name="operatingTemperatureInKelvins">Circuit temperature</param>
        protected static void SetCircuitTemperature(BaseSimulation simulation, double operatingTemperatureInKelvins)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    rs.Temperature = operatingTemperatureInKelvins;
                }

                //TODO: What to do with complex state?
            };

            simulation.OnBeforeTemperatureCalculations += setState;
        }

        /// <summary>
        /// Sets the base parameters.
        /// </summary>
        /// <param name="baseConfiguration">The configuration to set.</param>
        /// <param name="context">The processing context.</param>
        protected void SetBaseConfiguration(BaseConfiguration baseConfiguration, IProcessingContext context)
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

using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .OP <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class OPControl : SimulationControl
    {
        public override string TypeName => "op";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            if (context.Result.SimulationConfiguration.TemperaturesInKelvins.Count > 0)
            {
                foreach (double temp in context.Result.SimulationConfiguration.TemperaturesInKelvins)
                {
                    CreateOperatingPointSimulation(context, temp);
                }
            }
            else
            {
                CreateOperatingPointSimulation(context);
            }
        }

        private void CreateOperatingPointSimulation(IProcessingContext context, double? operatingTemperatureInKelvins = null)
        {
            var op = new OP(GetSimulationName(context, operatingTemperatureInKelvins));

            SetTempVariable(context, operatingTemperatureInKelvins, op);
            SetBaseConfiguration(op.BaseConfiguration, context);
            SetTemperatures(op, operatingTemperatureInKelvins, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);

            context.Result.AddSimulation(op);
        }
    }
}

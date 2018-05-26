using SpiceSharp;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .TEMP <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class TempControl : BaseControl
    {
        public override string TypeName => "temp";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            if (statement.Parameters.Count == 0)
            {
                throw new WrongParametersCountException("No parameters for .TEMP");
            }

            if (context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
            {
                context.Result.SimulationConfiguration.TemperaturesInKelvins.Remove(context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
            }

            foreach (Model.Netlist.Spice.Objects.Parameter param in statement.Parameters)
            {
                if (param is Model.Netlist.Spice.Objects.Parameters.SingleParameter s
                    && (param is Model.Netlist.Spice.Objects.Parameters.ValueParameter || param is Model.Netlist.Spice.Objects.Parameters.ExpressionParameter))
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(context.ParseDouble(param.Image) + Circuit.CelsiusKelvin);
                }
                else
                {
                    throw new WrongParameterException("Wrong type of parameter for .temp: " + param.GetType());
                }
            }
        }
    }
}

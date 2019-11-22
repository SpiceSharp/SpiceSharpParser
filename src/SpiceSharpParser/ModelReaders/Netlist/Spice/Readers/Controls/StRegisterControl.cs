using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .ST_R <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StRegisterControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            if (statement.Parameters.Count < 3)
            {
                throw new WrongParametersCountException();
            }

            string firstParam = statement.Parameters[0].Image;

            switch (firstParam.ToLower())
            {
                case "oct":
                case "dec":
                case "list":
                case "lin":
                    RegisterParameter(statement.Parameters.Skip(1), context);
                    break;

                default:
                    RegisterParameter(statement.Parameters, context);
                    break;
            }
        }

        private void RegisterParameter(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            context.CircuitEvaluator.SetParameter(variableParameter.Image, 0);
        }
    }
}
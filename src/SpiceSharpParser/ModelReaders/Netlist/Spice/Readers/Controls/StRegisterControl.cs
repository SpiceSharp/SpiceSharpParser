using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .ST_R <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StRegisterControl : BaseControl
    {
        public override string SpiceCommandName => "st_r";

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
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
                    RegisterParametr(statement.Parameters.Skip(1), context);
                    break;
                default:
                    RegisterParametr(statement.Parameters, context);
                    break;
            }
        }

        private void RegisterParametr(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            context.ReadingEvaluator.SetParameter(variableParameter.Image, 0);
        }
    }
}

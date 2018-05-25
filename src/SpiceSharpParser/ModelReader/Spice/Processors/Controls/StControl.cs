using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .ST <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StControl : BaseControl
    {
        public override string TypeName => "st";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Process(Control statement, IProcessingContext context)
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
                case "dec":
                    ProcessDec(statement.Parameters.Skip(1), context);
                    break;
                case "oct":
                    ProcessOct(statement.Parameters.Skip(1), context);
                    break;
                case "list":
                    ProcessList(statement.Parameters.Skip(1), context);
                    break;
                case "lin":
                    ProcessLin(statement.Parameters.Skip(1), context);
                    break;
                default:
                    ProcessLin(statement.Parameters, context);
                    break;
            }
        }

        private void ProcessDec(ParameterCollection parameters, IProcessingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.ParseDouble(parameters[1].Image),
                    context.ParseDouble(parameters[2].Image),
                    (int)context.ParseDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ProcessOct(ParameterCollection parameters, IProcessingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new OctaveSweep(
                    context.ParseDouble(parameters[1].Image),
                    context.ParseDouble(parameters[2].Image),
                    (int)context.ParseDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ProcessList(ParameterCollection parameters, IProcessingContext context)
        {
            var variableParameter = parameters[0];
            var values = new List<double>();

            foreach (Parameter parameter in parameters.Skip(1))
            {
                if (!(parameter is SingleParameter))
                {
                    throw new WrongParameterTypeException();
                }

                values.Add(context.ParseDouble(parameter.Image));
            }

            context.Result.SimulationConfiguration.ParameterSweeps.Add(
                new ParameterSweep()
                {
                    Parameter = variableParameter,
                    Sweep = new ListSweep(values),
                }
            );
        }

        private void ProcessLin(ParameterCollection parameters, IProcessingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.ParseDouble(parameters[1].Image),
                    context.ParseDouble(parameters[2].Image),
                    context.ParseDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }
    }
}

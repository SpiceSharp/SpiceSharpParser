using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using ParameterSweep = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps.ParameterSweep;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .ST <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StControl : BaseControl
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
                context.Result.Validation.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        "Too less parameters for .ST",
                        statement.LineInfo));
            }

            string firstParam = statement.Parameters[0].Image;

            switch (firstParam.ToLower())
            {
                case "dec":
                    ReadDec(statement.Parameters.Skip(1), context);
                    break;

                case "oct":
                    ReadOct(statement.Parameters.Skip(1), context);
                    break;

                case "list":
                    ReadList(statement.Parameters.Skip(1), context);
                    break;

                case "lin":
                    ReadLin(statement.Parameters.Skip(1), context);
                    break;

                default:
                    ReadLin(statement.Parameters, context);
                    break;
            }
        }

        private static void ReadLin(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.Evaluator.EvaluateDouble(parameters[1].Image),
                    context.Evaluator.EvaluateDouble(parameters[2].Image),
                    context.Evaluator.EvaluateDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadDec(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.Evaluator.EvaluateDouble(parameters[1].Image),
                    context.Evaluator.EvaluateDouble(parameters[2].Image),
                    (int)context.Evaluator.EvaluateDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadOct(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new OctaveSweep(
                    context.Evaluator.EvaluateDouble(parameters[1].Image),
                    context.Evaluator.EvaluateDouble(parameters[2].Image),
                    (int)context.Evaluator.EvaluateDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadList(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var values = new List<double>();

            foreach (Parameter parameter in parameters.Skip(1))
            {
                if (!(parameter is SingleParameter))
                {
                    context.Result.Validation.Add(
                        new ValidationEntry(
                            ValidationEntrySource.Reader,
                            ValidationEntryLevel.Warning,
                            ".ST list needs to have single parameters",
                            parameter.LineInfo));
                    continue;
                }

                values.Add(context.Evaluator.EvaluateDouble(parameter.Image));
            }

            context.Result.SimulationConfiguration.ParameterSweeps.Add(
                new ParameterSweep()
                {
                    Parameter = variableParameter,
                    Sweep = new ListSweep(values),
                });
        }
    }
}
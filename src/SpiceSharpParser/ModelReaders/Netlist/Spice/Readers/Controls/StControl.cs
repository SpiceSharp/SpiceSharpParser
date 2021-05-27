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
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters == null)
            {
                throw new System.ArgumentNullException(nameof(statement.Parameters));
            }

            if (statement.Parameters.Count < 3)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Too few parameters for .ST",
                    statement.LineInfo);
            }

            string firstParam = statement.Parameters[0].Value;

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

        private static void ReadLin(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[1].Value),
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[2].Value),
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[3].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadDec(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[1].Value),
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[2].Value),
                    (int)context.EvaluationContext.Evaluator.EvaluateDouble(parameters[3].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadOct(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new OctaveSweep(
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[1].Value),
                    context.EvaluationContext.Evaluator.EvaluateDouble(parameters[2].Value),
                    (int)context.EvaluationContext.Evaluator.EvaluateDouble(parameters[3].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadList(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            var values = new List<double>();

            foreach (Parameter parameter in parameters.Skip(1))
            {
                if (!(parameter is SingleParameter))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        ".ST list needs to have single parameters",
                        parameter.LineInfo);
                    continue;
                }

                values.Add(context.EvaluationContext.Evaluator.EvaluateDouble(parameter.Value));
            }

            context.SimulationConfiguration.ParameterSweeps.Add(
                new ParameterSweep()
                {
                    Parameter = variableParameter,
                    Sweep = new ListSweep(values),
                });
        }
    }
}
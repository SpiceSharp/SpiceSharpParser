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
    /// Reads .STEP <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StepControl : BaseControl
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

            if (statement.Parameters.Count < 4)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Too few parameters for .STEP",
                    statement.LineInfo);
            }

            string firstParam = statement.Parameters[0].Value;

            switch (firstParam.ToLower())
            {
                case "param":
                    ReadParam(statement.Parameters.Skip(1), context);
                    break;

                case "lin":
                    ReadLin(statement.Parameters.Skip(1), context);
                    break;

                case "dec":
                    ReadDec(statement.Parameters.Skip(1), context);
                    break;

                case "oct":
                    ReadOct(statement.Parameters.Skip(1), context);
                    break;

                default:
                    ReadOtherCases(statement.Parameters, context);
                    break;
            }
        }

        private void ReadParam(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            string type = parameters[1].Value;

            switch (type.ToLower())
            {
                case "dec":
                    ReadDec(variableParameter, parameters.Skip(2), context);
                    break;

                case "oct":
                    ReadOct(variableParameter, parameters.Skip(2), context);
                    break;

                case "list":
                    ReadList(variableParameter, parameters.Skip(2), context);
                    break;

                case "lin":
                    ReadLin(variableParameter, parameters.Skip(2), context);
                    break;

                default:
                    ReadLin(variableParameter, parameters.Skip(1), context);
                    break;
            }
        }

        private void ReadOtherCases(ParameterCollection parameters, IReadingContext context)
        {
            bool list = false;
            for (var i = 0; i <= 2; i++)
            {
                if (parameters[i].Value.ToLower() == "list")
                {
                    list = true;
                }
            }

            if (list)
            {
                if (parameters[1] is BracketParameter bp)
                {
                    ReadList(bp, parameters.Skip(3), context); // model parameter
                }
                else
                {
                    ReadList(parameters[0], parameters.Skip(2), context); // source
                }
            }
            else
            {
                // lin
                if (parameters[1] is BracketParameter bp)
                {
                    ReadLin(bp, parameters.Skip(2), context); // model parameter
                }
                else
                {
                    ReadLin(parameters[0], parameters.Skip(1), context); // source
                }
            }
        }

        private void ReadOct(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                ReadOct(bp, parameters.Skip(2), context); // model parameter
            }
            else
            {
                ReadOct(parameters[0], parameters.Skip(1), context); // source
            }
        }

        private void ReadDec(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                ReadDec(bp, parameters.Skip(2), context); // model parameter
            }
            else
            {
                ReadDec(parameters[0], parameters.Skip(1), context); // source
            }
        }

        private void ReadLin(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters[1] is BracketParameter bp)
            {
                ReadLin(bp, parameters.Skip(2), context); // model parameter
            }
            else
            {
                ReadLin(parameters[0], parameters.Skip(1), context); // source
            }
        }

        private void ReadDec(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.Evaluator.EvaluateDouble(parameters[0].Value),
                    context.Evaluator.EvaluateDouble(parameters[1].Value),
                    (int)context.Evaluator.EvaluateDouble(parameters[2].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadOct(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new OctaveSweep(
                    context.Evaluator.EvaluateDouble(parameters[0].Value),
                    context.Evaluator.EvaluateDouble(parameters[1].Value),
                    (int)context.Evaluator.EvaluateDouble(parameters[2].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadList(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var values = new List<double>();

            foreach (Parameter parameter in parameters)
            {
                if (!(parameter is SingleParameter))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        ".STEP list needs to have single parameters",
                        parameter.LineInfo);
                    continue;
                }

                values.Add(context.Evaluator.EvaluateDouble(parameter.Value));
            }

            context.SimulationConfiguration.ParameterSweeps.Add(
                new ParameterSweep()
                {
                    Parameter = variableParameter,
                    Sweep = new ListSweep(values),
                });
        }

        private void ReadLin(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.Evaluator.EvaluateDouble(parameters[0].Value),
                    context.Evaluator.EvaluateDouble(parameters[1].Value),
                    context.Evaluator.EvaluateDouble(parameters[2].Value)),
            };

            context.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }
    }
}
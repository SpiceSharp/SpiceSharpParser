using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

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
                throw new WrongParametersCountException();
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

        private void ReadDec(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.CircuitEvaluator.EvaluateDouble(parameters[1].Image),
                    context.CircuitEvaluator.EvaluateDouble(parameters[2].Image),
                    (int)context.CircuitEvaluator.EvaluateDouble(parameters[3].Image)),
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
                    context.CircuitEvaluator.EvaluateDouble(parameters[1].Image),
                    context.CircuitEvaluator.EvaluateDouble(parameters[2].Image),
                    (int)context.CircuitEvaluator.EvaluateDouble(parameters[3].Image)),
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
                    throw new WrongParameterTypeException();
                }

                values.Add(context.CircuitEvaluator.EvaluateDouble(parameter.Image));
            }

            context.Result.SimulationConfiguration.ParameterSweeps.Add(
                new ParameterSweep()
                {
                    Parameter = variableParameter,
                    Sweep = new ListSweep(values),
                });
        }

        private static void ReadLin(ParameterCollection parameters, ICircuitContext context)
        {
            var variableParameter = parameters[0];
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.CircuitEvaluator.EvaluateDouble(parameters[1].Image),
                    context.CircuitEvaluator.EvaluateDouble(parameters[2].Image),
                    context.CircuitEvaluator.EvaluateDouble(parameters[3].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }
    }
}
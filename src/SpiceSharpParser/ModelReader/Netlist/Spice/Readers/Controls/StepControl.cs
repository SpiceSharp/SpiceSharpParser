using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reades .STEP <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class StepControl : BaseControl
    {
        public override string SpiceName => "step";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context.
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
                throw new WrongParametersCountException();
            }

            string firstParam = statement.Parameters[0].Image;

            switch (firstParam.ToLower())
            {
                case "param":
                    ReadParam(statement.Parameters.Skip(1), context);
                    break;
            }
        }

        private void ReadParam(ParameterCollection parameters, IReadingContext context)
        {
            var variableParameter = parameters[0];
            string type = parameters[1].Image;

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

        private void ReadDec(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new DecadeSweep(
                    context.ParseDouble(parameters[0].Image),
                    context.ParseDouble(parameters[1].Image),
                    (int)context.ParseDouble(parameters[2].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadOct(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new OctaveSweep(
                    context.ParseDouble(parameters[0].Image),
                    context.ParseDouble(parameters[1].Image),
                    (int)context.ParseDouble(parameters[2].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }

        private void ReadList(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var values = new List<double>();

            foreach (Parameter parameter in parameters)
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

        private void ReadLin(Parameter variableParameter, ParameterCollection parameters, IReadingContext context)
        {
            var pSweep = new ParameterSweep()
            {
                Parameter = variableParameter,
                Sweep = new LinearSweep(
                    context.ParseDouble(parameters[0].Image),
                    context.ParseDouble(parameters[1].Image),
                    context.ParseDouble(parameters[2].Image)),
            };

            context.Result.SimulationConfiguration.ParameterSweeps.Add(pSweep);
        }
    }
}

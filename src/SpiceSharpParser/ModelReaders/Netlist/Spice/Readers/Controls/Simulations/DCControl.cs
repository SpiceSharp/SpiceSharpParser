using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .DC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class DCControl : SimulationControl
    {
        /// <summary>
        /// Gets the Spice type
        /// </summary>
        public override string SpiceCommandName => "dc";

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateDCSimulation);
        }

        private DC CreateDCSimulation(string name, Control statement, IReadingContext context)
        {
            int count = statement.Parameters.Count / 4;
            switch (statement.Parameters.Count - (4 * count))
            {
                case 0:
                    if (statement.Parameters.Count == 0)
                    {
                        throw new WrongParametersCountException(".dc - Source Name expected");
                    }

                    break;

                case 1: throw new WrongParametersCountException(".dc - Start value expected");
                case 2: throw new WrongParametersCountException(".dc - Stop value expected");
                case 3: throw new WrongParametersCountException(".dc - Step value expected");
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            List<SweepConfiguration> sweeps = new List<SweepConfiguration>();

            for (int i = 0; i < count; i++)
            {
                SweepConfiguration sweep = new SweepConfiguration(
                    statement.Parameters.GetString(4 * i),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 1)),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 2)),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 3)));

                sweeps.Add(sweep);
            }

            DC dc = new DC(name, sweeps);
            dc.OnParameterSearch += (sender, e) =>
            {
                string sweepParameterName = e.Name.ToString();
                if (context.GetSimulationEvaluator(dc).HasParameter(sweepParameterName))
                {
                    e.TemperatureNeeded = true;
                    e.Result = new EvaluationParameter(context.GetSimulationEvaluator(dc), sweepParameterName);
                }
            };

            SetBaseConfiguration(dc.BaseConfiguration, context);
            SetDcParameters(dc.DcConfiguration, context);

            context.Result.AddSimulation(dc);

            return dc;
        }

        private void SetDcParameters(DcConfiguration dCConfiguration, IReadingContext context)
        {
            if (context.Result.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.Result.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}

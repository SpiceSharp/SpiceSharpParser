using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Evaluation;
using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .DC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class DCControl : SimulationControl
    {
        /// <summary>
        /// Gets the Spice type
        /// </summary>
        public override string TypeName => "dc";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
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

            DC dc = new DC((context.Result.Simulations.Count() + 1) + " - DC", sweeps);
            dc.OnParameterSearch += (sender, e) =>
            {
                string sweepParameterName = e.Name.ToString();
                if (context.Evaluator.HasParameter(sweepParameterName))
                {
                    e.TemperatureNeeded = true;
                    e.Result = new EvaluationParameter(context.Evaluator, sweepParameterName);
                }
            };

            SetBaseParameters(dc.BaseConfiguration, context);
            SetDcParameters(dc.DcConfiguration, context);

            context.Result.AddSimulation(dc);
        }

        private void SetDcParameters(DcConfiguration dCConfiguration, IProcessingContext context)
        {
            if (context.Result.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.Result.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}

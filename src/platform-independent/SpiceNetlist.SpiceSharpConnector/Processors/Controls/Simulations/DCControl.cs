using System;
using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .DC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class DCControl : SimulationControl
    {
        public override string TypeName => "dc";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContext context)
        {
            int count = statement.Parameters.Count / 4;
            switch (statement.Parameters.Count - (4 * count))
            {
                case 0:
                    if (statement.Parameters.Count == 0)
                    {
                        throw new Exception("Source st.Name expected");
                    }

                    break;

                case 1: throw new Exception("Start value expected");
                case 2: throw new Exception("Stop value expected");
                case 3: throw new Exception("Step value expected");
            }

            // Format: .DC SRCNAM VSTART VSTOP VINCR [SRC2 START2 STOP2 INCR2]
            List<SweepConfiguration> sweeps = new List<SweepConfiguration>();

            for (int i = 0; i < count; i++)
            {
                SweepConfiguration sweep = new SweepConfiguration(
                    statement.Parameters.GetString(4 * i).ToLower(),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 1).ToLower()),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 2).ToLower()),
                    context.ParseDouble(statement.Parameters.GetString((4 * i) + 3).ToLower()));

                sweeps.Add(sweep);
            }

            DC dc = new DC((context.Simulations.Count() + 1) + " - DC", sweeps);
            dc.OnParameterSearch += (sender, e) => 
            {
                string sweepParameterName = e.Name.Name;
                if (context.AvailableParameters.ContainsKey(sweepParameterName))
                {
                    e.Result = new EvaluationParameter(context.Evaluator, sweepParameterName);
                }
            };

            SetBaseParameters(dc.BaseConfiguration, context);
            SetDcParameters(dc.DcConfiguration, context);

            context.AddSimulation(dc);
        }

        private void SetDcParameters(DcConfiguration dCConfiguration, ProcessingContext context)
        {
            if (context.SimulationConfiguration.SweepMaxIterations.HasValue)
            {
                dCConfiguration.SweepMaxIterations = context.SimulationConfiguration.SweepMaxIterations.Value;
            }
        }
    }
}

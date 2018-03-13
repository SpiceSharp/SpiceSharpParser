using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .AC <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class ACControl : SimulationControl
    {
        public override string TypeName => "ac";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            switch (statement.Parameters.Count)
            {
                case 0: throw new Exception("LIN, DEC or OCT expected");
                case 1: throw new Exception("Number of points expected");
                case 2: throw new Exception("Starting frequency expected");
                case 3: throw new Exception("Stopping frequency expected");
            }

            AC ac;

            string type = statement.Parameters.GetString(0);
            var numberSteps = context.ParseDouble(statement.Parameters.GetString(1));
            var start = context.ParseDouble(statement.Parameters.GetString(2));
            var stop = context.ParseDouble(statement.Parameters.GetString(3));

            switch (type)
            {
                case "lin": ac = new AC((context.Simulations.Count() + 1) + " - AC", new SpiceSharp.Simulations.LinearSweep(start, stop, (int)numberSteps)); break;
                case "oct": ac = new AC((context.Simulations.Count() + 1) + " - AC", new SpiceSharp.Simulations.OctaveSweep(start, stop, (int)numberSteps)); break;
                case "dec": ac = new AC((context.Simulations.Count() + 1) + " - AC", new SpiceSharp.Simulations.DecadeSweep(start, stop, (int)numberSteps)); break;
                default:
                    throw new Exception("LIN, DEC or OCT expected");
            }

            SetBaseParameters(ac.BaseConfiguration, context);
            SetACParameters(ac.FrequencyConfiguration, context);
            context.AddSimulation(ac);
        }

        private void SetACParameters(FrequencyConfiguration frequencyConfiguration, IProcessingContext context)
        {
            if (context.SimulationConfiguration.KeepOpInfo.HasValue)
            {
                frequencyConfiguration.KeepOpInfo = context.SimulationConfiguration.KeepOpInfo.Value;
            }
        }
    }
}

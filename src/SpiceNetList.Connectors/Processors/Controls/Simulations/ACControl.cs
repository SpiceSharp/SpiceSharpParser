using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    public class ACControl : SimulationControl
    {
        public override void Process(Control statement, ProcessingContext context)
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
                case "lin": ac = new AC((context.SimulationsCount + 1) + " - DC", new SpiceSharp.Simulations.Sweeps.LinearSweep(start, stop, (int)numberSteps)); break;
                case "oct": ac = new AC((context.SimulationsCount + 1) + " - DC", new SpiceSharp.Simulations.Sweeps.OctaveSweep(start, stop, (int)numberSteps)); break;
                case "dec": ac = new AC((context.SimulationsCount + 1) + " - DC", new SpiceSharp.Simulations.Sweeps.DecadeSweep(start, stop, (int)numberSteps)); break;
                default:
                    throw new Exception("LIN, DEC or OCT expected");
            }

            SetBaseParameters(ac.BaseConfiguration, context);
            SetACParameters(ac.FrequencyConfiguration, context);
            context.AddSimulation(ac);
        }

        private void SetACParameters(FrequencyConfiguration frequencyConfiguration, ProcessingContext context)
        {
            if (context.GlobalConfiguration.KeepOpInfo.HasValue)
            {
                frequencyConfiguration.KeepOpInfo = context.GlobalConfiguration.KeepOpInfo.Value;
            }
        }
    }
}

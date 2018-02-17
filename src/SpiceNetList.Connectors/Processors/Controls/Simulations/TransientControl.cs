using System;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    public class TransientControl : SimulationControl
    {
        public override string Type => "tran";

        public override void Process(Control statement, ProcessingContext context)
        {
            Transient tran = null;

            switch (statement.Parameters.Count)
            {
                case 0: throw new Exception("Step expected");
                case 1: throw new Exception("Maximum time expected");
            }

            switch (statement.Parameters.Count)
            {
                case 2:
                    tran = new Transient(
                        (context.SimulationsCount + 1) + " - Transient",
                        context.ParseDouble(statement.Parameters[0].Image),
                        context.ParseDouble(statement.Parameters[1].Image));
                    break;
                case 3:
                    tran = new Transient(
                        (context.SimulationsCount + 1) + " - Transient",
                        context.ParseDouble(statement.Parameters[0].Image),
                        context.ParseDouble(statement.Parameters[1].Image),
                        context.ParseDouble(statement.Parameters[2].Image));
                    break;
                case 4:
                    throw new Exception("Wrong number of parameters for .tran");
            }

            SetBaseParameters(tran.BaseConfiguration, context);
            SetTransientParamters(tran, context);
            context.AddSimulation(tran);
        }

        private void SetTransientParamters(Transient tran, ProcessingContext context)
        {
            if (context.GlobalConfiguration.Method != null)
            {
                tran.TimeConfiguration.Method = context.GlobalConfiguration.Method;
            }

            if (context.GlobalConfiguration.TranMaxIterations.HasValue)
            {
                tran.TimeConfiguration.TranMaxIterations = context.GlobalConfiguration.TranMaxIterations.Value;
            }
        }
    }
}

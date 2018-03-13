using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    /// <summary>
    /// Processes .TRAN <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class TransientControl : SimulationControl
    {
        public override string TypeName => "tran";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContextBase context)
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
                        (context.Simulations.Count() + 1) + " - Transient",
                        context.ParseDouble(statement.Parameters[0].Image),
                        context.ParseDouble(statement.Parameters[1].Image));
                    break;
                case 3:
                    tran = new Transient(
                        (context.Simulations.Count() + 1) + " - Transient",
                        context.ParseDouble(statement.Parameters[0].Image),
                        context.ParseDouble(statement.Parameters[1].Image),
                        context.ParseDouble(statement.Parameters[2].Image));
                    break;
                case 4:
                    throw new Exception("Wrong number of parameters for .tran");
            }

            SetBaseParameters(tran.ParameterSets.Get<BaseConfiguration>(), context);
            SetTransientParamters(tran, context);
            context.Adder.AddSimulation(tran);
        }

        private void SetTransientParamters(Transient tran, ProcessingContextBase context)
        {
            if (context.SimulationConfiguration.Method != null)
            {
                tran.TimeConfiguration.Method = context.SimulationConfiguration.Method;
            }

            if (context.SimulationConfiguration.TranMaxIterations.HasValue)
            {
                tran.TimeConfiguration.TranMaxIterations = context.SimulationConfiguration.TranMaxIterations.Value;
            }
        }
    }
}

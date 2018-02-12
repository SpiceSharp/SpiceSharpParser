using System;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public class TransientControl : SingleControlProcessor
    {
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
                        "Transient -" + context.SimulationsCount,
                        context.ParseDouble((statement.Parameters[0] as ValueParameter).RawValue),
                        context.ParseDouble((statement.Parameters[1] as ValueParameter).RawValue));
                    break;
                case 3:
                    tran = new Transient(
                        "Transient -" + context.SimulationsCount,
                        context.ParseDouble((statement.Parameters[0] as ValueParameter).RawValue),
                        context.ParseDouble((statement.Parameters[1] as ValueParameter).RawValue),
                        context.ParseDouble((statement.Parameters[2] as ValueParameter).RawValue));
                    break;
                case 4:
                    throw new Exception("TODO");
            }

            context.AddSimulation(tran);
        }
    }
}

using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class OPControl : SingleControlProcessor
    {
        public override void Process(Control statement, ProcessingContext context)
        {
            var op = new OP("OP " + context.SimulationsCount.ToString());
            context.AddSimulation(op);
        }
    }
}

using System;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    public class OPControl : SimulationControl
    {
        public override void Process(Control statement, ProcessingContext context)
        {
            var op = new OP((context.SimulationsCount + 1).ToString() + " - OP");

            SetBaseParameters(op.BaseConfiguration, context);
            context.AddSimulation(op);
        }
    }
}

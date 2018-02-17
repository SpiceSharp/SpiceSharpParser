using SpiceNetlist.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations
{
    public class OPControl : SimulationControl
    {
        public override string Type => "op";

        public override void Process(Control statement, ProcessingContext context)
        {
            var op = new OP((context.SimulationsCount + 1).ToString() + " - OP");

            SetBaseParameters(op.BaseConfiguration, context);
            context.AddSimulation(op);
        }
    }
}

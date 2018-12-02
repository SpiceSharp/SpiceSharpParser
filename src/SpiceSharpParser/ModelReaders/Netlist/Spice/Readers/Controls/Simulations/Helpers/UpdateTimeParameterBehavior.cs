using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Helpers
{
    public class UpdateTimeParameterBehavior : IAcceptBehavior
    {
        public UpdateTimeParameterBehavior(string name, IReadingContext context)
        {
            this.Context = context;
            this.Name = name;
        }

        public string Name { get; private set; }

        public IReadingContext Context { get; private set; }

        public ExpressionContext ExpressionContext { get; set; }

        public void Setup(Simulation simulation, SetupDataProvider provider)
        {
            ExpressionContext = Context.SimulationExpressionContexts.GetContext(simulation);
        }

        public void Unsetup(Simulation simulation)
        {
            Context = null;
            Name = null;
            ExpressionContext = null;
        }

        public void Probe(TimeSimulation simulation)
        {
            ExpressionContext.SetParameter("TIME", simulation.Method.Time);
        }

        public void Accept(TimeSimulation simulation)
        {
        }
    }
}

using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Helpers
{
    public class UpdateTimeParameterEntity : Entity
    {
        public UpdateTimeParameterEntity(string name, IReadingContext context)
            : base(name)
        {
            // Register factories
            Behaviors.Add(typeof(IAcceptBehavior), () => new UpdateTimeParameterBehavior(name, context));
        }
    }
}

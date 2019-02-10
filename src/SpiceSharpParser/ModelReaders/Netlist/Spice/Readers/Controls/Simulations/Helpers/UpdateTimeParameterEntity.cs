using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Helpers
{
    public class UpdateTimeParameterEntity : Entity
    {
        private static readonly UpdateTimeParameterBehavior updateBehavior = new UpdateTimeParameterBehavior()
        {
            Name = "intial"
        };

        static UpdateTimeParameterEntity()
        {
            RegisterBehaviorFactory(typeof(UpdateTimeParameterEntity), new BehaviorFactoryDictionary
            {
                { typeof(IAcceptBehavior), e => updateBehavior },
            });
        }

        public UpdateTimeParameterEntity(string name, IReadingContext context)
            : base(name)
        {
            updateBehavior.Context = context;
            updateBehavior.Name = name;
        }
    }
}

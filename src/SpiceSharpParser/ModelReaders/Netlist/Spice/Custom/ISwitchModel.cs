using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public partial class ISwitchModel : Entity<ISwitchModelParameters>
    {
        public ISwitchModel(string name)
            : base(name)
        {
        }

        public override void CreateBehaviors(ISimulation simulation)
        {
        }
    }
}

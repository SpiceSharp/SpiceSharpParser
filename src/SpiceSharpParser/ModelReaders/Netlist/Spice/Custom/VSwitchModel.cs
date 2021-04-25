using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public partial class VSwitchModel : Entity<VSwitchModelParameters>
    {
        public VSwitchModel(string name)
            : base(name)
        {
        }

        public override void CreateBehaviors(ISimulation simulation)
        {
        }
    }
}
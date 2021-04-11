using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class VSwitchModel : Entity<BindingContext>, IParameterized<VSwitchModelParameters>
    {
        public VSwitchModel(string name)
            : base(name)
        {
        }

        public VSwitchModelParameters Parameters { get; } = new VSwitchModelParameters();
    }
}
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class VSwitchModel : Entity<BindingContext>, IParameterized<VSwitchModelBaseParameters>
    {
        public VSwitchModel(string name)
            : base(name)
        {
        }

        public VSwitchModelBaseParameters Parameters { get; } = new VSwitchModelBaseParameters();
    }
}
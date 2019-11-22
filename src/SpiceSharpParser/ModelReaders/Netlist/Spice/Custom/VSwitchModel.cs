using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class VSwitchModel : Model
    {
        public VSwitchModel(string name)
            : base(name)
        {
            ParameterSets.Add(new VSwitchModelBaseParameters());
        }
    }
}
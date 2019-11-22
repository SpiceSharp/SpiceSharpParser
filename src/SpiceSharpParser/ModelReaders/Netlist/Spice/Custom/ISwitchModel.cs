using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class ISwitchModel : Model
    {
        public ISwitchModel(string name)
            : base(name)
        {
            ParameterSets.Add(new ISwitchModelBaseParameters());
        }
    }
}
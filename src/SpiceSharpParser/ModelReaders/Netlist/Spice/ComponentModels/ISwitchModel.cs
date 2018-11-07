using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.ComponentModels
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

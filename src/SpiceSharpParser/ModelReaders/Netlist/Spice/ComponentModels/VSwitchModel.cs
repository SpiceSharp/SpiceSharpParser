using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.ComponentModels
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

using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class ParameterRandomness
    {
        public string RandomDistribiutionName { get; set; }

        public Parameter Parameter { get; set; }
    }
}

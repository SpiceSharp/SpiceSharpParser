using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class PercentSpecification
    {
        public string DistributionName { get; set; }

        public Parameter Parameter { get; set; }
    }
}

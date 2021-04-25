using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    [GeneratedParameters]
    public partial class ISwitchModelParameters : ParameterSet<ISwitchModelParameters>
    {
        [ParameterName("ron")]
        [ParameterInfo("Resistance when closed/on")]
        public double OnResistance { get; set; } = 1.0;

        [ParameterName("roff")]
        [ParameterInfo("Resistance when off")]
        public double OffResistance { get; set; } = 1.0e12;

        [ParameterName("ion")]
        [ParameterInfo("On current")]
        public double OnCurrent { get; set; } = 1E-3;

        [ParameterName("ioff")]
        [ParameterInfo("Off current")]
        public double OffCurrent { get; set; } = 0.0;
    }
}
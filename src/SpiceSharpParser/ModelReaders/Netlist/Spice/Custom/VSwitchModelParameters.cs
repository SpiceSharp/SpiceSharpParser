using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    [GeneratedParameters]
    public partial class VSwitchModelParameters : ParameterSet<VSwitchModelParameters>
    {
        [ParameterName("ron")]
        [ParameterInfo("Resistance when closed/on")]
        public double OnResistance { get; set; } = 1.0;

        [ParameterName("roff")]
        [ParameterInfo("Resistance when off")]
        public double OffResistance { get; set; } = 1.0e12;

        [ParameterName("von")]
        [ParameterInfo("On voltage")]
        public double OnVoltage { get; set; } = 1.0;

        [ParameterName("voff")]
        [ParameterInfo("Off voltage")]
        public double OffVoltage { get; set; } = 0.0;
    }
}
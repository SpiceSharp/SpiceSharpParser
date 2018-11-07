using SpiceSharp;
using SpiceSharp.Attributes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.ComponentModels
{
    public class VSwitchModelBaseParameters : ParameterSet
    {
        [ParameterName("ron"), ParameterInfo("Resistance when closed/on")]
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);

        [ParameterName("roff"), ParameterInfo("Resistance when off")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);

        [ParameterName("von"), ParameterInfo("On voltage")]
        public GivenParameter<double> OnVoltage { get; } = new GivenParameter<double>(1.0);

        [ParameterName("voff"), ParameterInfo("Off voltage")]
        public GivenParameter<double> OffVoltage { get; } = new GivenParameter<double>(0.0);
    }
}

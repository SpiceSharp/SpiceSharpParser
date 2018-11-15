using SpiceSharp;
using SpiceSharp.Attributes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class ISwitchModelBaseParameters : ParameterSet
    {
        [ParameterName("ron"), ParameterInfo("Resistance when closed/on")]
        public GivenParameter<double> OnResistance { get; } = new GivenParameter<double>(1.0);

        [ParameterName("roff"), ParameterInfo("Resistance when off")]
        public GivenParameter<double> OffResistance { get; } = new GivenParameter<double>(1.0e12);

        [ParameterName("ion"), ParameterInfo("On current")]
        public GivenParameter<double> OnCurrent { get; } = new GivenParameter<double>(1E-3);

        [ParameterName("ioff"), ParameterInfo("Off current")]
        public GivenParameter<double> OffCurrent { get; } = new GivenParameter<double>(0.0);
    }
}

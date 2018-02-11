using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    class SineGenerator
    {
        internal Waveform Generate(ComplexParameter parameter, NetList netlist)
        {
            var sine = new Sine();

            sine.Offset.Set(netlist.ParseDouble((parameter.Parameters[0] as SingleParameter).RawValue));
            sine.Amplitude.Set(netlist.ParseDouble((parameter.Parameters[1] as SingleParameter).RawValue));
            sine.Frequency.Set(netlist.ParseDouble((parameter.Parameters[2] as SingleParameter).RawValue));
            sine.Delay.Set(netlist.ParseDouble((parameter.Parameters[3] as SingleParameter).RawValue));
            sine.Theta.Set(netlist.ParseDouble((parameter.Parameters[4] as SingleParameter).RawValue));

            return sine;
        }
    }
}

using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    class PulseGenerator
    {
        internal Waveform Generate(ComplexParameter parameter, NetList netlist)
        {
            var w = new Pulse();

            w.InitialValue.Set(netlist.ParseDouble((parameter.Parameters.Values[0] as SingleParameter).RawValue));
            w.PulsedValue.Set(netlist.ParseDouble((parameter.Parameters.Values[1] as SingleParameter).RawValue));
            w.Delay.Set(netlist.ParseDouble((parameter.Parameters.Values[2] as SingleParameter).RawValue));
            w.RiseTime.Set(netlist.ParseDouble((parameter.Parameters.Values[3] as SingleParameter).RawValue));
            w.FallTime.Set(netlist.ParseDouble((parameter.Parameters.Values[4] as SingleParameter).RawValue));
            w.PulseWidth.Set(netlist.ParseDouble((parameter.Parameters.Values[5] as SingleParameter).RawValue));
            w.Period.Set(netlist.ParseDouble((parameter.Parameters.Values[6] as SingleParameter).RawValue));

            return w;
        }
    }
}

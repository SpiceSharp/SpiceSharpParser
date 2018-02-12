using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    class PulseGenerator
    {
        internal Waveform Generate(ComplexParameter parameter, ProcessingContext context)
        {
            var w = new Pulse();

            w.InitialValue.Set(context.ParseDouble((parameter.Parameters[0] as SingleParameter).RawValue));
            w.PulsedValue.Set(context.ParseDouble((parameter.Parameters[1] as SingleParameter).RawValue));
            w.Delay.Set(context.ParseDouble((parameter.Parameters[2] as SingleParameter).RawValue));
            w.RiseTime.Set(context.ParseDouble((parameter.Parameters[3] as SingleParameter).RawValue));
            w.FallTime.Set(context.ParseDouble((parameter.Parameters[4] as SingleParameter).RawValue));
            w.PulseWidth.Set(context.ParseDouble((parameter.Parameters[5] as SingleParameter).RawValue));
            w.Period.Set(context.ParseDouble((parameter.Parameters[6] as SingleParameter).RawValue));

            return w;
        }
    }
}

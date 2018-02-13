using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    public class PulseGenerator
    {
        internal Waveform Generate(ComplexParameter parameter, ProcessingContext context)
        {
            var w = new Pulse();

            w.InitialValue.Set(context.ParseDouble(parameter.Parameters.GetString(0)));
            w.PulsedValue.Set(context.ParseDouble(parameter.Parameters.GetString(1)));
            w.Delay.Set(context.ParseDouble(parameter.Parameters.GetString(2)));
            w.RiseTime.Set(context.ParseDouble(parameter.Parameters.GetString(3)));
            w.FallTime.Set(context.ParseDouble(parameter.Parameters.GetString(4)));
            w.PulseWidth.Set(context.ParseDouble(parameter.Parameters.GetString(5)));
            w.Period.Set(context.ParseDouble(parameter.Parameters.GetString(6)));

            return w;
        }
    }
}

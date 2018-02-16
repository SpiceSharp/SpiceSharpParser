using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    public class PulseGenerator
    {
        internal Waveform Generate(BracketParameter bracketParam, ProcessingContext context)
        {
            var w = new Pulse();

            w.InitialValue.Set(context.ParseDouble(bracketParam.Parameters.GetString(0)));
            w.PulsedValue.Set(context.ParseDouble(bracketParam.Parameters.GetString(1)));
            w.Delay.Set(context.ParseDouble(bracketParam.Parameters.GetString(2)));
            w.RiseTime.Set(context.ParseDouble(bracketParam.Parameters.GetString(3)));
            w.FallTime.Set(context.ParseDouble(bracketParam.Parameters.GetString(4)));
            w.PulseWidth.Set(context.ParseDouble(bracketParam.Parameters.GetString(5)));
            w.Period.Set(context.ParseDouble(bracketParam.Parameters.GetString(6)));

            return w;
        }
    }
}

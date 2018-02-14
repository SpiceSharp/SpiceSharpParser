using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    public class SineGenerator
    {
        internal Waveform Generate(BracketParameter parameter, ProcessingContext context)
        {
            var sine = new Sine();

            sine.Offset.Set(context.ParseDouble(parameter.Content.Parameters.GetString(0)));
            sine.Amplitude.Set(context.ParseDouble(parameter.Content.Parameters.GetString(1)));
            sine.Frequency.Set(context.ParseDouble(parameter.Content.Parameters.GetString(2)));
            sine.Delay.Set(context.ParseDouble(parameter.Content.Parameters.GetString(3)));
            sine.Theta.Set(context.ParseDouble(parameter.Content.Parameters.GetString(4)));

            return sine;
        }
    }
}

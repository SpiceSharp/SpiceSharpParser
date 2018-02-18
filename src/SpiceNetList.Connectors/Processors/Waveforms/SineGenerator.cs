using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class SineGenerator
    {
        internal Waveform Generate(BracketParameter bracketParameter, ProcessingContext context)
        {
            var sine = new Sine();

            sine.Offset.Set(context.ParseDouble(bracketParameter.Parameters.GetString(0)));
            sine.Amplitude.Set(context.ParseDouble(bracketParameter.Parameters.GetString(1)));
            sine.Frequency.Set(context.ParseDouble(bracketParameter.Parameters.GetString(2)));
            sine.Delay.Set(context.ParseDouble(bracketParameter.Parameters.GetString(3)));
            sine.Theta.Set(context.ParseDouble(bracketParameter.Parameters.GetString(4)));

            return sine;
        }
    }
}

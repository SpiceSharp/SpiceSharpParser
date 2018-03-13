using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Waveforms
{
    /// <summary>
    /// Generator for pulse waveform
    /// </summary>
    public class PulseGenerator : WaveformGenerator
    {
        public override string TypeName => "pulse";

        /// <summary>
        /// Generats a new waveform
        /// </summary>
        /// <param name="bracketParam">A parameter for waveform</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new waveform
        /// </returns>
        public override Waveform Generate(BracketParameter bracketParam, ProcessingContextBase context)
        {
            var w = new Pulse();

            if (bracketParam.Parameters.Count != 7)
            {
                throw new System.Exception("Wrong number of arguments for pulse");
            }

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

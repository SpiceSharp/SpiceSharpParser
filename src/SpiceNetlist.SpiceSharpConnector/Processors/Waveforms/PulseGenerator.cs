using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
using SpiceSharp.Components;
using SpiceNetlist.SpiceSharpConnector.Context;

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
        public override Waveform Generate(BracketParameter bracketParam, IProcessingContext context)
        {
            var w = new Pulse();

            if (bracketParam.Parameters.Count != 7)
            {
                throw new System.Exception("Wrong number of arguments for pulse");
            }

            w.InitialValue.Value = context.ParseDouble(bracketParam.Parameters.GetString(0));
            w.PulsedValue.Value = context.ParseDouble(bracketParam.Parameters.GetString(1));
            w.Delay.Value = context.ParseDouble(bracketParam.Parameters.GetString(2));
            w.RiseTime.Value = context.ParseDouble(bracketParam.Parameters.GetString(3));
            w.FallTime.Value = context.ParseDouble(bracketParam.Parameters.GetString(4));
            w.PulseWidth.Value = context.ParseDouble(bracketParam.Parameters.GetString(5));
            w.Period.Value = context.ParseDouble(bracketParam.Parameters.GetString(6));

            return w;
        }
    }
}

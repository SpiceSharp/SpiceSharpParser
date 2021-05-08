using SpiceSharp.Components;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.IO;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for wave waveform.
    /// </summary>
    public class WaveGenerator : WaveformGenerator
    {
        public const double DefaultAmplidude = 1.0;

        /// <summary>
        /// Generates a new waveform.
        /// </summary>
        /// <param name="parameters">Parameters for waveform.</param>
        /// <param name="context">A context.</param>
        /// <returns>
        /// A new waveform.
        /// </returns>
        public override IWaveformDescription Generate(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return CreateWaveFromFile(parameters, context);;
        }

        private static IWaveformDescription CreateWaveFromFile(ParameterCollection parameters, IReadingContext context)
        {
            var fileParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter ap && ap.Name.ToLower() == "wavefile");
            var channelParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter ap && ap.Name.ToLower() == "chan");
            var ampliduteParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLower() == "amplitude");

            var filePath = PathConverter.Convert(fileParameter.Value);
            
            var workingDirectory = context.WorkingDirectory ?? Directory.GetCurrentDirectory();
            var fullFilePath = Path.Combine(workingDirectory, filePath);

            if (!File.Exists(fullFilePath))
            {
                throw new ArgumentException("WAVE file does not exist:" + fullFilePath);
            }

            byte[] fileContent = File.ReadAllBytes(fullFilePath);

            var reader = new WaveFileReader(fileContent);
            var result = reader.Read();

            var amplitude = DefaultAmplidude;

            if (ampliduteParameter != null)
            {
                amplitude = context.Evaluator.EvaluateDouble(ampliduteParameter.Value);
            }

            int channel = (int)context.Evaluator.EvaluateDouble(channelParameter.Value);
            var pwlRawData = result.ConverToPwl(channel, amplitude);

            var pwl = new Pwl();
            pwl.Points = pwlRawData.Select(raw => new Point(raw.Item1, raw.Item2));

            return pwl;
        }
    }
}
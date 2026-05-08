using SpiceSharp.Components;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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

            return CreateWaveFromFile(parameters, context);
        }

        private static IWaveformDescription CreateWaveFromFile(ParameterCollection parameters, IReadingContext context)
        {
            var fileParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLowerInvariant() == "wavefile");
            var channelParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLowerInvariant() == "chan");
            var ampliduteParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLowerInvariant() == "amplitude");

            if (fileParameter == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "wavefile source requires wavefile=<path>.",
                    parameters.LineInfo);
                return null;
            }

            if (channelParameter == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "wavefile source requires explicit chan=<n>; LTspice channel defaults are not inferred.",
                    fileParameter.LineInfo);
                return null;
            }

            var filePath = PathConverter.Convert(fileParameter.Value);
            var workingDirectory = context.ReaderSettings.WorkingDirectory ?? Directory.GetCurrentDirectory();
            var fullFilePath = Path.Combine(workingDirectory, filePath);

            if (!File.Exists(fullFilePath))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "wavefile source file does not exist: " + fullFilePath,
                    fileParameter.LineInfo);
                return null;
            }

            byte[] fileContent = File.ReadAllBytes(fullFilePath);
            var amplitude = DefaultAmplidude;

            if (ampliduteParameter != null)
            {
                amplitude = context.Evaluator.EvaluateDouble(ampliduteParameter.Value);
            }

            int channel;
            try
            {
                channel = (int)context.Evaluator.EvaluateDouble(channelParameter.Value);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "wavefile source chan=<n> must evaluate to a channel number.",
                    channelParameter.LineInfo,
                    ex);
                return null;
            }

            return new Wave.Wave(fileContent, channel, amplitude);
        }
    }
}

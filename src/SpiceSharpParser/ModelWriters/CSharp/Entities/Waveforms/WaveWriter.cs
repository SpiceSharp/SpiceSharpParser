using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    /// <summary>
    /// Generator for wave waveform.
    /// </summary>
    public class WaveWriter : BaseWriter, IWaveformWriter
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
        public List<CSharpStatement> Generate(ParameterCollection parameters, IWriterContext context, out string waveFormId)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return CreateWaveFromFile(parameters, context, out waveFormId);
        }

        private List<CSharpStatement> CreateWaveFromFile(ParameterCollection parameters, IWriterContext context, out string waveFormId)
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

            
            var amplitude = DefaultAmplidude;

            if (ampliduteParameter != null)
            {
                amplitude = context.EvaluationContext.Evaluate(ampliduteParameter.Value);
            }

            int channel = (int)context.EvaluationContext.Evaluate(channelParameter.Value);

            byte[] fileContent = File.ReadAllBytes(fullFilePath);

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("wave");
            result.Add(new CSharpNewStatement(waveFormId, $@"new SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave.Wave(File.ReadAllBytes(""{fullFilePath}""), {channel}, {amplitude})") { IncludeInCollection = false });
            return result;
        }
    }
}
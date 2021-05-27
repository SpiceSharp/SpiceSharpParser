using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms
{
    /// <summary>
    /// Generator for PWL waveform.
    /// </summary>
    public class PwlWriter : BaseWriter, IWaveformWriter
    {
        /// <summary>
        /// Generates a new waveform.
        /// </summary>
        /// <param name="parameters">Parameters for waveform.</param>
        /// <param name="context">A context.</param>
        /// <param name="waveformId">Waveform id.</param>
        /// <returns>
        /// A new waveform.
        /// </returns>
        public List<CSharpStatement> Generate(ParameterCollection parameters, IWriterContext context, out string waveformId)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (parameters.Count > 0 && parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file"))
            {
                return CreatePwlFromFile(parameters, context, out waveformId);
            }

            bool vectorMode = parameters.Count > 1 && parameters[1] is VectorParameter vp && vp.Elements.Count == 2;

            if (!vectorMode)
            {
                return CreatePwlFromSequence(parameters, context, out waveformId);
            }
            else
            {
                return CreatePwlFromVector(parameters, context, out waveformId);
            }
        }

        private List<CSharpStatement> CreatePwlFromSequence(ParameterCollection parameters, IWriterContext context, out string waveFormId)
        {
            if (parameters.Count % 2 != 0)
            {
                throw new ArgumentException("PWL waveform expects even count of parameters");
            }

            List<double> values = new List<double>();
            for (var i = 0; i < parameters.Count / 2; i++)
            {
                values.Add(context.EvaluationContext.Evaluate(parameters.Get(2 * i)));
                values.Add(context.EvaluationContext.Evaluate(parameters.Get((2 * i) + 1)));
            }

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", values.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }

        private List<CSharpStatement> CreatePwlFromVector(ParameterCollection parameters, IWriterContext context, out string waveFormId)
        {
            List<double> values = new List<double>();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is VectorParameter vp2 && vp2.Elements.Count == 2)
                {
                    values.Add(context.EvaluationContext.Evaluate(vp2.Elements[0].Value));
                    values.Add(context.EvaluationContext.Evaluate(vp2.Elements[1].Value));
                }
                else
                {
                    values.Add(context.EvaluationContext.Evaluate(parameters[i].Value));
                }
            }

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", values.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }

        private List<CSharpStatement> CreatePwlFromFile(ParameterCollection parameters, IWriterContext context, out string waveFormId)
        {
            var fileParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file");
            var filePath = PathConverter.Convert(fileParameter.Value);
            var workingDirectory = context.WorkingDirectory ?? Directory.GetCurrentDirectory();
            var fullFilePath = Path.Combine(workingDirectory, filePath);

            if (!File.Exists(fullFilePath))
            {
                throw new ArgumentException("PWL file does not exist:" + fullFilePath);
            }

            List<double[]> csvData = CsvFileReader.Read(fullFilePath, true, context.ExternalFilesEncoding).ToList();
            var data = new List<double>();

            for (var i = 0; i < csvData.LongCount(); i++)
            {
                data.Add(csvData[i][0]);
                data.Add(csvData[i][1]);
            }

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", data.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }
    }
}
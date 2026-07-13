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

            var pwlParameters = ExtractLtspiceScaleFactors(parameters, context, out var timeScale, out var valueScale);

            if (pwlParameters.Count > 0 && pwlParameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file"))
            {
                return CreatePwlFromFile(pwlParameters, context, timeScale, valueScale, out waveformId);
            }

            bool vectorMode = pwlParameters.Any(parameter =>
                parameter is VectorParameter || parameter is PointParameter);

            if (!vectorMode)
            {
                return CreatePwlFromSequence(pwlParameters, context, timeScale, valueScale, out waveformId);
            }
            else
            {
                return CreatePwlFromVector(pwlParameters, context, timeScale, valueScale, out waveformId);
            }
        }

        private List<CSharpStatement> CreatePwlFromSequence(
            ParameterCollection parameters,
            IWriterContext context,
            double timeScale,
            double valueScale,
            out string waveFormId)
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

            ScaleValues(values, timeScale, valueScale);

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", values.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }

        private List<CSharpStatement> CreatePwlFromVector(
            ParameterCollection parameters,
            IWriterContext context,
            double timeScale,
            double valueScale,
            out string waveFormId)
        {
            List<double> values = new List<double>();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is VectorParameter vp2 && vp2.Elements.Count == 2)
                {
                    values.Add(context.EvaluationContext.Evaluate(vp2.Elements[0].Value));
                    values.Add(context.EvaluationContext.Evaluate(vp2.Elements[1].Value));
                }
                else if (parameters[i] is PointParameter point)
                {
                    foreach (var item in point.Values.Items)
                    {
                        values.Add(context.EvaluationContext.Evaluate(item.Value));
                    }
                }
                else
                {
                    values.Add(context.EvaluationContext.Evaluate(parameters[i].Value));
                }
            }

            if (values.Count % 2 != 0)
            {
                throw new ArgumentException("PWL waveform expects even count of parameters");
            }

            ScaleValues(values, timeScale, valueScale);

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", values.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }

        private List<CSharpStatement> CreatePwlFromFile(
            ParameterCollection parameters,
            IWriterContext context,
            double timeScale,
            double valueScale,
            out string waveFormId)
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

            ScaleValues(data, timeScale, valueScale);

            var result = new List<CSharpStatement>();
            waveFormId = context.GetNewIdentifier("pwl");
            result.Add(new CSharpNewStatement(waveFormId, "new Pwl()") { IncludeInCollection = false });
            result.Add(new CSharpCallStatement(waveFormId, $"SetPoints({string.Join(",", data.Select(v => v.ToString(CultureInfo.InvariantCulture)))})"));
            return result;
        }

        private static ParameterCollection ExtractLtspiceScaleFactors(
            ParameterCollection parameters,
            IWriterContext context,
            out double timeScale,
            out double valueScale)
        {
            timeScale = 1.0;
            valueScale = 1.0;

            if (!context.EvaluationContext.Compatibility.IsLTspice)
            {
                return parameters;
            }

            int firstPwlSpecIndex = 0;
            AssignmentParameter timeScaleAssignment = null;
            AssignmentParameter valueScaleAssignment = null;
            while (firstPwlSpecIndex < parameters.Count
                && parameters[firstPwlSpecIndex] is AssignmentParameter assignment
                && IsScaleFactor(assignment))
            {
                if (IsTimeScaleFactor(assignment))
                {
                    timeScaleAssignment = assignment;
                }
                else
                {
                    valueScaleAssignment = assignment;
                }

                firstPwlSpecIndex++;
            }

            if (timeScaleAssignment != null)
            {
                timeScale = EvaluateScaleFactor(timeScaleAssignment, context);
                if (timeScale <= 0.0)
                {
                    throw new ArgumentException("LTspice PWL TIME_SCALE_FACTOR must be positive.");
                }
            }

            if (valueScaleAssignment != null)
            {
                valueScale = EvaluateScaleFactor(valueScaleAssignment, context);
            }

            var pwlParameters = parameters.Skip(firstPwlSpecIndex);
            if (pwlParameters.Any(parameter =>
                parameter is AssignmentParameter assignment && IsScaleFactor(assignment)))
            {
                throw new ArgumentException(
                    "LTspice PWL TIME_SCALE_FACTOR and VALUE_SCALE_FACTOR must precede all PWL specifications.");
            }

            return pwlParameters;
        }

        private static double EvaluateScaleFactor(AssignmentParameter assignment, IWriterContext context)
        {
            double factor = context.EvaluationContext.Evaluate(assignment.Value);
            if (double.IsNaN(factor) || double.IsInfinity(factor))
            {
                throw new ArgumentException($"LTspice PWL {assignment.Name.ToUpperInvariant()} must be finite.");
            }

            return factor;
        }

        private static void ScaleValues(List<double> values, double timeScale, double valueScale)
        {
            for (var i = 0; i < values.Count; i++)
            {
                double scaled = values[i] * (i % 2 == 0 ? timeScale : valueScale);
                if (double.IsNaN(scaled) || double.IsInfinity(scaled))
                {
                    throw new ArgumentException("LTspice PWL scale factors produced a non-finite time or value.");
                }

                values[i] = scaled;
            }
        }

        private static bool IsScaleFactor(AssignmentParameter assignment)
        {
            return IsTimeScaleFactor(assignment)
                || string.Equals(assignment.Name, "value_scale_factor", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTimeScaleFactor(AssignmentParameter assignment)
        {
            return string.Equals(assignment.Name, "time_scale_factor", StringComparison.OrdinalIgnoreCase);
        }
    }
}

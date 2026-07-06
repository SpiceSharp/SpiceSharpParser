using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    /// <summary>
    /// Generator for PWL waveform.
    /// </summary>
    public class PwlGenerator : WaveformGenerator
    {
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

            if (HasUnsupportedLtspiceRepeatSyntax(parameters, context))
            {
                return null;
            }

            if (parameters.Count > 0 && parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file"))
            {
                return CreatePwlFromFile(parameters, context);
            }

            bool vectorMode = parameters.Count > 1 && parameters[1] is VectorParameter vp && vp.Elements.Count == 2;

            if (!vectorMode)
            {
                return CreatePwlFromSequence(parameters, context);
            }
            else
            {
                return CreatePwlFromVector(parameters, context);
            }
        }

        private static IWaveformDescription CreatePwlFromSequence(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count % 2 != 0)
            {
                throw new ArgumentException("PWL waveform expects even count of parameters");
            }

            double[] times = new double[parameters.Count / 2];
            double[] voltages = new double[parameters.Count / 2];
            var points = new List<Point>();

            for (var i = 0; i < parameters.Count / 2; i++)
            {
                times[i] = context.Evaluator.EvaluateDouble(parameters.Get(2 * i));
                voltages[i] = context.Evaluator.EvaluateDouble(parameters.Get((2 * i) + 1));
                points.Add(new Point(times[i], voltages[i]));
            }

            return new Pwl() { Points = points };
        }

        private static IWaveformDescription CreatePwlFromVector(ParameterCollection parameters, IReadingContext context)
        {
            List<double> values = new List<double>();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is VectorParameter vp2 && vp2.Elements.Count == 2)
                {
                    values.Add(context.Evaluator.EvaluateDouble(vp2.Elements[0].Value));
                    values.Add(context.Evaluator.EvaluateDouble(vp2.Elements[1].Value));
                }
                else
                {
                    values.Add(context.Evaluator.EvaluateDouble(parameters[i].Value));
                }
            }

            int pwlPoints = values.Count / 2;
            double[] times = new double[pwlPoints];
            double[] voltages = new double[pwlPoints];
            var points = new List<Point>();

            for (var i = 0; i < pwlPoints; i++)
            {
                times[i] = values[2 * i];
                voltages[i] = values[(2 * i) + 1];
                points.Add(new Point(times[i], voltages[i]));
            }

            return new Pwl() { Points = points };
        }

        private static IWaveformDescription CreatePwlFromFile(ParameterCollection parameters, IReadingContext context)
        {
            var fileParameter = (AssignmentParameter)parameters.First(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file");
            var filePath = PathConverter.Convert(fileParameter.Value);
            var workingDirectory = context.ReaderSettings.WorkingDirectory ?? Directory.GetCurrentDirectory();
            var fullFilePath = Path.Combine(workingDirectory, filePath);

            if (!File.Exists(fullFilePath))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "PWL file does not exist: " + fullFilePath,
                    fileParameter.LineInfo);
                return null;
            }

            string[] lines;
            try
            {
                var reader = new FileReader(() => context.ReaderSettings.ExternalFilesEncoding);
                lines = reader.ReadAllLines(fullFilePath);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "PWL file could not be read: " + fullFilePath,
                    fileParameter.LineInfo,
                    ex);
                return null;
            }

            var points = ReadPoints(lines, fullFilePath, fileParameter, context);
            if (points == null)
            {
                return null;
            }

            return new Pwl() { Points = points };
        }

        private static bool HasUnsupportedLtspiceRepeatSyntax(ParameterCollection parameters, IReadingContext context)
        {
            if (!context.ReaderSettings.Compatibility.IsLTspice)
            {
                return false;
            }

            var repeatParameter = parameters.FirstOrDefault(parameter =>
                string.Equals(parameter.Value, "repeat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(parameter.Value, "endrepeat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(parameter.Value, "forever", StringComparison.OrdinalIgnoreCase)
                || (parameter is AssignmentParameter assignment
                    && string.Equals(assignment.Name, "repeat", StringComparison.OrdinalIgnoreCase)));

            if (repeatParameter == null)
            {
                return false;
            }

            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                "Unsupported LTspice PWL repeat syntax: repeat forms are not mapped yet.",
                repeatParameter.LineInfo);
            return true;
        }

        private static List<Point> ReadPoints(
            string[] lines,
            string fullFilePath,
            Parameter fileParameter,
            IReadingContext context)
        {
            if (lines.Length == 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "PWL file is empty: " + fullFilePath,
                    fileParameter.LineInfo);
                return null;
            }

            var separator = GetSeparator(lines[0]);
            var points = new List<Point>();

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = SplitLine(line, separator);
                if (parts.Length < 2
                    || !double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var time)
                    || !double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"PWL file row {i + 1} is malformed: expected two numeric columns.",
                        fileParameter.LineInfo);
                    return null;
                }

                points.Add(new Point(time, value));
            }

            if (points.Count == 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "PWL file has no data rows: " + fullFilePath,
                    fileParameter.LineInfo);
                return null;
            }

            return points;
        }

        private static char GetSeparator(string header)
        {
            if (header.Contains(";"))
            {
                return ';';
            }

            if (header.Contains(","))
            {
                return ',';
            }

            if (header.Contains('\t'))
            {
                return '\t';
            }

            return ' ';
        }

        private static string[] SplitLine(string line, char separator)
        {
            if (separator == ' ')
            {
                return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return line.Split(new[] { separator }, StringSplitOptions.None);
        }
    }
}

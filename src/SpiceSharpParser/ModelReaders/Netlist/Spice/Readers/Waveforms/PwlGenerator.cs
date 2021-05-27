using SpiceSharp.Components;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                throw new ArgumentException("PWL file does not exist:" + fullFilePath);
            }

            List<double[]> csvData = CsvFileReader.Read(fullFilePath, true, context.ReaderSettings.ExternalFilesEncoding).ToList();
            double[] times = new double[csvData.LongCount()];
            double[] voltages = new double[csvData.LongCount()];
            var points = new List<Point>();

            for (var i = 0; i < csvData.LongCount(); i++)
            {
                times[i] = csvData[i][0];
                voltages[i] = csvData[i][1];
                points.Add(new Point(times[i], voltages[i]));
            }

            return new Pwl() { Points = points };
        }
    }
}
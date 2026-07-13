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
        private const double TimeEpsilon = 1e-18;
        private const double ValueEpsilon = 1e-12;

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

            if (!TryExtractLtspiceScaleFactors(parameters, context, out var pwlParameters, out var scaleFactors))
            {
                return null;
            }

            if (TryCreateLtspiceRepeatingPwl(pwlParameters, context, scaleFactors, out var repeatingPwl))
            {
                return repeatingPwl;
            }

            if (pwlParameters.Count > 0 && pwlParameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "file"))
            {
                return CreatePwlFromFile(pwlParameters, context, scaleFactors);
            }

            bool vectorMode = pwlParameters.Any(parameter =>
                parameter is VectorParameter || parameter is PointParameter);

            if (!vectorMode)
            {
                return CreatePwlFromSequence(pwlParameters, context, scaleFactors);
            }
            else
            {
                return CreatePwlFromVector(pwlParameters, context, scaleFactors);
            }
        }

        private static IWaveformDescription CreatePwlFromSequence(
            ParameterCollection parameters,
            IReadingContext context,
            PwlScaleFactors scaleFactors)
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

            return TryScalePoints(points, scaleFactors, context, parameters.FirstOrDefault(), out var scaledPoints)
                ? new Pwl() { Points = scaledPoints }
                : null;
        }

        private static IWaveformDescription CreatePwlFromVector(
            ParameterCollection parameters,
            IReadingContext context,
            PwlScaleFactors scaleFactors)
        {
            List<double> values = new List<double>();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is VectorParameter vp2 && vp2.Elements.Count == 2)
                {
                    values.Add(context.Evaluator.EvaluateDouble(vp2.Elements[0].Value));
                    values.Add(context.Evaluator.EvaluateDouble(vp2.Elements[1].Value));
                }
                else if (parameters[i] is PointParameter point)
                {
                    foreach (var item in point.Values.Items)
                    {
                        values.Add(context.Evaluator.EvaluateDouble(item.Value));
                    }
                }
                else
                {
                    values.Add(context.Evaluator.EvaluateDouble(parameters[i].Value));
                }
            }

            if (values.Count % 2 != 0)
            {
                throw new ArgumentException("PWL waveform expects even count of parameters");
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

            return TryScalePoints(points, scaleFactors, context, parameters.FirstOrDefault(), out var scaledPoints)
                ? new Pwl() { Points = scaledPoints }
                : null;
        }

        private static IWaveformDescription CreatePwlFromFile(
            ParameterCollection parameters,
            IReadingContext context,
            PwlScaleFactors scaleFactors)
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

            return TryScalePoints(points, scaleFactors, context, fileParameter, out var scaledPoints)
                ? new Pwl() { Points = scaledPoints }
                : null;
        }

        private static bool TryCreateLtspiceRepeatingPwl(
            ParameterCollection parameters,
            IReadingContext context,
            PwlScaleFactors scaleFactors,
            out IWaveformDescription waveform)
        {
            waveform = null;

            if (!context.ReaderSettings.Compatibility.IsLTspice)
            {
                return false;
            }

            int repeatIndex = IndexOf(parameters, IsRepeatStart);
            int endRepeatIndex = IndexOf(parameters, parameter => IsWord(parameter, "endrepeat"));
            if (repeatIndex < 0)
            {
                var strayParameter = parameters.FirstOrDefault(parameter =>
                    IsWord(parameter, "endrepeat") || IsWord(parameter, "forever"));
                if (strayParameter != null)
                {
                    AddPwlError(
                        context,
                        $"LTspice PWL {strayParameter.Value} requires a preceding REPEAT block.",
                        strayParameter);
                    return true;
                }

                return false;
            }

            var repeatParameter = parameters[repeatIndex];
            if (endRepeatIndex < 0 || endRepeatIndex < repeatIndex)
            {
                AddPwlError(context, "LTspice PWL REPEAT requires a matching ENDREPEAT.", repeatParameter);
                return true;
            }

            int nestedRepeatIndex = IndexOf(
                parameters,
                parameter => IsRepeatStart(parameter),
                repeatIndex + 1,
                endRepeatIndex);
            if (nestedRepeatIndex >= 0)
            {
                AddPwlError(context, "Nested LTspice PWL REPEAT blocks are not supported yet.", parameters[nestedRepeatIndex]);
                return true;
            }

            if (!TryReadInlinePoints(parameters, 0, repeatIndex, context, repeatParameter, "prefix", 0, out var prefixPoints))
            {
                return true;
            }

            if (!TryReadRepeatCount(parameters, repeatIndex, endRepeatIndex, context, out var repeatCount, out var blockStartIndex))
            {
                return true;
            }

            if (!TryReadInlinePoints(
                parameters,
                blockStartIndex,
                endRepeatIndex - blockStartIndex,
                context,
                repeatParameter,
                "repeat",
                1,
                out var repeatPoints))
            {
                return true;
            }

            if (!TryScalePoints(prefixPoints, scaleFactors, context, repeatParameter, out prefixPoints)
                || !TryScalePoints(repeatPoints, scaleFactors, context, repeatParameter, out repeatPoints))
            {
                return true;
            }

            if (!ValidateRepeatPoints(repeatPoints, context, repeatParameter, out var period))
            {
                return true;
            }

            double repeatStartTime = prefixPoints.Count == 0 ? 0.0 : prefixPoints[prefixPoints.Count - 1].Time;
            if (!ValidateRepeatBoundary(prefixPoints, repeatPoints, repeatStartTime, context, repeatParameter))
            {
                return true;
            }

            if (repeatCount.HasValue)
            {
                if (endRepeatIndex != parameters.Count - 1)
                {
                    AddPwlError(context, "LTspice PWL parameters after ENDREPEAT are not supported yet.", parameters[endRepeatIndex]);
                    return true;
                }

                var expandedPoints = new List<Point>(prefixPoints);
                int count = (int)Math.Round(repeatCount.Value);
                for (var cycle = 0; cycle < count; cycle++)
                {
                    double offset = repeatStartTime + (cycle * period);
                    foreach (var point in repeatPoints)
                    {
                        if (!TryAddPoint(
                            expandedPoints,
                            new Point(offset + point.Time, point.Value),
                            context,
                            repeatParameter))
                        {
                            return true;
                        }
                    }
                }

                waveform = new Pwl() { Points = expandedPoints };
                return true;
            }

            if (endRepeatIndex != parameters.Count - 1)
            {
                AddPwlError(context, "LTspice PWL parameters after REPEAT FOREVER are not supported.", parameters[endRepeatIndex]);
                return true;
            }

            waveform = new RepeatingPwl(prefixPoints, repeatPoints, repeatStartTime, null);
            return true;
        }

        private static bool TryExtractLtspiceScaleFactors(
            ParameterCollection parameters,
            IReadingContext context,
            out ParameterCollection pwlParameters,
            out PwlScaleFactors scaleFactors)
        {
            pwlParameters = parameters;
            scaleFactors = PwlScaleFactors.Identity;

            if (!context.ReaderSettings.Compatibility.IsLTspice)
            {
                return true;
            }

            double timeScale = 1.0;
            double valueScale = 1.0;
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

            if (timeScaleAssignment != null
                && !TryEvaluateScaleFactor(timeScaleAssignment, context, out timeScale))
            {
                return false;
            }

            if (valueScaleAssignment != null
                && !TryEvaluateScaleFactor(valueScaleAssignment, context, out valueScale))
            {
                return false;
            }

            pwlParameters = parameters.Skip(firstPwlSpecIndex);
            var misplacedFactor = pwlParameters.FirstOrDefault(parameter =>
                parameter is AssignmentParameter assignment && IsScaleFactor(assignment));
            if (misplacedFactor != null)
            {
                AddPwlError(
                    context,
                    "LTspice PWL TIME_SCALE_FACTOR and VALUE_SCALE_FACTOR must precede all PWL specifications.",
                    misplacedFactor);
                return false;
            }

            scaleFactors = new PwlScaleFactors(timeScale, valueScale, firstPwlSpecIndex > 0);
            return true;
        }

        private static bool TryEvaluateScaleFactor(
            AssignmentParameter assignment,
            IReadingContext context,
            out double factor)
        {
            factor = 0.0;
            string factorName = assignment.Name.ToUpperInvariant();

            try
            {
                factor = context.Evaluator.EvaluateDouble(assignment.Value);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"LTspice PWL {factorName} could not be evaluated.",
                    assignment.LineInfo,
                    ex);
                return false;
            }

            if (double.IsNaN(factor) || double.IsInfinity(factor))
            {
                AddPwlError(context, $"LTspice PWL {factorName} must be finite.", assignment);
                return false;
            }

            if (IsTimeScaleFactor(assignment) && factor <= 0.0)
            {
                AddPwlError(context, "LTspice PWL TIME_SCALE_FACTOR must be positive.", assignment);
                return false;
            }

            return true;
        }

        private static bool TryScalePoints(
            IReadOnlyList<Point> points,
            PwlScaleFactors scaleFactors,
            IReadingContext context,
            Parameter diagnosticParameter,
            out List<Point> scaledPoints)
        {
            if (!scaleFactors.IsSpecified)
            {
                scaledPoints = points.ToList();
                return true;
            }

            scaledPoints = new List<Point>(points.Count);
            foreach (var point in points)
            {
                double time = point.Time * scaleFactors.Time;
                double value = point.Value * scaleFactors.Value;
                if (double.IsNaN(time)
                    || double.IsInfinity(time)
                    || double.IsNaN(value)
                    || double.IsInfinity(value))
                {
                    AddPwlError(
                        context,
                        "LTspice PWL scale factors produced a non-finite time or value.",
                        diagnosticParameter);
                    return false;
                }

                scaledPoints.Add(new Point(time, value));
            }

            return true;
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

        private static bool TryReadRepeatCount(
            ParameterCollection parameters,
            int repeatIndex,
            int endRepeatIndex,
            IReadingContext context,
            out double? repeatCount,
            out int blockStartIndex)
        {
            var repeatParameter = parameters[repeatIndex];
            repeatCount = null;
            blockStartIndex = repeatIndex + 1;

            if (repeatParameter is AssignmentParameter repeatAssignment)
            {
                if (!TryEvaluateRepeatCount(repeatAssignment.Value, repeatAssignment, context, out var assignmentCount))
                {
                    return false;
                }

                repeatCount = assignmentCount;
                return true;
            }

            if (blockStartIndex >= endRepeatIndex)
            {
                AddPwlError(context, "LTspice PWL REPEAT requires FOR <n> or FOREVER.", repeatParameter);
                return false;
            }

            var next = parameters[blockStartIndex];
            if (next is BracketParameter foreverBracket
                && string.Equals(foreverBracket.Name, "forever", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (IsWord(next, "forever"))
            {
                blockStartIndex++;
                return true;
            }

            if (next is AssignmentParameter forAssignment
                && string.Equals(forAssignment.Name, "for", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryEvaluateRepeatCount(forAssignment.Value, forAssignment, context, out var assignmentCount))
                {
                    return false;
                }

                repeatCount = assignmentCount;
                blockStartIndex++;
                return true;
            }

            if (IsWord(next, "for"))
            {
                if (blockStartIndex + 1 >= endRepeatIndex)
                {
                    AddPwlError(context, "LTspice PWL REPEAT FOR requires a repeat count.", next);
                    return false;
                }

                var countParameter = parameters[blockStartIndex + 1];
                if (!TryEvaluateRepeatCount(countParameter.Value, countParameter, context, out var wordCount))
                {
                    return false;
                }

                repeatCount = wordCount;
                blockStartIndex += 2;
                return true;
            }

            AddPwlError(context, "LTspice PWL REPEAT requires FOR <n> or FOREVER.", repeatParameter);
            return false;
        }

        private static bool TryEvaluateRepeatCount(
            string expression,
            Parameter parameter,
            IReadingContext context,
            out double repeatCount)
        {
            repeatCount = 0.0;

            try
            {
                repeatCount = context.Evaluator.EvaluateDouble(expression);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "LTspice PWL repeat count could not be evaluated.",
                    parameter.LineInfo,
                    ex);
                return false;
            }

            if (repeatCount <= 0 || Math.Abs(repeatCount - Math.Round(repeatCount)) > ValueEpsilon)
            {
                AddPwlError(context, "LTspice PWL repeat count must be a positive integer.", parameter);
                return false;
            }

            return true;
        }

        private static bool TryReadInlinePoints(
            ParameterCollection parameters,
            int startIndex,
            int count,
            IReadingContext context,
            Parameter diagnosticParameter,
            string sectionName,
            int minimumPointCount,
            out List<Point> points)
        {
            points = new List<Point>();
            var values = new List<double>();
            var relativeTimes = new List<bool>();

            for (var i = startIndex; i < startIndex + count; i++)
            {
                var parameter = parameters[i];
                if (parameter is VectorParameter vector)
                {
                    foreach (var element in vector.Elements)
                    {
                        if (!TryAppendPointValue(element.Value, element, context, values, relativeTimes))
                        {
                            return false;
                        }
                    }
                }
                else if (parameter is PointParameter point)
                {
                    foreach (var item in point.Values.Items)
                    {
                        if (!TryAppendPointValue(item.Value, item, context, values, relativeTimes))
                        {
                            return false;
                        }
                    }
                }
                else if (parameter is BracketParameter bracket
                    && string.Equals(bracket.Name, "forever", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryAppendPointValues(bracket.Parameters, context, values, relativeTimes))
                    {
                        return false;
                    }
                }
                else if (parameter is AssignmentParameter)
                {
                    AddPwlError(context, $"LTspice PWL {sectionName} data contains an unexpected assignment.", parameter);
                    return false;
                }
                else
                {
                    if (!TryAppendPointValue(parameter.Value, parameter, context, values, relativeTimes))
                    {
                        return false;
                    }
                }
            }

            if (values.Count % 2 != 0)
            {
                AddPwlError(context, $"LTspice PWL {sectionName} data expects time/value pairs.", diagnosticParameter);
                return false;
            }

            double previousTime = 0.0;
            for (var i = 0; i < values.Count / 2; i++)
            {
                double time = values[2 * i];
                if (relativeTimes[i])
                {
                    time += previousTime;
                    if (double.IsNaN(time) || double.IsInfinity(time))
                    {
                        AddPwlError(context, $"LTspice PWL {sectionName} relative +time produced a non-finite time.", diagnosticParameter);
                        return false;
                    }
                }

                points.Add(new Point(time, values[(2 * i) + 1]));
                previousTime = time;
            }

            if (points.Count < minimumPointCount)
            {
                AddPwlError(context, $"LTspice PWL {sectionName} data has no time/value points.", diagnosticParameter);
                return false;
            }

            return true;
        }

        private static bool TryAppendPointValues(
            ParameterCollection parameters,
            IReadingContext context,
            List<double> values,
            List<bool> relativeTimes)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (parameter is VectorParameter vector)
                {
                    foreach (var element in vector.Elements)
                    {
                        if (!TryAppendPointValue(element.Value, element, context, values, relativeTimes))
                        {
                            return false;
                        }
                    }
                }
                else if (parameter is PointParameter point)
                {
                    foreach (var item in point.Values.Items)
                    {
                        if (!TryAppendPointValue(item.Value, item, context, values, relativeTimes))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (!TryAppendPointValue(parameter.Value, parameter, context, values, relativeTimes))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool TryAppendPointValue(
            string expression,
            Parameter parameter,
            IReadingContext context,
            List<double> values,
            List<bool> relativeTimes)
        {
            bool isTimeValue = values.Count % 2 == 0;
            bool isRelativeTime = isTimeValue && expression.TrimStart().StartsWith("+", StringComparison.Ordinal);

            if (!TryEvaluatePointValue(expression, parameter, context, out var value))
            {
                return false;
            }

            values.Add(value);
            if (isTimeValue)
            {
                relativeTimes.Add(isRelativeTime);
            }

            return true;
        }

        private static bool TryEvaluatePointValue(
            string expression,
            Parameter parameter,
            IReadingContext context,
            out double value)
        {
            value = 0.0;

            try
            {
                value = context.Evaluator.EvaluateDouble(expression);
                return true;
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "LTspice PWL repeat point could not be evaluated.",
                    parameter.LineInfo,
                    ex);
                return false;
            }
        }

        private static bool ValidateRepeatPoints(
            IReadOnlyList<Point> repeatPoints,
            IReadingContext context,
            Parameter repeatParameter,
            out double period)
        {
            period = repeatPoints[repeatPoints.Count - 1].Time;

            for (var i = 0; i < repeatPoints.Count; i++)
            {
                if (repeatPoints[i].Time < -TimeEpsilon)
                {
                    AddPwlError(context, "LTspice PWL repeat times must be non-negative.", repeatParameter);
                    return false;
                }

                if (i > 0 && repeatPoints[i].Time <= repeatPoints[i - 1].Time + TimeEpsilon)
                {
                    AddPwlError(context, "LTspice PWL repeat times must be strictly increasing.", repeatParameter);
                    return false;
                }
            }

            if (period <= TimeEpsilon)
            {
                AddPwlError(context, "LTspice PWL repeat period must be positive.", repeatParameter);
                return false;
            }

            Point first = repeatPoints[0];
            Point last = repeatPoints[repeatPoints.Count - 1];
            if (Math.Abs(first.Time) <= TimeEpsilon && Math.Abs(first.Value - last.Value) > ValueEpsilon)
            {
                AddPwlError(
                    context,
                    "LTspice PWL repeat data is contradictory: a zero-time first point must match the final repeated value.",
                    repeatParameter);
                return false;
            }

            return true;
        }

        private static bool ValidateRepeatBoundary(
            IReadOnlyList<Point> prefixPoints,
            IReadOnlyList<Point> repeatPoints,
            double repeatStartTime,
            IReadingContext context,
            Parameter repeatParameter)
        {
            if (prefixPoints.Count == 0)
            {
                return true;
            }

            for (var i = 1; i < prefixPoints.Count; i++)
            {
                if (prefixPoints[i].Time < prefixPoints[i - 1].Time - TimeEpsilon)
                {
                    AddPwlError(context, "LTspice PWL prefix times must be non-decreasing before REPEAT.", repeatParameter);
                    return false;
                }
            }

            if (Math.Abs(repeatPoints[0].Time) <= TimeEpsilon
                && Math.Abs(prefixPoints[prefixPoints.Count - 1].Value - repeatPoints[0].Value) > ValueEpsilon)
            {
                AddPwlError(
                    context,
                    "LTspice PWL repeat data is contradictory at the REPEAT boundary.",
                    repeatParameter);
                return false;
            }

            return repeatStartTime >= prefixPoints[prefixPoints.Count - 1].Time - TimeEpsilon;
        }

        private static bool TryAddPoint(
            List<Point> points,
            Point point,
            IReadingContext context,
            Parameter repeatParameter)
        {
            if (points.Count > 0)
            {
                Point previous = points[points.Count - 1];
                if (Math.Abs(point.Time - previous.Time) <= TimeEpsilon)
                {
                    if (Math.Abs(point.Value - previous.Value) > ValueEpsilon)
                    {
                        AddPwlError(context, "LTspice PWL repeat expansion creates contradictory values at the same time.", repeatParameter);
                        return false;
                    }

                    return true;
                }

                if (point.Time < previous.Time)
                {
                    AddPwlError(context, "LTspice PWL repeat expansion creates non-monotonic point times.", repeatParameter);
                    return false;
                }
            }

            points.Add(point);
            return true;
        }

        private static int IndexOf(
            ParameterCollection parameters,
            Func<Parameter, bool> predicate,
            int startIndex = 0,
            int endIndex = -1)
        {
            int effectiveEndIndex = endIndex < 0 ? parameters.Count : endIndex;
            for (var i = startIndex; i < effectiveEndIndex; i++)
            {
                if (predicate(parameters[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsRepeatStart(Parameter parameter)
        {
            return IsWord(parameter, "repeat")
                || (parameter is AssignmentParameter assignment
                    && string.Equals(assignment.Name, "repeat", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsWord(Parameter parameter, string value)
        {
            return string.Equals(parameter.Value, value, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddPwlError(IReadingContext context, string message, Parameter parameter)
        {
            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                message,
                parameter.LineInfo);
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

            var points = new List<Point>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (IsSkippableLine(line))
                {
                    continue;
                }

                var separator = GetSeparator(line);
                var firstRowKind = ReadPoint(line, separator, out var firstPoint);
                if (firstRowKind == PwlFileRowKind.Point)
                {
                    points.Add(firstPoint);
                }
                else if (firstRowKind == PwlFileRowKind.Malformed)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"PWL file row {i + 1} is malformed: expected two numeric columns.",
                        fileParameter.LineInfo);
                    return null;
                }

                for (var j = i + 1; j < lines.Length; j++)
                {
                    var dataLine = lines[j];
                    if (IsSkippableLine(dataLine))
                    {
                        continue;
                    }

                    if (ReadPoint(dataLine, separator, out var point) != PwlFileRowKind.Point)
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $"PWL file row {j + 1} is malformed: expected two numeric columns.",
                            fileParameter.LineInfo);
                        return null;
                    }

                    points.Add(point);
                }

                break;
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

        private static PwlFileRowKind ReadPoint(string line, char separator, out Point point)
        {
            point = default;
            var parts = SplitLine(line, separator);
            if (parts.Length < 2)
            {
                return PwlFileRowKind.Malformed;
            }

            var hasTime = double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var time);
            var hasValue = double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value);

            if (hasTime && hasValue)
            {
                point = new Point(time, value);
                return PwlFileRowKind.Point;
            }

            return !hasTime && !hasValue
                ? PwlFileRowKind.Header
                : PwlFileRowKind.Malformed;
        }

        private static bool IsSkippableLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return true;
            }

            var trimmed = line.TrimStart();
            return trimmed.StartsWith(";", StringComparison.Ordinal)
                || trimmed.StartsWith("#", StringComparison.Ordinal)
                || trimmed.StartsWith("*", StringComparison.Ordinal)
                || trimmed.StartsWith("//", StringComparison.Ordinal);
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

        private enum PwlFileRowKind
        {
            Header,
            Point,
            Malformed,
        }

        private readonly struct PwlScaleFactors
        {
            public static readonly PwlScaleFactors Identity = new PwlScaleFactors(1.0, 1.0, false);

            public PwlScaleFactors(double time, double value, bool isSpecified)
            {
                Time = time;
                Value = value;
                IsSpecified = isSpecified;
            }

            public double Time { get; }

            public double Value { get; }

            public bool IsSpecified { get; }
        }
    }
}

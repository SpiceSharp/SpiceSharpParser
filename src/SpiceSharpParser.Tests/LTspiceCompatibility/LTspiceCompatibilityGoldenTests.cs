using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.LTspiceCompatibility
{
    public class LTspiceCompatibilityGoldenTests
    {
        private const string LtspiceExecutableVariable = "LTSPICE_EXE";
        private const int LtspiceTimeoutMilliseconds = 30000;

        [LtspiceFact]
        public void OpFeatures_WhenComparedWithLtspice_MatchSavedVoltages()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateOpCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var model = ReadWithLtspiceCompatibility(testCase.NetlistLines);
                var actualValues = SpiceSimulationTestHelper.RunOp(model, testCase.ExportNames);

                for (int i = 0; i < testCase.ExportNames.Length; i++)
                {
                    var exportName = testCase.ExportNames[i];
                    var expected = LtspiceAsciiRawFile.GetRealValue(raw, exportName);
                    AssertClose(
                        testCase.Name + " " + exportName,
                        0.0,
                        expected,
                        actualValues[i],
                        testCase.AbsoluteTolerance,
                        testCase.RelativeTolerance);
                }
            }
        }

        [LtspiceFact]
        public void TransientWaveforms_WhenComparedWithLtspice_MatchSamples()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateTransientCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var model = ReadWithLtspiceCompatibility(testCase.NetlistLines);

                foreach (var exportName in testCase.ExportNames)
                {
                    var ltspiceSeries = LtspiceAsciiRawFile.GetRealSeries(raw, exportName);
                    var spiceSharpSeries = SpiceSimulationTestHelper.RunTransient(model, exportName)
                        .Select(point => (Time: point.Item1, Value: point.Item2))
                        .ToArray();

                    Assert.True(ltspiceSeries.Count > 2, $"Case '{testCase.Name}' should produce LTspice transient points for '{exportName}'.");
                    Assert.True(spiceSharpSeries.Length > 2, $"Case '{testCase.Name}' should produce SpiceSharp transient points for '{exportName}'.");

                    foreach (var time in testCase.SampleTimes)
                    {
                        var expected = Interpolate(ltspiceSeries, time);
                        var actual = Interpolate(spiceSharpSeries, time);
                        AssertClose(
                            testCase.Name + " " + exportName,
                            time,
                            expected,
                            actual,
                            testCase.AbsoluteTolerance,
                            testCase.RelativeTolerance);
                    }
                }
            }
        }

        [Fact]
        public void AsciiRawReader_WhenValuesUseMultilineBlocks_ReadsSeries()
        {
            string path = Path.GetTempFileName();
            string[] rawLines =
            {
                "Title: multiline",
                "Date: Thu Jul 09 12:00:00 2026",
                "Plotname: Transient Analysis",
                "Flags: real forward",
                "No. Variables: 3",
                "No. Points: 2",
                "Variables:",
                "\t0\ttime\ttime",
                "\t1\tV(out)\tvoltage",
                "\t2\tV(in)\tvoltage",
                "Values:",
                "\t0\t0.000000000000000e+00",
                "\t\t1.000000000000000e+00",
                "\t\t2.000000000000000e+00",
                "\t1\t1.000000000000000e-09",
                "\t\t3.000000000000000e+00",
                "\t\t4.000000000000000e+00",
            };

            try
            {
                File.WriteAllLines(path, rawLines);

                var raw = LtspiceAsciiRawFile.Read(path);
                var series = LtspiceAsciiRawFile.GetRealSeries(raw, "V(out)");
                var expected = new[] { (Time: 0.0, Value: 1.0), (Time: 1e-9, Value: 3.0) };

                Assert.Equal(expected, series);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static IReadOnlyList<LtspiceGoldenCase> CreateOpCases()
        {
            return new[]
            {
                new LtspiceGoldenCase(
                    "op_smooth_limits_and_table",
                    BuildNetlist(
                        "op_smooth_limits_and_table",
                        ".options plotwinsize=0",
                        "VIN in 0 2",
                        "BUP up 0 V={uplim(V(in),1,0.2)}",
                        "BDN dn 0 V={dnlim(V(in)-2,1,0.2)}",
                        "BTBL tblout 0 V={table(1.5,0,0,1,10,2,20)}",
                        "RUP up 0 1k",
                        "RDN dn 0 1k",
                        "RTBL tblout 0 1k",
                        ".op",
                        ".save V(up) V(dn) V(tblout)"),
                    new[] { "V(up)", "V(dn)", "V(tblout)" },
                    absoluteTolerance: 1e-7,
                    relativeTolerance: 1e-6),

                new LtspiceGoldenCase(
                    "op_source_series_resistance",
                    BuildNetlist(
                        "op_source_series_resistance",
                        ".options plotwinsize=0",
                        "V1 out 0 1 Rser=10",
                        "RLOAD out 0 90",
                        ".op",
                        ".save V(out)"),
                    new[] { "V(out)" },
                    absoluteTolerance: 1e-7,
                    relativeTolerance: 1e-6),
                    
                 new LtspiceGoldenCase(
                    "op_resistor_parasitics",
                    BuildNetlist(
                        "op_resistor_parasitics",
                        ".options plotwinsize=0",
                        "V1 in 0 1",
                        "R1 in out 90 Rser=10 Rpar=900",
                        "RLOAD out 0 900",
                        ".op",
                        ".save V(out)"),
                    new[] { "V(out)" },
                    absoluteTolerance: 1e-7,
                    relativeTolerance: 1e-6),

                new LtspiceGoldenCase(
                    "op_capacitor_parallel_resistance",
                    BuildNetlist(
                        "op_capacitor_parallel_resistance",
                        ".options plotwinsize=0",
                        "V1 in 0 1",
                        "C1 in out 1n Rpar=1k",
                        "RLOAD out 0 1k",
                        ".op",
                        ".save V(out)"),
                    new[] { "V(out)" },
                    absoluteTolerance: 1e-7,
                    relativeTolerance: 1e-6),
            };
        }

        private static IReadOnlyList<LtspiceTransientGoldenCase> CreateTransientCases()
        {
            return new[]
            {
                new LtspiceTransientGoldenCase(
                    "tran_finite_cycle_sources",
                    BuildNetlist(
                        "tran_finite_cycle_sources",
                        ".options plotwinsize=0 reltol=1e-6 abstol=1e-12",
                        "VPULSE pulse 0 PULSE(0 1 2n 1n 1n 3n 10n 2)",
                        "VSINE sine 0 SINE(1 2 250Meg 1n 0 0 2)",
                        "RP pulse 0 1k",
                        "RS sine 0 1k",
                        ".tran 0.05n 25n 0 0.05n",
                        ".save V(pulse) V(sine)"),
                    new[] { "V(pulse)", "V(sine)" },
                    new[] { 0.5e-9, 2.5e-9, 3.5e-9, 6.5e-9, 8.5e-9, 12.5e-9, 13.5e-9, 16.5e-9, 22.5e-9, 24.0e-9 },
                    absoluteTolerance: 2e-3,
                    relativeTolerance: 2e-3),

                new LtspiceTransientGoldenCase(
                    "tran_exp_waveform",
                    BuildNetlist(
                        "tran_exp_waveform",
                        ".options plotwinsize=0 reltol=1e-6 abstol=1e-12",
                        "VEXP out 0 EXP(0 1 1n 1n 4n 1n)",
                        "RLOAD out 0 1k",
                        ".tran 0.05n 6n 0 0.05n",
                        ".save V(out)"),
                    new[] { "V(out)" },
                    new[] { 0.5e-9, 1.5e-9, 2.5e-9, 3.5e-9, 4.5e-9, 5.5e-9 },
                    absoluteTolerance: 2e-3,
                    relativeTolerance: 2e-3),

                new LtspiceTransientGoldenCase(
                    "tran_pwl_repeat_for",
                    BuildNetlist(
                        "tran_pwl_repeat_for",
                        ".options plotwinsize=0 reltol=1e-6 abstol=1e-12",
                        "VPWL out 0 PWL REPEAT FOR 3 (1n,1,3n,3) ENDREPEAT",
                        "RLOAD out 0 1k",
                        ".tran 0.25n 9.5n 0 0.25n",
                        ".save V(out)"),
                    new[] { "V(out)" },
                    new[] { 0.5e-9, 1.5e-9, 2.5e-9, 3.5e-9, 4.5e-9, 6.5e-9, 8.5e-9, 9.25e-9 },
                    absoluteTolerance: 2e-3,
                    relativeTolerance: 2e-3),
            };
        }

        private static IReadOnlyList<string> BuildNetlist(string name, params string[] bodyLines)
        {
            var lines = new List<string>
            {
                "LTspice compatibility golden comparison: " + name,
            };

            lines.AddRange(bodyLines);
            lines.Add(".end");
            return lines;
        }

        private static string GetLtspiceExecutable()
        {
            string executable = Environment.GetEnvironmentVariable(LtspiceExecutableVariable);
            if (string.IsNullOrWhiteSpace(executable))
            {
                throw new InvalidOperationException($"Set {LtspiceExecutableVariable} to the LTspice executable path to run these golden tests.");
            }

            if (!File.Exists(executable))
            {
                throw new FileNotFoundException($"The LTspice executable configured by {LtspiceExecutableVariable} was not found.", executable);
            }

            return executable;
        }

        private static SpiceSharpModel ReadWithLtspiceCompatibility(IReadOnlyList<string> netlistLines)
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                new SpiceNetlistTestOptions { Compatibility = CompatibilityOptions.LTspice },
                netlistLines.ToArray());

            Assert.False(
                model.ValidationResult.HasError,
                string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message)));
            return model;
        }

        private static LtspiceAsciiRawFile RunLtspiceRaw(
            string ltspiceExecutable,
            string caseName,
            IReadOnlyList<string> netlistLines)
        {
            string directory = Path.Combine(Path.GetTempPath(), "SpiceSharpParser.LTspice", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                string circuitPath = Path.Combine(directory, caseName + ".cir");
                string rawPath = Path.ChangeExtension(circuitPath, ".raw");
                File.WriteAllLines(circuitPath, netlistLines, Encoding.ASCII);

                var result = RunLtspice(ltspiceExecutable, circuitPath, directory);
                if (!WaitForFile(rawPath, LtspiceTimeoutMilliseconds))
                {
                    throw new InvalidOperationException(
                        $"LTspice did not produce '{rawPath}' for case '{caseName}'."
                        + Environment.NewLine
                        + result);
                }

                return LtspiceAsciiRawFile.Read(rawPath);
            }
            finally
            {
                TryDeleteDirectory(directory);
            }
        }

        private static ProcessResult RunLtspice(string ltspiceExecutable, string circuitPath, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ltspiceExecutable,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            startInfo.ArgumentList.Add("-b");
            startInfo.ArgumentList.Add("-ascii");
            startInfo.ArgumentList.Add(circuitPath);

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start LTspice.");
                }

                if (!process.WaitForExit(LtspiceTimeoutMilliseconds))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    throw new TimeoutException($"LTspice did not finish within {LtspiceTimeoutMilliseconds} ms for '{circuitPath}'.");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                string log = ReadLtspiceLog(circuitPath);
                var result = new ProcessResult(output, error, log);
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"LTspice exited with code {process.ExitCode} for '{circuitPath}'."
                        + Environment.NewLine
                        + result);
                }

                return result;
            }
        }

        private static string ReadLtspiceLog(string circuitPath)
        {
            string logPath = Path.ChangeExtension(circuitPath, ".log");
            return File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;
        }

        private static bool WaitForFile(string path, int timeoutMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                if (File.Exists(path))
                {
                    return true;
                }

                System.Threading.Thread.Sleep(100);
            }

            return File.Exists(path);
        }

        private static double Interpolate(IReadOnlyList<(double Time, double Value)> points, double time)
        {
            if (points.Count == 0)
            {
                throw new InvalidOperationException("Cannot interpolate an empty series.");
            }

            if (time <= points[0].Time)
            {
                return points[0].Value;
            }

            for (int i = 1; i < points.Count; i++)
            {
                if (time > points[i].Time)
                {
                    continue;
                }

                double t0 = points[i - 1].Time;
                double t1 = points[i].Time;
                double value0 = points[i - 1].Value;
                double value1 = points[i].Value;
                if (Math.Abs(t1 - t0) < 1e-30)
                {
                    return value1;
                }

                double fraction = (time - t0) / (t1 - t0);
                return value0 + ((value1 - value0) * fraction);
            }

            return points[points.Count - 1].Value;
        }

        private static void AssertClose(
            string caseName,
            double x,
            double expected,
            double actual,
            double absoluteTolerance,
            double relativeTolerance)
        {
            double tolerance = absoluteTolerance
                + (relativeTolerance * Math.Max(Math.Abs(expected), Math.Abs(actual)));
            double difference = Math.Abs(expected - actual);

            Assert.True(
                difference <= tolerance,
                FormattableString.Invariant(
                    $"Case '{caseName}' differs at x={x}: LTspice={expected}, SpiceSharpParser={actual}, difference={difference}, tolerance={tolerance}."));
        }

        private static void TryDeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private sealed class LtspiceFactAttribute : FactAttribute
        {
            public LtspiceFactAttribute()
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(LtspiceExecutableVariable)))
                {
                    this.Skip = $"Set {LtspiceExecutableVariable} to the LTspice executable path to run this external golden test.";
                }
            }
        }

        private class LtspiceGoldenCase
        {
            public LtspiceGoldenCase(
                string name,
                IReadOnlyList<string> netlistLines,
                string[] exportNames,
                double absoluteTolerance,
                double relativeTolerance)
            {
                this.Name = name;
                this.NetlistLines = netlistLines;
                this.ExportNames = exportNames;
                this.AbsoluteTolerance = absoluteTolerance;
                this.RelativeTolerance = relativeTolerance;
            }

            public string Name { get; }

            public IReadOnlyList<string> NetlistLines { get; }

            public string[] ExportNames { get; }

            public double AbsoluteTolerance { get; }

            public double RelativeTolerance { get; }
        }

        private sealed class LtspiceTransientGoldenCase : LtspiceGoldenCase
        {
            public LtspiceTransientGoldenCase(
                string name,
                IReadOnlyList<string> netlistLines,
                string[] exportNames,
                double[] sampleTimes,
                double absoluteTolerance,
                double relativeTolerance)
                : base(name, netlistLines, exportNames, absoluteTolerance, relativeTolerance)
            {
                this.SampleTimes = sampleTimes;
            }

            public double[] SampleTimes { get; }
        }

        private sealed class LtspiceAsciiRawFile
        {
            private LtspiceAsciiRawFile(IReadOnlyList<string> variables, IReadOnlyList<Complex[]> points)
            {
                this.Variables = variables;
                this.Points = points;
            }

            private IReadOnlyList<string> Variables { get; }

            private IReadOnlyList<Complex[]> Points { get; }

            public static LtspiceAsciiRawFile Read(string path)
            {
                string[] lines = File.ReadAllLines(path);
                int variableCount = ReadHeaderInt(lines, "No. Variables:");
                int pointCount = ReadHeaderInt(lines, "No. Points:");
                int variablesIndex = FindLine(lines, "Variables:");
                int valuesIndex = FindLine(lines, "Values:");

                var variables = new List<string>();
                for (int i = 0; i < variableCount; i++)
                {
                    string line = lines[variablesIndex + 1 + i].Trim();
                    string[] parts = SplitRawLine(line);
                    if (parts.Length < 2)
                    {
                        throw new FormatException($"Invalid LTspice variable line in '{path}': {line}");
                    }

                    variables.Add(parts[1]);
                }

                var points = new List<Complex[]>();
                int cursor = valuesIndex + 1;
                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                {
                    points.Add(ReadPoint(lines, ref cursor, pointIndex, variableCount, path));
                }

                return new LtspiceAsciiRawFile(variables, points);
            }

            public static double GetRealValue(LtspiceAsciiRawFile raw, string valueName)
            {
                int valueIndex = raw.FindVariable(valueName);
                return raw.Points.Single()[valueIndex].Real;
            }

            public static IReadOnlyList<(double Time, double Value)> GetRealSeries(LtspiceAsciiRawFile raw, string valueName)
            {
                int valueIndex = raw.FindVariable(valueName);
                return raw.Points
                    .Select(point => (Time: point[0].Real, Value: point[valueIndex].Real))
                    .ToArray();
            }

            private static int ReadHeaderInt(string[] lines, string name)
            {
                string line = lines.FirstOrDefault(candidate => candidate.StartsWith(name, StringComparison.OrdinalIgnoreCase));
                if (line == null)
                {
                    throw new FormatException($"LTspice raw output does not contain header '{name}'.");
                }

                return int.Parse(line.Substring(name.Length).Trim(), CultureInfo.InvariantCulture);
            }

            private static int FindLine(string[] lines, string value)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.Equals(lines[i].Trim(), value, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                throw new FormatException($"LTspice raw output does not contain section '{value}'.");
            }

            private static string ReadNextNonEmptyLine(string[] lines, ref int cursor, string path)
            {
                while (cursor < lines.Length)
                {
                    string line = lines[cursor++].Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        return line;
                    }
                }

                throw new FormatException($"Unexpected end of LTspice raw output in '{path}'.");
            }

            private static string[] SplitRawLine(string line)
            {
                return line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            }

            private static Complex[] ReadPoint(string[] lines, ref int cursor, int pointIndex, int variableCount, string path)
            {
                string firstLine = ReadNextNonEmptyLine(lines, ref cursor, path);
                string[] firstParts = SplitRawLine(firstLine);
                if (firstParts.Length == 0 || !IsInteger(firstParts[0]))
                {
                    throw new FormatException($"Invalid LTspice value line in '{path}': {firstLine}");
                }

                int actualPointIndex = int.Parse(firstParts[0], CultureInfo.InvariantCulture);
                if (actualPointIndex != pointIndex)
                {
                    throw new FormatException(
                        $"Unexpected LTspice point index in '{path}': expected {pointIndex}, got {actualPointIndex}.");
                }

                var values = new List<Complex>();
                AddValues(firstParts, 1, values);
                while (values.Count < variableCount)
                {
                    string line = ReadNextNonEmptyLine(lines, ref cursor, path);
                    string[] parts = SplitRawLine(line);
                    AddValues(parts, 0, values);
                }

                if (values.Count != variableCount)
                {
                    throw new FormatException(
                        $"Invalid LTspice point in '{path}': expected {variableCount} values, got {values.Count}.");
                }

                return values.ToArray();
            }

            private static void AddValues(string[] parts, int startIndex, List<Complex> values)
            {
                for (int i = startIndex; i < parts.Length; i++)
                {
                    values.Add(ParseComplex(parts[i]));
                }
            }

            private static bool IsInteger(string value)
            {
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
            }

            private static double ParseDouble(string value)
            {
                return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private static Complex ParseComplex(string value)
            {
                value = value.Trim();
                if (value.StartsWith("(", StringComparison.Ordinal) && value.EndsWith(")", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                int commaIndex = value.IndexOf(',');
                if (commaIndex < 0)
                {
                    return new Complex(ParseDouble(value), 0.0);
                }

                string real = value.Substring(0, commaIndex);
                string imaginary = value.Substring(commaIndex + 1);
                return new Complex(ParseDouble(real), ParseDouble(imaginary));
            }

            private int FindVariable(string name)
            {
                for (int i = 0; i < this.Variables.Count; i++)
                {
                    if (string.Equals(this.Variables[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                throw new InvalidOperationException(
                    $"LTspice raw output did not contain variable '{name}'. Available variables: {string.Join(", ", this.Variables)}.");
            }
        }

        private sealed class ProcessResult
        {
            public ProcessResult(string output, string error, string log)
            {
                this.Output = output;
                this.Error = error;
                this.Log = log;
            }

            private string Output { get; }

            private string Error { get; }

            private string Log { get; }

            public override string ToString()
            {
                return "stdout:"
                    + Environment.NewLine
                    + this.Output
                    + Environment.NewLine
                    + "stderr:"
                    + Environment.NewLine
                    + this.Error
                    + Environment.NewLine
                    + "log:"
                    + Environment.NewLine
                    + this.Log;
            }
        }
    }
}

using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class NonlinearPassiveLtspiceGoldenTests
    {
        private const string LtspiceExecutableVariable = "LTSPICE_EXE";
        private const int LtspiceTimeoutMilliseconds = 30000;

        [LtspiceFact]
        public void AcCapacitors_WhenComparedWithLtspice_MatchSmallSignalVoltage()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateCapacitorAcCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var ltspicePoint = LtspiceAsciiRawFile.GetComplexSeries(raw, "V(out)").Single();
                var spiceSharpPoint = RunSpiceSharpAcVoltage(testCase.NetlistLines, "out");

                AssertClose(testCase.Name, ltspicePoint.Frequency, ltspicePoint.Frequency, spiceSharpPoint.Frequency, 1e-9, 1e-9);
                AssertComplexClose(
                    testCase.Name,
                    ltspicePoint.Frequency,
                    ltspicePoint.Value,
                    spiceSharpPoint.Value,
                    testCase.AbsoluteTolerance,
                    testCase.RelativeTolerance);
            }
        }

        [LtspiceFact]
        public void AcInductors_WhenComparedWithLtspice_MatchSmallSignalVoltage()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateInductorAcCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var ltspicePoint = LtspiceAsciiRawFile.GetComplexSeries(raw, "V(out)").Single();
                var spiceSharpPoint = RunSpiceSharpAcVoltage(testCase.NetlistLines, "out");

                AssertClose(testCase.Name, ltspicePoint.Frequency, ltspicePoint.Frequency, spiceSharpPoint.Frequency, 1e-9, 1e-9);
                AssertComplexClose(
                    testCase.Name,
                    ltspicePoint.Frequency,
                    ltspicePoint.Value,
                    spiceSharpPoint.Value,
                    testCase.AbsoluteTolerance,
                    testCase.RelativeTolerance);
            }
        }

        [LtspiceFact]
        public void TransientCapacitors_WhenComparedWithLtspice_MatchWaveformSamples()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateCapacitorTransientCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var ltspiceSeries = LtspiceAsciiRawFile.GetRealSeries(raw, "V(out)");
                var spiceSharpSeries = RunSpiceSharpTransientVoltage(testCase.NetlistLines, "out");

                Assert.True(ltspiceSeries.Count > 5, $"Case '{testCase.Name}' should produce LTspice transient points.");
                Assert.True(spiceSharpSeries.Count > 5, $"Case '{testCase.Name}' should produce SpiceSharp transient points.");

                foreach (double time in testCase.SampleTimes)
                {
                    double expected = Interpolate(ltspiceSeries, time);
                    double actual = Interpolate(spiceSharpSeries, time);
                    AssertClose(testCase.Name, time, expected, actual, testCase.AbsoluteTolerance, testCase.RelativeTolerance);
                }
            }
        }

        [LtspiceFact]
        public void TransientInductors_WhenComparedWithLtspice_MatchWaveformSamples()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateInductorTransientCases())
            {
                var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, testCase.NetlistLines);
                var ltspiceSeries = LtspiceAsciiRawFile.GetRealSeries(raw, "V(out)");
                var spiceSharpSeries = RunSpiceSharpTransientVoltage(testCase.NetlistLines, "out");

                Assert.True(ltspiceSeries.Count > 5, $"Case '{testCase.Name}' should produce LTspice transient points.");
                Assert.True(spiceSharpSeries.Count > 5, $"Case '{testCase.Name}' should produce SpiceSharp transient points.");

                foreach (double time in testCase.SampleTimes)
                {
                    double expected = Interpolate(ltspiceSeries, time);
                    double actual = Interpolate(spiceSharpSeries, time);
                    AssertClose(testCase.Name, time, expected, actual, testCase.AbsoluteTolerance, testCase.RelativeTolerance);
                }
            }
        }

        private static IReadOnlyList<LtspicePassiveCase> CreateCapacitorAcCases()
        {
            return new[]
            {
                CreateAcCase(
                    "cap_ac_linear_charge",
                    "V1 in 0 DC 0 AC 1",
                    "R1 in out 1k",
                    "C1 out 0 Q=1u*x"),

                CreateAcCase(
                    "cap_ac_quadratic_positive_bias",
                    "V1 in 0 DC 2 AC 1",
                    "R1 in out 100",
                    "C1 out 0 Q=1u*x+100n*x*x"),

                CreateAcCase(
                    "cap_ac_cubic_positive_bias",
                    "V1 in 0 DC 1.5 AC 1",
                    "R1 in out 200",
                    "C1 out 0 Q=500n*x+50n*x*x+20n*x*x*x"),

                CreateAcCase(
                    "cap_ac_scaled_m_n",
                    "V1 in 0 DC 0 AC 1",
                    "R1 in out 470",
                    "C1 out 0 Q=2u*x M=3 N=2"),
            };
        }

        private static IReadOnlyList<LtspicePassiveCase> CreateInductorAcCases()
        {
            return new[]
            {
                CreateAcCase(
                    "ind_ac_linear_flux",
                    "V1 in 0 DC 1 AC 1",
                    "R1 in out 10",
                    "L1 out 0 Flux=2m*x"),

                CreateAcCase(
                    "ind_ac_quadratic_current_bias",
                    "V1 in 0 DC 1 AC 1",
                    "R1 in out 10",
                    "L1 out 0 Flux=1m*x+200u*x*x"),

                CreateAcCase(
                    "ind_ac_cubic_current_bias",
                    "V1 in 0 DC 2 AC 1",
                    "R1 in out 10",
                    "L1 out 0 Flux=500u*x+100u*x*x+50u*x*x*x"),

                CreateAcCase(
                    "ind_ac_scaled_m_n",
                    "V1 in 0 DC 1 AC 1",
                    "R1 in out 10",
                    "L1 out 0 Flux=1m*x M=2 N=3"),
            };
        }

        private static IReadOnlyList<LtspicePassiveTransientCase> CreateCapacitorTransientCases()
        {
            return new[]
            {
                CreateTransientCase(
                    "cap_tran_linear_charge_step",
                    new[] { 50e-6, 100e-6, 250e-6, 500e-6, 750e-6, 1e-3 },
                    "V1 in 0 5",
                    "R1 in out 1k",
                    "C1 out 0 Q=1u*x IC=0",
                    ".tran 10u 1m 0 5u uic"),

                CreateTransientCase(
                    "cap_tran_quadratic_charge_step",
                    new[] { 50e-6, 100e-6, 250e-6, 500e-6, 750e-6, 1e-3 },
                    "V1 in 0 5",
                    "R1 in out 1k",
                    "C1 out 0 Q=1u*x+100n*x*x IC=0",
                    ".tran 10u 1m 0 5u uic",
                    absoluteTolerance: 2e-3,
                    relativeTolerance: 2e-3),

                CreateTransientCase(
                    "cap_tran_scaled_charge_step",
                    new[] { 25e-6, 50e-6, 100e-6, 250e-6, 500e-6, 750e-6 },
                    "V1 in 0 5",
                    "R1 in out 1k",
                    "C1 out 0 Q=1u*x M=2 N=4 IC=0",
                    ".tran 5u 750u 0 2.5u uic"),

                CreateTransientCase(
                    "cap_tran_quadratic_ic_discharge",
                    new[] { 50e-6, 100e-6, 250e-6, 500e-6, 750e-6, 1e-3 },
                    "V1 in 0 0",
                    "R1 out 0 1k",
                    "C1 out 0 Q=1u*x+50n*x*x IC=3",
                    ".tran 10u 1m 0 5u uic",
                    absoluteTolerance: 2e-3,
                    relativeTolerance: 2e-3),
            };
        }

        private static IReadOnlyList<LtspicePassiveTransientCase> CreateInductorTransientCases()
        {
            return new[]
            {
                CreateTransientCase(
                    "ind_tran_linear_flux_decay",
                    new[] { 10e-6, 25e-6, 50e-6, 100e-6, 200e-6, 400e-6 },
                    "V1 in 0 0",
                    "L1 in out Flux=1m*x IC=0.5",
                    "R1 out 0 10",
                    ".tran 2u 400u 0 1u uic"),

                CreateTransientCase(
                    "ind_tran_quadratic_flux_decay",
                    new[] { 10e-6, 25e-6, 50e-6, 100e-6, 200e-6, 400e-6 },
                    "V1 in 0 0",
                    "L1 in out Flux=1m*x+200u*x*x IC=0.25",
                    "R1 out 0 10",
                    ".tran 2u 400u 0 1u uic",
                    absoluteTolerance: 3e-3,
                    relativeTolerance: 3e-3),

                CreateTransientCase(
                    "ind_tran_scaled_flux_decay",
                    new[] { 10e-6, 25e-6, 50e-6, 100e-6, 200e-6, 400e-6 },
                    "V1 in 0 0",
                    "L1 in out Flux=1m*x M=2 N=3 IC=0.5",
                    "R1 out 0 10",
                    ".tran 2u 400u 0 1u uic"),

                CreateTransientCase(
                    "ind_tran_cubic_flux_voltage_step",
                    new[] { 10e-6, 25e-6, 50e-6, 100e-6, 200e-6, 400e-6 },
                    "V1 in 0 5",
                    "R1 in out 10",
                    "L1 out 0 Flux=700u*x+100u*x*x+20u*x*x*x IC=0",
                    ".tran 2u 400u 0 1u uic",
                    absoluteTolerance: 3e-3,
                    relativeTolerance: 3e-3),
            };
        }

        private static LtspicePassiveCase CreateAcCase(string name, params string[] circuitLines)
        {
            return new LtspicePassiveCase(name, BuildNetlist(name, circuitLines.Concat(new[] { ".ac lin 1 1k 1k" }).ToArray()));
        }

        private static LtspicePassiveTransientCase CreateTransientCase(
            string name,
            double[] sampleTimes,
            string sourceLine,
            string firstDeviceLine,
            string secondDeviceLine,
            string transientLine,
            double absoluteTolerance = 1e-3,
            double relativeTolerance = 1e-3)
        {
            return new LtspicePassiveTransientCase(
                name,
                BuildNetlist(name, sourceLine, firstDeviceLine, secondDeviceLine, transientLine),
                sampleTimes,
                absoluteTolerance,
                relativeTolerance);
        }

        private static IReadOnlyList<string> BuildNetlist(string name, params string[] bodyLines)
        {
            var lines = new List<string>
            {
                "Nonlinear passive LTspice golden comparison: " + name,
                ".options plotwinsize=0 reltol=1e-5 abstol=1e-12",
            };

            lines.AddRange(bodyLines);
            lines.Add(".save V(out)");
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
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"LTspice exited with code {process.ExitCode} for '{circuitPath}'."
                        + Environment.NewLine
                        + new ProcessResult(output, error));
                }

                return new ProcessResult(output, error);
            }
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

        private static (double Frequency, Complex Value) RunSpiceSharpAcVoltage(IReadOnlyList<string> netlistLines, string node)
        {
            var model = ReadWithCustomComponents(netlistLines);
            var ac = model.Simulations.OfType<AC>().Single();
            var export = new ComplexVoltageExport(ac, node);

            foreach (int code in ac.Run(model.Circuit, AC.ExportSmallSignal))
            {
                if (code == AC.ExportSmallSignal)
                {
                    return (ac.Frequency, export.Value);
                }
            }

            throw new InvalidOperationException("Expected one AC small-signal export.");
        }

        private static IReadOnlyList<(double Time, double Value)> RunSpiceSharpTransientVoltage(
            IReadOnlyList<string> netlistLines,
            string node)
        {
            var model = ReadWithCustomComponents(netlistLines);
            var transient = model.Simulations.OfType<Transient>().Single();
            var export = new RealVoltageExport(transient, node);
            var points = new List<(double Time, double Value)>();

            foreach (int ignored in transient.Run(model.Circuit))
            {
                points.Add((transient.Time, export.Value));
            }

            return points;
        }

        private static SpiceSharpModel ReadWithCustomComponents(IReadOnlyList<string> netlistLines)
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Compatibility = CompatibilityOptions.LTspice;
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(string.Join(Environment.NewLine, netlistLines));
            var reader = new SpiceSharpReader();
            reader.Settings.Compatibility = CompatibilityOptions.LTspice;
            reader.Settings.UseCustomComponents();

            var model = reader.Read(parseResult.FinalModel);
            Assert.False(
                model.ValidationResult.HasError,
                string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message)));
            return model;
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

        private static void AssertComplexClose(
            string caseName,
            double x,
            Complex expected,
            Complex actual,
            double absoluteTolerance,
            double relativeTolerance)
        {
            AssertClose(caseName + " real", x, expected.Real, actual.Real, absoluteTolerance, relativeTolerance);
            AssertClose(caseName + " imaginary", x, expected.Imaginary, actual.Imaginary, absoluteTolerance, relativeTolerance);
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

        private sealed class LtspicePassiveCase
        {
            public LtspicePassiveCase(
                string name,
                IReadOnlyList<string> netlistLines,
                double absoluteTolerance = 1e-6,
                double relativeTolerance = 1e-4)
            {
                this.Name = name;
                this.NetlistLines = netlistLines;
                this.AbsoluteTolerance = absoluteTolerance;
                this.RelativeTolerance = relativeTolerance;
            }

            public string Name { get; }

            public IReadOnlyList<string> NetlistLines { get; }

            public double AbsoluteTolerance { get; }

            public double RelativeTolerance { get; }
        }

        private sealed class LtspicePassiveTransientCase
        {
            public LtspicePassiveTransientCase(
                string name,
                IReadOnlyList<string> netlistLines,
                double[] sampleTimes,
                double absoluteTolerance,
                double relativeTolerance)
            {
                this.Name = name;
                this.NetlistLines = netlistLines;
                this.SampleTimes = sampleTimes;
                this.AbsoluteTolerance = absoluteTolerance;
                this.RelativeTolerance = relativeTolerance;
            }

            public string Name { get; }

            public IReadOnlyList<string> NetlistLines { get; }

            public double[] SampleTimes { get; }

            public double AbsoluteTolerance { get; }

            public double RelativeTolerance { get; }
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
                    var point = new Complex[variableCount];
                    for (int variableIndex = 0; variableIndex < variableCount; variableIndex++)
                    {
                        string line = ReadNextNonEmptyLine(lines, ref cursor, path);
                        string[] parts = SplitRawLine(line);
                        if (parts.Length < 2)
                        {
                            throw new FormatException($"Invalid LTspice value line in '{path}': {line}");
                        }

                        point[variableIndex] = ParseComplex(parts[parts.Length - 1]);
                    }

                    points.Add(point);
                }

                return new LtspiceAsciiRawFile(variables, points);
            }

            public static IReadOnlyList<(double Time, double Value)> GetRealSeries(LtspiceAsciiRawFile raw, string valueName)
            {
                int valueIndex = raw.FindVariable(valueName);
                return raw.Points
                    .Select(point => (Time: point[0].Real, Value: point[valueIndex].Real))
                    .ToArray();
            }

            public static IReadOnlyList<(double Frequency, Complex Value)> GetComplexSeries(
                LtspiceAsciiRawFile raw,
                string valueName)
            {
                int valueIndex = raw.FindVariable(valueName);
                return raw.Points
                    .Select(point => (Frequency: point[0].Real, Value: point[valueIndex]))
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
            public ProcessResult(string output, string error)
            {
                this.Output = output;
                this.Error = error;
            }

            private string Output { get; }

            private string Error { get; }

            public override string ToString()
            {
                return "stdout:"
                    + Environment.NewLine
                    + this.Output
                    + Environment.NewLine
                    + "stderr:"
                    + Environment.NewLine
                    + this.Error;
            }
        }
    }
}

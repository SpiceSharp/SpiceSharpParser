using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.CustomComponents.IdealDiodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class IdealDiodeLtspiceGoldenTests
    {
        private const string LtspiceExecutableVariable = "LTSPICE_EXE";
        private const int LtspiceTimeoutMilliseconds = 30000;

        [LtspiceFact]
        public void DcSweep_WhenComparedWithLtspice_MatchesIdealDiodeParameters()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateCases())
            {
                var ltspicePoints = RunLtspiceDcSweep(ltspiceExecutable, testCase);
                var spiceSharpPoints = RunSpiceSharpOperatingPoints(testCase, ltspicePoints.Select(point => point.Voltage));

                Assert.Equal(ltspicePoints.Count, spiceSharpPoints.Count);
                for (int i = 0; i < ltspicePoints.Count; i++)
                {
                    var ltspicePoint = ltspicePoints[i];
                    var spiceSharpPoint = spiceSharpPoints[i];
                    AssertClose(testCase, ltspicePoint.Voltage, ltspicePoint.Current, spiceSharpPoint.Current);
                }
            }
        }

        private static IReadOnlyList<LtspiceIdealDiodeCase> CreateCases()
        {
            return new[]
            {
                new LtspiceIdealDiodeCase(
                    "ron_roff_vfwd",
                    "D1 in 0 did",
                    ".model did D(Ron=2 Roff=1e9 Vfwd=1)",
                    -1.0,
                    3.0,
                    0.1,
                    parameters =>
                    {
                        parameters.OnResistance = 2.0;
                        parameters.OffResistance = 1e9;
                        parameters.ForwardVoltage = 1.0;
                    }),

                new LtspiceIdealDiodeCase(
                    "vrev_rrev",
                    "D1 in 0 did",
                    ".model did D(Ron=2 Roff=1e9 Vfwd=1 Vrev=2 Rrev=4)",
                    -6.0,
                    3.0,
                    0.1,
                    parameters =>
                    {
                        parameters.OnResistance = 2.0;
                        parameters.OffResistance = 1e9;
                        parameters.ForwardVoltage = 1.0;
                        parameters.ReverseVoltage = 2.0;
                        parameters.ReverseResistance = 4.0;
                    }),

                new LtspiceIdealDiodeCase(
                    "forward_and_reverse_current_limits",
                    "D1 in 0 did",
                    ".model did D(Ron=1 Roff=1e9 Vfwd=0 Vrev=0 Rrev=1 Ilimit=2 RevIlimit=3)",
                    -8.0,
                    8.0,
                    0.2,
                    parameters =>
                    {
                        parameters.OnResistance = 1.0;
                        parameters.OffResistance = 1e9;
                        parameters.ForwardVoltage = 0.0;
                        parameters.ReverseVoltage = 0.0;
                        parameters.ReverseResistance = 1.0;
                        parameters.ForwardCurrentLimit = 2.0;
                        parameters.ReverseCurrentLimit = 3.0;
                    }),

                new LtspiceIdealDiodeCase(
                    "forward_and_reverse_epsilon",
                    "D1 in 0 did",
                    ".model did D(Ron=1 Roff=1e12 Vfwd=1 Vrev=2 Rrev=2 Epsilon=0.2 RevEpsilon=0.4)",
                    -3.0,
                    2.0,
                    0.05,
                    parameters =>
                    {
                        parameters.OnResistance = 1.0;
                        parameters.OffResistance = 1e12;
                        parameters.ForwardVoltage = 1.0;
                        parameters.ReverseVoltage = 2.0;
                        parameters.ReverseResistance = 2.0;
                        parameters.ForwardEpsilon = 0.2;
                        parameters.ReverseEpsilon = 0.4;
                    },
                    1e-7,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "m_n_with_ignored_area_rs_and_off",
                    "D1 in 0 did 2 off m=3 n=2",
                    ".model did D(Ron=2 Roff=1e9 Vfwd=1 Rs=3)",
                    -1.0,
                    10.0,
                    0.25,
                    parameters =>
                    {
                        parameters.Area = 2.0;
                        parameters.Off = true;
                        parameters.ParallelMultiplier = 3.0;
                        parameters.SeriesMultiplier = 2.0;
                        parameters.Resistance = 3.0;
                        parameters.OnResistance = 2.0;
                        parameters.OffResistance = 1e9;
                        parameters.ForwardVoltage = 1.0;
                    },
                    1e-6,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "rrev_omitted_with_reverse_limit_and_m_n",
                    "D1 in 0 did m=2 n=3",
                    ".model did D(Ron=1.5 Roff=1e10 Vfwd=0.6 Vrev=1.2 RevIlimit=2.5)",
                    -12.0,
                    6.0,
                    0.2,
                    parameters =>
                    {
                        parameters.ParallelMultiplier = 2.0;
                        parameters.SeriesMultiplier = 3.0;
                        parameters.OnResistance = 1.5;
                        parameters.OffResistance = 1e10;
                        parameters.ForwardVoltage = 0.6;
                        parameters.ReverseVoltage = 1.2;
                        parameters.ReverseCurrentLimit = 2.5;
                    },
                    1e-6,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "forward_current_limit_and_parallel_cells",
                    "D1 in 0 did m=4",
                    ".model did D(Ron=0.25 Roff=1e12 Vfwd=0.8 Ilimit=1.5)",
                    -0.5,
                    4.0,
                    0.05,
                    parameters =>
                    {
                        parameters.ParallelMultiplier = 4.0;
                        parameters.OnResistance = 0.25;
                        parameters.OffResistance = 1e12;
                        parameters.ForwardVoltage = 0.8;
                        parameters.ForwardCurrentLimit = 1.5;
                    },
                    1e-6,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "reverse_limit_and_asymmetric_rrev",
                    "D1 in 0 did",
                    ".model did D(Ron=3 Roff=1e11 Vfwd=0.5 Vrev=1.25 Rrev=0.75 RevIlimit=2.2)",
                    -4.0,
                    2.0,
                    0.05,
                    parameters =>
                    {
                        parameters.OnResistance = 3.0;
                        parameters.OffResistance = 1e11;
                        parameters.ForwardVoltage = 0.5;
                        parameters.ReverseVoltage = 1.25;
                        parameters.ReverseResistance = 0.75;
                        parameters.ReverseCurrentLimit = 2.2;
                    },
                    1e-6,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "moderate_roff_intersections",
                    "D1 in 0 did",
                    ".model did D(Ron=2 Roff=20 Vfwd=1 Vrev=2 Rrev=4)",
                    -6.0,
                    4.0,
                    0.1,
                    parameters =>
                    {
                        parameters.OnResistance = 2.0;
                        parameters.OffResistance = 20.0;
                        parameters.ForwardVoltage = 1.0;
                        parameters.ReverseVoltage = 2.0;
                        parameters.ReverseResistance = 4.0;
                    },
                    1e-6,
                    1e-3),

                new LtspiceIdealDiodeCase(
                    "combined_limits_asymmetric_reverse_and_m_n",
                    "D1 in 0 did m=2 n=2",
                    ".model did D(Ron=1.25 Roff=1e10 Vfwd=0.4 Vrev=1.6 Rrev=2.2 Ilimit=3 RevIlimit=2)",
                    -7.0,
                    7.0,
                    0.1,
                    parameters =>
                    {
                        parameters.ParallelMultiplier = 2.0;
                        parameters.SeriesMultiplier = 2.0;
                        parameters.OnResistance = 1.25;
                        parameters.OffResistance = 1e10;
                        parameters.ForwardVoltage = 0.4;
                        parameters.ReverseVoltage = 1.6;
                        parameters.ReverseResistance = 2.2;
                        parameters.ForwardCurrentLimit = 3.0;
                        parameters.ReverseCurrentLimit = 2.0;
                    },
                    1e-6,
                    1e-3),
            };
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

        private static IReadOnlyList<(double Voltage, double Current)> RunLtspiceDcSweep(
            string ltspiceExecutable,
            LtspiceIdealDiodeCase testCase)
        {
            string directory = Path.Combine(Path.GetTempPath(), "SpiceSharpParser.LTspice", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                string circuitPath = Path.Combine(directory, testCase.Name + ".cir");
                string rawPath = Path.ChangeExtension(circuitPath, ".raw");
                File.WriteAllLines(circuitPath, CreateLtspiceNetlist(testCase), Encoding.ASCII);

                var result = RunLtspice(ltspiceExecutable, circuitPath, directory);
                if (!WaitForFile(rawPath, LtspiceTimeoutMilliseconds))
                {
                    throw new InvalidOperationException(
                        $"LTspice did not produce '{rawPath}' for case '{testCase.Name}'."
                        + Environment.NewLine
                        + result);
                }

                var raw = LtspiceAsciiRawFile.Read(rawPath);
                return LtspiceAsciiRawFile.GetRealSeries(raw, "V(in)", "I(D1)");
            }
            finally
            {
                TryDeleteDirectory(directory);
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

        private static IReadOnlyList<string> CreateLtspiceNetlist(LtspiceIdealDiodeCase testCase)
        {
            return new[]
            {
                "Ideal diode LTspice golden comparison",
                "V1 in 0 0",
                testCase.InstanceLine,
                testCase.ModelLine,
                FormattableString.Invariant($".dc V1 {testCase.Start} {testCase.Stop} {testCase.Step}"),
                ".save V(in) I(D1)",
                ".end",
            };
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

        private static IReadOnlyList<(double Voltage, double Current)> RunSpiceSharpOperatingPoints(
            LtspiceIdealDiodeCase testCase,
            IEnumerable<double> voltages)
        {
            var result = new List<(double Voltage, double Current)>();
            foreach (double voltage in voltages)
            {
                var diode = new IdealDiode("D1", "in", "0");
                testCase.Configure(diode.Parameters);

                var circuit = new Circuit(
                    new VoltageSource("V1", "in", "0", voltage),
                    diode);

                var op = new OP("op");
                var currentExport = new RealPropertyExport(op, "D1", "i");

                double current = double.NaN;
                foreach (int ignored in op.Run(circuit))
                {
                    current = currentExport.Value;
                }

                result.Add((voltage, current));
            }

            return result;
        }

        private static void AssertClose(
            LtspiceIdealDiodeCase testCase,
            double voltage,
            double expected,
            double actual)
        {
            double tolerance = testCase.AbsoluteTolerance
                + (testCase.RelativeTolerance * Math.Max(Math.Abs(expected), Math.Abs(actual)));
            double difference = Math.Abs(expected - actual);

            Assert.True(
                difference <= tolerance,
                FormattableString.Invariant(
                    $"Case '{testCase.Name}' differs at V(in)={voltage}: LTspice I(D1)={expected}, SpiceSharpParser I(D1)={actual}, difference={difference}, tolerance={tolerance}."));
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

        private sealed class LtspiceIdealDiodeCase
        {
            public LtspiceIdealDiodeCase(
                string name,
                string instanceLine,
                string modelLine,
                double start,
                double stop,
                double step,
                Action<IdealDiodeParameters> configure,
                double absoluteTolerance = 1e-8,
                double relativeTolerance = 1e-4)
            {
                this.Name = name;
                this.InstanceLine = instanceLine;
                this.ModelLine = modelLine;
                this.Start = start;
                this.Stop = stop;
                this.Step = step;
                this.Configure = configure;
                this.AbsoluteTolerance = absoluteTolerance;
                this.RelativeTolerance = relativeTolerance;
            }

            public string Name { get; }

            public string InstanceLine { get; }

            public string ModelLine { get; }

            public double Start { get; }

            public double Stop { get; }

            public double Step { get; }

            public Action<IdealDiodeParameters> Configure { get; }

            public double AbsoluteTolerance { get; }

            public double RelativeTolerance { get; }
        }

        private sealed class LtspiceAsciiRawFile
        {
            private LtspiceAsciiRawFile(IReadOnlyList<string> variables, IReadOnlyList<double[]> points)
            {
                this.Variables = variables;
                this.Points = points;
            }

            private IReadOnlyList<string> Variables { get; }

            private IReadOnlyList<double[]> Points { get; }

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

                var points = new List<double[]>();
                int cursor = valuesIndex + 1;
                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                {
                    var point = new double[variableCount];
                    for (int variableIndex = 0; variableIndex < variableCount; variableIndex++)
                    {
                        string line = ReadNextNonEmptyLine(lines, ref cursor, path);
                        string[] parts = SplitRawLine(line);
                        if (variableIndex == 0)
                        {
                            if (parts.Length < 2)
                            {
                                throw new FormatException($"Invalid LTspice value line in '{path}': {line}");
                            }

                            point[variableIndex] = ParseDouble(parts[parts.Length - 1]);
                        }
                        else
                        {
                            point[variableIndex] = ParseDouble(parts[parts.Length - 1]);
                        }
                    }

                    points.Add(point);
                }

                return new LtspiceAsciiRawFile(variables, points);
            }

            public static IReadOnlyList<(double Voltage, double Current)> GetRealSeries(
                LtspiceAsciiRawFile raw,
                string voltageName,
                string currentName)
            {
                int voltageIndex = raw.FindVariable(voltageName);
                int currentIndex = raw.FindVariable(currentName);
                return raw.Points
                    .Select(point => (Voltage: point[voltageIndex], Current: point[currentIndex]))
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

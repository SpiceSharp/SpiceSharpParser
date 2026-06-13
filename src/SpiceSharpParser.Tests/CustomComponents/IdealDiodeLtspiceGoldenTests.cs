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
using System.Numerics;
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

        [LtspiceFact]
        public void Ac_WhenComparedWithLtspice_MatchesSmallSignalDerivatives()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            foreach (var testCase in CreateAcCases())
            {
                var raw = RunLtspiceRaw(
                    ltspiceExecutable,
                    testCase.Name,
                    CreateLtspiceAcNetlist(testCase));
                var ltspicePoints = LtspiceAsciiRawFile.GetComplexSeries(raw, "V(out)");
                var spiceSharpPoints = RunSpiceSharpAcSmallSignal(testCase);

                Assert.Equal(ltspicePoints.Count, spiceSharpPoints.Count);
                for (int i = 0; i < ltspicePoints.Count; i++)
                {
                    AssertClose(
                        testCase.Name,
                        ltspicePoints[i].Frequency,
                        ltspicePoints[i].Frequency,
                        spiceSharpPoints[i].Frequency,
                        1e-9,
                        1e-9);
                    AssertComplexClose(
                        testCase.Name,
                        ltspicePoints[i].Frequency,
                        ltspicePoints[i].Value,
                        spiceSharpPoints[i].Value,
                        testCase.AbsoluteTolerance,
                        testCase.RelativeTolerance);
                }
            }
        }

        [LtspiceFact]
        public void TransientBridge_WhenComparedWithLtspice_MatchesSinRectifierWaveform()
        {
            string ltspiceExecutable = GetLtspiceExecutable();

            var raw = RunLtspiceRaw(
                ltspiceExecutable,
                "tran_sin_bridge_rectifier",
                CreateLtspiceTransientBridgeNetlist());
            var ltspicePoints = LtspiceAsciiRawFile.GetBridgeTransientSeries(raw, "V(acp)", "V(outp)", "V(outn)");
            var spiceSharpPoints = RunSpiceSharpBridgeOperatingPoints(ltspicePoints.Select(point => point.Input));

            Assert.True(ltspicePoints.Count > 20);
            Assert.Equal(ltspicePoints.Count, spiceSharpPoints.Count);
            for (int i = 0; i < ltspicePoints.Count; i++)
            {
                AssertClose(
                    "tran_sin_bridge_rectifier",
                    ltspicePoints[i].Time,
                    ltspicePoints[i].Output,
                    spiceSharpPoints[i],
                    1e-5,
                    1e-3);
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

        private static IReadOnlyList<LtspiceIdealDiodeAcCase> CreateAcCases()
        {
            return new[]
            {
                new LtspiceIdealDiodeAcCase(
                    "ac_forward_biased_small_signal",
                    3.0,
                    ".model did D(Ron=2 Roff=1e9 Vfwd=1)",
                    parameters =>
                    {
                        parameters.OnResistance = 2.0;
                        parameters.OffResistance = 1e9;
                        parameters.ForwardVoltage = 1.0;
                    }),

                new LtspiceIdealDiodeAcCase(
                    "ac_forward_current_limit_derivative",
                    5.0,
                    ".model did D(Ron=1 Roff=1e12 Vfwd=0 Ilimit=2)",
                    parameters =>
                    {
                        parameters.OnResistance = 1.0;
                        parameters.OffResistance = 1e12;
                        parameters.ForwardVoltage = 0.0;
                        parameters.ForwardCurrentLimit = 2.0;
                    },
                    1e-7,
                    1e-4),

                new LtspiceIdealDiodeAcCase(
                    "ac_forward_epsilon_ramp_derivative",
                    1.1,
                    ".model did D(Ron=1 Roff=1e12 Vfwd=1 Epsilon=0.2)",
                    parameters =>
                    {
                        parameters.OnResistance = 1.0;
                        parameters.OffResistance = 1e12;
                        parameters.ForwardVoltage = 1.0;
                        parameters.ForwardEpsilon = 0.2;
                    },
                    1e-7,
                    1e-4),

                new LtspiceIdealDiodeAcCase(
                    "ac_reverse_breakdown_rrev_derivative",
                    -5.0,
                    ".model did D(Ron=1 Roff=1e12 Vfwd=1 Vrev=1.5 Rrev=3)",
                    parameters =>
                    {
                        parameters.OnResistance = 1.0;
                        parameters.OffResistance = 1e12;
                        parameters.ForwardVoltage = 1.0;
                        parameters.ReverseVoltage = 1.5;
                        parameters.ReverseResistance = 3.0;
                    }),
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
            var raw = RunLtspiceRaw(ltspiceExecutable, testCase.Name, CreateLtspiceNetlist(testCase));
            return LtspiceAsciiRawFile.GetRealSeries(raw, "V(in)", "I(D1)");
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

        private static IReadOnlyList<string> CreateLtspiceAcNetlist(LtspiceIdealDiodeAcCase testCase)
        {
            return new[]
            {
                "Ideal diode LTspice AC golden comparison",
                FormattableString.Invariant($"V1 in 0 DC {testCase.SourceVoltage} AC 1"),
                FormattableString.Invariant($"R1 in out {testCase.SourceResistance}"),
                "D1 out 0 did",
                testCase.ModelLine,
                ".ac lin 1 1k 1k",
                ".save V(out)",
                ".end",
            };
        }

        private static IReadOnlyList<string> CreateLtspiceTransientBridgeNetlist()
        {
            return new[]
            {
                "Ideal diode LTspice transient bridge golden comparison",
                "VIN acp 0 SIN(0 10 1k)",
                "DPLUS acp outp rect",
                "DRETURN outn 0 rect",
                "DNEG 0 outp rect",
                "DNEGRETURN outn acp rect",
                "RLOAD outp outn 10",
                ".model rect D(Ron=0.5 Roff=1e12 Vfwd=0.7)",
                ".tran 25u 1m 0 25u",
                ".save V(acp) V(outp) V(outn)",
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

        private static IReadOnlyList<(double Frequency, Complex Value)> RunSpiceSharpAcSmallSignal(
            LtspiceIdealDiodeAcCase testCase)
        {
            var diode = new IdealDiode("D1", "out", "0");
            testCase.Configure(diode.Parameters);

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", testCase.SourceVoltage).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", testCase.SourceResistance),
                diode);

            var ac = new AC("ac", new LinearSweep(1000.0, 1000.0, 1));
            var export = new ComplexVoltageExport(ac, "out");
            var result = new List<(double Frequency, Complex Value)>();

            foreach (int ignored in ac.Run(circuit, AC.ExportSmallSignal))
            {
                result.Add((ac.Frequency, export.Value));
            }

            return result;
        }

        private static IReadOnlyList<double> RunSpiceSharpBridgeOperatingPoints(IEnumerable<double> inputVoltages)
        {
            var result = new List<double>();
            foreach (double inputVoltage in inputVoltages)
            {
                var circuit = new Circuit(
                    new VoltageSource("VIN", "acp", "0", inputVoltage),
                    CreateRectifierDiode("DPLUS", "acp", "outp"),
                    CreateRectifierDiode("DRETURN", "outn", "0"),
                    CreateRectifierDiode("DNEG", "0", "outp"),
                    CreateRectifierDiode("DNEGRETURN", "outn", "acp"),
                    new Resistor("RLOAD", "outp", "outn", 10.0));

                var op = new OP("op");
                var export = new RealVoltageExport(op, "outp", "outn");
                double output = double.NaN;
                foreach (int ignored in op.Run(circuit))
                {
                    output = export.Value;
                }

                result.Add(output);
            }

            return result;
        }

        private static IdealDiode CreateRectifierDiode(string name, string anode, string cathode)
        {
            var diode = new IdealDiode(name, anode, cathode);
            diode.Parameters.OnResistance = 0.5;
            diode.Parameters.OffResistance = 1e12;
            diode.Parameters.ForwardVoltage = 0.7;
            return diode;
        }

        private static void AssertClose(
            LtspiceIdealDiodeCase testCase,
            double voltage,
            double expected,
            double actual)
        {
            AssertClose(testCase.Name, voltage, expected, actual, testCase.AbsoluteTolerance, testCase.RelativeTolerance);
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

        private sealed class LtspiceIdealDiodeAcCase
        {
            public LtspiceIdealDiodeAcCase(
                string name,
                double sourceVoltage,
                string modelLine,
                Action<IdealDiodeParameters> configure,
                double absoluteTolerance = 1e-8,
                double relativeTolerance = 1e-5,
                double sourceResistance = 1.0)
            {
                this.Name = name;
                this.SourceVoltage = sourceVoltage;
                this.ModelLine = modelLine;
                this.Configure = configure;
                this.AbsoluteTolerance = absoluteTolerance;
                this.RelativeTolerance = relativeTolerance;
                this.SourceResistance = sourceResistance;
            }

            public string Name { get; }

            public double SourceVoltage { get; }

            public string ModelLine { get; }

            public Action<IdealDiodeParameters> Configure { get; }

            public double AbsoluteTolerance { get; }

            public double RelativeTolerance { get; }

            public double SourceResistance { get; }
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
                        if (variableIndex == 0)
                        {
                            if (parts.Length < 2)
                            {
                                throw new FormatException($"Invalid LTspice value line in '{path}': {line}");
                            }

                            point[variableIndex] = ParseComplex(parts[parts.Length - 1]);
                        }
                        else
                        {
                            point[variableIndex] = ParseComplex(parts[parts.Length - 1]);
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
                    .Select(point => (Voltage: point[voltageIndex].Real, Current: point[currentIndex].Real))
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

            public static IReadOnlyList<(double Time, double Input, double Output)> GetBridgeTransientSeries(
                LtspiceAsciiRawFile raw,
                string inputName,
                string positiveName,
                string negativeName)
            {
                int inputIndex = raw.FindVariable(inputName);
                int positiveIndex = raw.FindVariable(positiveName);
                int negativeIndex = raw.FindVariable(negativeName);
                return raw.Points
                    .Select(point => (
                        Time: point[0].Real,
                        Input: point[inputIndex].Real,
                        Output: point[positiveIndex].Real - point[negativeIndex].Real))
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SpiceSharpParser.Common;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    /// <summary>
    /// Golden tests for LTspice A-device compatibility.
    /// </summary>
    public class LTspiceADeviceCompatibilityGoldenTests
    {
        private const string LtspiceExecutableVariable = "LTSPICE_EXE";
        private const int LtspiceTimeoutMilliseconds = 30000;

        [LtspiceFact]
        public void SrFlipFlop_WhenComparedWithNativeADevice_MatchesStateSequence()
        {
            string[] sharedNetlist =
            {
                "LTspice SRFLOP A-device compatibility",
                "VDD vdd 0 5",
                "VSET set 0 PULSE(0 5 10n 100p 100p 2n 100n)",
                "VRESET reset 0 PULSE(0 5 30n 100p 100p 2n 100n)",
                "ASR set reset 0 0 0 qb q 0 SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 45n 0 100p UIC",
                ".meas tran q05 FIND V(q) AT=5n",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".meas tran qb40 FIND V(qb) AT=40n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_srflop",
                sharedNetlist,
                "q05",
                "q20",
                "q40",
                "qb40");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("SRFLOP", "q05", golden["q05"], actual["q05"], 0.03);
            AssertCompatible("SRFLOP", "q20", golden["q20"], actual["q20"], 0.03);
            AssertCompatible("SRFLOP", "q40", golden["q40"], actual["q40"], 0.03);
            AssertCompatible("SRFLOP", "qb40", golden["qb40"], actual["qb40"], 0.03);
        }

        [LtspiceFact]
        public void DFlipFlop_WhenComparedWithNativeADevice_MatchesRisingEdgeCapture()
        {
            string[] sharedNetlist =
            {
                "LTspice DFLOP A-device compatibility",
                "VDD vdd 0 5",
                "VD data 0 PULSE(0 5 5n 100p 100p 30n 100n)",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "ADFF data 0 clock preset clear qb q 0 DFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 60n 0 100p UIC",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".meas tran q58 FIND V(q) AT=58n",
                ".meas tran qb58 FIND V(qb) AT=58n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_dflop",
                sharedNetlist,
                "q20",
                "q40",
                "q58",
                "qb58");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("DFLOP", "q20", golden["q20"], actual["q20"], 0.03);
            AssertCompatible("DFLOP", "q40", golden["q40"], actual["q40"], 0.03);
            AssertCompatible("DFLOP", "q58", golden["q58"], actual["q58"], 0.03);
            AssertCompatible("DFLOP", "qb58", golden["qb58"], actual["qb58"], 0.03);
        }

        [LtspiceFact]
        public void PhaseDetector_WhenComparedWithNativeADevice_MatchesSourceAndSinkStates()
        {
            string[] sharedNetlist =
            {
                "LTspice PHASEDET A-device compatibility",
                "VA a 0 PULSE(0 1 10n 100p 100p 2n 50n)",
                "VB b 0 PULSE(0 1 20n 100p 100p 2n 30n)",
                "APD a b 0 0 0 0 out 0 PHASEDET Iout=1m Vhigh=10 Vlow=-10",
                "ROUT out 0 1k",
                ".tran 100p 75n 0 100p UIC",
                ".meas tran out15 FIND V(out) AT=15n",
                ".meas tran out30 FIND V(out) AT=30n",
                ".meas tran out55 FIND V(out) AT=55n",
                ".meas tran out70 FIND V(out) AT=70n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_phasedet",
                sharedNetlist,
                "out15",
                "out30",
                "out55",
                "out70");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("PHASEDET", "out15", golden["out15"], actual["out15"], 0.05);
            AssertCompatible("PHASEDET", "out30", golden["out30"], actual["out30"], 0.05);
            AssertCompatible("PHASEDET", "out55", golden["out55"], actual["out55"], 0.05);
            AssertCompatible("PHASEDET", "out70", golden["out70"], actual["out70"], 0.05);
        }

        [LtspiceFact]
        public void Counter_WhenComparedWithNativeADevice_MatchesDivideByFourWaveform()
        {
            string[] sharedNetlist =
            {
                "LTspice COUNTER A-device compatibility",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)",
                "VRESET reset 0 0",
                "ACOUNT clock reset 0 0 0 qb q 0 COUNTER cycles=4 duty=0.5 Vhigh=5 Vlow=0 Rout=50",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 42n 0 100p UIC",
                ".meas tran q02 FIND V(q) AT=2n",
                ".meas tran q10 FIND V(q) AT=10n",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q30 FIND V(q) AT=30n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_counter",
                sharedNetlist,
                "q02",
                "q10",
                "q20",
                "q30",
                "q40");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("COUNTER", "q02", golden["q02"], actual["q02"], 0.03);
            AssertCompatible("COUNTER", "q10", golden["q10"], actual["q10"], 0.03);
            AssertCompatible("COUNTER", "q20", golden["q20"], actual["q20"], 0.03);
            AssertCompatible("COUNTER", "q30", golden["q30"], actual["q30"], 0.03);
            AssertCompatible("COUNTER", "q40", golden["q40"], actual["q40"], 0.03);
        }

        [LtspiceFact]
        public void SampleHold_WhenComparedWithNativeADevice_MatchesSampleAndTrackModes()
        {
            string[] sharedNetlist =
            {
                "LTspice SAMPLEHOLD A-device compatibility",
                "VIN in 0 PWL(0 0 5n 2 15n 2 16n 4 30n 4)",
                "VCLK clock 0 PULSE(0 1 10n 100p 100p 2n 100n)",
                "VINP inp 0 2.5",
                "VINN inn 0 0.5",
                "VTRACK track_mode 0 1",
                "ASAMPLE in 0 clock 0 0 0 out 0 SAMPLEHOLD Rout=1k",
                "ATRACK inp inn 0 track_mode 0 0 track 0 SAMPLEHOLD Rout=1k",
                "ROUT out 0 100k",
                "RTRACK track 0 100k",
                ".tran 100p 30n 0 100p UIC",
                ".meas tran out14 FIND V(out) AT=14n",
                ".meas tran out25 FIND V(out) AT=25n",
                ".meas tran track25 FIND V(track) AT=25n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_samplehold",
                sharedNetlist,
                "out14",
                "out25",
                "track25");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("SAMPLEHOLD", "out14", golden["out14"], actual["out14"], 0.03);
            AssertCompatible("SAMPLEHOLD", "out25", golden["out25"], actual["out25"], 0.03);
            AssertCompatible("SAMPLEHOLD", "track25", golden["track25"], actual["track25"], 0.03);
        }

        [LtspiceFact]
        public void Ota_WhenComparedWithNativeADevice_MatchesLinearTransconductance()
        {
            string[] sharedNetlist =
            {
                "LTspice OTA A-device compatibility",
                "VIN1N in1n 0 0",
                "VIN1P in1p 0 0.1",
                "VIN2P in2p 0 1",
                "VIN2N in2n 0 0",
                "AOTA in1n in1p in2p in2n 0 rail out 0 OTA G=1m Linear Vhigh=5 Vlow=-5 Rout=1T",
                "ROUT out 0 10k",
                ".tran 10n 1u UIC",
                ".meas tran out500 FIND V(out) AT=500n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_ota",
                sharedNetlist,
                "out500");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("OTA", "out500", golden["out500"], actual["out500"], 0.02);
        }

        [LtspiceFact]
        public void Varistor_WhenComparedWithNativeADevice_MatchesControlledClamp()
        {
            string[] sharedNetlist =
            {
                "LTspice VARISTOR A-device compatibility",
                "VCONTROL control 0 2",
                "VSUPPLY supply 0 10",
                "RDRIVE supply out 1k",
                "AVAR control 0 0 0 0 0 out 0 VARISTOR Rclamp=10",
                ".tran 10n 1u UIC",
                ".meas tran out500 FIND V(out) AT=500n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_varistor",
                sharedNetlist,
                "out500");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible("VARISTOR", "out500", golden["out500"], actual["out500"], 0.02);
        }

        [LtspiceFact]
        public void Modulator_WhenComparedWithNativeADevice_MatchesFrequencyAndAmplitude()
        {
            string[] sharedNetlist =
            {
                "LTspice MODULATOR A-device compatibility",
                "VFM fm 0 0.5",
                "VAM am 0 2",
                "AMOD fm am 0 0 0 0 out 0 MODULATOR mark=2k space=1k Rout=1",
                "ROUT out 0 100k",
                ".options plotwinsize=0",
                ".tran 1u 400u 0 1u UIC",
                ".meas tran quarter FIND V(out) AT=166.6666667u",
                ".meas tran half FIND V(out) AT=333.3333333u",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_modulator",
                sharedNetlist,
                "quarter",
                "half");

            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            AssertCompatible(
                "MODULATOR",
                "quarter",
                golden["quarter"],
                actual["quarter"],
                0.05);
            AssertCompatible(
                "MODULATOR",
                "half",
                golden["half"],
                actual["half"],
                0.05);
        }

        private static IReadOnlyDictionary<string, double> RunSpiceSharpMeasurements(
            IEnumerable<string> netlistLines,
            params string[] measurementNames)
        {
            var options = new SpiceNetlistTestOptions
            {
                Compatibility = CompatibilityOptions.LTspice,
                UseCustomComponents = true,
            };
            var model = SpiceNetlistTestHelper.ParseAndRead(options, netlistLines.ToArray());
            Assert.False(
                model.ValidationResult.HasError,
                string.Join(
                    Environment.NewLine,
                    model.ValidationResult.Errors.Select(error => error.Message)));

            SpiceSimulationTestHelper.RunSimulations(model);

            var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (string measurementName in measurementNames)
            {
                Assert.True(
                    model.Measurements.TryGetValue(measurementName, out var measurements),
                    $"SpiceSharpParser did not produce measurement '{measurementName}'.");
                var measurement = measurements.Last();
                Assert.True(
                    measurement.Success,
                    $"SpiceSharpParser measurement '{measurementName}' did not succeed.");
                result.Add(measurementName, measurement.Value);
            }

            return result;
        }

        private static IReadOnlyDictionary<string, double> RunLtspiceMeasurements(
            string caseName,
            IEnumerable<string> netlistLines,
            params string[] measurementNames)
        {
            string ltspiceExecutable = GetLtspiceExecutable();
            string directory = Path.Combine(
                Path.GetTempPath(),
                "SpiceSharpParser.LTspice",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                string circuitPath = Path.Combine(directory, caseName + ".net");
                File.WriteAllLines(circuitPath, netlistLines, Encoding.ASCII);

                ProcessResult processResult = RunLtspice(ltspiceExecutable, circuitPath);
                string logPath = Path.ChangeExtension(circuitPath, ".log");
                if (!File.Exists(logPath))
                {
                    throw new InvalidOperationException(
                        $"LTspice did not produce the expected log '{logPath}'."
                        + Environment.NewLine
                        + processResult);
                }

                string log = File.ReadAllText(logPath);
                var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (string measurementName in measurementNames)
                {
                    result.Add(measurementName, ReadMeasurement(log, measurementName, logPath));
                }

                return result;
            }
            finally
            {
                TryDeleteDirectory(directory);
            }
        }

        private static ProcessResult RunLtspice(string executable, string circuitPath)
        {
            var startInfo = new ProcessStartInfo(executable)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("-b");
            startInfo.ArgumentList.Add("-ascii");
            startInfo.ArgumentList.Add(circuitPath);

            using (var process = new Process { StartInfo = startInfo })
            {
                if (!process.Start())
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

                    throw new TimeoutException(
                        $"LTspice did not finish within {LtspiceTimeoutMilliseconds} ms for '{circuitPath}'.");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                var result = new ProcessResult(output, error);
                if (process.ExitCode != 0)
                {
                    string logPath = Path.ChangeExtension(circuitPath, ".log");
                    string log = File.Exists(logPath)
                        ? Environment.NewLine + "log:" + Environment.NewLine + File.ReadAllText(logPath)
                        : string.Empty;
                    throw new InvalidOperationException(
                        $"LTspice exited with code {process.ExitCode} for '{circuitPath}'."
                        + Environment.NewLine
                        + result
                        + log);
                }

                return result;
            }
        }

        private static double ReadMeasurement(string log, string name, string logPath)
        {
            string pattern = "^\\s*"
                + Regex.Escape(name)
                + "\\s*:\\s*.*?=\\s*(?<value>[-+]?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?)";
            Match match = Regex.Match(
                log,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    $"LTspice log '{logPath}' did not contain a numeric measurement named '{name}'."
                    + Environment.NewLine
                    + log);
            }

            return double.Parse(
                match.Groups["value"].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture);
        }

        private static string GetLtspiceExecutable()
        {
            string executable = Environment.GetEnvironmentVariable(LtspiceExecutableVariable);
            if (string.IsNullOrWhiteSpace(executable))
            {
                throw new InvalidOperationException(
                    $"Set {LtspiceExecutableVariable} to the LTspice executable path.");
            }

            if (!File.Exists(executable))
            {
                throw new FileNotFoundException(
                    $"The LTspice executable configured by {LtspiceExecutableVariable} was not found.",
                    executable);
            }

            return executable;
        }

        private static void AssertCompatible(
            string device,
            string measurement,
            double ltspice,
            double portable,
            double absoluteTolerance,
            double relativeTolerance = 0.01)
        {
            double difference = Math.Abs(ltspice - portable);
            double tolerance = absoluteTolerance
                + (relativeTolerance * Math.Max(Math.Abs(ltspice), Math.Abs(portable)));
            Assert.True(
                difference <= tolerance,
                FormattableString.Invariant(
                    $"LTspice A-device compatibility failed for {device}/{measurement}: LTspice={ltspice}, portable={portable}, difference={difference}, tolerance={tolerance}."));
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
                    this.Skip =
                        $"Set {LtspiceExecutableVariable} to the LTspice executable path "
                        + "to run this LTspice A-device compatibility golden test.";
                }
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

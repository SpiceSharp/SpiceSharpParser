using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SpiceSharpParser.CustomComponents.Analog;
using SpiceSharpParser.CustomComponents.Digital;
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
            string[] ltspiceNetlist =
            {
                "LTspice SRFLOP A-device compatibility",
                "VDD vdd 0 5",
                "VSET set 0 PULSE(0 5 10n 100p 100p 2n 100n)",
                "VRESET reset 0 PULSE(0 5 30n 100p 100p 2n 100n)",
                "ASR set reset 0 0 0 qb q 0 SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 45n 0 100p",
                ".meas tran q05 FIND V(q) AT=5n",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".meas tran qb40 FIND V(qb) AT=40n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_srflop",
                ltspiceNetlist,
                "q05",
                "q20",
                "q40",
                "qb40");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable SRFLOP compatibility",
                "VDD vdd 0 5",
                "VSET set 0 PULSE(0 5 10n 100p 100p 2n 100n)",
                "VRESET reset 0 PULSE(0 5 30n 100p 100p 2n 100n)",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 45n 0 100p UIC",
                ".save V(q) V(qb)",
                ".end");
            DigitalSubcircuitLibrary.LoadBuiltIn().AddSetResetFlipFlop(
                model.Circuit,
                "XSR",
                "set",
                "reset",
                "q",
                "qb",
                "vdd",
                "0",
                FastStateParameters());

            Tuple<double, double, double>[] actual =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");

            AssertCompatible("SRFLOP", "q05", golden["q05"], Nearest(actual, 5e-9).Item2, 0.03);
            AssertCompatible("SRFLOP", "q20", golden["q20"], Nearest(actual, 20e-9).Item2, 0.03);
            AssertCompatible("SRFLOP", "q40", golden["q40"], Nearest(actual, 40e-9).Item2, 0.03);
            AssertCompatible("SRFLOP", "qb40", golden["qb40"], Nearest(actual, 40e-9).Item3, 0.03);
        }

        [LtspiceFact]
        public void DFlipFlop_WhenComparedWithNativeADevice_MatchesRisingEdgeCapture()
        {
            string[] ltspiceNetlist =
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
                ".tran 100p 60n 0 100p",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".meas tran q58 FIND V(q) AT=58n",
                ".meas tran qb58 FIND V(qb) AT=58n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_dflop",
                ltspiceNetlist,
                "q20",
                "q40",
                "q58",
                "qb58");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable DFLOP compatibility",
                "VDD vdd 0 5",
                "VD data 0 PULSE(0 5 5n 100p 100p 30n 100n)",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 60n 0 100p UIC",
                ".save V(q) V(qb)",
                ".end");
            DigitalSubcircuitLibrary.LoadBuiltIn().AddDFlipFlop(
                model.Circuit,
                "XDFF",
                "data",
                "clock",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0",
                FastStateParameters());

            Tuple<double, double, double>[] actual =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");

            AssertCompatible("DFLOP", "q20", golden["q20"], Nearest(actual, 20e-9).Item2, 0.03);
            AssertCompatible("DFLOP", "q40", golden["q40"], Nearest(actual, 40e-9).Item2, 0.03);
            AssertCompatible("DFLOP", "q58", golden["q58"], Nearest(actual, 58e-9).Item2, 0.03);
            AssertCompatible("DFLOP", "qb58", golden["qb58"], Nearest(actual, 58e-9).Item3, 0.03);
        }

        [LtspiceFact]
        public void PhaseDetector_WhenComparedWithNativeADevice_MatchesSourceAndSinkStates()
        {
            string[] ltspiceNetlist =
            {
                "LTspice PHASEDET A-device compatibility",
                "VA a 0 PULSE(0 1 10n 100p 100p 2n 50n)",
                "VB b 0 PULSE(0 1 20n 100p 100p 2n 30n)",
                "APD a b 0 0 0 0 out 0 PHASEDET Iout=1m Vhigh=10 Vlow=-10",
                "ROUT out 0 1k",
                ".tran 100p 75n 0 100p",
                ".meas tran out15 FIND V(out) AT=15n",
                ".meas tran out30 FIND V(out) AT=30n",
                ".meas tran out55 FIND V(out) AT=55n",
                ".meas tran out70 FIND V(out) AT=70n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_phasedet",
                ltspiceNetlist,
                "out15",
                "out30",
                "out55",
                "out70");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable PHASEDET compatibility",
                "VA a 0 PULSE(0 1 10n 100p 100p 2n 50n)",
                "VB b 0 PULSE(0 1 20n 100p 100p 2n 30n)",
                "ROUT out 0 1k",
                ".tran 100p 75n 0 100p UIC",
                ".save V(out) V(a)",
                ".end");
            DigitalSubcircuitLibrary.LoadBuiltIn().AddPhaseDetector(
                model.Circuit,
                "XPD",
                "a",
                "b",
                "out",
                "0",
                new Dictionary<string, string>
                {
                    ["RSTATE"] = "1",
                    ["CMEM"] = "1p",
                });

            Tuple<double, double, double>[] actual =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(out)", "V(a)");

            AssertCompatible("PHASEDET", "out15", golden["out15"], Nearest(actual, 15e-9).Item2, 0.05);
            AssertCompatible("PHASEDET", "out30", golden["out30"], Nearest(actual, 30e-9).Item2, 0.05);
            AssertCompatible("PHASEDET", "out55", golden["out55"], Nearest(actual, 55e-9).Item2, 0.05);
            AssertCompatible("PHASEDET", "out70", golden["out70"], Nearest(actual, 70e-9).Item2, 0.05);
        }

        [LtspiceFact]
        public void Counter_WhenComparedWithNativeADevice_MatchesDivideByFourWaveform()
        {
            string[] ltspiceNetlist =
            {
                "LTspice COUNTER A-device compatibility",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)",
                "VRESET reset 0 0",
                "ACOUNT clock reset 0 0 0 qb q 0 COUNTER cycles=4 duty=0.5 Vhigh=5 Vlow=0 Rout=50",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 42n 0 100p",
                ".meas tran q02 FIND V(q) AT=2n",
                ".meas tran q10 FIND V(q) AT=10n",
                ".meas tran q20 FIND V(q) AT=20n",
                ".meas tran q30 FIND V(q) AT=30n",
                ".meas tran q40 FIND V(q) AT=40n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_counter",
                ltspiceNetlist,
                "q02",
                "q10",
                "q20",
                "q30",
                "q40");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable COUNTER compatibility",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)",
                "VRESET reset 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".tran 100p 42n 0 100p UIC",
                ".save V(q) V(qb)",
                ".end");
            DigitalSubcircuitLibrary.LoadBuiltIn().AddCounter(
                model.Circuit,
                "XCOUNT",
                "clock",
                "reset",
                "q",
                "qb",
                "vdd",
                "0",
                4,
                0.5);

            Tuple<double, double, double>[] actual =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");

            AssertCompatible("COUNTER", "q02", golden["q02"], Nearest(actual, 2e-9).Item2, 0.03);
            AssertCompatible("COUNTER", "q10", golden["q10"], Nearest(actual, 10e-9).Item2, 0.03);
            AssertCompatible("COUNTER", "q20", golden["q20"], Nearest(actual, 20e-9).Item2, 0.03);
            AssertCompatible("COUNTER", "q30", golden["q30"], Nearest(actual, 30e-9).Item2, 0.03);
            AssertCompatible("COUNTER", "q40", golden["q40"], Nearest(actual, 40e-9).Item2, 0.03);
        }

        [LtspiceFact]
        public void SampleHold_WhenComparedWithNativeADevice_MatchesSampleAndTrackModes()
        {
            string[] ltspiceNetlist =
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
                ".tran 100p 30n 0 100p",
                ".meas tran out14 FIND V(out) AT=14n",
                ".meas tran out25 FIND V(out) AT=25n",
                ".meas tran track25 FIND V(track) AT=25n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_samplehold",
                ltspiceNetlist,
                "out14",
                "out25",
                "track25");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable SAMPLEHOLD compatibility",
                "VIN in 0 PWL(0 0 5n 2 15n 2 16n 4 30n 4)",
                "VCLK clock 0 PULSE(0 1 10n 100p 100p 2n 100n)",
                "VINP inp 0 2.5",
                "VINN inn 0 0.5",
                "VTRACK track_mode 0 1",
                "ROUT out 0 100k",
                "RTRACK track 0 100k",
                ".tran 100p 30n 0 100p UIC",
                ".save V(out) V(track)",
                ".end");
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();
            analog.AddSampleHold(
                model.Circuit,
                "XSAMPLE",
                "in",
                "0",
                "clock",
                "0",
                "out",
                "0");
            analog.AddSampleHold(
                model.Circuit,
                "XTRACK",
                "inp",
                "inn",
                "0",
                "track_mode",
                "track",
                "0");

            Tuple<double, double, double>[] actual =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(out)", "V(track)");

            AssertCompatible("SAMPLEHOLD", "out14", golden["out14"], Nearest(actual, 14e-9).Item2, 0.03);
            AssertCompatible("SAMPLEHOLD", "out25", golden["out25"], Nearest(actual, 25e-9).Item2, 0.03);
            AssertCompatible("SAMPLEHOLD", "track25", golden["track25"], Nearest(actual, 25e-9).Item3, 0.03);
        }

        [LtspiceFact]
        public void Ota_WhenComparedWithNativeADevice_MatchesLinearTransconductance()
        {
            string[] ltspiceNetlist =
            {
                "LTspice OTA A-device compatibility",
                "VIN1N in1n 0 0",
                "VIN1P in1p 0 0.1",
                "VIN2P in2p 0 1",
                "VIN2N in2n 0 0",
                "AOTA in1n in1p in2p in2n 0 rail out 0 OTA G=1m Linear Vhigh=5 Vlow=-5 Rout=1T",
                "ROUT out 0 10k",
                ".tran 10n 1u",
                ".meas tran out500 FIND V(out) AT=500n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_ota",
                ltspiceNetlist,
                "out500");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable OTA compatibility",
                "VIN1N in1n 0 0",
                "VIN1P in1p 0 0.1",
                "VIN2P in2p 0 1",
                "VIN2N in2n 0 0",
                "ROUT out 0 10k",
                ".tran 10n 1u UIC",
                ".save V(out) V(rail)",
                ".end");
            AnalogSubcircuitLibrary.LoadBuiltIn().AddOperationalTransconductanceAmplifier(
                model.Circuit,
                "XOTA",
                "in1n",
                "in1p",
                "in2p",
                "in2n",
                "rail",
                "out",
                "0",
                new Dictionary<string, string>
                {
                    ["G"] = "1m",
                    ["LINEAR"] = "1",
                    ["VHIGH"] = "5",
                    ["VLOW"] = "-5",
                    ["ROUT"] = "1T",
                });

            Tuple<double, double>[] actual =
                SpiceSimulationTestHelper.RunTransient(model, "V(out)");

            AssertCompatible("OTA", "out500", golden["out500"], Nearest(actual, 500e-9).Item2, 0.02);
        }

        [LtspiceFact]
        public void Varistor_WhenComparedWithNativeADevice_MatchesControlledClamp()
        {
            string[] ltspiceNetlist =
            {
                "LTspice VARISTOR A-device compatibility",
                "VCONTROL control 0 2",
                "VSUPPLY supply 0 10",
                "RDRIVE supply out 1k",
                "AVAR control 0 0 0 0 0 out 0 VARISTOR Rclamp=10",
                ".tran 10n 1u",
                ".meas tran out500 FIND V(out) AT=500n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_varistor",
                ltspiceNetlist,
                "out500");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable VARISTOR compatibility",
                "VCONTROL control 0 2",
                "VSUPPLY supply 0 10",
                "RDRIVE supply out 1k",
                ".tran 10n 1u UIC",
                ".save V(out) V(control)",
                ".end");
            AnalogSubcircuitLibrary.LoadBuiltIn().AddVoltageControlledVaristor(
                model.Circuit,
                "XVAR",
                "control",
                "0",
                "out",
                "0",
                new Dictionary<string, string> { ["RCLAMP"] = "10" });

            Tuple<double, double>[] actual =
                SpiceSimulationTestHelper.RunTransient(model, "V(out)");

            AssertCompatible("VARISTOR", "out500", golden["out500"], Nearest(actual, 500e-9).Item2, 0.02);
        }

        [LtspiceFact]
        public void Modulator_WhenComparedWithNativeADevice_MatchesFrequencyAndAmplitude()
        {
            const double frequency = 1500.0;
            const double quarterPeriod = 0.25 / frequency;
            const double halfPeriod = 0.5 / frequency;

            string[] ltspiceNetlist =
            {
                "LTspice MODULATOR A-device compatibility",
                "VFM fm 0 0.5",
                "VAM am 0 2",
                "AMOD fm am 0 0 0 0 out 0 MODULATOR mark=2k space=1k Rout=1",
                "ROUT out 0 100k",
                ".options plotwinsize=0",
                ".tran 1u 400u 0 1u",
                ".meas tran quarter FIND V(out) AT=166.6666667u",
                ".meas tran half FIND V(out) AT=333.3333333u",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_modulator",
                ltspiceNetlist,
                "quarter",
                "half");

            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Portable MODULATOR compatibility",
                "VFM fm 0 0.5",
                "VAM am 0 2",
                "ROUT out 0 100k",
                ".tran 1u 400u 0 1u UIC",
                ".save V(out) V(fm)",
                ".end");
            AnalogSubcircuitLibrary.LoadBuiltIn().AddModulator(
                model.Circuit,
                "XMOD",
                "fm",
                "am",
                "out",
                "0",
                new Dictionary<string, string>
                {
                    ["MARK"] = "2k",
                    ["SPACE"] = "1k",
                });

            Tuple<double, double>[] actual =
                SpiceSimulationTestHelper.RunTransient(model, "V(out)");

            AssertCompatible(
                "MODULATOR",
                "quarter",
                golden["quarter"],
                Nearest(actual, quarterPeriod).Item2,
                0.05);
            AssertCompatible(
                "MODULATOR",
                "half",
                golden["half"],
                Nearest(actual, halfPeriod).Item2,
                0.05);
        }

        private static IReadOnlyDictionary<string, string> FastStateParameters()
        {
            return new Dictionary<string, string>
            {
                ["TPD"] = "1n",
                ["ROUT"] = "1",
                ["COUT"] = "1f",
                ["RSTATE"] = "1",
                ["CMEM"] = "1p",
            };
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

        private static Tuple<double, double> Nearest(
            IEnumerable<Tuple<double, double>> samples,
            double time)
        {
            return samples.OrderBy(item => Math.Abs(item.Item1 - time)).First();
        }

        private static Tuple<double, double, double> Nearest(
            IEnumerable<Tuple<double, double, double>> samples,
            double time)
        {
            return samples.OrderBy(item => Math.Abs(item.Item1 - time)).First();
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

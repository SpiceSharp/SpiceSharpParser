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

#pragma warning disable SA1118 // Inline netlist arrays keep each golden case self-contained.
        [LtspiceFact]
        public void DefaultParameters_WhenComparedWithNativeADevices_MatchObservableBehavior()
        {
            AssertGoldenCase(
                "a_device_defaults_srflop",
                "SRFLOP_DEFAULTS",
                new[]
                {
                    "LTspice SRFLOP default-only compatibility",
                    "VSET set 0 PULSE(0 1 0.5m 1u 1u 0.5m 10m)",
                    "VRESET reset 0 PULSE(0 1 2.5m 1u 1u 0.5m 10m)",
                    "ASR set reset 0 0 0 qb q 0 SRFLOP",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    ".tran 1u 4m 0 1u UIC",
                    ".meas tran set_state FIND V(q) AT=1.5m",
                    ".meas tran reset_state FIND V(q) AT=3.5m",
                    ".end",
                },
                new[] { "set_state", "reset_state" },
                new[] { 0.95, -0.01 },
                new[] { 1.01, 0.05 });
            AssertGoldenCase(
                "a_device_defaults_dflop",
                "DFLOP_DEFAULTS",
                new[]
                {
                    "LTspice DFLOP default-only compatibility",
                    "VDATA data 0 PULSE(0 1 0.25m 1u 1u 2m 10m)",
                    "VCLK clock 0 PULSE(0 1 1m 1u 1u 0.25m 2m)",
                    "ADFF data 0 clock 0 0 qb q 0 DFLOP",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    ".tran 1u 4m 0 1u UIC",
                    ".meas tran high_state FIND V(q) AT=1.5m",
                    ".meas tran low_state FIND V(q) AT=3.5m",
                    ".end",
                },
                new[] { "high_state", "low_state" },
                new[] { 0.95, -0.01 },
                new[] { 1.01, 0.05 });
            AssertGoldenCase(
                "a_device_defaults_phasedet",
                "PHASEDET_DEFAULTS",
                new[]
                {
                    "LTspice PHASEDET default-only compatibility",
                    "VA a 0 PULSE(0 1 1m 1u 1u 0.25m 10m)",
                    "VB b 0 PULSE(0 1 2m 1u 1u 0.25m 10m)",
                    "APD a b 0 0 0 0 out 0 PHASEDET",
                    "ROUT out 0 1k",
                    ".tran 1u 3m 0 1u UIC",
                    ".meas tran source_state FIND V(out) AT=1.5m",
                    ".meas tran reset_state FIND V(out) AT=2.5m",
                    ".end",
                },
                new[] { "source_state", "reset_state" },
                new[] { 0.095, -0.01 },
                new[] { 0.105, 0.01 });
            AssertGoldenCase(
                "a_device_defaults_counter",
                "COUNTER_DEFAULTS",
                new[]
                {
                    "LTspice COUNTER optional-default compatibility",
                    "VCLK clock 0 PULSE(0 1 0.5m 1u 1u 0.25m 1m)",
                    "ACOUNT clock 0 0 0 0 qb q 0 COUNTER cycles=4",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    ".tran 1u 2m 0 1u UIC",
                    ".meas tran state_1 FIND V(q) AT=0.75m",
                    ".meas tran state_2 FIND V(q) AT=1.75m",
                    ".end",
                },
                new[] { "state_1", "state_2" },
                new[] { 0.95, -0.01 },
                new[] { 1.01, 0.05 });
            AssertGoldenCase(
                "a_device_defaults_samplehold",
                "SAMPLEHOLD_DEFAULTS",
                new[]
                {
                    "LTspice SAMPLEHOLD default-only compatibility",
                    "VINP inp 0 2.5",
                    "VINN inn 0 0.5",
                    "VTRACK track 0 1",
                    "ASH inp inn 0 track 0 0 out 0 SAMPLEHOLD",
                    "ROUT out 0 100k",
                    ".tran 1u 2m 0 1u UIC",
                    ".meas tran tracked FIND V(out) AT=1.5m",
                    ".end",
                },
                new[] { "tracked" },
                new[] { 1.95 },
                new[] { 2.01 });
            AssertGoldenCase(
                "a_device_defaults_ota",
                "OTA_DEFAULTS",
                new[]
                {
                    "LTspice OTA default-only compatibility",
                    "VINN inn 0 0",
                    "VINP inp 0 0.1",
                    "VMULP mulp 0 1",
                    "VMULN muln 0 0",
                    "AOTA inn inp mulp muln 0 rail out 0 OTA",
                    "ROUT out 0 100k",
                    ".tran 1u 2m 0 1u UIC",
                    ".meas tran output FIND V(out) AT=1.5m",
                    ".end",
                },
                new[] { "output" },
                new[] { 0.95 },
                new[] { 1.05 });
            AssertGoldenCase(
                "a_device_defaults_varistor",
                "VARISTOR_DEFAULTS",
                new[]
                {
                    "LTspice VARISTOR default-only compatibility",
                    "VCONTROL control 0 2",
                    "VSUPPLY supply 0 10",
                    "RDRIVE supply out 1k",
                    "AVAR control 0 0 0 0 0 out 0 VARISTOR",
                    ".tran 1u 2m 0 1u UIC",
                    ".meas tran output FIND V(out) AT=1.5m",
                    ".end",
                },
                new[] { "output" },
                new[] { 1.95 },
                new[] { 2.05 });
            AssertGoldenCase(
                "a_device_defaults_modulator",
                "MODULATOR_DEFAULTS",
                new[]
                {
                    "LTspice MODULATOR optional-default compatibility",
                    "AMOD 0 0 0 0 0 0 out 0 MODULATOR mark=1k space=1k",
                    "ROUT out 0 100k",
                    ".options plotwinsize=0",
                    ".tran 1u 500u 0 1u UIC",
                    ".meas tran peak FIND V(out) AT=250u",
                    ".end",
                },
                new[] { "peak" },
                new[] { 0.95 },
                new[] { 1.05 });
        }
#pragma warning restore SA1118

        [LtspiceFact]
        public void StatefulDevices_LongDuration_WhenComparedWithNativeADevices_RetainState()
        {
            AssertLongDurationStateCompatibility(useIc: false);
            AssertLongDurationStateCompatibility(useIc: true);
        }

        [LtspiceFact]
        public void Modulator_LongDuration_WhenComparedWithNativeADevice_MaintainsPeriod()
        {
            AssertLongDurationModulatorCompatibility(useIc: false);
            AssertLongDurationModulatorCompatibility(useIc: true);
        }

        [LtspiceFact]
        public void Modulator_UnusedAmplitude_WhenComparedWithNativeADevice_DefaultsToOne()
        {
            string[] sharedNetlist =
            {
                "LTspice MODULATOR unused AM compatibility",
                "VFM fm 0 0.5",
                "VZERO zero 0 0",
                "ADEFAULT fm 0 0 0 0 0 out_default 0 MODULATOR mark=2k space=1k Rout=1",
                "ADRIVEN fm zero 0 0 0 0 out_zero 0 MODULATOR mark=2k space=1k Rout=1",
                "RDEFAULT out_default 0 100k",
                "RZERO out_zero 0 100k",
                ".options plotwinsize=0",
                ".tran 1u 200u 0 1u UIC",
                ".meas tran unused_am FIND V(out_default) AT=166.6666667u",
                ".meas tran driven_zero FIND V(out_zero) AT=166.6666667u",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_modulator_unused_am",
                sharedNetlist,
                "unused_am",
                "driven_zero");
            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            Assert.InRange(golden["unused_am"], 0.95, 1.05);
            Assert.InRange(golden["driven_zero"], -0.01, 0.01);
            AssertCompatible("MODULATOR", "unused_am", golden["unused_am"], actual["unused_am"], 0.03);
            AssertCompatible("MODULATOR", "driven_zero", golden["driven_zero"], actual["driven_zero"], 0.01);
        }

        [LtspiceFact]
        public void SampleHold_UnusedAndGroundedInputs_WhenComparedWithNativeADevice_RemainDistinct()
        {
            string[] sharedNetlist =
            {
                "LTspice SAMPLEHOLD unused and grounded input compatibility",
                "VCOMMON common 0 1",
                "VINP inp common 2",
                "VTRACK track common 1",
                "AUNUSED inp common common track common common out_unused common SAMPLEHOLD Rout=1",
                "AGROUNDED inp 0 common track common common out_grounded common SAMPLEHOLD Rout=1",
                "RUNUSED out_unused common 100k",
                "RGROUNDED out_grounded common 100k",
                ".tran 10n 1u 0 10n UIC",
                ".meas tran unused FIND V(out_unused,common) AT=500n",
                ".meas tran grounded FIND V(out_grounded,common) AT=500n",
                ".end",
            };

            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_samplehold_unused_input",
                sharedNetlist,
                "unused",
                "grounded");
            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            Assert.InRange(golden["unused"], 1.99, 2.01);
            Assert.InRange(golden["grounded"], 2.99, 3.01);
            AssertCompatible("SAMPLEHOLD", "unused", golden["unused"], actual["unused"], 0.02);
            AssertCompatible("SAMPLEHOLD", "grounded", golden["grounded"], actual["grounded"], 0.02);
        }

        [LtspiceFact]
        public void SrFlipFlop_UnusedOutputs_WhenComparedWithNativeADevice_AreDetached()
        {
            string[] qOmitted =
            {
                "LTspice SRFLOP unused Q compatibility",
                "VCOMMON common 0 1",
                "VSET set common 5",
                "VRESET reset common 0",
                "ASR set reset common common common qb common common SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "RLOAD qb common 10k",
                ".tran 1n 20n 0 1n UIC",
                ".meas tran loaded FIND V(qb,common) AT=10n",
                ".meas tran common_current FIND I(VCOMMON) AT=10n",
                ".end",
            };
            string[] qbOmitted =
            {
                "LTspice SRFLOP unused QB compatibility",
                "VCOMMON common 0 1",
                "VSET set common 0",
                "VRESET reset common 5",
                "ASR set reset common common common common q common SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "RLOAD q common 10k",
                ".tran 1n 20n 0 1n UIC",
                ".meas tran loaded FIND V(q,common) AT=10n",
                ".meas tran common_current FIND I(VCOMMON) AT=10n",
                ".end",
            };

            AssertUnusedLatchOutput("q", qOmitted);
            AssertUnusedLatchOutput("qb", qbOmitted);
        }

        internal static IReadOnlyDictionary<string, double> RunNativeExampleMeasurements(
            string caseName,
            IEnumerable<string> netlistLines,
            params string[] measurementNames)
        {
            return RunLtspiceMeasurements(
                caseName,
                netlistLines,
                measurementNames);
        }

        private static void AssertUnusedLatchOutput(string outputName, string[] sharedNetlist)
        {
            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_srflop_unused_" + outputName,
                sharedNetlist,
                "loaded",
                "common_current");
            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                golden.Keys.ToArray());

            Assert.InRange(golden["loaded"], -0.01, 0.01);
            Assert.InRange(Math.Abs(golden["common_current"]), 0.0, 1e-6);
            AssertCompatible("SRFLOP", outputName + "_loaded", golden["loaded"], actual["loaded"], 0.01);
            AssertCompatible(
                "SRFLOP",
                outputName + "_common_current",
                golden["common_current"],
                actual["common_current"],
                1e-6);
        }

        private static void AssertGoldenCase(
            string caseName,
            string device,
            string[] sharedNetlist,
            string[] measurementNames,
            double[] minimums,
            double[] maximums,
            double absoluteTolerance = 0.05)
        {
            Assert.Equal(measurementNames.Length, minimums.Length);
            Assert.Equal(measurementNames.Length, maximums.Length);
            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                caseName,
                sharedNetlist,
                measurementNames);
            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                measurementNames);

            for (int index = 0; index < measurementNames.Length; index++)
            {
                string measurementName = measurementNames[index];
                Assert.InRange(golden[measurementName], minimums[index], maximums[index]);
                AssertCompatible(
                    device,
                    measurementName,
                    golden[measurementName],
                    actual[measurementName],
                    absoluteTolerance);
            }
        }

#pragma warning disable SA1118 // Inline netlist arrays keep each golden case self-contained.
        private static void AssertLongDurationStateCompatibility(bool useIc)
        {
            string suffix = useIc ? "uic" : "op";
            string transient = $".tran 10m 30s 0 10m{(useIc ? " UIC" : string.Empty)}";
            AssertGoldenCase(
                "a_device_long_srflop_" + suffix,
                "LONG_SRFLOP_" + suffix,
                new[]
                {
                    "LTspice SRFLOP long-duration compatibility",
                    "VSET set 0 PULSE(0 1 100m 10m 10m 100m 100)",
                    "VRESET reset 0 0",
                    "ASR set reset 0 0 0 qb q 0 SRFLOP",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    transient,
                    ".meas tran late FIND V(q) AT=29s",
                    ".end",
                },
                new[] { "late" },
                new[] { 0.95 },
                new[] { 1.01 });
            AssertGoldenCase(
                "a_device_long_dflop_" + suffix,
                "LONG_DFLOP_" + suffix,
                new[]
                {
                    "LTspice DFLOP long-duration compatibility",
                    "VPRE pre 0 PULSE(0 1 100m 10m 10m 100m 100)",
                    "ADFF 0 0 0 pre 0 qb q 0 DFLOP",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    transient,
                    ".meas tran late FIND V(q) AT=29s",
                    ".end",
                },
                new[] { "late" },
                new[] { 0.95 },
                new[] { 1.01 });
            AssertGoldenCase(
                "a_device_long_phasedet_" + suffix,
                "LONG_PHASEDET_" + suffix,
                new[]
                {
                    "LTspice PHASEDET long-duration compatibility",
                    "VA a 0 PULSE(0 1 100m 10m 10m 100m 100)",
                    "APD a 0 0 0 0 0 out 0 PHASEDET",
                    "ROUT out 0 1k",
                    transient,
                    ".meas tran late FIND V(out) AT=29s",
                    ".end",
                },
                new[] { "late" },
                new[] { 0.095 },
                new[] { 0.105 });
            AssertGoldenCase(
                "a_device_long_counter_" + suffix,
                "LONG_COUNTER_" + suffix,
                new[]
                {
                    "LTspice COUNTER long-duration compatibility",
                    "VCLK clock 0 PULSE(0 1 1 100m 100m 1 4)",
                    "ACOUNT clock 0 0 0 0 qb q 0 COUNTER cycles=4",
                    "RQ q 0 10k",
                    "RQB qb 0 10k",
                    transient,
                    ".meas tran state_1 FIND V(q) AT=2s",
                    ".meas tran state_2 FIND V(q) AT=6s",
                    ".meas tran state_3 FIND V(q) AT=10s",
                    ".meas tran state_4 FIND V(q) AT=14s",
                    ".meas tran state_5 FIND V(q) AT=18s",
                    ".end",
                },
                new[] { "state_1", "state_2", "state_3", "state_4", "state_5" },
                new[] { 0.95, -0.01, -0.01, 0.95, 0.95 },
                new[] { 1.01, 0.05, 0.05, 1.01, 1.01 });
            AssertGoldenCase(
                "a_device_long_samplehold_" + suffix,
                "LONG_SAMPLEHOLD_" + suffix,
                new[]
                {
                    "LTspice SAMPLEHOLD long-duration compatibility",
                    "VIN in 0 2",
                    "VTRACK track 0 PULSE(1 0 500m 10m 10m 100 200)",
                    "ASH in 0 0 track 0 0 out 0 SAMPLEHOLD",
                    "ROUT out 0 100k",
                    transient,
                    ".meas tran held_1 FIND V(out) AT=1s",
                    ".meas tran held_10 FIND V(out) AT=10s",
                    ".meas tran held_29 FIND V(out) AT=29s",
                    ".end",
                },
                new[] { "held_1", "held_10", "held_29" },
                new[] { 1.95, 1.95, 1.95 },
                new[] { 2.01, 2.01, 2.01 });
        }
#pragma warning restore SA1118

        private static void AssertLongDurationModulatorCompatibility(bool useIc)
        {
            string[] sharedNetlist =
            {
                "LTspice MODULATOR long-duration compatibility",
                "VFM fm 0 0",
                "VAM am 0 1",
                "AMOD fm am 0 0 0 0 out 0 MODULATOR mark=1k space=1k Rout=1",
                "ROUT out 0 100k",
                ".options plotwinsize=0",
                $".tran 100u 5.01s 0 100u{(useIc ? " UIC" : string.Empty)}",
                ".meas tran period_1m TRIG V(out) VAL=0 RISE=1 TD=1m TARG V(out) VAL=0 RISE=2 TD=1m",
                ".meas tran period_100m TRIG V(out) VAL=0 RISE=1 TD=100m TARG V(out) VAL=0 RISE=2 TD=100m",
                ".meas tran period_1s TRIG V(out) VAL=0 RISE=1 TD=1s TARG V(out) VAL=0 RISE=2 TD=1s",
                ".meas tran period_5s TRIG V(out) VAL=0 RISE=1 TD=5s TARG V(out) VAL=0 RISE=2 TD=5s",
                ".end",
            };
            string[] measurementNames =
            {
                "period_1m",
                "period_100m",
                "period_1s",
                "period_5s",
            };
            string suffix = useIc ? "uic" : "op";
            IReadOnlyDictionary<string, double> golden = RunLtspiceMeasurements(
                "a_device_long_modulator_" + suffix,
                sharedNetlist,
                measurementNames);
            IReadOnlyDictionary<string, double> actual = RunSpiceSharpMeasurements(
                sharedNetlist,
                measurementNames);

            foreach (string measurementName in measurementNames)
            {
                Assert.InRange(golden[measurementName], 0.00099, 0.00101);
                AssertCompatible(
                    "LONG_MODULATOR_" + suffix,
                    measurementName,
                    golden[measurementName],
                    actual[measurementName],
                    5e-6,
                    0.002);
            }
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
                + "\\s*(?::\\s*.*?=|=)\\s*(?<value>[-+]?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?)";
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

        internal sealed class LtspiceFactAttribute : FactAttribute
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

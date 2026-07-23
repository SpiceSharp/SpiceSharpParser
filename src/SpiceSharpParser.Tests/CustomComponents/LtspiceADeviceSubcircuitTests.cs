using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.Analog;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class LtspiceADeviceSubcircuitTests
    {
        [Fact]
        public void LoadBuiltIn_SeparatesDigitalAndAnalogADeviceModels()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();

            Assert.Equal(23, digital.Library.Subcircuits.Count);
            Assert.Equal(4, analog.Library.Subcircuits.Count);
            Assert.Equal(
                new[] { "D", "CLK", "PRE", "CLR", "Q", "QB", "VDD", "VSS" },
                digital.Library["DIG_DFF"].Pins);
            Assert.Equal(
                new[] { "A", "B", "OUT", "COM" },
                digital.Library["DIG_PHASE_DETECTOR"].Pins);
            Assert.Equal(
                new[] { "INP", "INN", "CLK", "SH", "OUT", "COM" },
                analog.Library["ANALOG_SAMPLE_HOLD"].Pins);
            Assert.Equal("1k", analog.Library["ANALOG_SAMPLE_HOLD"].DefaultParameters["ROUT"]);
            Assert.Equal("10", analog.Library["ANALOG_SAMPLE_HOLD"].DefaultParameters["VHIGH"]);
            Assert.Equal("-10", analog.Library["ANALOG_SAMPLE_HOLD"].DefaultParameters["VLOW"]);
            Assert.Equal("1", analog.Library["ANALOG_OTA"].DefaultParameters["G"]);
            Assert.Equal("2", digital.Library["DIG_COUNTER"].DefaultParameters["CYCLES"]);
            Assert.Equal("0.5", digital.Library["DIG_COUNTER"].DefaultParameters["DUTY"]);
            Assert.Empty(digital.Library.Diagnostics);
            Assert.Empty(analog.Library.Diagnostics);
        }

        [Fact]
        public void SetResetFlipFlop_InitialConditionStartsHigh()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VSET", "set", "0", 0.0),
                new VoltageSource("VRESET", "reset", "0", 0.0),
                new Resistor("RQ", "q", "0", 10000.0),
                new Resistor("RQB", "qb", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddSetResetFlipFlop(
                circuit,
                "XSR",
                "set",
                "reset",
                "q",
                "qb",
                "vdd",
                "0",
                new Dictionary<string, string> { ["IC"] = "1" });

            Assert.InRange(RunOperatingPoint(circuit, "q"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(circuit, "qb"), 0.0, 0.1);
        }

        [Fact]
        public void DFlipFlop_CapturesOnlyOnRisingEdges()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "D flip-flop sequence",
                "VDD vdd 0 5",
                "VD data 0 PULSE(0 5 5n 100p 100p 30n 100n)",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".TRAN 100p 60n 0 100p UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddDFlipFlop(
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

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");

            Assert.InRange(Nearest(samples, 20e-9).Item2, 4.9, 5.0);
            Assert.InRange(Nearest(samples, 40e-9).Item2, 4.9, 5.0);
            Assert.InRange(Nearest(samples, 58e-9).Item2, 0.0, 0.1);
            Assert.InRange(Nearest(samples, 58e-9).Item3, 4.9, 5.0);
        }

        [Fact]
        public void DFlipFlop_ClearDominatesPreset()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VD", "data", "0", 5.0),
                new VoltageSource("VCLK", "clock", "0", 0.0),
                new VoltageSource("VPRE", "preset", "0", 5.0),
                new VoltageSource("VCLR", "clear", "0", 5.0),
                new Resistor("RQ", "q", "0", 10000.0),
                new Resistor("RQB", "qb", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddDFlipFlop(
                circuit,
                "XDFF",
                "data",
                "clock",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0");

            Assert.InRange(RunOperatingPoint(circuit, "q"), 0.0, 0.1);
            Assert.InRange(RunOperatingPoint(circuit, "qb"), 4.9, 5.0);
        }

        [Fact]
        public void PhaseDetector_SourcesAndSinksUntilMatchingEdge()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Type-II phase detector",
                "VA a 0 PULSE(0 1 10n 100p 100p 2n 50n)",
                "VB b 0 PULSE(0 1 20n 100p 100p 2n 30n)",
                "ROUT out 0 1k",
                ".TRAN 100p 75n 0 100p UIC",
                ".SAVE V(out) V(a)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddPhaseDetector(
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

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(out)", "V(a)");

            Assert.InRange(Nearest(samples, 15e-9).Item2, 0.95, 1.05);
            Assert.InRange(Nearest(samples, 30e-9).Item2, -0.05, 0.05);
            Assert.InRange(Nearest(samples, 55e-9).Item2, -1.05, -0.95);

            Assert.InRange(Nearest(samples, 70e-9).Item2, -0.05, 0.05);
        }

        [Fact]
        public void SampleHold_TracksAndSamplesDifferentialInput()
        {
            var tracking = new Circuit(
                new VoltageSource("VINP", "inp", "0", 2.5),
                new VoltageSource("VINN", "inn", "0", 0.5),
                new VoltageSource("VCLK", "clock", "0", 0.0),
                new VoltageSource("VSH", "sample", "0", 1.0),
                new Resistor("ROUT", "out", "0", 100000.0));
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();
            analog.AddSampleHold(
                tracking,
                "XTRACK",
                "inp",
                "inn",
                "clock",
                "sample",
                "out",
                "0");

            Assert.InRange(RunOperatingPoint(tracking, "out"), 1.98, 2.0);

            var sampled = SpiceNetlistTestHelper.ParseAndRead(
                "Clocked sample and hold",
                "VIN in 0 PWL(0 0 5n 2 15n 2 16n 4 30n 4)",
                "VCLK clock 0 PULSE(0 1 10n 100p 100p 2n 100n)",
                "VSH sample 0 0",
                "ROUT out 0 100k",
                ".TRAN 100p 30n 0 100p UIC",
                ".SAVE V(out) V(in)",
                ".END");
            analog.AddSampleHold(
                sampled.Circuit,
                "XSAMPLE",
                "in",
                "0",
                "clock",
                "sample",
                "out",
                "0");

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(sampled, "V(out)", "V(in)");

            Assert.InRange(Nearest(samples, 14e-9).Item2, 1.95, 2.0);
            Assert.InRange(Nearest(samples, 25e-9).Item2, 1.95, 2.0);
            Assert.InRange(Nearest(samples, 25e-9).Item3, 3.95, 4.05);
        }

        [Fact]
        public void OperationalTransconductanceAmplifier_LinearModeMultipliesInputs()
        {
            var circuit = new Circuit(
                new VoltageSource("VIN1N", "in1n", "0", 0.0),
                new VoltageSource("VIN1P", "in1p", "0", 0.1),
                new VoltageSource("VIN2P", "in2p", "0", 1.0),
                new VoltageSource("VIN2N", "in2n", "0", 0.0),
                new Resistor("ROUT", "out", "0", 10000.0));
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();

            analog.AddOperationalTransconductanceAmplifier(
                circuit,
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
                });

            Assert.InRange(RunOperatingPoint(circuit, "out"), 0.99, 1.01);
            Assert.InRange(RunOperatingPoint(circuit, "rail"), -5.01, -4.99);
        }

        [Fact]
        public void VoltageControlledVaristor_ClampsAtControlMagnitude()
        {
            var circuit = new Circuit(
                new VoltageSource("VCONTROL", "control", "0", 2.0),
                new VoltageSource("VSUPPLY", "supply", "0", 10.0),
                new Resistor("RDRIVE", "supply", "out", 1000.0));
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();

            analog.AddVoltageControlledVaristor(
                circuit,
                "XVAR",
                "control",
                "0",
                "out",
                "0",
                new Dictionary<string, string> { ["RCLAMP"] = "10" });

            Assert.InRange(RunOperatingPoint(circuit, "out"), 2.0, 2.1);
        }

        [Fact]
        public void Modulator_InterpolatesFrequencyAndUsesAmplitudeInput()
        {
            const double frequency = 1500.0;
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Voltage-controlled oscillator",
                "VFM fm 0 0.5",
                "VAM am 0 2",
                "ROUT out 0 100k",
                ".TRAN 1u 400u 0 1u UIC",
                ".SAVE V(out) V(fm)",
                ".END");
            AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();
            analog.AddModulator(
                model.Circuit,
                "XVCO",
                "fm",
                "am",
                "out",
                "0",
                new Dictionary<string, string>
                {
                    ["MARK"] = "2k",
                    ["SPACE"] = "1k",
                });

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(out)", "V(fm)");

            Assert.InRange(
                Nearest(samples, 0.25 / frequency).Item2,
                1.95,
                2.0);
            Assert.InRange(
                Nearest(samples, 0.5 / frequency).Item2,
                -0.05,
                0.05);
        }

        [Fact]
        public void Counter_DividesClockAndValidatesConfiguration()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Divide by four counter",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)",
                "VRESET reset 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".TRAN 100p 42n 0 100p UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddCounter(
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

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");

            Assert.InRange(Nearest(samples, 2e-9).Item2, 4.9, 5.0);
            Assert.InRange(Nearest(samples, 10e-9).Item2, 4.9, 5.0);
            Assert.InRange(Nearest(samples, 20e-9).Item2, 0.0, 0.1);
            Assert.InRange(Nearest(samples, 30e-9).Item2, 0.0, 0.1);
            Assert.InRange(Nearest(samples, 40e-9).Item2, 4.9, 5.0);

            var empty = new Circuit();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => digital.AddCounter(
                    empty,
                    "XBAD",
                    "clock",
                    "reset",
                    "q",
                    "qb",
                    "vdd",
                    "0",
                    1));
            Assert.Empty(empty);
        }

        private static IReadOnlyDictionary<string, string> FastStateParameters()
        {
            return new Dictionary<string, string>
            {
                ["TPD"] = "1n",

                ["RSTATE"] = "1",
                ["CMEM"] = "1p",
            };
        }

        private static double RunOperatingPoint(Circuit circuit, string node)
        {
            var simulation = new OP("op");
            var export = new RealVoltageExport(simulation, node);
            double result = double.NaN;
            foreach (int ignored in simulation.Run(circuit))
            {
                result = export.Value;
            }

            return result;
        }

        private static Tuple<double, double, double> Nearest(
            IEnumerable<Tuple<double, double, double>> samples,
            double time)
        {
            return samples.OrderBy(item => Math.Abs(item.Item1 - time)).First();
        }
    }
}

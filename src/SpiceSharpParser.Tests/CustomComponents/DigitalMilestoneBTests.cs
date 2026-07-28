using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class DigitalMilestoneBTests
    {
        [Fact]
        public void LoadBuiltIn_ExposesMilestoneBDefinitionsPinsAndDefaults()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Equal(27, digital.Library.Subcircuits.Count);
            Assert.Equal(
                new[] { "D", "EN", "PRE", "CLR", "Q", "QB", "VDD", "VSS" },
                digital.Library["DIG_D_LATCH"].Pins);
            Assert.Equal(
                new[] { "T", "CLK", "PRE", "CLR", "Q", "QB", "VDD", "VSS" },
                digital.Library["DIG_TFF"].Pins);
            Assert.Equal(
                new[]
                {
                    "D0", "D1", "D2", "D3", "CLK", "CLR", "OE",
                    "Q0", "Q1", "Q2", "Q3", "VDD", "VSS",
                },
                digital.Library["DIG_REG4"].Pins);
            Assert.Equal(
                new[] { "CLK", "EN", "CLR", "Q0", "Q1", "Q2", "Q3", "CARRY", "VDD", "VSS" },
                digital.Library["DIG_COUNTER4_UP"].Pins);
            Assert.Equal("0", digital.Library["DIG_D_LATCH"].DefaultParameters["PRE_PRIORITY"]);
            Assert.Equal("0", digital.Library["DIG_DFF"].DefaultParameters["PRE_PRIORITY"]);
            Assert.Equal("1T", digital.Library["DIG_REG4"].DefaultParameters["ROFF"]);
            Assert.Equal("0", digital.Library["DIG_COUNTER4_UP"].DefaultParameters["IC"]);
            Assert.Empty(digital.Library.Diagnostics);
        }

        [Fact]
        public void DLatch_TracksWhileEnabledThenHoldsAndClearsAsynchronously()
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "D latch timing",
                "VDD vdd 0 5",
                "VD data 0 PWL(0 0 5n 0 5.1n 5 16n 5 16.1n 0 18n 0 18.1n 5 25n 5 25.1n 0)",
                "VEN enable 0 PULSE(0 5 10n 100p 100p 10n 100n)",
                "VPRE preset 0 0",
                "VCLR clear 0 PULSE(0 5 30n 100p 100p 5n 100n)",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".TRAN 50p 38n 0 50p UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddDLatch(
                model.Circuit,
                "XLATCH",
                "data",
                "enable",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0",
                FastParameters());

            DigitalSample[] samples = RunTransient(model, "q", "qb");

            AssertLogic(samples, 9e-9, 0, expectedHigh: false);
            AssertLogic(samples, 15e-9, 0, expectedHigh: true);
            AssertLogic(samples, 17.5e-9, 0, expectedHigh: false);
            AssertLogic(samples, 20e-9, 0, expectedHigh: true);
            AssertLogic(samples, 28e-9, 0, expectedHigh: true);
            AssertLogic(samples, 34e-9, 0, expectedHigh: false);
            AssertLogic(samples, 34e-9, 1, expectedHigh: true);
        }

        [Fact]
        public void DFlipFlop_TypedPresetPriorityIsDeterministicWhenBothControlsAreHigh()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VD", "data", "0", 0.0),
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
                "0",
                new DigitalSequentialParameters
                {
                    AsynchronousPriority = DigitalAsynchronousPriority.Preset,
                });

            AssertLogicLevel(RunOperatingPoint(circuit, "q"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "qb"), expectedHigh: false);
        }

        [Fact]
        public void ToggleFlipFlop_TogglesOnRisingEdgesAndHoldsWhenToggleIsLow()
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "T flip-flop sequence",
                "VDD vdd 0 5",
                "VT toggle 0 PWL(0 5 55n 5 55.1n 0)",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".TRAN 100p 82n 0 100p UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddToggleFlipFlop(
                model.Circuit,
                "XTFF",
                "toggle",
                "clock",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0",
                FastParameters());

            DigitalSample[] samples = RunTransient(model, "q", "qb");

            AssertLogic(samples, 14e-9, 0, expectedHigh: true);
            AssertLogic(samples, 34e-9, 0, expectedHigh: false);
            AssertLogic(samples, 54e-9, 0, expectedHigh: true);
            AssertLogic(samples, 74e-9, 0, expectedHigh: true);
        }

        [Fact]
        public void Register4_CapturesDisablesOutputsReenablesAndClears()
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "Four-bit register sequence",
                "VDD vdd 0 5",
                "VBIAS bias 0 2.5",
                "VD0 d0 0 0",
                "VD1 d1 0 5",
                "VD2 d2 0 0",
                "VD3 d3 0 5",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 100n)",
                "VCLR clear 0 PULSE(0 5 55n 100p 100p 5n 100n)",
                "VOE oe 0 PULSE(5 0 30n 100p 100p 15n 100n)",
                "RQ0 q0 bias 10k",
                "RQ1 q1 bias 10k",
                "RQ2 q2 bias 10k",
                "RQ3 q3 bias 10k",
                ".TRAN 100p 65n 0 100p UIC",
                ".SAVE V(q0) V(q1) V(q2) V(q3)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            DigitalSequentialParameters parameters = FastParameters(includeDisabledOutput: true);
            parameters.InitialValue = 5;
            digital.AddRegister4(
                model.Circuit,
                "XREG",
                new[] { "d0", "d1", "d2", "d3" },
                "clock",
                "clear",
                "oe",
                new[] { "q0", "q1", "q2", "q3" },
                "vdd",
                "0",
                parameters);

            DigitalSample[] samples = RunTransient(model, "q0", "q1", "q2", "q3");

            AssertWord(samples, 5e-9, 0x5);
            AssertWord(samples, 20e-9, 0xA);
            DigitalSample disabled = Nearest(samples, 40e-9);
            Assert.All(disabled.Values, value => Assert.InRange(value, 2.49, 2.51));
            AssertWord(samples, 50e-9, 0xA);
            AssertWord(samples, 60e-9, 0x0);
        }

        [Fact]
        public void Counter4_RollsOverHoldsWhenDisabledAndReportsCarry()
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "Four-bit counter control",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)",
                "VEN enable 0 PWL(0 5 40n 5 40.1n 0 80n 0 80.1n 5)",
                "VCLR clear 0 PULSE(0 5 100n 100p 100p 5n 200n)",
                "RQ0 q0 0 10k",
                "RQ1 q1 0 10k",
                "RQ2 q2 0 10k",
                "RQ3 q3 0 10k",
                "RCARRY carry 0 10k",
                ".TRAN 100p 108n 0 100p UIC",
                ".SAVE V(q0) V(q1) V(q2) V(q3) V(carry)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            DigitalSequentialParameters parameters = FastParameters();
            parameters.InitialValue = 14;
            digital.AddCounter4(
                model.Circuit,
                "XCOUNT",
                "clock",
                "enable",
                "clear",
                new[] { "q0", "q1", "q2", "q3" },
                "carry",
                "vdd",
                "0",
                parameters);

            DigitalSample[] samples = RunTransient(model, "q0", "q1", "q2", "q3", "carry");

            AssertWord(samples, 5e-9, 14);
            AssertWord(samples, 16e-9, 15);
            AssertLogic(samples, 16e-9, 4, expectedHigh: true);
            AssertWord(samples, 36e-9, 0);
            AssertWord(samples, 76e-9, 0);
            AssertWord(samples, 96e-9, 1);
            AssertWord(samples, 104e-9, 0);
        }

        [Fact]
        public void Counter4_DefaultParametersProduceTheGoldenModulo16Sequence()
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "Default four-bit counter",
                "VDD vdd 0 5",
                "VCLK clock 0 PULSE(0 5 20n 100p 100p 20n 100n)",
                "VEN enable 0 5",
                "VCLR clear 0 0",
                "RQ0 q0 0 10k",
                "RQ1 q1 0 10k",
                "RQ2 q2 0 10k",
                "RQ3 q3 0 10k",
                "RCARRY carry 0 10k",
                ".TRAN 500p 1.58u 0 500p UIC",
                ".SAVE V(q0) V(q1) V(q2) V(q3) V(carry)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddCounter4(
                model.Circuit,
                "XCOUNT_DEFAULT",
                "clock",
                "enable",
                "clear",
                new[] { "q0", "q1", "q2", "q3" },
                "carry",
                "vdd",
                "0");

            DigitalSample[] samples = RunTransient(model, "q0", "q1", "q2", "q3", "carry");

            for (int edge = 0; edge < 16; edge++)
            {
                double sampleTime = 50e-9 + (edge * 100e-9);
                AssertWord(samples, sampleTime, (edge + 1) & 0xF);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void DLatch_HoldsLoadedStateForThirtySeconds(bool useIc)
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "Long D-latch retention",
                "VDD vdd 0 5",
                "VD data 0 5",
                "VEN enable 0 PULSE(0 5 10m 1m 1m 1 100)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                $".TRAN 10m 30s{(useIc ? " UIC" : string.Empty)}",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddDLatch(
                model.Circuit,
                "XLONG",
                "data",
                "enable",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0",
                new DigitalSequentialParameters { PropagationDelay = 1.0 });

            DigitalSample[] samples = RunTransient(model, "q", "qb");

            AssertLogic(samples, 29.9, 0, expectedHigh: true);
            AssertLogic(samples, 29.9, 1, expectedHigh: false);
        }

        [Theory]
        [InlineData(0.2, "1p", "100p")]
        [InlineData(0.5, "100p", "1n")]
        [InlineData(0.8, "100n", "100n")]
        public void ToggleFlipFlop_AdvancesOnceAcrossThresholdRiseAndStepChanges(
            double threshold,
            string riseTime,
            string maximumStep)
        {
            SpiceSharpModel model = SpiceNetlistTestHelper.ParseAndRead(
                "T flip-flop edge independence",
                "VDD vdd 0 5",
                "VT toggle 0 5",
                $"VCLK clock 0 PULSE(0 5 100n {riseTime} {riseTime} 200n 500n)",
                "VPRE preset 0 0",
                "VCLR clear 0 0",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                $".TRAN {maximumStep} 950n 0 {maximumStep} UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddToggleFlipFlop(
                model.Circuit,
                "XTFF_EDGE",
                "toggle",
                "clock",
                "preset",
                "clear",
                "q",
                "qb",
                "vdd",
                "0",
                new DigitalSequentialParameters
                {
                    LogicThresholdRatio = threshold,
                });

            DigitalSample[] samples = RunTransient(model, "q", "qb");

            AssertLogic(samples, 450e-9, 0, expectedHigh: true);
            AssertLogic(samples, 900e-9, 0, expectedHigh: false);
        }

        [Fact]
        public void TypedParametersRejectInvalidValuesBeforeCircuitMutation()
        {
            var circuit = new Circuit();
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Throws<ArgumentOutOfRangeException>(() => digital.AddDLatch(
                circuit,
                "XBAD_LATCH",
                "d",
                "en",
                "pre",
                "clr",
                "q",
                "qb",
                "vdd",
                "0",
                new DigitalSequentialParameters { LogicThresholdRatio = 1.0 }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentException>(() => digital.AddRegister4(
                circuit,
                "XBAD_DEFAULT_OFF",
                new[] { "d0", "d1", "d2", "d3" },
                "clk",
                "clr",
                "oe",
                new[] { "q0", "q1", "q2", "q3" },
                "vdd",
                "0",
                new DigitalSequentialParameters { OutputResistance = 2.0e12 }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentOutOfRangeException>(() => digital.AddCounter4(
                circuit,
                "XBAD_COUNT",
                "clk",
                "en",
                "clr",
                new[] { "q0", "q1", "q2", "q3" },
                "carry",
                "vdd",
                "0",
                new DigitalSequentialParameters { InitialValue = 16 }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentException>(() => digital.AddRegister4(
                circuit,
                "XBAD_REG",
                new[] { "d0", "d1", "d2", "d3" },
                "clk",
                "clr",
                "oe",
                new[] { "q0", "q1", "q2", "q3" },
                "vdd",
                "0",
                new DigitalSequentialParameters
                {
                    OutputResistance = 100.0,
                    DisabledOutputResistance = 100.0,
                }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentException>(() => digital.AddCounter4(
                circuit,
                "XBAD_PRIORITY",
                "clk",
                "en",
                "clr",
                new[] { "q0", "q1", "q2", "q3" },
                "carry",
                "vdd",
                "0",
                new DigitalSequentialParameters
                {
                    AsynchronousPriority = DigitalAsynchronousPriority.Preset,
                }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentException>(() => digital.AddCounter4(
                circuit,
                "XBAD_NODES",
                "clk",
                "en",
                "clr",
                new[] { "q0", "q1", "q2" },
                "carry",
                "vdd",
                "0"));
            Assert.Empty(circuit);
        }

        private static DigitalSequentialParameters FastParameters(bool includeDisabledOutput = false)
        {
            return new DigitalSequentialParameters
            {
                PropagationDelay = 1e-9,
                OutputCapacitance = 1e-15,
                StateResistance = 1e-3,
                StateCapacitance = 1e-12,
                DisabledOutputResistance = includeDisabledOutput ? 1e12 : (double?)null,
            };
        }

        private static DigitalSample[] RunTransient(SpiceSharpModel model, params string[] nodes)
        {
            var simulation = model.Simulations.Single(item => item is Transient);
            var transient = (Transient)simulation;
            RealVoltageExport[] exports = nodes
                .Select(node => new RealVoltageExport(transient, node))
                .ToArray();
            var samples = new List<DigitalSample>();
            simulation.EventExportData += (sender, args) => samples.Add(
                new DigitalSample(
                    transient.Time,
                    exports.Select(export => export.Value).ToArray()));

            foreach (int ignored in simulation.InvokeEvents(simulation.Run(model.Circuit, -1)))
            {
            }

            return samples.ToArray();
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

        private static void AssertWord(
            IEnumerable<DigitalSample> samples,
            double time,
            int expected)
        {
            DigitalSample sample = Nearest(samples, time);
            int actual = 0;
            for (int bit = 0; bit < 4; bit++)
            {
                if (sample.Values[bit] > 2.5)
                {
                    actual |= 1 << bit;
                }
            }

            Assert.Equal(expected, actual);
        }

        private static void AssertLogic(
            IEnumerable<DigitalSample> samples,
            double time,
            int valueIndex,
            bool expectedHigh)
        {
            AssertLogicLevel(Nearest(samples, time).Values[valueIndex], expectedHigh);
        }

        private static void AssertLogicLevel(double value, bool expectedHigh)
        {
            if (expectedHigh)
            {
                Assert.InRange(value, 4.9, 5.0);
            }
            else
            {
                Assert.InRange(value, -0.02, 0.1);
            }
        }

        private static DigitalSample Nearest(
            IEnumerable<DigitalSample> samples,
            double time)
        {
            return samples.OrderBy(sample => Math.Abs(sample.Time - time)).First();
        }

        private sealed class DigitalSample
        {
            public DigitalSample(double time, double[] values)
            {
                this.Time = time;
                this.Values = values;
            }

            public double Time { get; }

            public double[] Values { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Analysis;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.Testing;
using SpiceSharpParser.Validation;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class DigitalMilestoneATests
    {
        [Fact]
        public void LoadBuiltIn_ExposesMilestoneADefinitionsAndPins()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Equal(23, digital.Library.Subcircuits.Count);
            Assert.Equal(
                new[] { "A", "Y", "VDD", "VSS" },
                digital.Library["DIG_SCHMITT_BUF"].Pins);
            Assert.Equal("0.65", digital.Library["DIG_SCHMITT_BUF"].DefaultParameters["VTH_RISE"]);
            Assert.Equal("0.35", digital.Library["DIG_SCHMITT_BUF"].DefaultParameters["VTH_FALL"]);
            Assert.Equal(
                new[] { "A", "OE", "Y", "VDD", "VSS" },
                digital.Library["DIG_TRI_BUF"].Pins);
            Assert.Equal("1T", digital.Library["DIG_TRI_BUF"].DefaultParameters["ROFF"]);
            Assert.Equal(
                new[] { "D0", "D1", "S", "Y", "VDD", "VSS" },
                digital.Library["DIG_MUX2"].Pins);
            Assert.Equal(
                new[] { "D0", "D1", "D2", "D3", "S0", "S1", "Y", "VDD", "VSS" },
                digital.Library["DIG_MUX4"].Pins);
            Assert.Equal(
                new[] { "A", "B", "CIN", "SUM", "COUT", "VDD", "VSS" },
                digital.Library["DIG_FULL_ADDER"].Pins);
            Assert.Equal(
                new[] { "A", "B", "EN", "Y0", "Y1", "Y2", "Y3", "VDD", "VSS" },
                digital.Library["DIG_DEC2TO4"].Pins);
            Assert.Empty(digital.Library.Diagnostics);
        }

        [Fact]
        public void SchmittBuffer_UsesSeparateRisingAndFallingThresholdsAndRetainsState()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Schmitt hysteresis sequence",
                "VDD vdd 0 5",
                "VIN input 0 PWL(0 0 100n 0 1.1u 5 1.3u 5 1.6u 2.4 1.9u 2.8 2.2u 2.3 2.5u 2.7 2.8u 5 3u 5 4u 0)",
                "RLOAD output 0 10k",
                ".OPTIONS method=gear",
                ".TRAN 1n 4u 0 1n UIC",
                ".SAVE V(input) V(output)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(
                model.Circuit,
                "DIG_SCHMITT_BUF",
                "XSCHMITT",
                "input",
                "output",
                "vdd",
                "0");

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(input)", "V(output)");
            Tuple<double, double, double> rising = FirstRisingCrossing(samples, 2.5);
            Tuple<double, double, double> falling = FirstFallingCrossing(samples, 2.5);
            Tuple<double, double, double> insideBand = NearestSample(samples, 2.2e-6);

            Assert.InRange(rising.Item2, 3.2, 3.4);
            Assert.InRange(falling.Item2, 1.6, 1.8);
            Assert.InRange(insideBand.Item3, 4.9, 5.0);
            Assert.Equal(1, CountCrossings(samples, rising: true, threshold: 2.5));
            Assert.Equal(1, CountCrossings(samples, rising: false, threshold: 2.5));
        }

        [Fact]
        public void SchmittInverter_ComplementsBufferAtStableInputs()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VLOW", "low", "0", 0.0),
                new VoltageSource("VHIGH", "high", "0", 5.0),
                new Resistor("RLOW", "ylow", "0", 10000.0),
                new Resistor("RHIGH", "yhigh", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(circuit, "DIG_SCHMITT_NOT", "XLOW", "low", "ylow", "vdd", "0");
            digital.Library.AddInstance(circuit, "DIG_SCHMITT_NOT", "XHIGH", "high", "yhigh", "vdd", "0");

            Assert.InRange(RunOperatingPoint(circuit, "ylow"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(circuit, "yhigh"), 0.0, 0.1);
        }

        [Fact]
        public void TriStateBufferAndInverter_DriveCorrectEnabledLevels()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            foreach (bool inputHigh in new[] { false, true })
            {
                var circuit = new Circuit(
                    new VoltageSource("VDD", "vdd", "0", 5.0),
                    new VoltageSource("VIN", "input", "0", inputHigh ? 5.0 : 0.0),
                    new VoltageSource("VOE", "oe", "0", 5.0),
                    new Resistor("RBUF", "buffer", "0", 10000.0),
                    new Resistor("RNOT", "inverter", "0", 10000.0));
                digital.Library.AddInstance(circuit, "DIG_TRI_BUF", "XBUF", "input", "oe", "buffer", "vdd", "0");
                digital.Library.AddInstance(circuit, "DIG_TRI_NOT", "XNOT", "input", "oe", "inverter", "vdd", "0");

                AssertLogicLevel(RunOperatingPoint(circuit, "buffer"), inputHigh);
                AssertLogicLevel(RunOperatingPoint(circuit, "inverter"), !inputHigh);
            }
        }

        [Fact]
        public void TriStateBuffer_WhenDisabledFollowsExternalBias()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VIN", "input", "0", 5.0),
                new VoltageSource("VOE", "oe", "0", 0.0),
                new Resistor("RPULLUP", "vdd", "pulled-up", 10000.0),
                new Resistor("RPULLDOWN", "pulled-down", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(circuit, "DIG_TRI_BUF", "XUP", "input", "oe", "pulled-up", "vdd", "0");
            digital.Library.AddInstance(circuit, "DIG_TRI_BUF", "XDOWN", "input", "oe", "pulled-down", "vdd", "0");

            Assert.InRange(RunOperatingPoint(circuit, "pulled-up"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(circuit, "pulled-down"), 0.0, 0.1);
        }

        [Fact]
        public void TriStateBuffer_OpposingEnabledDriversProduceFiniteContentionLevel()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VHIGH", "high", "0", 5.0),
                new VoltageSource("VLOW", "low", "0", 0.0),
                new VoltageSource("VOE", "oe", "0", 5.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(circuit, "DIG_TRI_BUF", "XHIGH", "high", "oe", "bus", "vdd", "0");
            digital.Library.AddInstance(circuit, "DIG_TRI_BUF", "XLOW", "low", "oe", "bus", "vdd", "0");

            Assert.InRange(RunOperatingPoint(circuit, "bus"), 2.4, 2.6);
        }

        [Fact]
        public void TriStateBuffer_AppliesConfiguredEnableDelay()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Tri-state enable timing",
                "VDD vdd 0 5",
                "VIN input 0 5",
                "VOE oe 0 PULSE(0 5 50n 100p 100p 100n 300n)",
                "RLOAD output 0 10k",
                ".TRAN 100p 200n 0 100p",
                ".SAVE V(oe) V(output)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(
                model.Circuit,
                "DIG_TRI_BUF",
                "XTRI",
                new[] { "input", "oe", "output", "vdd", "0" },
                new Dictionary<string, string>
                {
                    ["TPD"] = "10n",
                    ["RON"] = "10",
                    ["COUT"] = "1f",
                });

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(oe)", "V(output)");
            double enableCrossing = FirstInputRisingCrossing(samples, 2.5).Item1;
            double outputCrossing = FirstOutputRisingCrossing(samples, 2.5).Item1;

            Assert.InRange(outputCrossing - enableCrossing, 9.5e-9, 10.8e-9);
        }

        [Fact]
        public void Multiplexer2_SelectsRequestedInputForEveryCombination()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            for (int d0 = 0; d0 <= 1; d0++)
            {
                for (int d1 = 0; d1 <= 1; d1++)
                {
                    for (int select = 0; select <= 1; select++)
                    {
                        Circuit circuit = CreateLogicCircuit(
                            ("VD0", "d0", d0),
                            ("VD1", "d1", d1),
                            ("VS", "select", select));
                        digital.Library.AddInstance(circuit, "DIG_MUX2", "XMUX", "d0", "d1", "select", "output", "vdd", "0");

                        AssertLogicLevel(
                            RunOperatingPoint(circuit, "output"),
                            (select == 0 ? d0 : d1) == 1);
                    }
                }
            }
        }

        [Fact]
        public void Multiplexer4_UsesS0AsLeastSignificantSelectBit()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            for (int selected = 0; selected < 4; selected++)
            {
                for (int highInput = 0; highInput < 4; highInput++)
                {
                    Circuit circuit = CreateLogicCircuit(
                        ("VD0", "d0", highInput == 0 ? 1 : 0),
                        ("VD1", "d1", highInput == 1 ? 1 : 0),
                        ("VD2", "d2", highInput == 2 ? 1 : 0),
                        ("VD3", "d3", highInput == 3 ? 1 : 0),
                        ("VS0", "s0", selected & 1),
                        ("VS1", "s1", (selected >> 1) & 1));
                    digital.Library.AddInstance(
                        circuit,
                        "DIG_MUX4",
                        "XMUX",
                        "d0",
                        "d1",
                        "d2",
                        "d3",
                        "s0",
                        "s1",
                        "output",
                        "vdd",
                        "0");

                    AssertLogicLevel(RunOperatingPoint(circuit, "output"), selected == highInput);
                }
            }
        }

        [Fact]
        public void FullAdder_MatchesAllEightInputCombinations()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            for (int value = 0; value < 8; value++)
            {
                int a = value & 1;
                int b = (value >> 1) & 1;
                int carryIn = (value >> 2) & 1;
                Circuit circuit = CreateLogicCircuit(
                    ("VA", "a", a),
                    ("VB", "b", b),
                    ("VCIN", "carry-in", carryIn));
                circuit.Add(new Resistor("RCARRY", "carry-out", "0", 10000.0));
                digital.Library.AddInstance(
                    circuit,
                    "DIG_FULL_ADDER",
                    "XADD",
                    "a",
                    "b",
                    "carry-in",
                    "output",
                    "carry-out",
                    "vdd",
                    "0");

                int total = a + b + carryIn;
                AssertLogicLevel(RunOperatingPoint(circuit, "output"), (total & 1) == 1);
                AssertLogicLevel(RunOperatingPoint(circuit, "carry-out"), total >= 2);
            }
        }

        [Fact]
        public void Decoder2To4_ProducesOneHotOutputOnlyWhenEnabled()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            for (int enabled = 0; enabled <= 1; enabled++)
            {
                for (int address = 0; address < 4; address++)
                {
                    Circuit circuit = CreateLogicCircuit(
                        ("VA", "a", address & 1),
                        ("VB", "b", (address >> 1) & 1),
                        ("VEN", "enable", enabled));
                    circuit.Add(new Resistor("RY1", "y1", "0", 10000.0));
                    circuit.Add(new Resistor("RY2", "y2", "0", 10000.0));
                    circuit.Add(new Resistor("RY3", "y3", "0", 10000.0));
                    digital.Library.AddInstance(
                        circuit,
                        "DIG_DEC2TO4",
                        "XDEC",
                        "a",
                        "b",
                        "enable",
                        "output",
                        "y1",
                        "y2",
                        "y3",
                        "vdd",
                        "0");

                    string[] outputs = { "output", "y1", "y2", "y3" };
                    for (int output = 0; output < outputs.Length; output++)
                    {
                        AssertLogicLevel(
                            RunOperatingPoint(circuit, outputs[output]),
                            enabled == 1 && output == address);
                    }
                }
            }
        }

        [Fact]
        public void MilestoneA_StaticFixturePassesSmokeAndLintChecks()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Milestone A static fixture",
                "VDD vdd 0 5",
                "VHIGH high 0 5",
                "VLOW low 0 0",
                "ROUT output 0 10k",
                ".OP",
                ".SAVE V(output)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.Library.AddInstance(model.Circuit, "DIG_SCHMITT_BUF", "XSCH", "high", "schmitt", "vdd", "0");
            digital.Library.AddInstance(model.Circuit, "DIG_MUX2", "XMUX", "low", "schmitt", "high", "mux", "vdd", "0");
            digital.Library.AddInstance(model.Circuit, "DIG_TRI_BUF", "XTRI", "mux", "high", "output", "vdd", "0");

            LintResult lint = NetlistLinter.Lint(model);
            Assert.False(lint.HasErrors, lint.ToString());
            SmokeTestResult smoke = SmokeTester.QuickCheck(model);
            Assert.True(smoke.IsPass, smoke.DiagnosticSummary());
        }

        [Fact]
        public void MilestoneA_FacadeMethodsAddEveryNewComponent()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VHIGH", "high", "0", 5.0),
                new VoltageSource("VLOW", "low", "0", 0.0),
                new Resistor("RSCHBUF", "schmitt-buffer", "0", 10000.0),
                new Resistor("RSCHNOT", "schmitt-inverter", "0", 10000.0),
                new Resistor("RTRIBUF", "tri-buffer", "0", 10000.0),
                new Resistor("RTRINOT", "tri-inverter", "0", 10000.0),
                new Resistor("RMUX2", "mux2", "0", 10000.0),
                new Resistor("RMUX4", "mux4", "0", 10000.0),
                new Resistor("RSUM", "sum", "0", 10000.0),
                new Resistor("RCARRY", "carry", "0", 10000.0),
                new Resistor("RY0", "y0", "0", 10000.0),
                new Resistor("RY1", "y1", "0", 10000.0),
                new Resistor("RY2", "y2", "0", 10000.0),
                new Resistor("RY3", "y3", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddSchmittBuffer(circuit, "XSB", "high", "schmitt-buffer", "vdd", "0");
            digital.AddSchmittInverter(circuit, "XSI", "high", "schmitt-inverter", "vdd", "0");
            digital.AddTriStateBuffer(circuit, "XTB", "high", "high", "tri-buffer", "vdd", "0");
            digital.AddTriStateInverter(circuit, "XTI", "high", "high", "tri-inverter", "vdd", "0");
            digital.AddMultiplexer2(circuit, "XM2", "low", "high", "high", "mux2", "vdd", "0");
            digital.AddMultiplexer4(
                circuit,
                "XM4",
                "low",
                "low",
                "high",
                "low",
                "low",
                "high",
                "mux4",
                "vdd",
                "0");
            digital.AddFullAdder(circuit, "XFA", "high", "high", "low", "sum", "carry", "vdd", "0");
            digital.AddDecoder2To4(
                circuit,
                "XDEC",
                "low",
                "high",
                "high",
                "y0",
                "y1",
                "y2",
                "y3",
                "vdd",
                "0");

            AssertLogicLevel(RunOperatingPoint(circuit, "schmitt-buffer"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "schmitt-inverter"), expectedHigh: false);
            AssertLogicLevel(RunOperatingPoint(circuit, "tri-buffer"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "tri-inverter"), expectedHigh: false);
            AssertLogicLevel(RunOperatingPoint(circuit, "mux2"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "mux4"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "sum"), expectedHigh: false);
            AssertLogicLevel(RunOperatingPoint(circuit, "carry"), expectedHigh: true);
            AssertLogicLevel(RunOperatingPoint(circuit, "y2"), expectedHigh: true);
        }

        [Fact]
        public void MilestoneA_TypedParametersRejectInvalidRelationshipsBeforeMutation()
        {
            var circuit = new Circuit();
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Throws<ArgumentException>(
                () => digital.AddSchmittBuffer(
                    circuit,
                    "XSCH",
                    "input",
                    "output",
                    "vdd",
                    "0",
                    new DigitalSchmittParameters
                    {
                        RisingThresholdRatio = 0.3,
                        FallingThresholdRatio = 0.4,
                    }));
            Assert.Empty(circuit);

            Assert.Throws<ArgumentException>(
                () => digital.AddTriStateBuffer(
                    circuit,
                    "XTRI",
                    "input",
                    "enable",
                    "output",
                    "vdd",
                    "0",
                    new DigitalTriStateParameters
                    {
                        OnResistance = 100.0,
                        OffResistance = 100.0,
                    }));
            Assert.Empty(circuit);
        }

        [Fact]
        public void MilestoneA_TypedParameterOverridesChangeElectricalBehavior()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VIN", "input", "0", 3.0),
                new VoltageSource("VOE", "enable", "0", 0.0),
                new Resistor("RSCHDEFAULT", "schmitt-default", "0", 10000.0),
                new Resistor("RSCHOVERRIDE", "schmitt-override", "0", 10000.0),
                new Resistor("RTRIDEFAULT", "vdd", "tri-default", 10000.0),
                new Resistor("RTRIOVERRIDE", "vdd", "tri-override", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddSchmittBuffer(
                circuit,
                "XSCHDEFAULT",
                "input",
                "schmitt-default",
                "vdd",
                "0");
            digital.AddSchmittBuffer(
                circuit,
                "XSCHOVERRIDE",
                "input",
                "schmitt-override",
                "vdd",
                "0",
                new DigitalSchmittParameters
                {
                    RisingThresholdRatio = 0.55,
                    FallingThresholdRatio = 0.25,
                });
            digital.AddTriStateBuffer(
                circuit,
                "XTRIDEFAULT",
                "input",
                "enable",
                "tri-default",
                "vdd",
                "0");
            digital.AddTriStateBuffer(
                circuit,
                "XTRIOVERRIDE",
                "input",
                "enable",
                "tri-override",
                "vdd",
                "0",
                new DigitalTriStateParameters
                {
                    OnResistance = 50.0,
                    OffResistance = 10000.0,
                });

            AssertLogicLevel(RunOperatingPoint(circuit, "schmitt-default"), expectedHigh: false);
            AssertLogicLevel(RunOperatingPoint(circuit, "schmitt-override"), expectedHigh: true);
            Assert.InRange(RunOperatingPoint(circuit, "tri-default"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(circuit, "tri-override"), 2.49, 2.51);
        }

        private static Circuit CreateLogicCircuit(params (string Name, string Node, int Value)[] inputs)
        {
            var entities = new List<IEntity>
            {
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new Resistor("RLOAD", "output", "0", 10000.0),
            };
            entities.AddRange(inputs.Select(input =>
                (IEntity)new VoltageSource(input.Name, input.Node, "0", input.Value == 1 ? 5.0 : 0.0)));
            return new Circuit(entities.ToArray());
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

        private static void AssertLogicLevel(double value, bool expectedHigh)
        {
            if (expectedHigh)
            {
                Assert.InRange(value, 4.9, 5.0);
            }
            else
            {
                Assert.InRange(value, 0.0, 0.1);
            }
        }

        private static Tuple<double, double, double> FirstRisingCrossing(
            IEnumerable<Tuple<double, double, double>> samples,
            double threshold)
        {
            Tuple<double, double, double>[] values = samples.ToArray();
            for (int index = 1; index < values.Length; index++)
            {
                if (values[index - 1].Item3 < threshold && values[index].Item3 >= threshold)
                {
                    return values[index];
                }
            }

            throw new InvalidOperationException("No rising output crossing was found.");
        }

        private static Tuple<double, double, double> FirstOutputRisingCrossing(
            IEnumerable<Tuple<double, double, double>> samples,
            double threshold)
        {
            return FirstRisingCrossing(samples, threshold);
        }

        private static Tuple<double, double, double> FirstInputRisingCrossing(
            IEnumerable<Tuple<double, double, double>> samples,
            double threshold)
        {
            Tuple<double, double, double>[] values = samples.ToArray();
            for (int index = 1; index < values.Length; index++)
            {
                if (values[index - 1].Item2 < threshold && values[index].Item2 >= threshold)
                {
                    return values[index];
                }
            }

            throw new InvalidOperationException("No rising input crossing was found.");
        }

        private static Tuple<double, double, double> FirstFallingCrossing(
            IEnumerable<Tuple<double, double, double>> samples,
            double threshold)
        {
            Tuple<double, double, double>[] values = samples.ToArray();
            for (int index = 1; index < values.Length; index++)
            {
                if (values[index - 1].Item3 > threshold && values[index].Item3 <= threshold)
                {
                    return values[index];
                }
            }

            throw new InvalidOperationException("No falling output crossing was found.");
        }

        private static int CountCrossings(
            IEnumerable<Tuple<double, double, double>> samples,
            bool rising,
            double threshold)
        {
            Tuple<double, double, double>[] values = samples.ToArray();
            int result = 0;
            for (int index = 1; index < values.Length; index++)
            {
                double previous = values[index - 1].Item3;
                double current = values[index].Item3;
                if (rising
                    ? previous < threshold && current >= threshold
                    : previous > threshold && current <= threshold)
                {
                    result++;
                }
            }

            return result;
        }

        private static Tuple<double, double, double> NearestSample(
            IEnumerable<Tuple<double, double, double>> samples,
            double time)
        {
            return samples.OrderBy(item => Math.Abs(item.Item1 - time)).First();
        }
    }
}

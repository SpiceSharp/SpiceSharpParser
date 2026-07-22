using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class DigitalSubcircuitLibraryTests
    {
        public static IEnumerable<object[]> TruthTableCases()
        {
            yield return Case(DigitalGateKind.Buffer, false, false, false);
            yield return Case(DigitalGateKind.Buffer, true, false, true);
            yield return Case(DigitalGateKind.Inverter, false, false, true);
            yield return Case(DigitalGateKind.Inverter, true, false, false);

            foreach (DigitalGateKind kind in Enum.GetValues(typeof(DigitalGateKind)))
            {
                if (kind == DigitalGateKind.Buffer || kind == DigitalGateKind.Inverter)
                {
                    continue;
                }

                for (int first = 0; first <= 1; first++)
                {
                    for (int second = 0; second <= 1; second++)
                    {
                        bool firstHigh = first == 1;
                        bool secondHigh = second == 1;
                        yield return Case(
                            kind,
                            firstHigh,
                            secondHigh,
                            Expected(kind, firstHigh, secondHigh));
                    }
                }
            }
        }

        [Fact]
        public void LoadBuiltIn_ExposesDocumentedGateDefinitionsAndParameters()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Equal(8, digital.Library.Subcircuits.Count);
            SpiceSubcircuitInfo inverter = digital.Library["DIG_NOT"];
            Assert.Equal(new[] { "A", "Y", "VDD", "VSS" }, inverter.Pins);
            Assert.Equal("0.5", inverter.DefaultParameters["VTH"]);
            Assert.Equal("10n", inverter.DefaultParameters["TPD"]);
            Assert.Equal("1G", inverter.DefaultParameters["RIN"]);
            Assert.Equal("50", inverter.DefaultParameters["ROUT"]);
            Assert.Equal("5p", inverter.DefaultParameters["COUT"]);
            Assert.Empty(digital.Library.Diagnostics);
        }

        [Theory]
        [MemberData(nameof(TruthTableCases))]
        public void BuiltInGates_MatchBooleanTruthTables(
            DigitalGateKind kind,
            bool firstInputHigh,
            bool secondInputHigh,
            bool expectedHigh)
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VA", "a", "0", firstInputHigh ? 5.0 : 0.0),
                new VoltageSource("VB", "b", "0", secondInputHigh ? 5.0 : 0.0),
                new Resistor("RLOAD", "y", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            if (kind == DigitalGateKind.Buffer || kind == DigitalGateKind.Inverter)
            {
                digital.AddGate(circuit, kind, "XU1", new[] { "a" }, "y", "vdd", "0");
            }
            else
            {
                digital.AddBinaryGate(circuit, kind, "XU1", "a", "b", "y", "vdd", "0");
            }

            double output = RunOperatingPoint(circuit, "y");

            if (expectedHigh)
            {
                Assert.InRange(output, 4.9, 5.0);
            }
            else
            {
                Assert.InRange(output, -1e-9, 0.1);
            }
        }

        [Fact]
        public void AddBuffer_PerInstanceThresholdOverrideChangesLogicDecision()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VIN", "in", "0", 3.0),
                new Resistor("RDEFAULT", "default", "0", 10000.0),
                new Resistor("ROVERRIDE", "override", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddBuffer(circuit, "XDEFAULT", "in", "default", "vdd", "0");
            digital.AddBuffer(
                circuit,
                "XOVERRIDE",
                "in",
                "override",
                "vdd",
                "0",
                new DigitalGateParameters
                {
                    LogicThresholdRatio = 0.7,
                });

            double defaultOutput = RunOperatingPoint(circuit, "default");
            double overriddenOutput = RunOperatingPoint(circuit, "override");

            Assert.InRange(defaultOutput, 4.9, 5.0);
            Assert.InRange(overriddenOutput, -1e-9, 0.1);
        }

        [Fact]
        public void AddBuffer_MixesWithCustomEnabledParsedCircuitAndAppliesPropagationDelay()
        {
            var options = new SpiceNetlistTestOptions
            {
                UseCustomComponents = true,
            };
            var model = SpiceNetlistTestHelper.ParseAndRead(
                options,
                "Digital subcircuit mixed with parser custom components",
                "VDD vdd 0 5",
                "VIN in 0 PULSE(0 5 1n 100p 100p 20n 40n)",
                "CLOAD out 0 Q=20p*x",
                "RLOAD out 0 100k",
                ".TRAN 100p 20n 0 100p",
                ".SAVE V(in) V(out)",
                ".END");
            Assert.False(model.ValidationResult.HasError);
            Assert.IsType<NonlinearCapacitor>(model.Circuit["CLOAD"]);

            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddBuffer(
                model.Circuit,
                "XBUF",
                "in",
                "out",
                "vdd",
                "0",
                new DigitalGateParameters
                {
                    PropagationDelay = 5e-9,
                    OutputResistance = 10.0,
                    OutputCapacitance = 1e-12,
                });

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(in)", "V(out)");
            double inputCrossing = FindRisingCrossing(samples, item => item.Item2, 2.5);
            double outputCrossing = FindRisingCrossing(samples, item => item.Item3, 2.5);

            Assert.InRange(inputCrossing, 1.0e-9, 1.2e-9);
            Assert.InRange(outputCrossing - inputCrossing, 4.8e-9, 5.4e-9);
        }

        [Fact]
        public void AddGate_RejectsInvalidElectricalParametersBeforeMutatingCircuit()
        {
            var circuit = new Circuit();
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => digital.AddBuffer(
                    circuit,
                    "X1",
                    "in",
                    "out",
                    "vdd",
                    "0",
                    new DigitalGateParameters
                    {
                        LogicThresholdRatio = 1.0,
                    }));
            Assert.Empty(circuit);
        }

        private static object[] Case(
            DigitalGateKind kind,
            bool firstInputHigh,
            bool secondInputHigh,
            bool expectedHigh)
        {
            return new object[] { kind, firstInputHigh, secondInputHigh, expectedHigh };
        }

        private static bool Expected(
            DigitalGateKind kind,
            bool firstInputHigh,
            bool secondInputHigh)
        {
            switch (kind)
            {
                case DigitalGateKind.And2:
                    return firstInputHigh && secondInputHigh;
                case DigitalGateKind.Nand2:
                    return !(firstInputHigh && secondInputHigh);
                case DigitalGateKind.Or2:
                    return firstInputHigh || secondInputHigh;
                case DigitalGateKind.Nor2:
                    return !(firstInputHigh || secondInputHigh);
                case DigitalGateKind.Xor2:
                    return firstInputHigh != secondInputHigh;
                case DigitalGateKind.Xnor2:
                    return firstInputHigh == secondInputHigh;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
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

        private static double FindRisingCrossing(
            IEnumerable<Tuple<double, double, double>> samples,
            Func<Tuple<double, double, double>, double> selector,
            double threshold)
        {
            Tuple<double, double, double> result =
                samples.FirstOrDefault(item => selector(item) >= threshold);
            Assert.NotNull(result);
            return result.Item1;
        }
    }
}

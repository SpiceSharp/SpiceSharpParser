using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Analysis;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.Testing;
using SpiceSharpParser.Validation;
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

            Assert.Equal(23, digital.Library.Subcircuits.Count);
            SpiceSubcircuitInfo inverter = digital.Library["DIG_NOT"];
            Assert.Equal(new[] { "A", "Y", "VDD", "VSS" }, inverter.Pins);
            Assert.Equal("0.5", inverter.DefaultParameters["VTH"]);
            Assert.Equal("10n", inverter.DefaultParameters["TPD"]);
            Assert.Equal("1G", inverter.DefaultParameters["RIN"]);
            Assert.Equal("50", inverter.DefaultParameters["ROUT"]);
            Assert.Equal("5p", inverter.DefaultParameters["COUT"]);
            Assert.Equal(
                new[] { "GND", "TRIG", "OUT", "RESET", "CTRL", "THRESH", "DISCH", "VCC" },
                digital.Library["TIMER555"].Pins);
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
        public void AddComparator_UsesDifferentialInputPolarity()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VP", "positive", "0", 3.0),
                new VoltageSource("VN", "negative", "0", 2.0),
                new Resistor("RHIGH", "high", "0", 10000.0),
                new Resistor("RLOW", "low", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddComparator(
                circuit,
                "XHIGH",
                "positive",
                "negative",
                "high",
                "vdd",
                "0");
            digital.AddComparator(
                circuit,
                "XLOW",
                "negative",
                "positive",
                "low",
                "vdd",
                "0");

            Assert.InRange(RunOperatingPoint(circuit, "high"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(circuit, "low"), -1e-9, 0.1);
        }

        [Fact]
        public void AddOpenDrain_PullsLowWhenEnabledAndReleasesWhenDisabled()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VENABLED", "enabled", "0", 5.0),
                new VoltageSource("VDISABLED", "disabled", "0", 0.0),
                new Resistor("RPULL1", "vdd", "pulled-low", 10000.0),
                new Resistor("RPULL2", "vdd", "released", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddOpenDrain(circuit, "XON", "enabled", "pulled-low", "vdd", "0");
            digital.AddOpenDrain(circuit, "XOFF", "disabled", "released", "vdd", "0");

            Assert.InRange(RunOperatingPoint(circuit, "pulled-low"), 0.0, 0.1);
            Assert.InRange(RunOperatingPoint(circuit, "released"), 4.9, 5.0);
        }

        [Fact]
        public void AddSetResetLatch_SetsHoldsAndResetsState()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Set-reset latch sequence",
                "VDD vdd 0 5",
                "VSET set 0 PULSE(0 5 5n 100p 100p 10n 200n)",
                "VRESET reset 0 PULSE(0 5 60n 100p 100p 10n 200n)",
                "RQ q 0 10k",
                "RQB qb 0 10k",
                ".TRAN 200p 100n 0 200p UIC",
                ".SAVE V(q) V(qb)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddSetResetLatch(
                model.Circuit,
                "XLATCH",
                "set",
                "reset",
                "q",
                "qb",
                "vdd",
                "0",
                new Dictionary<string, string>
                {
                    ["TPD"] = "1n",
                    ["RSTATE"] = "100",
                    ["CMEM"] = "1p",
                });

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(q)", "V(qb)");
            Tuple<double, double, double> held = NearestSample(samples, 30e-9);
            Tuple<double, double, double> reset = NearestSample(samples, 80e-9);

            Assert.InRange(held.Item2, 4.9, 5.0);
            Assert.InRange(held.Item3, -1e-9, 0.1);
            Assert.InRange(reset.Item2, -1e-9, 0.1);
            Assert.InRange(reset.Item3, 4.9, 5.0);
        }

        [Fact]
        public void AddSetResetLatch_WhenSetAndResetAreHigh_ResetDominates()
        {
            var circuit = new Circuit(
                new VoltageSource("VDD", "vdd", "0", 5.0),
                new VoltageSource("VSET", "set", "0", 5.0),
                new VoltageSource("VRESET", "reset", "0", 5.0),
                new Resistor("RQ", "q", "0", 10000.0),
                new Resistor("RQB", "qb", "0", 10000.0));
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();

            digital.AddSetResetLatch(
                circuit,
                "XLATCH",
                "set",
                "reset",
                "q",
                "qb",
                "vdd",
                "0");

            Assert.InRange(RunOperatingPoint(circuit, "q"), -1e-9, 0.1);
            Assert.InRange(RunOperatingPoint(circuit, "qb"), 4.9, 5.0);
        }

        [Fact]
        public void AddTimer555_ImplementsResetTriggerThresholdPriority()
        {
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            Circuit triggerAndThreshold = CreateTimerPriorityCircuit(resetHigh: true);

            digital.AddTimer555(
                triggerAndThreshold,
                "XTIMER",
                "0",
                "trigger",
                "out",
                "reset",
                "control",
                "threshold",
                "discharge",
                "vcc");

            Assert.InRange(RunOperatingPoint(triggerAndThreshold, "out"), 4.9, 5.0);
            Assert.InRange(RunOperatingPoint(triggerAndThreshold, "discharge"), 4.9, 5.0);

            Circuit reset = CreateTimerPriorityCircuit(resetHigh: false);
            digital.AddTimer555(
                reset,
                "XTIMER",
                "0",
                "trigger",
                "out",
                "reset",
                "control",
                "threshold",
                "discharge",
                "vcc");

            Assert.InRange(RunOperatingPoint(reset, "out"), -1e-9, 0.1);
            Assert.InRange(RunOperatingPoint(reset, "discharge"), 0.0, 0.1);
        }

        [Fact]
        public void AddTimer555_StaticCircuitPassesSmokeAndLintChecks()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Functional 555 static smoke fixture",
                "VCC vcc 0 5",
                "VTRIGGER trigger 0 5",
                "VRESET reset 0 0",
                "VTHRESHOLD threshold 0 0",
                "RLOAD out 0 10k",
                "RDISCH vcc discharge 10k",
                ".OP",
                ".SAVE V(out) V(discharge)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddTimer555(
                model.Circuit,
                "XTIMER",
                "0",
                "trigger",
                "out",
                "reset",
                "control",
                "threshold",
                "discharge",
                "vcc");

            LintResult lint = NetlistLinter.Lint(model);
            Assert.False(lint.HasErrors, lint.ToString());

            SmokeTestResult smoke = SmokeTester.QuickCheck(model);
            Assert.True(smoke.IsPass, smoke.DiagnosticSummary());
        }

        [Fact]
        public void AddTimer555_InAstableCircuitMatchesTimingEquations()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "Functional 555 astable",
                "VCC vcc 0 5",
                "RA vcc discharge 10k",
                "RB discharge timing 10k",
                "CT timing 0 10n IC=0",
                "CCTRL control 0 10n IC=3.333333333",
                "RLOAD out 0 10k",
                ".OPTIONS method=gear",
                ".TRAN 1u 1m 0 10n UIC",
                ".MEAS TRAN period TRIG V(out) VAL=2.5 RISE=2 TARG V(out) VAL=2.5 RISE=3",
                ".MEAS TRAN high_time TRIG V(out) VAL=2.5 RISE=2 TARG V(out) VAL=2.5 FALL=2",
                ".MEAS TRAN low_time TRIG V(out) VAL=2.5 FALL=2 TARG V(out) VAL=2.5 RISE=3",
                ".MEAS TRAN timing_min MIN V(timing) FROM=300u TO=1m",
                ".MEAS TRAN timing_max MAX V(timing) FROM=300u TO=1m",
                ".SAVE V(out) V(timing)",
                ".END");
            DigitalSubcircuitLibrary digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            digital.AddTimer555(
                model.Circuit,
                "XTIMER",
                "0",
                "timing",
                "out",
                "vcc",
                "control",
                "timing",
                "discharge",
                "vcc");

            Tuple<double, double, double>[] samples =
                SpiceSimulationTestHelper.RunTransientPair(model, "V(out)", "V(timing)");
            Tuple<double, double, double>[] settled =
                samples.Where(item => item.Item1 >= 300e-6).ToArray();
            int risingCrossings = CountRisingCrossings(samples, item => item.Item2, 2.5);
            int fallingCrossings = CountFallingCrossings(samples, item => item.Item2, 2.5);
            Assert.True(
                model.Measurements["period"][0].Success,
                string.Format(
                    "No measured period. Output {0} to {1} V; timing {2} to {3} V; "
                    + "settled timing {4} to {5} V; crossings {6} rising/{7} falling; "
                    + "final out/timing {8}/{9} V.",
                    samples.Min(item => item.Item2),
                    samples.Max(item => item.Item2),
                    samples.Min(item => item.Item3),
                    samples.Max(item => item.Item3),
                    settled.Min(item => item.Item3),
                    settled.Max(item => item.Item3),
                    risingCrossings,
                    fallingCrossings,
                    samples.Last().Item2,
                    samples.Last().Item3));

            double period =
                SpiceNetlistAssertions.AssertMeasurementSuccess(model, "period").Value;
            double highTime =
                SpiceNetlistAssertions.AssertMeasurementSuccess(model, "high_time").Value;
            double lowTime =
                SpiceNetlistAssertions.AssertMeasurementSuccess(model, "low_time").Value;
            double timingMinimum =
                SpiceNetlistAssertions.AssertMeasurementSuccess(model, "timing_min").Value;
            double timingMaximum =
                SpiceNetlistAssertions.AssertMeasurementSuccess(model, "timing_max").Value;

            Assert.InRange(period, 0.95 * 207.9e-6, 1.05 * 207.9e-6);
            Assert.InRange(highTime, 0.95 * 138.6e-6, 1.05 * 138.6e-6);
            Assert.InRange(lowTime, 0.95 * 69.3e-6, 1.05 * 69.3e-6);
            Assert.InRange(timingMinimum, 1.5, 1.85);
            Assert.InRange(timingMaximum, 3.15, 3.5);
        }

        [Fact]
        public void Timer555AstableExample_CompilesIncludesAndMeasuresTiming()
        {
            string path = FindRepositoryFile(
                "circuits",
                "timer555",
                "timer555-astable.cir");
            SpiceCompilationResult result = SpiceCompiler.CompileFile(path);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.NotNull(result.Model);

            SpiceSimulationTestHelper.RunTransientPair(
                result.Model,
                "V(out)",
                "V(timing)");
            double period =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "period").Value;
            double highTime =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "high_time").Value;
            double lowTime =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "low_time").Value;

            Assert.InRange(period, 0.95 * 207.9e-6, 1.05 * 207.9e-6);
            Assert.InRange(highTime, 0.95 * 138.6e-6, 1.05 * 138.6e-6);
            Assert.InRange(lowTime, 0.95 * 69.3e-6, 1.05 * 69.3e-6);
        }

        [Fact]
        public void MilestoneARoutingExample_CompilesIncludesAndMeasuresBusBehavior()
        {
            string path = FindRepositoryFile(
                "circuits",
                "digital-milestone-a",
                "milestone-a-routing.cir");
            SpiceCompilationResult result = SpiceCompiler.CompileFile(path);

            Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.NotNull(result.Model);

            SpiceSimulationTestHelper.RunTransientPair(
                result.Model,
                "V(conditioned)",
                "V(bus)");
            double conditionedHigh =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "conditioned_high").Value;
            double disabledBus =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "disabled_bus").Value;
            double enabledBus =
                SpiceNetlistAssertions.AssertMeasurementSuccess(result.Model, "enabled_bus").Value;

            Assert.InRange(conditionedHigh, 4.9, 5.0);
            Assert.InRange(disabledBus, 0.0, 0.1);
            Assert.InRange(enabledBus, 4.9, 5.0);
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

        private static Circuit CreateTimerPriorityCircuit(bool resetHigh)
        {
            return new Circuit(
                new VoltageSource("VCC", "vcc", "0", 5.0),
                new VoltageSource("VTRIGGER", "trigger", "0", 0.0),
                new VoltageSource("VTHRESHOLD", "threshold", "0", 5.0),
                new VoltageSource("VRESET", "reset", "0", resetHigh ? 5.0 : 0.0),
                new Resistor("ROUT", "out", "0", 10000.0),
                new Resistor("RDISCH", "vcc", "discharge", 10000.0));
        }

        private static Tuple<double, double, double> NearestSample(
            IEnumerable<Tuple<double, double, double>> samples,
            double time)
        {
            return samples.OrderBy(item => Math.Abs(item.Item1 - time)).First();
        }

        private static string FindRepositoryFile(params string[] pathSegments)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(
                    directory.FullName,
                    Path.Combine(pathSegments));
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException(
                "Could not locate repository fixture.",
                Path.Combine(pathSegments));
        }

        private static int CountRisingCrossings(
            IEnumerable<Tuple<double, double, double>> samples,
            Func<Tuple<double, double, double>, double> selector,
            double threshold)
        {
            return CountCrossings(samples, selector, threshold, rising: true);
        }

        private static int CountFallingCrossings(
            IEnumerable<Tuple<double, double, double>> samples,
            Func<Tuple<double, double, double>, double> selector,
            double threshold)
        {
            return CountCrossings(samples, selector, threshold, rising: false);
        }

        private static int CountCrossings(
            IEnumerable<Tuple<double, double, double>> samples,
            Func<Tuple<double, double, double>, double> selector,
            double threshold,
            bool rising)
        {
            Tuple<double, double, double>[] values = samples.ToArray();
            int result = 0;
            for (int index = 1; index < values.Length; index++)
            {
                double previous = selector(values[index - 1]);
                double current = selector(values[index]);
                if (rising
                    ? previous < threshold && current >= threshold
                    : previous > threshold && current <= threshold)
                {
                    result++;
                }
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

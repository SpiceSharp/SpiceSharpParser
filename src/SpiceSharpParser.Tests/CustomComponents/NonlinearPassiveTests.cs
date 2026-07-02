using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using System;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class NonlinearPassiveTests
    {
        [Fact]
        public void Parser_WhenCustomComponentsEnabled_MapsChargeDefinedCapacitor()
        {
            var model = ReadWithCustomComponents(
                "Charge capacitor parser",
                "C1 out 0 Q=1e-6*x",
                "R1 in out 10k",
                "V1 in 0 10",
                ".ic V(out)=0",
                ".tran 1e-8 10e-6",
                ".save V(out)",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<NonlinearCapacitor>());

            var samples = RunTransient(model, "V(out)");
            double final = samples.Last().Value;
            double expected = 10.0 * (1.0 - Math.Exp(-10e-6 / (10e3 * 1e-6)));
            AssertClose(expected, final, 2e-4);
        }

        [Fact]
        public void Parser_WhenCustomComponentsEnabled_MapsFluxDefinedInductor()
        {
            var model = ReadWithCustomComponents(
                "Flux inductor parser",
                "V1 in 0 0",
                "L1 in out Flux=1*x ic=1",
                "R1 out 0 1k",
                ".tran 1e-6 1e-3 uic",
                ".ic V(out)=0",
                ".save V(out)",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<NonlinearInductor>());

            var samples = RunTransient(model, "V(out)");
            double actual = samples.Last().Value;
            double expected = 1e3 * Math.Exp(-1e-3 / (1.0 / 1e3));
            AssertClose(expected, actual, 2e-3);
        }

        [Fact]
        public void Tran_WhenChargeExpressionIsLinear_MatchesCapacitorReference()
        {
            var nonlinear = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10e3),
                new NonlinearCapacitor("C1", "out", "0", "1e-6*x"));

            var reference = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10e3),
                new Capacitor("C1", "out", "0", 1e-6));

            double actual = RunTransient(nonlinear, "out", 1e-8, 10e-6);
            double expected = RunTransient(reference, "out", 1e-8, 10e-6);

            AssertClose(expected, actual, 2e-4);
        }

        [Fact]
        public void Tran_WhenChargeExpressionHasInitialCondition_MatchesCapacitorReference()
        {
            var nonlinearCapacitor = new NonlinearCapacitor("C1", "out", "0", "1e-6*x");
            nonlinearCapacitor.Parameters.InitialCondition = 5.0;

            var referenceCapacitor = new Capacitor("C1", "out", "0", 1e-6);
            referenceCapacitor.Parameters.InitialCondition = 5.0;

            var nonlinear = new Circuit(
                nonlinearCapacitor,
                new Resistor("R1", "out", "0", 1e3));

            var reference = new Circuit(
                referenceCapacitor,
                new Resistor("R1", "out", "0", 1e3));

            double actual = RunTransient(nonlinear, "out", 1e-6, 1e-3, useIc: true);
            double expected = RunTransient(reference, "out", 1e-6, 1e-3, useIc: true);

            AssertClose(expected, actual, 2e-3);
        }

        [Fact]
        public void Parser_WhenChargeExpressionHasInitialCondition_MapsInitialVoltage()
        {
            var model = ReadWithCustomComponents(
                "Charge capacitor IC parser",
                "C1 out 0 Q=1e-6*x IC=5",
                "R1 out 0 1k",
                ".tran 1e-6 1e-3 uic",
                ".save V(out)",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<NonlinearCapacitor>());

            var samples = RunTransient(model, "V(out)");
            double actual = samples.Last().Value;
            double expected = 5.0 * Math.Exp(-1e-3 / (1e3 * 1e-6));

            AssertClose(expected, actual, 2e-3);
        }

        [Fact]
        public void Parser_WhenChargeExpressionHasScaling_AppliesParallelAndSeriesMultipliers()
        {
            var model = ReadWithCustomComponents(
                "Scaled charge capacitor parser",
                "C1 out 0 Q=1e-6*x M=2 N=4",
                "R1 in out 10k",
                "V1 in 0 10",
                ".ic V(out)=0",
                ".tran 1e-8 10e-6",
                ".save V(out)",
                ".end");

            AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<NonlinearCapacitor>());

            var samples = RunTransient(model, "V(out)");
            double final = samples.Last().Value;
            double expected = 10.0 * (1.0 - Math.Exp(-10e-6 / (10e3 * 0.5e-6)));
            AssertClose(expected, final, 2e-4);
        }

        [Fact]
        public void Ac_WhenChargeExpressionIsLinear_UsesIncrementalCapacitance()
        {
            var capacitor = new NonlinearCapacitor("C1", "out", "0", "2e-6*x");

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10.0),
                capacitor);

            Complex actual = RunAc(circuit, "out");
            Complex expected = 1.0 / (1.0 + (Complex.ImaginaryOne * 2.0 * Math.PI * 10.0 * 2e-6));

            AssertClose(expected.Real, actual.Real, 1e-9);
            AssertClose(expected.Imaginary, actual.Imaginary, 1e-9);
        }

        [Fact]
        public void Ac_WhenChargeExpressionIsNonlinear_UsesBiasedIncrementalCapacitance()
        {
            var capacitor = new NonlinearCapacitor("C1", "out", "0", "1e-6*x + 1e-7*x*x");

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 2.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10.0),
                capacitor);

            Complex actual = RunAc(circuit, "out");
            double capacitance = 1e-6 + (2e-7 * 2.0);
            Complex expected = 1.0 / (1.0 + (Complex.ImaginaryOne * 2.0 * Math.PI * 10.0 * capacitance));

            AssertClose(expected.Real, actual.Real, 1e-9);
            AssertClose(expected.Imaginary, actual.Imaginary, 1e-9);
        }

        [Fact]
        public void Tran_WhenFluxExpressionIsLinear_MatchesInductorReference()
        {
            var inductor = new NonlinearInductor("L1", "in", "out", "1*x");
            inductor.Parameters.InitialCondition = 1.0;

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                inductor,
                new Resistor("R1", "out", "0", 1e3));

            var tran = new Transient("tran", 1e-6, 1e-3);
            tran.TimeParameters.UseIc = true;
            var export = new RealVoltageExport(tran, "out");

            double actual = double.NaN;
            foreach (int ignored in tran.Run(circuit))
            {
                actual = export.Value;
            }

            double expected = 1e3 * Math.Exp(-1e-3 / (1.0 / 1e3));
            AssertClose(expected, actual, 2e-3);
        }

        [Fact]
        public void Ac_WhenFluxExpressionIsLinear_UsesIncrementalInductance()
        {
            var inductor = new NonlinearInductor("L1", "out", "0", "2*x");

            var circuit = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0).SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10.0),
                inductor);

            var ac = new AC("ac", new DecadeSweep(1.0, 10.0, 1));
            var export = new ComplexVoltageExport(ac, "out");

            bool asserted = false;
            foreach (int code in ac.Run(circuit, AC.ExportSmallSignal))
            {
                if (code != AC.ExportSmallSignal)
                {
                    continue;
                }

                var impedance = new System.Numerics.Complex(0.0, 2.0 * 2.0 * Math.PI);
                var expected = impedance / (10.0 + impedance);
                AssertClose(expected.Real, export.Value.Real, 1e-9);
                AssertClose(expected.Imaginary, export.Value.Imaginary, 1e-9);
                asserted = true;
                break;
            }

            Assert.True(asserted, "Expected at least one AC small-signal export.");
        }

        private static SpiceSharpModel ReadWithCustomComponents(params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.UseCustomComponents();

            return reader.Read(parseResult.FinalModel);
        }

        private static (double Time, double Value)[] RunTransient(SpiceSharpModel model, string exportName)
        {
            var simulation = model.Simulations.Single(s => s is Transient);
            var export = model.Exports.Single(e => e.Name == exportName && e.Simulation == simulation);
            var result = new System.Collections.Generic.List<(double Time, double Value)>();

            simulation.EventExportData += (_, _) =>
            {
                result.Add((((Transient)simulation).Time, export.Extract()));
            };

            var codes = simulation.Run(model.Circuit, -1);
            codes = simulation.InvokeEvents(codes);
            codes.ToArray();

            return result.ToArray();
        }

        private static double RunTransient(Circuit circuit, string exportNode, double step, double stop, bool useIc = false)
        {
            var tran = new Transient("tran", step, stop);
            tran.TimeParameters.UseIc = useIc;
            var export = new RealVoltageExport(tran, exportNode);

            double actual = double.NaN;
            foreach (int ignored in tran.Run(circuit))
            {
                actual = export.Value;
            }

            return actual;
        }

        private static Complex RunAc(Circuit circuit, string exportNode)
        {
            var ac = new AC("ac", new LinearSweep(1.0, 1.0, 1));
            var export = new ComplexVoltageExport(ac, exportNode);

            foreach (int code in ac.Run(circuit, AC.ExportSmallSignal))
            {
                if (code == AC.ExportSmallSignal)
                {
                    return export.Value;
                }
            }

            throw new InvalidOperationException("Expected at least one AC small-signal export.");
        }

        private static void AssertNoValidationErrors(SpiceSharpModel model)
        {
            Assert.False(
                model.ValidationResult.HasError,
                string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message)));
        }

        private static void AssertClose(double expected, double actual, double tolerance)
        {
            double effectiveTolerance = Math.Abs(expected) > 1e-6
                ? Math.Max(tolerance, Math.Abs(expected) * 1e-3)
                : tolerance;

            Assert.True(
                Math.Abs(expected - actual) <= effectiveTolerance,
                $"Expected {expected:R}, got {actual:R}.");
        }
    }
}

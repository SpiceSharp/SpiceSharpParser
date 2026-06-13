using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceIdealDiodeIntegrationTests : BaseTests
    {
        [Fact]
        public void When_FullBridgeRectifierUsesLtspiceIdealDiodes_Expect_BipolarInputIsRectified()
        {
            var model = ReadWithCustomComponents(
                "LTspice ideal diode bridge rectifier",
                "VIN acp 0 0",
                "DPLUS acp outp rect",
                "DRETURN outn 0 rect",
                "DNEG 0 outp rect",
                "DNEGRETURN outn acp rect",
                "RLOAD outp outn 10",
                ".model rect D(Ron=0.5 Roff=1e12 Vfwd=0.7)",
                ".dc VIN -10 10 10",
                ".save V(outp,outn)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Equal(4, model.Circuit.OfType<IdealDiode>().Count());

            var exports = RunDCSimulation(model, "V(outp,outn)");
            Assert.Equal(3, exports.Length);

            double expectedRectifiedVoltage = 10.0 * ((10.0 - (2.0 * 0.7)) / (10.0 + (2.0 * 0.5)));

            AssertSweepPoint(-10.0, expectedRectifiedVoltage, exports[0]);
            AssertSweepPoint(0.0, 0.0, exports[1]);
            AssertSweepPoint(10.0, expectedRectifiedVoltage, exports[2]);
        }

        [Fact]
        public void When_FullBridgeRectifierUsesLtspiceIdealDiodesInAc_Expect_ForwardPathSmallSignalGain()
        {
            var model = ReadWithCustomComponents(
                "LTspice AC ideal diode bridge rectifier",
                "VIN acp 0 DC 10 AC 1",
                "DPLUS acp outp rect",
                "DRETURN outn 0 rect",
                "DNEG 0 outp rect",
                "DNEGRETURN outn acp rect",
                "RLOAD outp outn 10",
                ".model rect D(Ron=0.5 Roff=1e12 Vfwd=0.7)",
                ".ac lin 1 1k 1k",
                ".save VM(outp,outn)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Equal(4, model.Circuit.OfType<IdealDiode>().Count());

            var exports = RunAcSimulation(model, "VM(outp,outn)");
            Assert.Single(exports);

            double expectedGain = 10.0 / (10.0 + (2.0 * 0.5));
            AssertSweepPoint(1000.0, expectedGain, exports[0]);
        }

        [Fact]
        public void When_FullBridgeRectifierUsesLtspiceIdealDiodesWithSinTransient_Expect_RectifiedWaveform()
        {
            var model = ReadWithCustomComponents(
                "LTspice TRAN ideal diode bridge rectifier",
                "VIN acp 0 SIN(0 10 1k)",
                "DPLUS acp outp rect",
                "DRETURN outn 0 rect",
                "DNEG 0 outp rect",
                "DNEGRETURN outn acp rect",
                "RLOAD outp outn 10",
                ".model rect D(Ron=0.5 Roff=1e12 Vfwd=0.7)",
                ".tran 25u 1m 0 25u",
                ".save V(acp) V(outp,outn)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Equal(4, model.Circuit.OfType<IdealDiode>().Count());

            var exports = RunTransientSimulation(model, "V(acp)", "V(outp,outn)");
            Assert.True(exports.Length > 20);

            double expectedPeak = ExpectedBridgeOutput(10.0, 10.0, 0.5, 0.7);
            AssertClose(expectedPeak, exports.Max(point => point.Item3), 1e-2);
            Assert.Contains(exports, point => point.Item2 < -9.9 && point.Item3 > expectedPeak - 1e-2);

            foreach (var point in exports)
            {
                double expected = ExpectedBridgeOutput(point.Item2, 10.0, 0.5, 0.7);
                AssertClose(expected, point.Item3, 1e-3);
            }
        }

        private static SpiceSharpModel ReadWithCustomComponents(params string[] lines)
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(string.Join(Environment.NewLine, lines));
            var reader = new SpiceSharpReader();
            reader.Settings.UseCustomComponents();

            return reader.Read(parseResult.FinalModel);
        }

        private static Tuple<double, double>[] RunAcSimulation(SpiceSharpModel model, string nameOfExport)
        {
            var export = model.Exports.Find(e => e.Name == nameOfExport && e.Simulation is AC);
            var simulation = model.Simulations.First(s => s is AC);
            var list = new List<Tuple<double, double>>();

            Assert.NotNull(export);

            simulation.EventExportData += (sender, e) =>
            {
                list.Add(new Tuple<double, double>(((AC)simulation).Frequency, export.Extract()));
            };

            var codes = simulation.Run(model.Circuit, -1);
            var attached = simulation.InvokeEvents(codes);
            attached.ToArray();

            return list.ToArray();
        }

        private static Tuple<double, double, double>[] RunTransientSimulation(
            SpiceSharpModel model,
            string inputExportName,
            string outputExportName)
        {
            var inputExport = model.Exports.Find(e => e.Name == inputExportName && e.Simulation is Transient);
            var outputExport = model.Exports.Find(e => e.Name == outputExportName && e.Simulation is Transient);
            var simulation = model.Simulations.First(s => s is Transient);
            var list = new List<Tuple<double, double, double>>();

            Assert.NotNull(inputExport);
            Assert.NotNull(outputExport);

            simulation.EventExportData += (sender, e) =>
            {
                list.Add(new Tuple<double, double, double>(
                    ((Transient)simulation).Time,
                    inputExport.Extract(),
                    outputExport.Extract()));
            };

            var codes = simulation.Run(model.Circuit, -1);
            var attached = simulation.InvokeEvents(codes);
            attached.ToArray();

            return list.ToArray();
        }

        private static double ExpectedBridgeOutput(double inputVoltage, double loadResistance, double onResistance, double forwardVoltage)
        {
            double rectifiedInput = Math.Abs(inputVoltage);
            double diodeDrop = 2.0 * forwardVoltage;
            if (rectifiedInput <= diodeDrop)
            {
                return 0.0;
            }

            return loadResistance * ((rectifiedInput - diodeDrop) / (loadResistance + (2.0 * onResistance)));
        }

        private static void AssertSweepPoint(double expectedSweep, double expectedValue, Tuple<double, double> actual)
        {
            AssertClose(expectedSweep, actual.Item1, 1e-12);
            AssertClose(expectedValue, actual.Item2, 1e-6);
        }

        private static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            string messages = string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
            Assert.False(validation.HasError, messages);
            Assert.False(validation.HasWarning, messages);
        }

        private static void AssertClose(double expected, double actual, double tolerance)
        {
            Assert.True(
                Math.Abs(expected - actual) <= tolerance,
                $"Expected {expected:R}, got {actual:R}.");
        }
    }
}

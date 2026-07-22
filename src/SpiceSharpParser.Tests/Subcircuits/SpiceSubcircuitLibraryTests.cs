using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.Diagnostics;
using Xunit;

namespace SpiceSharpParser.Tests.Subcircuits
{
    public class SpiceSubcircuitLibraryTests
    {
        [Fact]
        public void LoadText_ExposesPinsAndDefaultParameters()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt Filter IN OUT GND params: R=1k C=10n",
                    "R1 IN OUT {R}",
                    "C1 OUT GND {C}",
                    ".ends Filter"),
                "filters.lib");

            SpiceSubcircuitInfo info = library["filter"];

            Assert.Equal("Filter", info.Name);
            Assert.Equal(new[] { "IN", "OUT", "GND" }, info.Pins);
            Assert.Equal("1k", info.DefaultParameters["r"]);
            Assert.Equal("10n", info.DefaultParameters["c"]);
            Assert.Empty(library.Diagnostics);
        }

        [Fact]
        public void AddInstance_MixesParsedSubcircuitWithProgrammaticSpiceSharpCircuit()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt DIVIDER IN OUT GND",
                    "RUP IN OUT 1k",
                    "RDOWN OUT GND 1k",
                    ".ends DIVIDER"));
            var circuit = new Circuit(new VoltageSource("V1", "source", "0", 10.0));

            library.AddInstance(circuit, "DIVIDER", "XU1", "source", "out", "0");

            Assert.IsType<Resistor>(circuit["XU1.RUP"]);
            Assert.IsType<Resistor>(circuit["XU1.RDOWN"]);

            var simulation = new OP("op");
            var export = new RealVoltageExport(simulation, "out");
            double output = double.NaN;
            foreach (int ignored in simulation.Run(circuit))
            {
                output = export.Value;
            }

            Assert.Equal(5.0, output, 9);
        }

        [Fact]
        public void AddInstance_WithParameterOverride_UsesInstanceValue()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt LOAD IN GND params: R=1k",
                    "R1 IN GND {R}",
                    ".ends LOAD"));
            var circuit = new Circuit();

            library.AddInstance(
                circuit,
                "LOAD",
                "XLOAD",
                new[] { "in", "0" },
                new Dictionary<string, string> { ["R"] = "2k" });

            var resistor = Assert.IsType<Resistor>(circuit["XLOAD.R1"]);
            Assert.Equal(2000.0, resistor.Parameters.Resistance.Value);
        }

        [Fact]
        public void AddInstance_WhenExpansionDisabled_AddsNativeSpiceSharpSubcircuit()
        {
            var options = new SpiceCompileOptions
            {
                ExpandSubcircuits = false,
            };
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt BUFFER IN OUT",
                    "R1 IN OUT 1k",
                    ".ends BUFFER"),
                options);
            var circuit = new Circuit();

            IReadOnlyList<SpiceSharp.Entities.IEntity> added =
                library.AddInstance(circuit, "BUFFER", "XBUF", "in", "out");

            Assert.Single(added);
            Assert.IsType<Subcircuit>(circuit["XBUF"]);
        }

        [Fact]
        public void AddInstance_ReusesModelsInstalledByTheSameLibrary()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".model DIO D(Is=1e-12)",
                    ".subckt CLAMP IN GND",
                    "D1 IN GND DIO",
                    ".ends CLAMP"));
            var circuit = new Circuit();

            library.AddInstance(circuit, "CLAMP", "X1", "in1", "0");
            library.AddInstance(circuit, "CLAMP", "X2", "in2", "0");

            Assert.NotNull(circuit["DIO"]);
            Assert.IsType<Diode>(circuit["X1.D1"]);
            Assert.IsType<Diode>(circuit["X2.D1"]);
            Assert.Single(circuit, entity => entity.Name.Equals("DIO", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void AddInstance_WhenTargetContainsConflictingEntity_DoesNotPartiallyMutateTarget()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt BLOCK IN OUT",
                    "R1 IN OUT 1k",
                    ".ends BLOCK"));
            var existing = new Resistor("X1.R1", "other", "0", 5.0);
            var circuit = new Circuit(existing);

            Assert.Throws<InvalidOperationException>(
                () => library.AddInstance(circuit, "BLOCK", "X1", "in", "out"));

            Assert.Same(existing, circuit["X1.R1"]);
            Assert.Single(circuit);
        }

        [Fact]
        public void LoadFile_ResolvesNestedIncludesRelativeToEachLibrary()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "SpiceSharpParser.Subcircuits." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string rootPath = Path.Combine(directory, "root.lib");
            string childPath = Path.Combine(directory, "logic.lib");

            try
            {
                File.WriteAllText(rootPath, ".include \"logic.lib\"" + Environment.NewLine);
                string childSource = Lines(
                    ".subckt NAND A B Y",
                    "R1 A Y 1k",
                    "R2 B Y 1k",
                    ".ends NAND");
                File.WriteAllText(childPath, childSource);

                SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadFile(rootPath);

                Assert.True(library.Subcircuits.ContainsKey("NAND"));
                Assert.Equal(2, library.Dependencies.Count);
                Assert.All(library.Dependencies, dependency => Assert.True(dependency.IsResolved));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void AddInstance_WithCustomMappings_CreatesCustomComponents()
        {
            var options = new SpiceCompileOptions
            {
                ConfigureReader = settings => settings.UseCustomComponents(),
            };
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt NONLINEAR IN OUT",
                    "R1 IN OUT 1k",
                    "C1 OUT 0 Q=1u*x",
                    ".ends NONLINEAR"),
                options);
            var circuit = new Circuit();

            library.AddInstance(circuit, "NONLINEAR", "XNL", "in", "out");

            Assert.IsType<NonlinearCapacitor>(circuit["XNL.C1"]);
        }

        [Fact]
        public void AddInstance_DoesNotCopyTopLevelCircuitComponents()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    "VSTRAY stray 0 1",
                    ".subckt BLOCK IN OUT",
                    "R1 IN OUT 1k",
                    ".ends BLOCK"));
            var circuit = new Circuit();

            library.AddInstance(circuit, "BLOCK", "X1", "in", "out");

            Assert.False(circuit.Contains("VSTRAY"));
            Assert.IsType<Resistor>(circuit["X1.R1"]);
        }

        [Fact]
        public void AddInstance_WithWrongPinCount_RejectsInputBeforeChangingCircuit()
        {
            SpiceSubcircuitLibrary library = SpiceSubcircuitLibrary.LoadText(
                Lines(
                    ".subckt PAIR IN OUT",
                    "R1 IN OUT 1k",
                    ".ends PAIR"));
            var circuit = new Circuit();

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => library.AddInstance(circuit, "PAIR", "X1", "in"));

            Assert.Contains("requires 2 nodes", exception.Message);
            Assert.Empty(circuit);
        }

        [Fact]
        public void LoadText_WithoutSubcircuit_ReturnsStructuredFailure()
        {
            SpiceSubcircuitLibraryException exception = Assert.Throws<SpiceSubcircuitLibraryException>(
                () => SpiceSubcircuitLibrary.LoadText(".model D D(Is=1e-12)", "models.lib"));

            SpiceDiagnostic diagnostic = Assert.Single(
                exception.Diagnostics,
                item => item.Severity == DiagnosticSeverity.Error);
            Assert.Equal(SpiceDiagnosticCodes.ReaderError, diagnostic.Code);
            Assert.Contains(".SUBCKT", diagnostic.Message);
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
    }
}

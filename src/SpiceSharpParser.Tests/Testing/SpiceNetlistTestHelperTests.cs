using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.Testing
{
    public class SpiceNetlistTestHelperTests
    {
        [Fact]
        public void ParseAndRead_WhenUsingDefaultOptions_RunsOperatingPointExport()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "helper op",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            SpiceNetlistAssertions.AssertNoValidationErrors(model);
            SpiceNetlistAssertions.AssertClose(1.0, SpiceSimulationTestHelper.RunOp(model, "V(out)"), 1e-12);
        }

        [Fact]
        public void ParseAndRead_WhenLtspiceCompatibilityIsEnabled_UsesCompatibilityInReader()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                new SpiceNetlistTestOptions { Compatibility = CompatibilityOptions.LTspice },
                "helper ltspice",
                "V1 in 0 1",
                "R1 in out 90 Rser=10",
                "RLOAD out 0 900",
                ".op",
                ".save V(out)",
                ".end");

            SpiceNetlistAssertions.AssertNoValidationIssues(model.ValidationResult);
            SpiceNetlistAssertions.AssertClose(0.9, SpiceSimulationTestHelper.RunOp(model, "V(out)"));
            Assert.Contains(model.Circuit, entity => entity.Name == "R1_rser");
        }

        [Fact]
        public void ParseAndRead_WhenCustomComponentsAreEnabled_UsesCustomMappings()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                new SpiceNetlistTestOptions { UseCustomComponents = true },
                "helper custom components",
                "V1 in 0 3",
                "D1 in 0 ideal",
                ".model ideal D(Ron=2 Roff=1e9 Vfwd=1)",
                ".op",
                ".end");

            SpiceNetlistAssertions.AssertNoValidationErrors(model);
            Assert.Single(model.Circuit.OfType<IdealDiode>());
        }
    }
}

using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class OptionsTests : BaseTests
    {
        [Fact]
        public void When_DynamicResistorsIsSpecified_Expect_DynamicResistors()
        {
            var netlist = ParseNetlist(
                "DC Sweep - dynamic resistors",
                "V1 in 0 0",
                "V2 out 0 10",
                "R1 out 0 {max(V(in), 1e-3)}",
                ".DC V1 0 10 1e-3",
                ".SAVE I(R1)",
                ".OPTIONS dynamic-resistors",
                ".END");

            var exports = RunDCSimulation(netlist, "I(R1)");

            // Get references
            Func<double, double> reference = sweep => 10.0 / Math.Max(1e-3, (sweep - 1e-3));
            EqualsWithTol(exports, reference);
        }

        [Fact]
        public void When_GearMethodIsSpecified_Expect_Gear()
        {
            var result = ParseNetlist(
                "Tran - Gear",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = gear",
                ".END");

            var tran = result.Simulations.First();
            Assert.IsType<Gear>(tran.Configurations.Get<TimeConfiguration>().Method);
        }

        [Fact]
        public void When_TrapMethodIsSpecified_Expect_Trapezoidal()
        {
            var result = ParseNetlist(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = trap",
                ".END");

            var tran = result.Simulations.First();
            Assert.IsType<Trapezoidal>(tran.Configurations.Get<TimeConfiguration>().Method);
        }

        [Fact]
        public void When_TrapezoidalMethodIsSpecified_Expect_Trapezoidal()
        {
            var result = ParseNetlist(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = trapezoidal",
                ".END");

            var tran = result.Simulations.First();
            Assert.IsType<Trapezoidal>(tran.Configurations.Get<TimeConfiguration>().Method);
        }

        [Fact]
        public void When_EulerMethodIsSpecified_Expect_FixedEuler()
        {
            var result = ParseNetlist(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = euler",
                ".END");

            var tran = result.Simulations.First();
            Assert.IsType<FixedEuler>(tran.Configurations.Get<TimeConfiguration>().Method);
        }

        [Fact]
        public void When_CdfPoints_GreaterThan3_Expect_NoException()
        {
            //TODO
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".OPTIONS DISTRIBUTION = triangle_dist",
                ".OPTIONS CDFPOINTS = 4",
                ".DISTRIBUTION triangle_dist (-1,0) (0, 1) (1, 0)",
                ".END");

            RunSimulations(result);
        }

        [Fact]
        public void When_CdfPoints_LessThan4_Expect_Exception()
        {
            var result = ParseNetlist(
                "Monte Carlo Analysis - OP - POWER",
                "V1 0 1 100",
                "R1 1 0 {R}",
                ".OP",
                ".PARAM R={random()*1000}",
                ".LET power {V(1)*I(R1)}",
                ".SAVE power",
                ".MC 1000 OP power MAX",
                ".OPTIONS DISTRIBUTION = triangle_dist",
                ".OPTIONS CDFPOINTS = 3",
                ".DISTRIBUTION triangle_dist (-1,0) (0, 1) (1, 0)",
                ".END");

            Assert.True(result.ValidationResult.HasWarning);
        }
    }
}
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class OptionsTests : BaseTests
    {
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

            var tran = result.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Gear);
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

            var tran = result.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Trapezoidal);
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

            var tran = result.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Trapezoidal);
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

            var tran = result.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is FixedEuler);
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
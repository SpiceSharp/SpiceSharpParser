using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class OptionsTests : BaseTests
    {
        [Fact]
        public void When_GearMethodIsSpecified_Expect_Gear()
        {
            var model = GetSpiceSharpModel(
                "Tran - Gear",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = gear",
                ".END");

            var tran = model.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Gear);
        }

        [Fact]
        public void When_TrapMethodIsSpecified_Expect_Trapezoidal()
        {
            var model = GetSpiceSharpModel(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = trap",
                ".END");

            var tran = model.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Trapezoidal);
        }

        [Fact]
        public void When_TrapezoidalMethodIsSpecified_Expect_Trapezoidal()
        {
            var model = GetSpiceSharpModel(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = trapezoidal",
                ".END");

            var tran = model.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is Trapezoidal);
        }

        [Fact]
        public void When_EulerMethodIsSpecified_Expect_FixedEuler()
        {
            var model = GetSpiceSharpModel(
                "Tran - Trap",
                "V1 in 0 10",
                "V2 out 0 10",
                "R1 out 0 10",
                ".TRAN 1u 1000u",
                ".SAVE I(R1)",
                ".OPTIONS method = euler",
                ".END");

            var tran = model.Simulations.First() as Transient;
            Assert.True(tran.TimeParameters is FixedEuler);
        }

        [Fact]
        public void When_CdfPoints_GreaterThan3_Expect_NoException()
        {
            //TODO
            var model = GetSpiceSharpModel(
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

            var exception = Record.Exception(() => RunSimulations(model));
            Assert.Null(exception);
        }

        [Fact]
        public void When_CdfPoints_LessThan4_Expect_Exception()
        {
            var model = GetSpiceSharpModel(
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

            Assert.True(model.ValidationResult.HasError);
        }
    }
}
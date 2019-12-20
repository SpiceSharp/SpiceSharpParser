using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class ExceptionTests : BaseTests
    {
        [Fact]
        public void When_UnknownComponent_Expect_Exception()
        {
            var result = ParseNetlist(
                "Exceptions",
                "UMemory 1 0 1N914",
                "V1_a 1 0 0.0",
                ".DC V1_a -1 1.0 10e-3",
                ".SAVE i(V1_a) v(1,0)",
                ".END");
            Assert.False(result.ValidationResult.IsValid);
            Assert.Contains(result.ValidationResult.Exceptions,
                e => e.GetType() == typeof(UnknownComponentException));
        }

        [Fact]
        public void When_InvalidProperty_Expect_Exception()
        {
            var result = ParseNetlist(
                "Exceptions",
                "V1 1 0 150",
                "R1 1 0 10 a = 1.2",
                ".SAVE I(R1)",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.IsValid);
            Assert.Contains(result.ValidationResult.Exceptions,
                e => e.GetType() == typeof(ReadingException) && e.InnerException?.GetType() == typeof(InvalidParameterException));
        }
    }
}
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Macros
{
    public class MixedNotation : BaseTests
    {
        [Fact]
        public void Test01()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Suffix notation",
                ".SUBCKT complex_component b<0:3>",
                "R1 b<0> 1 100",
                "R2 b<1> 1 200",
                "R3 1 b<2> 300",
                "XX b<0:2> complex_component2",
                ".ENDS complex_component",
                ".SUBCKT complex_component2 input<0:2>",
                "R1 input<0> 1 100",
                "R2 input<1> 1 200",
                ".ENDS complex_component2",
                "X1 <*2> (0 1) complex_component",
                "V1 0 a<0> 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }
    }
}

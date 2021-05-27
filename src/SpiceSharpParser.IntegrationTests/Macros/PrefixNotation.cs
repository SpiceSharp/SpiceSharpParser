using Xunit;

namespace SpiceSharpParser.IntegrationTests.Macros
{
    public class PrefixNotation : BaseTests
    {
        [Fact]
        public void Test01()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Prefix notation",
                ".SUBCKT complex_component b1 b2 b3",
                "R1 b1 1 100",
                "R2 b2 1 200",
                "R3 1 b3 300",
                ".ENDS complex_component",
                "X1 <*2> 0 1  complex_component",
                "V1 0 1 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void Test02()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Prefix notation",
                ".SUBCKT complex_component b1 b2 b3 b4",
                "R1 b1 1 100",
                "R2 b2 1 200",
                "R3 1 b3 300",
                ".ENDS complex_component",
                "X1 <*2> (0, 1)  complex_component",
                "V1 0 1 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void Test03()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Prefix notation",
                ".SUBCKT complex_component b1 b2 b3 b4",
                "R1 b1 1 100",
                "R2 b2 1 200",
                "R3 1 b3 300",
                ".ENDS complex_component",
                "X1 <*2> (0  1)  complex_component",
                "V1 0 1 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void Test04()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Prefix notation",
                ".SUBCKT complex_component b1 b2 b3 b4 b5 b6 b7 b8 b9 b10 b11 b12",
                "R1 b1 1 100",
                "R2 b2 1 200",
                "R3 1 b3 300",
                ".ENDS complex_component",
                "X1 <*2> (0, <*2> (1, 2), 2)  complex_component",
                "V1 0 1 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void Test05()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Prefix notation",
                ".SUBCKT complex_component b1 b2 b3 b4 b5 b6 b7 b8 b9 b10 b11 b12",
                "R1 b1 1 100",
                "R2 b2 1 200",
                "R3 1 b3 300",
                ".ENDS complex_component",
                "X1 <*2> (0 <*2> (1, 2) 2)  complex_component",
                "V1 0 1 1000",
                ".OP",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }
    }
}

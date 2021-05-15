using Xunit;

namespace SpiceSharpParser.IntegrationTests.Macros
{
    public class SuffixNotation : BaseTests
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
                "X1 a<0:3> complex_component",
                "V1 0 a<0> 1000",
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
                "Suffix notation",
                ".SUBCKT complex_component2 input<0, 1,(1:3)*2>",
                "R1 input<0> 1 100",
                "R2 input<1> 1 200",
                ".ENDS complex_component2",
                "X1 a<0:7> complex_component2",
                "V1 0 a<0> 1000",
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
                "Suffix notation",
                ".SUBCKT mysubckt BYTE<0:7> GND ENABLE",
                "R1<0:7> BYTE<0:7> 1 100",
                "C1 BYTE<0> 1 100",
                ".ENDS mysubckt",
                "X1<0:30, 31> REGISTER<0:31><0:7> GND ENABLE_IN mysubckt",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void Test04()
        {
            var result = ParseNetlistRaw(
                enableBusSyntax: true,
                "Suffix notation",

                ".SUBCKT mysubckt bus<0:15><0:7> GND ENABLE",
                "X1<0:12> bus<0:12><0:3> GND ENABLE mysubckt2",
                "C1 bus<0><1> 1 100",
                ".ENDS mysubckt",

                ".SUBCKT mysubckt2 bus2<0:3> GND ENABLE",
                ".ENDS mysubckt2",

                "X1<0:30, 31> bus<0:31><0:15><0:7> GND ENABLE_IN mysubckt",
                ".END");

            Assert.False(result.ValidationResult.HasError);
            Assert.False(result.ValidationResult.HasWarning);
        }
    }
}

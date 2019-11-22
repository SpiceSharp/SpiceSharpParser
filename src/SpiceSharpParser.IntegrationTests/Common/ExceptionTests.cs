using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class ExceptionTests : BaseTests
    {
        [Fact]
        public void When_UnknownComponent_Expect_Exception()
        {
            Assert.Throws<ModelReaders.Netlist.Spice.Exceptions.UnknownComponentException>(
                () => ParseNetlist(
                    "Exceptions",
                    "UMemory 1 0 1N914",
                    "V1_a 1 0 0.0",
                    ".DC V1_a -1 1.0 10e-3",
                    ".SAVE i(V1_a) v(1,0)",
                    ".END"));
        }

        [Fact]
        public void When_UnknownProperty_Expect_Exception()
        {
            Assert.Throws<ModelReaders.Netlist.Spice.Exceptions.GeneralReaderException>(
                () =>
                    ParseNetlist(
                        "Exceptions",
                        "V1 1 0 150",
                        "R1 1 0 10 a = 1.2",
                        ".SAVE I(R1)",
                        ".OP",
                        ".END"));
        }
    }
}
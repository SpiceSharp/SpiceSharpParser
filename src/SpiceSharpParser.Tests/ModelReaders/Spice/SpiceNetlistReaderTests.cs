using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice
{
    public class SpiceNetlistReaderTests
    {
        [Fact]
        public void Read()
        {
            // arrange
            var reader = new SpiceNetlistReader(new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => null));
            var netlist = new SpiceNetlist();

            // act
            var result = reader.Read(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}
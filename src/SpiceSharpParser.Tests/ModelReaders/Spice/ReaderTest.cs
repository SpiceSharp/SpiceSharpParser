using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Xunit;
using SpiceSharpParser.ModelsReaders.Netlist.Spice;

namespace SpiceSharpParser.Tests.ModelReaders.Spice
{
    public class ReaderTest
    {
        [Fact]
        public void ReadTest()
        {
            // arrange
            var reader = new SpiceNetlistReader(new SpiceNetlistReaderSettings());
            var netlist = new SpiceNetlist();

            // act
            var result = reader.Read(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}

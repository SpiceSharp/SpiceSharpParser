using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Text;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice
{
    public class SpiceNetlistReaderTests
    {
        [Fact]
        public void Read()
        {
            // arrange
            var reader = new SpiceNetlistReader(new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings(), () => null, Encoding.Default));
            var netlist = new SpiceNetlist(string.Empty, new Statements());

            // act
            var result = reader.Read(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}
using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.Model.Netlist.Spice;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using Xunit;
using SpiceSharpParser.ModelReader.Netlist.Spice;

namespace SpiceSharpParser.Tests.ModelReader.Spice
{
    public class ReaderTest
    {
        [Fact]
        public void ReadTest()
        {
            // arrange
            var processor = Substitute.For<IStatementsProcessor>();
            processor.Process(Arg.Any<Statements>(), Arg.Any<IProcessingContext>());

            var reader = new SpiceModelReader(processor);
            var netlist = new SpiceNetlist();

            // act
            var result = reader.Read(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}

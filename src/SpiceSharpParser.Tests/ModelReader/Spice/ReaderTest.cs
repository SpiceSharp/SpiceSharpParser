using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Processors;
using SpiceSharpParser.Model.Spice;
using SpiceSharpParser.Model.Spice.Objects;
using Xunit;
using SpiceSharpParser.ModelReader.Spice;

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

            var reader = new SpiceReader(processor);
            var netlist = new Netlist();

            // act
            var result = reader.Read(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}

using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests
{
    public class ConnectorTest
    {
        [Fact]
        public void TranslateTest()
        {
            // arrange
            var processor = Substitute.For<IStatementsProcessor>();
            processor.Process(Arg.Any<Statements>(), Arg.Any<IProcessingContext>());

            var connector = new Connector(processor);
            var netlist = new SpiceNetlist.Netlist();

            // act
            var result = connector.Translate(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}

using NSubstitute;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Model;
using SpiceSharpParser.Model.SpiceObjects;
using Xunit;

namespace SpiceSharpParser.Tests.Connector
{
    public class ConnectorTest
    {
        [Fact]
        public void TranslateTest()
        {
            // arrange
            var processor = Substitute.For<IStatementsProcessor>();
            processor.Process(Arg.Any<Statements>(), Arg.Any<IProcessingContext>());

            var connector = new SpiceSharpParser.Connector.Connector(processor);
            var netlist = new Netlist();

            // act
            var result = connector.Translate(netlist);

            // assert
            Assert.NotNull(result);
        }
    }
}

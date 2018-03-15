using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using Xunit;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Processors.EntityGenerators.Components
{
    public class RLCGeneratorTest
    {
        [Fact]
        public void GenerateSimpleResistor()
        {
            var context = Substitute.For<IProcessingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var generator = new RLCGenerator();
            var resistor = generator.Generate(new SpiceSharp.Identifier("x1.r1"), "R1", "r", parameters, context);

            Assert.NotNull(resistor);
            context.Received().SetParameter(resistor, "resistance", "1.2");
        }

        [Fact]
        public void GenerateSemiconductorResistor()
        {
            var context = Substitute.For<IProcessingContext>();
            context.When(a => a.SetParameter(Arg.Any<Entity>(), "L", "12")).Do(x => ((Entity)x[0]).SetParameter("l", 12));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new WordParameter("test"),
                new AssignmentParameter() { Name = "L", Value = "12" }
            };

            var generator = new RLCGenerator();
            var resistor = generator.Generate(new SpiceSharp.Identifier("x1.r1"), "R1", "r", parameters, context);

            Assert.NotNull(resistor);
            context.Received().SetParameter(resistor, "L", "12");
            context.Received().FindModel<ResistorModel>("test");
        }
    }
}

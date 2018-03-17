using NSubstitute;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Evaluation;
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

        [Fact]
        public void GenerateMutualInductance()
        {
            var context = Substitute.For<IProcessingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("12.3")
            };

            var generator = new RLCGenerator();
            var mut = generator.Generate(new SpiceSharp.Identifier("kR1"), "kR1", "k", parameters, context);

            Assert.NotNull(mut);
            Assert.IsType<MutualInductance>(mut);
            context.Received().SetParameter(mut, "k", "12.3");
        }

        [Fact]
        public void GenerateInductance()
        {
            var context = Substitute.For<IProcessingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3")
            };

            var generator = new RLCGenerator();
            var inductor = generator.Generate(new SpiceSharp.Identifier("lA3"), "lA3", "l", parameters, context);

            Assert.NotNull(inductor);
            Assert.IsType<Inductor>(inductor);
            context.Received().SetParameter(inductor, "inductance", "4.3");
        }

        // cA3 1 0 4.3
        [Fact]
        public void GenerateCapacitor()
        {
            var context = Substitute.For<IProcessingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3")
            };

            var generator = new RLCGenerator();
            var cap = generator.Generate(new SpiceSharp.Identifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);
            context.Received().SetParameter(cap, "capacitance", "4.3");
        }

        // cA3 1 0 4.3 ic = 13.3
        [Fact]
        public void GenerateCapacitorWithIC()
        {
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.When(a => a.SetParameters(
               Arg.Any<Capacitor>(),
               Arg.Any<ParameterCollection>()
               )).Do(x => {
                   foreach (var parameter in (ParameterCollection)x[1])
                   {
                       if (parameter is AssignmentParameter ag)
                       {
                           ((Entity)x[0]).SetParameter(ag.Name.ToLower(), evaluator.EvaluateDouble(ag.Value));
                       }
                   }
               });

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3"),
                new AssignmentParameter() { Name = "ic", Value = "13.3"}
            };

            var generator = new RLCGenerator();
            var cap = generator.Generate(new SpiceSharp.Identifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);
            context.Received().SetParameter(cap, "capacitance", "4.3");
            Assert.Equal(13.3, cap.ParameterSets.GetParameter("ic").Value);
        }

        // cA3 1 0 CModel L=10u W=1u Ic=12
        [Fact]
        public void GenerateSemicondutorCapacitor()
        {
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.When(a => a.SetParameters(
                Arg.Any<Capacitor>(),
                Arg.Any<ParameterCollection>()
                )).Do(x => {
                    foreach (var parameter in (ParameterCollection)x[1])
                    {
                        if (parameter is AssignmentParameter ag) {
                            ((Entity)x[0]).SetParameter(ag.Name.ToLower(), evaluator.EvaluateDouble(ag.Value));
                        }
                    }
                });

            context.FindModel<CapacitorModel>(Arg.Any<string>()).Returns(new CapacitorModel("CModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new WordParameter("CModel"),
                new AssignmentParameter() { Name = "L", Value = "10u" },
                new AssignmentParameter() { Name = "W", Value = "1u" },
                new AssignmentParameter() { Name = "Ic", Value = "12" }
            };

            var generator = new RLCGenerator();
            var cap = generator.Generate(new SpiceSharp.Identifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);

            context.Received().SetParameters(cap, Arg.Is<ParameterCollection>(
               p => ((AssignmentParameter)p[0]).Name == "L" &&
                    ((AssignmentParameter)p[0]).Value == "10u"));
        }
    }
}

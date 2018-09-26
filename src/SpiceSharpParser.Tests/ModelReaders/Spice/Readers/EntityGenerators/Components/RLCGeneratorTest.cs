using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components
{
    public class RLCGeneratorTest
    {
        [Fact]
        public void GenerateSimpleResistor()
        {
            var context = Substitute.For<IReadingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var generator = new RLCGenerator();
            var resistor = generator.Generate(new SpiceSharp.StringIdentifier("r1"), "R1", "r", parameters, context);

            Assert.NotNull(resistor);
            context.Received().SetParameter(resistor, "resistance", "1.2");
        }

        [Fact]
        public void GenerateSemiconductorResistor()
        {
            var context = Substitute.For<IReadingContext>();
            context.When(a => a.SetParameter(Arg.Any<Entity>(), "L", "12")).Do(x => ((Entity)x[0]).SetParameter("l", 12));
            context.ModelsRegistry.FindModel<ResistorModel>(Arg.Any<string>()).Returns(new ResistorModel("test"));
            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new WordParameter("test"),
                new AssignmentParameter() { Name = "L", Value = "12" }
            };

            var generator = new RLCGenerator();
            var resistor = generator.Generate(new SpiceSharp.StringIdentifier("r1"), "R1", "r", parameters, context);

            Assert.NotNull(resistor);
            context.Received().SetParameter(resistor, "L", "12");
            context.Received().ModelsRegistry.FindModel<ResistorModel>("test");
        }

        [Fact]
        public void GenerateMutualInductance()
        {
            var context = Substitute.For<IReadingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("12.3")
            };

            var generator = new RLCGenerator();
            var mut = generator.Generate(new SpiceSharp.StringIdentifier("kR1"), "kR1", "k", parameters, context);

            Assert.NotNull(mut);
            Assert.IsType<MutualInductance>(mut);
            context.Received().SetParameter(mut, "k", "12.3");
        }

        [Fact]
        public void GenerateInductance()
        {
            var context = Substitute.For<IReadingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3")
            };

            var generator = new RLCGenerator();
            var inductor = generator.Generate(new SpiceSharp.StringIdentifier("lA3"), "lA3", "l", parameters, context);

            Assert.NotNull(inductor);
            Assert.IsType<Inductor>(inductor);
            context.Received().SetParameter(inductor, "inductance", "4.3");
        }

        // cA3 1 0 4.3
        [Fact]
        public void GenerateCapacitor()
        {
            var context = Substitute.For<IReadingContext>();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3")
            };

            var generator = new RLCGenerator();
            var cap = generator.Generate(new SpiceSharp.StringIdentifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);
            context.Received().SetParameter(cap, "capacitance", "4.3");
        }

        // cA3 1 0 4.3 ic = 13.3
        [Fact]
        public void GenerateCapacitorWithIC()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.When(a => a.SetParameter(
               Arg.Any<Capacitor>(),
               Arg.Any<string>(),
               Arg.Any<string>()
               )).Do(x =>
               {
                   ((Entity)x[0]).SetParameter(((string)x[1]).ToLower(), evaluator.EvaluateDouble((string)x[2]));
               });

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("4.3"),
                new AssignmentParameter() { Name = "ic", Value = "13.3"}
            };

            var generator = new RLCGenerator();
            var cap = generator.Generate(new SpiceSharp.StringIdentifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);
            context.Received().SetParameter(cap, "capacitance", "4.3");
            Assert.Equal(13.3, cap.ParameterSets.GetParameter<double>("ic").Value);
        }

        // cA3 1 0 CModel L=10u W=1u Ic=12
        [Fact]
        public void GenerateSemicondutorCapacitor()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.When(a => a.SetParameter(
               Arg.Any<Capacitor>(),
               Arg.Any<string>(),
               Arg.Any<string>()
               )).Do(x =>
               {
                   ((Entity)x[0]).SetParameter(((string)x[1]).ToLower(), evaluator.EvaluateDouble((string)x[2]));
               });
            context.ModelsRegistry.FindModel<CapacitorModel>(Arg.Any<string>()).Returns(new CapacitorModel("CModel"));

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
            var cap = generator.Generate(new SpiceSharp.StringIdentifier("cA3"), "cA3", "c", parameters, context);

            Assert.NotNull(cap);
            Assert.IsType<Capacitor>(cap);

            context.Received().SetParameter(cap, "L", "10u");
            Assert.Equal(12, cap.ParameterSets.GetParameter<double>("ic").Value);

        }
    }
}

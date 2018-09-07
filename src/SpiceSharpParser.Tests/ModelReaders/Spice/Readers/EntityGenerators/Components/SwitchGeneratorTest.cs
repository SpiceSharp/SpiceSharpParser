using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGeneratorTest
    {
        [Fact]
        public void GenerateVoltageSwitch()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.StochasticModelsRegistry.FindBaseModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("2"),
                new ValueParameter("0"),
                new WordParameter("model"),
                new WordParameter("On")
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate(new SpiceSharp.StringIdentifier("s1"), "s1", "s", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<VoltageSwitch>(@switch);
            Assert.True(@switch.ParameterSets.Get<SpiceSharp.Components.VoltageSwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateVoltageSwitchOff()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.StochasticModelsRegistry.FindBaseModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("2"),
                new ValueParameter("0"),
                new WordParameter("model"),
                new WordParameter("Off")
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate(new SpiceSharp.StringIdentifier("s1"), "s1", "s", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<VoltageSwitch>(@switch);
            Assert.False(@switch.ParameterSets.Get<SpiceSharp.Components.VoltageSwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateCurrentSwitch()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.StochasticModelsRegistry.FindBaseModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new IdentifierParameter("V3"),
                new WordParameter("model"),
                new WordParameter("On")
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate(new SpiceSharp.StringIdentifier("w1"), "w1", "w", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<CurrentSwitch>(@switch);
            Assert.True(@switch.ParameterSets.Get<SpiceSharp.Components.CurrentSwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateCurrentSwitchOff()
        {
            var evaluator = new SpiceEvaluator();
            var context = Substitute.For<IReadingContext>();
            context.StochasticModelsRegistry.FindBaseModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new IdentifierParameter("V3"),
                new WordParameter("model"),
                new WordParameter("Off")
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate(new SpiceSharp.StringIdentifier("w1"), "w1", "w", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<CurrentSwitch>(@switch);
            Assert.False(@switch.ParameterSets.Get<SpiceSharp.Components.CurrentSwitchBehaviors.BaseParameters>().ZeroState);
        }
    }
}

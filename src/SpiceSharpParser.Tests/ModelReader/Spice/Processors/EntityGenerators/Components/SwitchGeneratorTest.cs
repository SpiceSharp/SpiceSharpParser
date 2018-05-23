using NSubstitute;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Spice.Processors.EntityGenerators.Components;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors.EntityGenerators.Components
{
    public class SwitchGeneratorTest
    {
        [Fact]
        public void GenerateVoltageSwitch()
        {
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.FindModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

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
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.FindModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

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
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.FindModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

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
            var evaluator = new Evaluator();
            var context = Substitute.For<IProcessingContext>();
            context.FindModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

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

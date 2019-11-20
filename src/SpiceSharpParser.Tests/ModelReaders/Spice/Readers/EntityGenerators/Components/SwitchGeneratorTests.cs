using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components
{
    public class SwitchGeneratorTests
    {
        [Fact]
        public void GenerateVoltageSwitch()
        {
            var context = Substitute.For<ICircuitContext>();
            context.ModelsRegistry.FindModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

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
            var @switch = generator.Generate("s1", "s1", "s", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<VoltageSwitch>(@switch);
            Assert.True(@switch.ParameterSets.Get<SpiceSharp.Components.SwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateVoltageSwitchOff()
        {
            var context = Substitute.For<ICircuitContext>();
            context.ModelsRegistry.FindModel<VoltageSwitchModel>(Arg.Any<string>()).Returns(new VoltageSwitchModel("SModel"));

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
            var @switch = generator.Generate("s1", "s1", "s", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<VoltageSwitch>(@switch);
            Assert.False(@switch.ParameterSets.Get<SpiceSharp.Components.SwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateCurrentSwitch()
        {
            var context = Substitute.For<ICircuitContext>();
            context.ModelsRegistry.FindModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new IdentifierParameter("V3"),
                new WordParameter("model"),
                new WordParameter("On")
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate("w1", "w1", "w", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<CurrentSwitch>(@switch);
            Assert.True(@switch.ParameterSets.Get<SpiceSharp.Components.SwitchBehaviors.BaseParameters>().ZeroState);
        }

        [Fact]
        public void GenerateCurrentSwitchOff()
        {
            var context = Substitute.For<ICircuitContext>();
            context.ModelsRegistry.FindModel<CurrentSwitchModel>(Arg.Any<string>()).Returns(new CurrentSwitchModel("WModel"));

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new IdentifierParameter("V3"),
                new WordParameter("model"),
                new WordParameter("Off"),
            };

            var generator = new SwitchGenerator();
            var @switch = generator.Generate("w1", "w1", "w", parameters, context);

            Assert.NotNull(@switch);
            Assert.IsType<CurrentSwitch>(@switch);
            Assert.False(@switch.ParameterSets.Get<SpiceSharp.Components.SwitchBehaviors.BaseParameters>().ZeroState);
        }
    }
}

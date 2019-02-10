using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components.Sources
{
    public class VoltageSourceGeneratorTests
    {
        [Fact]
        public void GenerateDCVoltageSourceWithoutDC()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
        }

        [Fact]
        public void GenerateDCVoltageSourceWithDC()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2") // dc-value
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
        }

        [Fact]
        public void GenerateDCVoltageSourceWithoutVoltage()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);
        }

        [Fact]
        public void GenerateDCACVoltageSource()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2"), // dc-value
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
                new ValueParameter("0"), // ac-phase
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
            context.Received().SetParameter(entity, "acmag", "12", true);
            context.Received().SetParameter(entity, "acphase", "0", true);
        }

        [Fact]
        public void GenerateDCACWithoutPhaseVoltageSource()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2"), // dc-value
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
            context.Received().SetParameter(entity, "acmag", "12", true);
        }

        [Fact]
        public void GeneratACVoltageWithoutPhaseSource()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13") // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "acmag", "13", true);
        }

        [Fact]
        public void GeneratACVoltageWithoutPhaseSineSource()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13"), // ac-magnitude
                new BracketParameter()
                {
                    Name = "sine",
                    Parameters = new ParameterCollection()
                    {
                        new ValueParameter("0"),
                        new ValueParameter("1"),
                        new ValueParameter("2000")
                    }
                }
            };

            var context = Substitute.For<IReadingContext>();
            context.WaveformReader.Supports("sine", context).Returns(true);

            var entity = generator.Generate("v1", "v1", "v", parameters, context);
            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "acmag", "13", true);
        }

        [Fact]
        public void GeneratACVoltageSourceWithPhase()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13"), // ac-magnitude
                new ValueParameter("2") // ac-phase
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("x1.v1", "v1", "v", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageSource>(entity);

            context.Received().SetParameter(entity, "acmag", "13", true);
            context.Received().SetParameter(entity, "acphase", "2", true);
        }

        [Fact]
        public void GenerateCCVS()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("v1"), // controling source
                new ValueParameter("3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            var nameGenerator = Substitute.For<IObjectNameGenerator>();
            nameGenerator.Generate(Arg.Any<string>()).Returns("v1");
            context.ComponentNameGenerator.Returns(nameGenerator);
            var entity = generator.Generate("x1.h1", "h1", "h", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentControlledVoltageSource>(entity);
            Assert.Equal("v1", ((CurrentControlledVoltageSource)entity).ControllingName.ToString());
            context.Received().SetParameter(entity, "gain", "3", true);
        }

        [Fact]
        public void GeneratVCVS()
        {
            var generator = new VoltageSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new ValueParameter("2"), // pin
                new ValueParameter("3"), // pin
                new ValueParameter("1.3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("x1.e1", "e1", "e", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageControlledVoltageSource>(entity);
            context.Received().SetParameter(entity, "gain", "1.3", true);
        }
    }
}

using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharp;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components.Sources
{
    public class CurrentSourceGeneratorTest
    {
        [Fact]
        public void GenerateDCCurrentSourceWithoutDCTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
        }

        [Fact]
        public void GenerateDCCurrentSourceWithDCTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2") // dc-value
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
        }

        [Fact]
        public void GenerateDCCurrentSourceWithoutCurrentTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);
        }

        [Fact]
        public void GenerateDCACCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator();

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
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
            context.Received().SetParameter(entity, "acmag", "12", true);
            context.Received().SetParameter(entity, "acphase", "0", true);
        }

        [Fact]
        public void GenerateDCACWithoutPhaseCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator();

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
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", "1.2", true);
            context.Received().SetParameter(entity, "acmag", "12", true);
        }

        [Fact]
        public void GenerateACWithoutPhaseCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", "12", true);
        }

        [Fact]
        public void GenerateACWithoutPhaseWithSineCurrentSourceTest()
        {
            var sine = new Sine();
            var generator = new CurrentSourceGenerator();
            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
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
            context.StatementsReader = new SpiceStatementsReader(Substitute.For<IMapper<BaseControl>>(), Substitute.For<IMapper<IModelGenerator>>(), Substitute.For<IMapper<IComponentGenerator>>());

            context.WaveformReader = Substitute.For<IWaveformReader>();
            context.WaveformReader.Generate(Arg.Any<BracketParameter>(), Arg.Any<IReadingContext>()).Returns(sine);

            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", "12", true);
        }

        [Fact]
        public void GenerateWaveformCurrentSourceTest()
        {
            var sine = new Sine();
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
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
            context.StatementsReader = new SpiceStatementsReader(
                Substitute.For<IMapper<BaseControl>>(),
                Substitute.For<IMapper<IModelGenerator>>(),
                Substitute.For<IMapper<IComponentGenerator>>());
            context.WaveformReader = Substitute.For<IWaveformReader>();
            context.WaveformReader.Generate(Arg.Any<BracketParameter>(), Arg.Any<IReadingContext>()).Returns(sine);

            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);
        }

        [Fact]
        public void GeneratACCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13") // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", "13", true);
        }

        [Fact]
        public void GeneratACCurrentSourceWithPhaseTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13"), // ac-magnitude
                new ValueParameter("2") // ac-phase
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", "13", true);
            context.Received().SetParameter(entity, "acphase", "2", true);
        }

        [Fact]
        public void GeneratCCCSTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("v1"), // controling source
                new ValueParameter("3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            context.ComponentNameGenerator.Generate(Arg.Any<string>()).Returns("v1");

            var entity = generator.Generate("f1", "f1", "f", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentControlledCurrentSource>(entity);
            Assert.Equal("v1", ((CurrentControlledCurrentSource)entity).ControllingName);
            context.Received().SetParameter(entity, "gain", "3", true);
        }

        [Fact]
        public void GeneratCCVSTest()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new ValueParameter("2"), // pin
                new ValueParameter("3"), // pin
                new ValueParameter("1.3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate("g1", "g1", "g", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageControlledCurrentSource>(entity);
            context.Received().SetParameter(entity, "gain", "1.3", true);
        }
    }
}

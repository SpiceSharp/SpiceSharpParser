using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharp;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components.Sources
{
    public class CurrentSourceGeneratorTests
    {
        [Fact]
        public void GenerateDCCurrentSourceWithoutDC()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", parameters[2], true);
        }

        [Fact]
        public void GenerateDCCurrentSourceWithDC()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2") // dc-value
            };

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", parameters[3], true);
        }

        [Fact]
        public void GenerateDCCurrentSourceWithoutCurrent()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
            };

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);
        }

        [Fact]
        public void GenerateDCACCurrentSource()
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

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", parameters[3], true);
            context.Received().SetParameter(entity, "acmag", parameters[5], true);
            context.Received().SetParameter(entity, "acphase", parameters[6], true);
        }

        [Fact]
        public void GenerateDCACWithoutPhaseCurrentSource()
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

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "dc", parameters[3], true);
            context.Received().SetParameter(entity, "acmag", parameters[5], true);
        }

        [Fact]
        public void GenerateACWithoutPhaseCurrentSource()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
            };

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", parameters[3], true);
        }

        [Fact]
        public void GenerateACWithoutPhaseWithSineCurrentSource()
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

            var context = Substitute.For<ICircuitContext>();
            context.StatementsReader = new SpiceStatementsReader(Substitute.For<IMapper<BaseControl>>(), Substitute.For<IMapper<IModelGenerator>>(), Substitute.For<IMapper<IComponentGenerator>>());

            context.WaveformReader = Substitute.For<IWaveformReader>();
            context.WaveformReader.Supports("sine", context).Returns(true);
            context.WaveformReader.Generate(Arg.Any<string>(), Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>()).Returns(sine);


            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", parameters[3], true);
        }

        [Fact]
        public void GenerateWaveformCurrentSource()
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

            var context = Substitute.For<ICircuitContext>();
            context.StatementsReader = new SpiceStatementsReader(
                Substitute.For<IMapper<BaseControl>>(),
                Substitute.For<IMapper<IModelGenerator>>(),
                Substitute.For<IMapper<IComponentGenerator>>());
            context.WaveformReader = Substitute.For<IWaveformReader>();
            context.WaveformReader.Supports("sine", context).Returns(true);

            context.WaveformReader.Generate(Arg.Any<string>(), Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>()).Returns(sine);

            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);
        }

        [Fact]
        public void GeneratACCurrentSource()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13") // ac-magnitude
            };

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", parameters[3], true);
        }

        [Fact]
        public void GeneratACCurrentSourceWithPhase()
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

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("i1", "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "acmag", parameters[3], true);
            context.Received().SetParameter(entity, "acphase", parameters[4], true);
        }

        [Fact]
        public void GeneratCCCS()
        {
            var generator = new CurrentSourceGenerator();

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("v1"), // controling source
                new ValueParameter("3"), // gain
            };

            var context = Substitute.For<ICircuitContext>();
            context.NameGenerator = Substitute.For<INameGenerator>();
            context.NameGenerator.GenerateObjectName(Arg.Any<string>()).Returns(x => x[0].ToString());

            var entity = generator.Generate("f1", "f1", "f", parameters, context);
            Assert.NotNull(entity);
            Assert.IsType<CurrentControlledCurrentSource>(entity);
            Assert.Equal("v1", ((CurrentControlledCurrentSource)entity).ControllingName);
            context.Received().SetParameter(entity, "gain", parameters[3], true);
        }

        [Fact]
        public void GeneratVCCS()
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

            var context = Substitute.For<ICircuitContext>();
            var entity = generator.Generate("g1", "g1", "g", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageControlledCurrentSource>(entity);
            context.Received().SetParameter(entity, "gain", parameters[4], true);
        }
    }
}

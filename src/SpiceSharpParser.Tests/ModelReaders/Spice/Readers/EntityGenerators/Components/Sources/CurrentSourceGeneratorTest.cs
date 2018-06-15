using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components.Sources
{
    public class CurrentSourceGeneratorTest
    {
        [Fact]
        public void GenerateDCCurrentSourceWithoutDCTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"),
                new ValueParameter("0"),
                new ValueParameter("1.2")
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "dc", "1.2");
        }

        [Fact]
        public void GenerateDCCurrentSourceWithDCTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
                new ValueParameter("1.2") // dc-value
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "dc", "1.2");
        }

        [Fact]
        public void GenerateDCCurrentSourceWithoutCurrentTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Dc"), // dc
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);
        }

        [Fact]
        public void GenerateDCACCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

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
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "dc", "1.2");
            context.Received().SetEntityParameter(entity, "acmag", "12");
            context.Received().SetEntityParameter(entity, "acphase", "0");
        }

        [Fact]
        public void GenerateDCACWithoutPhaseCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

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
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "dc", "1.2");
            context.Received().SetEntityParameter(entity, "acmag", "12");
        }

        [Fact]
        public void GenerateACWithoutPhaseCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("Ac"), // ac
                new ValueParameter("12"), // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "acmag", "12");
        }

        [Fact]
        public void GenerateACWithoutPhaseWithSineCurrentSourceTest()
        {
            var waveformReader = Substitute.For<IWaveformReader>();

            var sine = new Sine();
            waveformReader.Generate(
                Arg.Any<BracketParameter>(), 
                Arg.Any<IReadingContext>()).Returns(
               sine);

            var generator = new CurrentSourceGenerator(waveformReader);

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
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "acmag", "12");
            context.Received().SetParameter(entity, "waveform", sine);
        }

        [Fact]
        public void GenerateWaveformCurrentSourceTest()
        {
            var waveformReader = Substitute.For<IWaveformReader>();

            var sine = new Sine();
            waveformReader.Generate(
                Arg.Any<BracketParameter>(),
                Arg.Any<IReadingContext>()).Returns(
               sine);

            var generator = new CurrentSourceGenerator(waveformReader);

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
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetParameter(entity, "waveform", sine);
        }

        [Fact]
        public void GeneratACCurrentSourceTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13") // ac-magnitude
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "acmag", "13");
        }

        [Fact]
        public void GeneratACCurrentSourceWithPhaseTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("ac"), // ac
                new ValueParameter("13"), // ac-magnitude
                new ValueParameter("2") // ac-phase
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("i1"), "i1", "i", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentSource>(entity);

            context.Received().SetEntityParameter(entity, "acmag", "13");
            context.Received().SetEntityParameter(entity, "acphase", "2");
        }

        [Fact]
        public void GeneratCCCSTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new WordParameter("v1"), // controling source
                new ValueParameter("3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("f1"), "f1", "f", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<CurrentControlledCurrentSource>(entity);
            Assert.Equal("v1", ((CurrentControlledCurrentSource)entity).ControllingName.ToString());
            context.Received().SetEntityParameter(entity, "gain", "3");
        }

        [Fact]
        public void GeneratCCVSTest()
        {
            var generator = new CurrentSourceGenerator(Substitute.For<IWaveformReader>());

            var parameters = new ParameterCollection
            {
                new ValueParameter("1"), // pin
                new ValueParameter("0"), // pin
                new ValueParameter("2"), // pin
                new ValueParameter("3"), // pin
                new ValueParameter("1.3"), // gain
            };

            var context = Substitute.For<IReadingContext>();
            var entity = generator.Generate(new SpiceSharp.StringIdentifier("g1"), "g1", "g", parameters, context);

            Assert.NotNull(entity);
            Assert.IsType<VoltageControlledCurrentSource>(entity);
            context.Received().SetEntityParameter(entity, "gain", "1.3");
        }
    }
}

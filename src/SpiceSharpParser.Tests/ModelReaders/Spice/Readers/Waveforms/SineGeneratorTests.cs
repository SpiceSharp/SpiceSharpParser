using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.Waveforms
{
    public class SineGeneratorTests
    {
        [Fact]
        public void GenerateWhenThereAre7Parameters()
        {
            // prepare
            var bracketParameter = new BracketParameter()
            {
                Name = "sine",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1"),
                    new ValueParameter("10"),
                    new ValueParameter("12"),
                    new ValueParameter("12"),
                    new ValueParameter("12"),
                    new ValueParameter("12"),
                    new ValueParameter("30"),
                }
            };

            var readingContext = Substitute.For<IReadingContext>();

            // act
            var generator = new SineGenerator();
            Assert.Throws<WrongParametersCountException>(() => generator.Generate(bracketParameter.Parameters, readingContext));
        }

        [Fact]
        public void GenerateWhenThereAreNoneParameters()
        {
            // prepare
            var bracketParameter = new BracketParameter()
            {
                Name = "sine",
                Parameters = new ParameterCollection()
                {
                }
            };

            var readingContext = Substitute.For<IReadingContext>();

            // act
            var generator = new SineGenerator();
            Assert.Throws<WrongParametersCountException>(() => generator.Generate(bracketParameter.Parameters, readingContext));
        }

        [Fact]
        public void GenerateWhenThereAre3Parameters()
        {
            // prepare
            var bracketParameter = new BracketParameter()
            {
                Name = "sine",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1"),
                    new ValueParameter("10"),
                    new ValueParameter("12"),
                }
            };

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.EvaluateDouble(Arg.Any<Parameter>()).Returns(x => double.Parse(((Parameter)x[0]).Image));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter.Parameters, readingContext);

            // assert
            Assert.NotNull(wave);
            Assert.IsType<Sine>(wave);
            Assert.Equal(1, (wave as Sine).Offset.Value);
            Assert.Equal(10, (wave as Sine).Amplitude.Value);
            Assert.Equal(12, (wave as Sine).Frequency.Value);
        }

        [Fact]
        public void GenerateWhenThereAre4Parameters()
        {
            // prepare
            var bracketParameter = new BracketParameter()
            {
                Name = "sine",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1"),
                    new ValueParameter("10"),
                    new ValueParameter("12"),
                    new ValueParameter("15"),
                }
            };

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.EvaluateDouble(Arg.Any<Parameter>()).Returns(x => double.Parse(((Parameter)x[0]).Image));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter.Parameters, readingContext);

            // assert
            Assert.NotNull(wave);
            Assert.IsType<Sine>(wave);
            Assert.Equal(1, (wave as Sine).Offset.Value);
            Assert.Equal(10, (wave as Sine).Amplitude.Value);
            Assert.Equal(12, (wave as Sine).Frequency.Value);
            Assert.Equal(15, (wave as Sine).Delay.Value);
        }

        [Fact]
        public void GenerateWhenThereAre5Parameters()
        {
            // prepare
            var bracketParameter = new BracketParameter()
            {
                Name = "sine",
                Parameters = new ParameterCollection()
                {
                    new ValueParameter("1"),
                    new ValueParameter("10"),
                    new ValueParameter("12"),
                    new ValueParameter("15"),
                    new ValueParameter("5"),
                }
            };

            var readingContext = Substitute.For<IReadingContext>();
            readingContext.EvaluateDouble(Arg.Any<Parameter>()).Returns(x => double.Parse(((Parameter)x[0]).Image));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter.Parameters, readingContext);

            // assert
            Assert.NotNull(wave);
            Assert.IsType<Sine>(wave);
            Assert.Equal(1, (wave as Sine).Offset.Value);
            Assert.Equal(10, (wave as Sine).Amplitude.Value);
            Assert.Equal(12, (wave as Sine).Frequency.Value);
            Assert.Equal(15, (wave as Sine).Delay.Value);
            Assert.Equal(5, (wave as Sine).Theta.Value);
        }
    }
}

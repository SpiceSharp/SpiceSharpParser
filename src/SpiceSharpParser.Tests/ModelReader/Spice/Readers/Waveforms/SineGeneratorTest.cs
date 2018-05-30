using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Readers.Waveforms
{
    public class SineGeneratorTest
    {
        [Fact]
        public void GenerateWhenThereAre6ParametersTest()
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
                }
            };

            var readingContext = Substitute.For<IReadingContext>();

            // act
            var generator = new SineGenerator();
            Assert.Throws<WrongParametersCountException>(() => generator.Generate(bracketParameter, readingContext));
        }

        [Fact]
        public void GenerateWhenThereAreNoneParametersTest()
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
            Assert.Throws<WrongParametersCountException>(() => generator.Generate(bracketParameter, readingContext));
        }

        [Fact]
        public void GenerateWhenThereAre3ParametersTest()
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
            readingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter, readingContext);

            // assert
            Assert.NotNull(wave);
            Assert.IsType<Sine>(wave);
            Assert.Equal(1, (wave as Sine).Offset.Value);
            Assert.Equal(10, (wave as Sine).Amplitude.Value);
            Assert.Equal(12, (wave as Sine).Frequency.Value);
        }

        [Fact]
        public void GenerateWhenThereAre4ParametersTest()
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
            readingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter, readingContext);

            // assert
            Assert.NotNull(wave);
            Assert.IsType<Sine>(wave);
            Assert.Equal(1, (wave as Sine).Offset.Value);
            Assert.Equal(10, (wave as Sine).Amplitude.Value);
            Assert.Equal(12, (wave as Sine).Frequency.Value);
            Assert.Equal(15, (wave as Sine).Delay.Value);
        }

        [Fact]
        public void GenerateWhenThereAre5ParametersTest()
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
            readingContext.ParseDouble(Arg.Any<string>()).Returns(x => double.Parse((string)x[0]));

            // act
            var generator = new SineGenerator();
            var wave = generator.Generate(bracketParameter, readingContext);

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

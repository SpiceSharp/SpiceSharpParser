using NSubstitute;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    public class WaveformReaderTests
    {
        [Fact]
        public void Generate()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<BracketParameter>(), Arg.Any<IReadingContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IMapper<WaveformGenerator>>();
            waveFormRegistry.Contains("func", false).Returns(true);
            waveFormRegistry.Get(Arg.Any<string>(), Arg.Any<bool>()).Returns(waveFormGenerator);

            var bracketParameter = new Models.Netlist.Spice.Objects.Parameters.BracketParameter();
            bracketParameter.Name = "FUNc";
            var readingContext = Substitute.For<IReadingContext>();
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            // act
            WaveformReader waveformReader = new WaveformReader(waveFormRegistry);
            var waveForm = waveformReader.Generate(bracketParameter, readingContext);

            // assert
            Assert.IsType<Sine>(waveForm);
        }

        [Fact]
        public void NotSupportedGenerate()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<BracketParameter>(), Arg.Any<IReadingContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IMapper<WaveformGenerator>>();
            waveFormRegistry.Contains("func", true).Returns(true);
            waveFormRegistry.Get(Arg.Any<string>(), Arg.Any<bool>()).Returns(waveFormGenerator);

            var bracketParameter = new BracketParameter();
            bracketParameter.Name = "func2";
            var readingContext = Substitute.For<IReadingContext>();
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            // act + assert
            WaveformReader waveformReader = new WaveformReader(waveFormRegistry);
            Assert.Throws<Exception>(() => waveformReader.Generate(bracketParameter, readingContext));
        }
    }
}

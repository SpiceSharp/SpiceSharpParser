using NSubstitute;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers
{
    using SpiceSharpParser.Models.Netlist.Spice.Objects;

    public class WaveformReaderTests
    {
        [Fact]
        public void Generate()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IMapper<WaveformGenerator>>();
            waveFormRegistry.ContainsKey("func", false).Returns(true);
            waveFormRegistry.GetValue(Arg.Any<string>(), Arg.Any<bool>()).Returns(waveFormGenerator);
            WaveformGenerator value;
            waveFormRegistry.TryGetValue("func", false, out value).Returns(
                x =>
                    {
                        x[2] = waveFormGenerator;
                        return true;
                    });
            var readingContext = Substitute.For<ICircuitContext>();
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            // act
            WaveformReader waveformReader = new WaveformReader(waveFormRegistry);
            var waveForm = waveformReader.Generate("func", new ParameterCollection(), readingContext);

            // assert
            Assert.IsType<Sine>(waveForm);
        }

        [Fact]
        public void NotSupportedGenerate()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<ParameterCollection>(), Arg.Any<ICircuitContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IMapper<WaveformGenerator>>();
            waveFormRegistry.ContainsKey("func", true).Returns(true);
            waveFormRegistry.GetValue(Arg.Any<string>(), Arg.Any<bool>()).Returns(waveFormGenerator);

            var readingContext = Substitute.For<ICircuitContext>();
            readingContext.CaseSensitivity.Returns(new SpiceNetlistCaseSensitivitySettings());

            var service = Substitute.For<IResultService>();
            service.Validation.Returns(new SpiceNetlistValidationResult());
            readingContext.Result.Returns(service);

            // act + assert
            WaveformReader waveformReader = new WaveformReader(waveFormRegistry);
            waveformReader.Generate("func2", new ParameterCollection(), readingContext);

            Assert.Single(readingContext.Result.Validation);
        }
    }
}
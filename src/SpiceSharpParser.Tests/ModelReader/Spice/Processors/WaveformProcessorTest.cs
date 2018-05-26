using NSubstitute;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Waveforms;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Components;
using System;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Processors
{
    public class WaveformProcessorTest
    {
        [Fact]
        public void GenerateTest()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<BracketParameter>(), Arg.Any<IProcessingContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IWaveformRegistry>();
            waveFormRegistry.Supports("func").Returns(true);
            waveFormRegistry.Get(Arg.Any<string>()).Returns(waveFormGenerator);

            var bracketParameter = new Model.Netlist.Spice.Objects.Parameters.BracketParameter();
            bracketParameter.Name = "FUNc";
            var processingContext = Substitute.For<IProcessingContext>();

            // act
            WaveformProcessor waveformProcessor = new WaveformProcessor(waveFormRegistry);
            var waveForm = waveformProcessor.Generate(bracketParameter, processingContext);

            // assert
            Assert.IsType<Sine>(waveForm);
        }

        [Fact]
        public void NotSupportedGenerateTest()
        {
            // arrange
            var waveFormGenerator = Substitute.For<WaveformGenerator>();
            waveFormGenerator.Generate(Arg.Any<BracketParameter>(), Arg.Any<IProcessingContext>()).Returns(new Sine());

            var waveFormRegistry = Substitute.For<IWaveformRegistry>();
            waveFormRegistry.Supports("func").Returns(true);
            waveFormRegistry.Get(Arg.Any<string>()).Returns(waveFormGenerator);

            var bracketParameter = new Model.Netlist.Spice.Objects.Parameters.BracketParameter();
            bracketParameter.Name = "func2";
            var processingContext = Substitute.For<IProcessingContext>();

            // act + assert
            WaveformProcessor waveformProcessor = new WaveformProcessor(waveFormRegistry);
            Assert.Throws<Exception>(() => waveformProcessor.Generate(bracketParameter, processingContext));
        }
    }
}

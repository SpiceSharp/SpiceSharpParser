using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Extensions;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Extensions
{
    public class IProcessingContextExtensionsTest
    {
        [Fact]
        public void SetParametersNoWarnings()
        {
            // prepare
            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.SetParameter(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var resultService = Substitute.For<IResultService>();
            processingContext.Result.Returns(resultService);

            var parameters = new ParameterCollection
            {
                new AssignmentParameter() { Name = "ron", Value = "100" },
                new AssignmentParameter() { Name = "roff", Value = "1001" },
            };

            // act
            processingContext.SetParameters(new VoltageSwitchModel("S1"), parameters);

            // assert
            resultService.Received(0).AddWarning(Arg.Any<string>());
        }

        [Fact]
        public void SetParametersWarnings()
        {
            // prepare
            var processingContext = Substitute.For<IProcessingContext>();
            processingContext.SetParameter(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var resultService = Substitute.For<IResultService>();
            processingContext.Result.Returns(resultService);

            var parameters = new ParameterCollection
            {
                new AssignmentParameter() { Name = "ron", Value = "100" },
                new AssignmentParameter() { Name = "roff", Value = "1001" },
            };

            // act
            processingContext.SetParameters(new VoltageSwitchModel("S1"), parameters);

            // assert
            resultService.Received(2).AddWarning(Arg.Any<string>());
        }
    }
}

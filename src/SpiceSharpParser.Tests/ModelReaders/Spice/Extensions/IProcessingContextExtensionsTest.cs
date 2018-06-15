using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Extensions;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Extensions
{
    public class IReadingContextExtensionsTest
    {
        [Fact]
        public void SetParametersNoWarnings()
        {
            // prepare
            var readingContext = Substitute.For<IReadingContext>();
            readingContext.SetEntityParameter(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            var parameters = new ParameterCollection
            {
                new AssignmentParameter() { Name = "ron", Value = "100" },
                new AssignmentParameter() { Name = "roff", Value = "1001" },
            };

            // act
            readingContext.SetParameters(new VoltageSwitchModel("S1"), parameters);

            // assert
            resultService.Received(0).AddWarning(Arg.Any<string>());
        }

        [Fact]
        public void SetParametersWarnings()
        {
            // prepare
            var readingContext = Substitute.For<IReadingContext>();
            readingContext.SetEntityParameter(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var resultService = Substitute.For<IResultService>();
            readingContext.Result.Returns(resultService);

            var parameters = new ParameterCollection
            {
                new AssignmentParameter() { Name = "ron", Value = "100" },
                new AssignmentParameter() { Name = "roff", Value = "1001" },
            };

            // act
            readingContext.SetParameters(new VoltageSwitchModel("S1"), parameters);

            // assert
            resultService.Received(2).AddWarning(Arg.Any<string>());
        }
    }
}

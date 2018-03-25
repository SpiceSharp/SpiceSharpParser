using NSubstitute;
using SpiceNetlist.SpiceSharpConnector.Context;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SpiceNetlist.SpiceSharpConnector.Extensions;
using SpiceSharp.Components;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Tests.Extensions
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

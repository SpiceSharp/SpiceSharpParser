using NSubstitute;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.EntityGenerators.Models;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Processors.EntityGenerators.Models
{
    public class SwitchModelGeneratorTest
    {
        [Fact]
        public void GenerateTest()
        {
            var parameters = new ParameterCollection
            {
                new AssignmentParameter() { Name = "ron", Value = "100" },
                new AssignmentParameter() { Name = "roff", Value = "1001" },
                new AssignmentParameter() { Name = "unkownParametr", Value = "1002" }
            };

            var context = Substitute.For<IProcessingContext>();
            var generator = new SwitchModelGenerator();
            var model = generator.Generate("SRES", "SRES", "sw", parameters, context);

            Assert.NotNull(model);
        }
    }
}

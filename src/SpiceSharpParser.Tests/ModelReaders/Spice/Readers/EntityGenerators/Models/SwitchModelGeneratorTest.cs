using NSubstitute;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Models
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

            var context = Substitute.For<IReadingContext>();
            var generator = new SwitchModelGenerator();
            var model = generator.Generate("SRES", "SRES", "sw", parameters, context);

            Assert.NotNull(model);
        }
    }
}

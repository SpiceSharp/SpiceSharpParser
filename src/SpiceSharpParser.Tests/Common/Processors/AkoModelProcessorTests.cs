using SpiceSharpParser.Common.Processors;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Processors
{
    public class AkoModelProcessorTests
    {
        [Fact]
        public void When_AkoReferencesSourceWithDifferentCase_Expect_ParametersCopied()
        {
            var statements = new Statements
            {
                CreateModel("d_base", new WordParameter("D"), new AssignmentParameter { Name = "Rs", Value = "0.5" }),
                CreateModel("d_derived", new WordParameter("AKO:D_BASE")),
            };

            new AkoModelProcessor().Process(statements);

            var derived = statements.OfType<Model>().Single(m => m.Name == "d_derived");
            Assert.Contains(derived.Parameters.OfType<AssignmentParameter>(), p => p.Name == "Rs");
        }

        [Fact]
        public void When_FirstParameterIsAkoWithoutColon_Expect_ModelLeftIntact()
        {
            var statements = new Statements
            {
                CreateModel("d1", new WordParameter("ako")),
            };

            var exception = Record.Exception(() => new AkoModelProcessor().Process(statements));

            Assert.Null(exception);
            var model = statements.OfType<Model>().Single();
            var parameter = Assert.Single(model.Parameters.OfType<Parameter>());
            Assert.Equal("ako", parameter.Value);
        }

        [Fact]
        public void When_ModelHasNoParameters_Expect_NoException()
        {
            var statements = new Statements
            {
                CreateModel("d1"),
            };

            var exception = Record.Exception(() => new AkoModelProcessor().Process(statements));

            Assert.Null(exception);
        }

        private static Model CreateModel(string name, params Parameter[] parameters)
        {
            var collection = new ParameterCollection();
            foreach (var parameter in parameters)
            {
                collection.Add(parameter);
            }

            return new Model(name, collection, null);
        }
    }
}

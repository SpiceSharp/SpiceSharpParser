using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;

namespace SpiceSharpParser.Tests.Parameters
{
    public class ParameterCollectionTests
    {
        [Fact]
        public void SetAddsMissingBracketParameter()
        {
            var target = new ParameterCollection();
            var source = new ParameterCollection
            {
                new BracketParameter(
                    "group",
                    new ParameterCollection { new WordParameter("value") },
                    null),
            };

            target.Set(source);

            var parameter = Assert.IsType<BracketParameter>(Assert.Single(target));
            Assert.Equal("group(value)", parameter.ToString());
        }

        [Fact]
        public void BracketParameterCloneDoesNotShareNestedParameters()
        {
            var original = new BracketParameter(
                "group",
                new ParameterCollection { new WordParameter("original") },
                null);

            var clone = Assert.IsType<BracketParameter>(original.Clone());
            clone.Parameters.Add(new WordParameter("added"));

            Assert.Single(original.Parameters);
            Assert.Equal(2, clone.Parameters.Count);
        }
    }
}
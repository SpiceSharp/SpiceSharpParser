using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Examples
{
    public class Example01 : BaseTests
    {
        [Fact]
        public void When_Simulated_Expect_NoExceptions()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources/example01.cir");
            var netlistContent = File.ReadAllText(path);

            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;

            var parseResult = parser.ParseNetlist(netlistContent);

            double[] exports = RunOpSimulation(parseResult.SpiceSharpModel, new[] { "V(N1)", "V(N2)", "V(N3)" });

            EqualsWithTol(1.0970919064909939, exports[0]);
            EqualsWithTol(0.014696545624995935, exports[1]);
            EqualsWithTol(0.014715219080886419, exports[2]);
        }
    }
}
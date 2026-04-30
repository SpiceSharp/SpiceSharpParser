using SpiceSharpParser.Common;
using SpiceSharpParser.ModelWriters.CSharp;
using SpiceSharpParser.Parsers.Expression;
using System.Globalization;
using System.Threading;
using Xunit;

namespace SpiceSharpParser.Tests.ModelWriters
{
    public class BaseWriterTests
    {
        [Fact]
        public void When_CurrentCultureUsesCommaDecimal_Expect_EvaluatedParameterUsesInvariantCulture()
        {
            var previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");

                var writer = new BaseWriter();
                var statement = Assert.IsType<CSharpCallStatement>(writer.SetParameter("r1", "resistance", "1.25", CreateContext()));

                Assert.Equal(@"SetParameter(""resistance"", 1.25d)", statement.CallExpression);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public void When_CurrentCultureUsesCommaDecimal_Expect_DoubleParameterUsesInvariantCulture()
        {
            var previousCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");

                var writer = new BaseWriter();
                var statement = Assert.IsType<CSharpCallStatement>(writer.SetParameter("r1", "resistance", 1.25, null));

                Assert.Equal(@"SetParameter(""resistance"", 1.25d)", statement.CallExpression);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
        }

        private static WriterContext CreateContext()
        {
            var parser = new ExpressionParser(
                new SpiceSharpBehavioral.Builders.Direct.RealBuilder(),
                false);

            return new WriterContext()
            {
                CaseSettings = new SpiceNetlistCaseSensitivitySettings(),
                EvaluationContext = new EvaluationContext(parser),
            };
        }
    }
}

using SpiceSharpParser.Parsers.Netlist.Spice;
using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class CaseSensitivityTest : BaseTest
    {
        [Fact]
        public void DotStatementsExceptionTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForDotStatements = false;

            try
            {
                var text = string.Join(Environment.NewLine,
                    "Title",
                    ".End");

                parser.ParseNetlist(text);
            }
            catch (NoEndKeywordException ex)
            {
            }
        }

        [Fact]
        public void CustomFunctionNamePositiveTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForFunctions = true;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 1 {fUn(1)}",
                "V1 0 1 10",
                ".FUNC fun(x) = { x* x +1}",
                ".OP",
                ".End");

            var parseResult = parser.ParseNetlist(text);

            parseResult.Result.Simulations[0].Run(parseResult.Result.Circuit);
        }

        [Fact]
        public void CustomFunctionNameNotFoundTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForFunctions = false;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 1 {fUn(1)}",
                "V1 0 1 10",
                ".FUNC fun(x) = { x* x +1}",
                ".OP",
                ".End");

            Assert.Throws<FunctionNotFoundException>(() => parser.ParseNetlist(text));
        }

        [Fact]
        public void BuiltInFunctionNamePositiveTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForFunctions = true;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 1 {Cos(1)}",
                "V1 0 1 10",
                ".OP",
                ".End");

            var parseResult = parser.ParseNetlist(text);

            parseResult.Result.Simulations[0].Run(parseResult.Result.Circuit);
        }

        [Fact]
        public void BuiltInFunctionNameNotFoundTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForFunctions = false;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 1 {Cos(1)}",
                "V1 0 1 10",
                ".OP",
                ".End");

            Assert.Throws<FunctionNotFoundException>(() => parser.ParseNetlist(text));
        }

        [Fact]
        public void DotStatementsNoException2Test()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = false;
            parser.Settings.Parsing.IsNewlineRequired = false;
            parser.Settings.CaseSensitivity.IgnoreCaseForDotStatements = false;

            var text = string.Join(Environment.NewLine,
                "Title",
                ".End");
            parser.ParseNetlist(text);
        }

        [Fact]
        public void DotStatementsNoException3Test()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = false;
            parser.Settings.Parsing.IsNewlineRequired = false;
            parser.Settings.CaseSensitivity.IgnoreCaseForDotStatements = false;

            var text = string.Join(Environment.NewLine,
                "Title",
                ".END");
            parser.ParseNetlist(text);
        }

        [Fact]
        public void DotStatementsNoExceptionTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForDotStatements = true;

            var text = string.Join(Environment.NewLine,
                "Title",
                ".End");

            parser.ParseNetlist(text);
        }
    }
}

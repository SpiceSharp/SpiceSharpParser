using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
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

            var text = string.Join(Environment.NewLine,
                "Title",
                ".End");
            Assert.Throws<NoEndKeywordException>(() => parser.ParseNetlist(text));
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

        [Fact]
        public void NodeNamesPositiveTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForNodes = true;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 out {cos(0)}",
                "V1 0 OUT 10",
                ".SAVE V(Out)",
                ".OP",
                ".END");

            var parseResult = parser.ParseNetlist(text);
            var export = RunOpSimulation(parseResult.Result, "V(Out)");

            Assert.Equal(-10, export);
        }

        [Fact]
        public void NodeNamesNegativeTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForNodes = false;

            var text = string.Join(Environment.NewLine,
                "Title",
                "R1 0 out {cos(0)}",
                "V1 0 OUT 10",
                ".SAVE V(Out)",
                ".OP",
                ".END");

            var parseResult = parser.ParseNetlist(text);
            Assert.Throws<GeneralReaderException>(() => RunOpSimulation(parseResult.Result, "V(Out)"));
        }

        [Fact]
        public void NodeNamesSubcircuitPositiveTest()
        {
            var parser = new SpiceParser();

            parser.Settings.Parsing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.CaseSensitivity.IgnoreCaseForNodes = true;

            var text = string.Join(Environment.NewLine,
                "Subcircuit - SingleSubcircuitWithParams",
                "V1 IN 0 4.0",
                "X1 IN Out twoResistorsInSeries R1=1 R2=2",
                "RX OUT 0 1",
                ".SUBCKT twoResistorsInSeries Input oUtput params: R1=10 R2=100",
                "R1 input 1 {R1}",
                "R2 1 output {R2}",
                ".ENDS twoResistorsInSeries",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            var parseResult = parser.ParseNetlist(text);
            double export = RunOpSimulation(parseResult.Result, "V(OUT)");

            // Create references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }
    }
}

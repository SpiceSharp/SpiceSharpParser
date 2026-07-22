using System;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using Xunit;

namespace SpiceSharpParser.Tests.Compilation
{
    public class SpiceNetlistParserRecoveryTests
    {
        [Fact]
        public void ParseNetlist_WhenRecoveryUsesDefaultSettings_StopsAtFirstError()
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Parsing.IsEndRequired = true;

            SpiceNetlistParseResult result = parser.ParseNetlist(BrokenNetlist(), "strict.net");

            Assert.Single(result.ValidationResult.Errors);
            Assert.Null(result.InputModel);
            Assert.Null(result.FinalModel);
        }

        [Fact]
        public void ParseNetlist_WhenRecoveryIsEnabled_ReturnsAllErrorsAndRecoveredModels()
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.ContinueAfterErrors = true;

            SpiceNetlistParseResult result = parser.ParseNetlist(BrokenNetlist(), "recovered.net");

            Assert.Equal(2, result.ValidationResult.Errors.Count());
            Assert.All(
                result.ValidationResult.Errors,
                error => Assert.Equal(ValidationEntrySource.Parser, error.Source));
            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.FinalModel);
        }

        [Fact]
        public void MaximumSyntaxErrors_WhenValueIsNotPositive_Throws()
        {
            var settings = new SpiceNetlistParserSettings();

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.MaximumSyntaxErrors = 0);
        }

        [Fact]
        public void ParseNetlist_WhenEndIsMissing_DoesNotDiscardValidStatementsToRecover()
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.ContinueAfterErrors = true;
            string source = string.Join(
                Environment.NewLine,
                "missing end",
                "V1 out 0 1",
                ".op") + Environment.NewLine;

            SpiceNetlistParseResult result = parser.ParseNetlist(source, "missing-end.net");

            Assert.Single(result.ValidationResult.Errors);
            Assert.Null(result.InputModel);
            Assert.Null(result.FinalModel);
        }

        private static string BrokenNetlist()
        {
            return string.Join(
                Environment.NewLine,
                "parser recovery",
                "=",
                "V1 out 0 1",
                ",",
                ".op",
                ".end");
        }
    }
}

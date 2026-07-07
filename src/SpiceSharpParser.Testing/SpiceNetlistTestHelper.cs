using System;
using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.CustomComponents;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Testing
{
    /// <summary>
    /// Shared parser and reader setup for tests that start from SPICE netlist text.
    /// </summary>
    public static class SpiceNetlistTestHelper
    {
        /// <summary>
        /// Parses and reads a netlist using the default test options.
        /// </summary>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The SpiceSharp model.</returns>
        public static SpiceSharpModel ParseAndRead(params string[] lines)
        {
            return ParseAndRead(new SpiceNetlistTestOptions(), lines);
        }

        /// <summary>
        /// Parses and reads a netlist using the supplied test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The SpiceSharp model.</returns>
        public static SpiceSharpModel ParseAndRead(SpiceNetlistTestOptions options, params string[] lines)
        {
            return ParseTextAndRead(options, FromLines(lines));
        }

        /// <summary>
        /// Parses and reads a netlist text block using the default test options.
        /// </summary>
        /// <param name="text">The netlist text.</param>
        /// <returns>The SpiceSharp model.</returns>
        public static SpiceSharpModel ParseTextAndRead(string text)
        {
            return ParseTextAndRead(new SpiceNetlistTestOptions(), text);
        }

        /// <summary>
        /// Parses and reads a netlist text block using the supplied test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="text">The netlist text.</param>
        /// <returns>The SpiceSharp model.</returns>
        public static SpiceSharpModel ParseTextAndRead(SpiceNetlistTestOptions options, string text)
        {
            options = OptionsOrDefault(options);

            var parser = CreateParser(options);
            var parserResult = parser.ParseNetlist(text);
            var reader = CreateReader(options, () => parser.Settings.WorkingDirectory);

            return reader.Read(parserResult.FinalModel);
        }

        /// <summary>
        /// Parses a netlist into the parser result using the default test options.
        /// </summary>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The parse result.</returns>
        public static SpiceNetlistParseResult ParseRaw(params string[] lines)
        {
            return ParseRaw(new SpiceNetlistTestOptions(), lines);
        }

        /// <summary>
        /// Parses a netlist into the parser result using the supplied test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The parse result.</returns>
        public static SpiceNetlistParseResult ParseRaw(SpiceNetlistTestOptions options, params string[] lines)
        {
            return ParseTextRaw(options, FromLines(lines));
        }

        /// <summary>
        /// Parses a netlist text block into the parser result.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="text">The netlist text.</param>
        /// <returns>The parse result.</returns>
        public static SpiceNetlistParseResult ParseTextRaw(SpiceNetlistTestOptions options, string text)
        {
            return CreateParser(options).ParseNetlist(text);
        }

        /// <summary>
        /// Parses a netlist into the object model using the default test options.
        /// </summary>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The parsed netlist model.</returns>
        public static SpiceNetlist Parse(params string[] lines)
        {
            return Parse(new SpiceNetlistTestOptions(), lines);
        }

        /// <summary>
        /// Parses a netlist into the object model using the supplied test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The parsed netlist model.</returns>
        public static SpiceNetlist Parse(SpiceNetlistTestOptions options, params string[] lines)
        {
            return ParseTextRaw(options, FromLines(lines)).FinalModel;
        }

        /// <summary>
        /// Reads an already parsed netlist using the supplied test options.
        /// </summary>
        /// <param name="netlist">The parsed netlist.</param>
        /// <param name="options">The test options.</param>
        /// <returns>The SpiceSharp model.</returns>
        public static SpiceSharpModel Read(SpiceNetlist netlist, SpiceNetlistTestOptions options = null)
        {
            var reader = CreateReader(options, () => OptionsOrDefault(options).WorkingDirectory);
            return reader.Read(netlist);
        }

        /// <summary>
        /// Creates a parser configured from test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <returns>The configured parser.</returns>
        public static SpiceNetlistParser CreateParser(SpiceNetlistTestOptions options = null)
        {
            options = OptionsOrDefault(options);

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = options.HasTitle;
            parser.Settings.Lexing.EnableBusSyntax = options.EnableBusSyntax;
            parser.Settings.Parsing.IsEndRequired = options.IsEndRequired;
            parser.Settings.Compatibility = options.Compatibility;
            parser.Settings.WorkingDirectory = options.WorkingDirectory;
            parser.Settings.ExternalFilesEncoding = options.ExternalFilesEncoding ?? Encoding.Default;

            if (options.IsNewlineRequired.HasValue)
            {
                parser.Settings.Parsing.IsNewlineRequired = options.IsNewlineRequired.Value;
            }

            CopyCaseSensitivity(options.CaseSensitivity, parser.Settings.CaseSensitivity);
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = parser.Settings.CaseSensitivity.IsDotStatementNameCaseSensitive;
            return parser;
        }

        /// <summary>
        /// Creates reader settings configured from test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="workingDirectoryProvider">The working directory provider.</param>
        /// <returns>The configured reader settings.</returns>
        public static SpiceNetlistReaderSettings CreateReaderSettings(
            SpiceNetlistTestOptions options = null,
            Func<string> workingDirectoryProvider = null)
        {
            options = OptionsOrDefault(options);

            var settings = new SpiceNetlistReaderSettings(
                options.CaseSensitivity ?? new SpiceNetlistCaseSensitivitySettings(),
                workingDirectoryProvider ?? (() => options.WorkingDirectory),
                options.ExternalFilesEncoding ?? Encoding.Default,
                options.Separator,
                options.ExpandSubcircuits)
            {
                Compatibility = options.Compatibility,
                Seed = options.Seed,
            };

            if (options.UseCustomComponents)
            {
                settings.UseCustomComponents();
            }

            return settings;
        }

        /// <summary>
        /// Creates a reader configured from test options.
        /// </summary>
        /// <param name="options">The test options.</param>
        /// <param name="workingDirectoryProvider">The working directory provider.</param>
        /// <returns>The configured reader.</returns>
        public static SpiceNetlistReader CreateReader(
            SpiceNetlistTestOptions options = null,
            Func<string> workingDirectoryProvider = null)
        {
            return new SpiceNetlistReader(CreateReaderSettings(options, workingDirectoryProvider));
        }

        /// <summary>
        /// Joins netlist lines with the platform newline used by the existing tests.
        /// </summary>
        /// <param name="lines">The netlist lines.</param>
        /// <returns>The joined text.</returns>
        public static string FromLines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private static SpiceNetlistTestOptions OptionsOrDefault(SpiceNetlistTestOptions options)
        {
            return options ?? new SpiceNetlistTestOptions();
        }

        private static void CopyCaseSensitivity(
            SpiceNetlistCaseSensitivitySettings source,
            SpiceNetlistCaseSensitivitySettings destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            destination.IsDistributionNameCaseSensitive = source.IsDistributionNameCaseSensitive;
            destination.IsDotStatementNameCaseSensitive = source.IsDotStatementNameCaseSensitive;
            destination.IsEntityNamesCaseSensitive = source.IsEntityNamesCaseSensitive;
            destination.IsExpressionNameCaseSensitive = source.IsExpressionNameCaseSensitive;
            destination.IsFunctionNameCaseSensitive = source.IsFunctionNameCaseSensitive;
            destination.IsModelTypeCaseSensitive = source.IsModelTypeCaseSensitive;
            destination.IsNodeNameCaseSensitive = source.IsNodeNameCaseSensitive;
            destination.IsParameterNameCaseSensitive = source.IsParameterNameCaseSensitive;
            destination.IsSubcircuitNameCaseSensitive = source.IsSubcircuitNameCaseSensitive;
        }
    }
}

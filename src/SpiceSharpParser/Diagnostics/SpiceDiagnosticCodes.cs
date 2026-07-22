using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Stable codes emitted by the built-in diagnostic adapters.
    /// </summary>
    public static class SpiceDiagnosticCodes
    {
        private const string DocumentationUrl =
            "https://github.com/SpiceSharp/SpiceSharpParser/blob/main/docs/diagnostics.md#";

        public const string LexerError = "SSP1000";
        public const string ParserError = "SSP1100";
        public const string PreprocessorError = "SSP2000";
        public const string SourceFileNotFound = "SSP2001";
        public const string SourceFileReadError = "SSP2002";

        public const string UnsupportedComponent = "SSP3001";
        public const string UnsupportedParameter = "SSP3002";
        public const string UnsupportedModel = "SSP3003";
        public const string UnsupportedControl = "SSP3004";
        public const string UnsupportedWaveform = "SSP3005";
        public const string UnsupportedOption = "SSP3006";
        public const string UnsupportedExport = "SSP3007";
        public const string UnsupportedSyntax = "SSP3008";

        public const string ReaderError = "SSP4000";

        public const string FloatingNode = "SSP5001";
        public const string MissingDcPath = "SSP5002";
        public const string MissingModel = "SSP5003";
        public const string DuplicateComponent = "SSP5004";
        public const string MissingAcMagnitude = "SSP5005";
        public const string MissingTranMaxStep = "SSP5006";
        public const string EmptyCircuit = "SSP5007";
        public const string NoSimulation = "SSP5008";
        public const string NoExports = "SSP5009";

        public const string IgnoredConstruct = "SSP6001";
        public const string NumericDivergence = "SSP6002";
        public const string CompatibilityApproximation = "SSP6003";

        private static readonly IReadOnlyList<string> BuiltInCodes = Array.AsReadOnly(new[]
        {
            LexerError,
            ParserError,
            PreprocessorError,
            SourceFileNotFound,
            SourceFileReadError,
            UnsupportedComponent,
            UnsupportedParameter,
            UnsupportedModel,
            UnsupportedControl,
            UnsupportedWaveform,
            UnsupportedOption,
            UnsupportedExport,
            UnsupportedSyntax,
            ReaderError,
            FloatingNode,
            MissingDcPath,
            MissingModel,
            DuplicateComponent,
            MissingAcMagnitude,
            MissingTranMaxStep,
            EmptyCircuit,
            NoSimulation,
            NoExports,
            IgnoredConstruct,
            NumericDivergence,
            CompatibilityApproximation,
        });

        /// <summary>
        /// Gets every diagnostic code emitted by the built-in compiler pipeline.
        /// </summary>
        public static IReadOnlyList<string> All => BuiltInCodes;

        /// <summary>
        /// Gets the documentation link for a built-in diagnostic code.
        /// </summary>
        /// <param name="code">The diagnostic code.</param>
        /// <returns>The documentation link, or null for a custom or unknown code.</returns>
        public static Uri GetHelpLink(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            for (int index = 0; index < BuiltInCodes.Count; index++)
            {
                if (string.Equals(BuiltInCodes[index], code, StringComparison.OrdinalIgnoreCase))
                {
                    return new Uri(DocumentationUrl + BuiltInCodes[index].ToLowerInvariant());
                }
            }

            return null;
        }
    }
}

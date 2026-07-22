namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Classifies the compatibility status of a recognized SPICE construct.
    /// </summary>
    public enum CompatibilityClass
    {
        /// <summary>
        /// The parser and runtime support the construct.
        /// </summary>
        Supported,

        /// <summary>
        /// Dialect syntax is lowered to equivalent existing behavior.
        /// </summary>
        ParserShim,

        /// <summary>
        /// Recognized metadata is ignored with a diagnostic.
        /// </summary>
        RecognizedNoOp,

        /// <summary>
        /// A known unsupported construct produces a targeted diagnostic.
        /// </summary>
        TargetedDiagnostic,

        /// <summary>
        /// Syntax is accepted without a runtime behavior claim.
        /// </summary>
        ParseOnly,

        /// <summary>
        /// The syntax is known but has not yet been classified.
        /// </summary>
        SyntaxAuditGap,

        /// <summary>
        /// Runtime support is required before the construct can run.
        /// </summary>
        EngineRequired,

        /// <summary>
        /// The construct runs with a documented numeric divergence.
        /// </summary>
        NumericDivergence,
    }
}

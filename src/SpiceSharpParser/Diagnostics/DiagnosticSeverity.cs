namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Specifies the severity of a SPICE diagnostic.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// The diagnostic prevents a successful compilation.
        /// </summary>
        Error,

        /// <summary>
        /// The diagnostic identifies a potentially unintended condition.
        /// </summary>
        Warning,

        /// <summary>
        /// The diagnostic provides non-blocking information.
        /// </summary>
        Info,
    }
}

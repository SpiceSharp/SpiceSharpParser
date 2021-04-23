namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// Settings for <see cref="SpiceLexer"/>.
    /// </summary>
    public class SpiceLexerSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether text has a first line with the title.
        /// </summary>
        public bool HasTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether dot statements names are case-sensitive.
        /// </summary>
        public bool IsDotStatementNameCaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether bus syntax is enabled.
        /// </summary>
        public bool EnableBusSyntax { get; set; } = false;
    }
}
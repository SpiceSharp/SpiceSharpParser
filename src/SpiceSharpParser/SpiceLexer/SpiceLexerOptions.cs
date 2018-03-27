namespace SpiceSharpParser.SpiceLexer
{
    /// <summary>
    /// Options for <see cref="SpiceLexer"/>
    /// </summary>
    public class SpiceLexerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether text has a first line with the title
        /// </summary>
        public bool HasTitle { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether keywords,values are case-sensitive
        /// </summary>
        public bool IgnoreCase { get; set; } = true;
    }
}

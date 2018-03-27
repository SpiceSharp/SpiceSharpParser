namespace SpiceSharpParser.Model.SpiceObjects
{
    /// <summary>
    /// A comment line
    /// </summary>
    public class CommentLine : Statement
    {
        /// <summary>
        /// Gets or sets the comment line's text
        /// </summary>
        public string Text { get; set; }
    }
}

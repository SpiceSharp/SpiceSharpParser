namespace SpiceSharpParser.Model.Spice.Objects
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

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            return new CommentLine() {
                Text = this.Text,
                LineNumber = this.LineNumber
            };
        }
    }
}

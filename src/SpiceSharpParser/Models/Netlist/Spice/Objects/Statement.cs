namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// Base class for all SPICE statements.
    /// </summary>
    public abstract class Statement : SpiceObject
    {
        protected Statement(SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
        }

        public int StartLineNumber => LineInfo.LineNumber;

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public virtual int EndLineNumber => LineInfo.LineNumber;
    }
}
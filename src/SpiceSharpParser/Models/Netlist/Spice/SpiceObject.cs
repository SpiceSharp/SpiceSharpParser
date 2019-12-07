namespace SpiceSharpParser.Models.Netlist.Spice
{
    /// <summary>
    /// Base class for all SPICE objects.
    /// </summary>
    public abstract class SpiceObject
    {
        protected SpiceObject(SpiceLineInfo lineInfo)
        {
            LineInfo = lineInfo;
        }

        protected SpiceObject()
        {
        }

        public virtual SpiceLineInfo LineInfo { get; }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public abstract SpiceObject Clone();
    }
}
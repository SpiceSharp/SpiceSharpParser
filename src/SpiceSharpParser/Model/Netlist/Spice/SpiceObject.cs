namespace SpiceSharpParser.Model.Netlist.Spice
{
    /// <summary>
    /// Base class for all spice objects.
    /// </summary>
    public abstract class SpiceObject
    {
        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public abstract SpiceObject Clone();
    }
}

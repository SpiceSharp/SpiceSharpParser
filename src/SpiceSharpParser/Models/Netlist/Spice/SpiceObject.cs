namespace SpiceSharpParser.Models.Netlist.Spice
{
    /// <summary>
    /// Base class for all SPICE objects.
    /// </summary>
    public abstract class SpiceObject
    {
        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public abstract SpiceObject Clone();
    }
}
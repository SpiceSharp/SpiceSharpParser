using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Describes a quantity that can be exported using simulated data.
    /// </summary>
    public abstract class Export
    {
        public Export(Simulation simulation)
        {
            Simulation = simulation ?? throw new System.ArgumentNullException(nameof(simulation));
        }

        public Simulation Simulation { get; }

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the export unit
        /// </summary>
        public abstract string QuantityUnit { get; }

        /// <summary>
        /// Gets or sets the type of simulation that the export should act on
        /// eg. "tran", "dc", etc. Default is null (any simulation/optional).
        /// </summary>
        public string SimulationType { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether exceptions are enabled.
        /// </summary>
        protected bool ExceptionsEnabled { get; set; } = true;

        /// <summary>
        /// Extract the quantity from simulated data.
        /// </summary>
        /// <returns>
        /// A quantity
        /// </returns>
        public abstract double Extract();

        /// <summary>
        /// Override string representation.
        /// </summary>
        /// <returns>
        /// A string represenation of export.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}

using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Describes a quantity that can be exported using simulated data.
    /// </summary>
    public abstract class Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Export"/> class.
        /// </summary>
        /// <param name="simulation">
        /// The simulation.
        /// </param>
        protected Export(Simulation simulation)
        {
            Simulation = simulation;
        }

        public Simulation Simulation { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the export unit.
        /// </summary>
        public abstract string QuantityUnit { get; }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions are enabled.
        /// </summary>
        protected bool ExceptionsEnabled { get; set; } = true;

        /// <summary>
        /// Extract the quantity from simulated data.
        /// </summary>
        /// <returns>
        /// A quantity.
        /// </returns>
        public abstract double Extract();

        /// <summary>
        /// Override string representation.
        /// </summary>
        /// <returns>
        /// A string representation of export.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
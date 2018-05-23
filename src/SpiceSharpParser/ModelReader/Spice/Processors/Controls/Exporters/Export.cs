using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReader.Spice.Processors.Controls.Exporters
{
    /// <summary>
    /// Describes a quantity that can be exported using simulated data.
    /// </summary>
    public abstract class Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Export"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public Export(Simulation simulation)
        {
            Simulation = simulation;
        }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Gets or sets the simulation
        /// </summary>
        public Simulation Simulation { get; protected set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the export unit
        /// </summary>
        public abstract string QuantityUnit { get; }

        /// <summary>
        /// Gets or sets the type of simulation that the export should act on
        /// eg. "tran", "dc", etc. Default is null (any simulation/optional)
        /// </summary>
        public string SimulationType { get; set; } = null;

        /// <summary>
        /// Extract the quantity from simulated data
        /// </summary>
        /// <returns>
        /// A quantity
        /// </returns>
        public abstract double Extract();

        /// <summary>
        /// Override string representation
        /// </summary>
        /// <returns>
        /// A string represenation of export
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}

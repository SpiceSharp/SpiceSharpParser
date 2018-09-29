using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Property export.
    /// </summary>
    public class PropertyExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">A identifier of component</param>
        /// <param name="property">Name of property for export</param>
        public PropertyExport(string name, Simulation simulation, Identifier source, string property)
            : base(simulation)
        {
            Name = name ?? throw new System.NullReferenceException(nameof(name));
            Source = source ?? throw new System.NullReferenceException(nameof(source));
            ExportRealImpl = new RealPropertyExport(simulation, source, property);
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => string.Empty;

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => string.Empty;

        /// <summary>
        /// Gets the real exporter
        /// </summary>
        protected RealPropertyExport ExportRealImpl { get; }

        /// <summary>
        /// Extracts the current value
        /// </summary>
        /// <returns>
        /// A current value at the source
        /// </returns>
        public override double Extract()
        {
            if (!ExportRealImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new GeneralReaderException($"Property export {Name} is invalid");
                }

                return double.NaN;
            }

            return ExportRealImpl.Value;
        }
    }
}

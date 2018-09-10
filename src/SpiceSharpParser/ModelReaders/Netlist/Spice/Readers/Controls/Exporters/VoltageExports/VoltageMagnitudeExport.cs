using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Magnitude of a complex voltage export.
    /// </summary>
    public class VoltageMagnitudeExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageMagnitudeExport"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageMagnitudeExport(Simulation simulation, Identifier node, Identifier reference = null, string nodePath = null, string referencePath = null)
            : base(simulation)
        {
            if (simulation == null)
            {
                throw new System.ArgumentNullException(nameof(simulation));
            }

            Name = "vm(" + nodePath.ToString() + (referencePath == null ? string.Empty : ", " + referencePath.ToString()) + ")";
            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;

            ExportImpl = new ComplexVoltageExport(simulation, node, reference);
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// Gets the reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage (V)";

        /// <summary>
        /// Gets the complex voltage export that provide voltage magnitude
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Extracts a voltage magnitude at main node
        /// </summary>
        /// <returns>
        /// A voltage magnitude at the main node
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new GeneralReaderException($"Voltage magnitude export {Name} is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value.Magnitude;
        }
    }
}

using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Real part of a complex voltage export.
    /// </summary>
    public class VoltageRealExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageRealExport"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageRealExport(Simulation simulation, Identifier node, Identifier reference = null, string nodePath = null, string referencePath = null)
            : base(simulation)
        {
            if (simulation == null)
            {
                throw new System.ArgumentNullException(nameof(simulation));
            }

            Name = "vr(" + nodePath.ToString() + (referencePath == null ? string.Empty : ", " + referencePath.ToString()) + ")";
            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;

            ExportImpl = new RealVoltageExport(simulation, node, reference);
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
        /// Gets the real voltage export that provides voltage
        /// </summary>
        protected RealVoltageExport ExportImpl { get; }

        /// <summary>
        /// Extracts the voltage at the main node
        /// </summary>
        /// <returns>
        /// A voltage (real) at the main node
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new GeneralReaderException($"Voltage real export '{Name}' is invalid");
                }
                return double.NaN;
            }

            return ExportImpl.Value;
        }
    }
}

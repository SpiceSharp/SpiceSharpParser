using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Magnitude in decibels of a complex voltage export.
    /// </summary>
    public class VoltageDecibelExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDecibelExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">Simulation.</param>
        /// <param name="node">Positive node.</param>
        /// <param name="reference">Negative reference node.</param>
        public VoltageDecibelExport(string name, Simulation simulation, string node, string reference = null)
            : base(simulation)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;
            ExportImpl = new ComplexVoltageExport((FrequencySimulation)simulation, node, reference);
        }

        /// <summary>
        /// Gets the main node.
        /// </summary>
        public string Node { get; }

        /// <summary>
        /// Gets the reference node.
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Gets the quantity unit.
        /// </summary>
        public override string QuantityUnit => "Voltage (db V)";

        /// <summary>
        /// Gets the complex voltage export that provide voltage decibels.
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Extracts a voltage decibels at the main node.
        /// </summary>
        /// <returns>
        /// A voltage decibels at the main node.
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Voltage decibel export '{Name}' is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Decibels;
        }
    }
}
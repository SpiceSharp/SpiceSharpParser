using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Phase of a complex voltage export.
    /// </summary>
    public class VoltagePhaseExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltagePhaseExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltagePhaseExport(string name, Simulation simulation, string node, string reference = null)
            : base(simulation)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;
            ExportImpl = new ComplexVoltageExport((FrequencySimulation)simulation, node, reference);
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public string Node { get; }

        /// <summary>
        /// Gets the reference node
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage phase (radians)";

        /// <summary>
        /// Gets the complex voltage export that provides voltage phase
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Extracts a voltage phase at main node
        /// </summary>
        /// <returns>
        /// A voltage phase at the main node
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Voltage phase export {Name} is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Phase;
        }
    }
}
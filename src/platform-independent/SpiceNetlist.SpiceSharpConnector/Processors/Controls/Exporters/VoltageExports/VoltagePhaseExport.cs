using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Phase of a complex voltage export.
    /// </summary>
    public class VoltagePhaseExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltagePhaseExport"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltagePhaseExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            if (simulation == null)
            {
                throw new System.ArgumentNullException(nameof(simulation));
            }

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
        public override string TypeName => "degrees";

        /// <summary>
        /// Gets get the name
        /// </summary>
        public override string Name => "vp(" + Node + (Reference == null ? string.Empty : ", " + Reference) + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage phase (degrees)";

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
                throw new GeneralConnectorException($"Voltage phase export {Name} is invalid");
            }

            return ExportImpl.Phase;
        }
    }
}

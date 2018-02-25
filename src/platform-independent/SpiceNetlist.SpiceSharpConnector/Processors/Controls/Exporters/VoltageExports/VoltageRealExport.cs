using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Real part of a complex voltage export.
    /// </summary>
    public class VoltageRealExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        protected RealVoltageExport ExportImpl { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageRealExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            Node = node;
            Reference = reference;
            ExportImpl = new RealVoltageExport(simulation, node, reference);
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vr(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage (V)";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            return this.ExportImpl.Value;
        }
    }
}

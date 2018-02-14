using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.Voltage
{
    /// <summary>
    /// Voltage export.
    /// </summary>
    public class VoltageExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        private readonly RealVoltageExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="reference">Reference</param>
        public VoltageExport(Simulation simulation, Identifier node, Identifier reference = null)
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
        /// Get the name based on the properties
        /// </summary>
        public override string Name => "v(" + Node.ToString() + (Reference == null ? "" : ", " + Reference.ToString()) + ")";

        /// <summary>
        /// Read the voltage and write to the output
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="ckt">Circuit</param>
        public override double Extract()
        {
            return this.ExportImpl.Value;
        }
    }
}

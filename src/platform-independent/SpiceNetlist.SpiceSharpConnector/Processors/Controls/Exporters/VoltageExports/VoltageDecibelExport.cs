using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Magnitude in decibels of a complex voltage export.
    /// </summary>
    public class VoltageDecibelExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        private readonly ComplexVoltageExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageDecibelExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            Node = node;
            Reference = reference;

            ExportImpl = new ComplexVoltageExport(simulation, node, reference);
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "none";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vdb(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage (db V)";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override double Extract()
        {
            return ExportImpl.Decibels;
        }
    }
}

using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
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

        private readonly ComplexVoltageExport ExportImpl;

        private readonly RealVoltageExport ExportRealImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="reference">Reference</param>
        public VoltageExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            Node = node;
            Reference = reference;

            /// TODO: Refactor this!!!!!
            if (simulation is DC || simulation is OP || simulation is Transient)
            {
                ExportRealImpl = new RealVoltageExport(simulation, node, reference);
            }
            else
            {

                ExportImpl = new ComplexVoltageExport(simulation, node, reference);
            }
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
        public override double Extract()
        {
            if (this.ExportImpl != null)
            {
                return this.ExportImpl.Value.Real;
            }
            else
            {
                return this.ExportRealImpl.Value;
            }
        }
    }
}

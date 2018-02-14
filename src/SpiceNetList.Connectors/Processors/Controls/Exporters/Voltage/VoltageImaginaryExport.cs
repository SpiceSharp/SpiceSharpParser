using SpiceSharp;
using SpiceSharp.Parser.Readers;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.Voltage
{
    /// <summary>
    /// Imaginary part of a complex voltage export.
    /// </summary>
    public class VoltageImaginaryExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// The reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageImaginaryExport(Identifier node, Identifier reference = null)
        {
            Node = node;
            Reference = reference;
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "vi(" + Node + (Reference == null ? "" : ", " + Reference) + ")";

        /// <summary>
        /// Extract
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override double Extract()
        {
            return double.NaN;
        }
    }
}

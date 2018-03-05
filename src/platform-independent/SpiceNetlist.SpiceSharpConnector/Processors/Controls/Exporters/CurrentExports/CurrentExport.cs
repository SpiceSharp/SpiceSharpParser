using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Current export.
    /// </summary>
    public class CurrentExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="reference">Reference</param>
        public CurrentExport(Simulation simulation, Identifier source)
        {
            Source = source;

            /// TODO: Refactor this!!!!!
            if (simulation is Dc || simulation is Op || simulation is Transient)
            {
                ExportRealImpl = new RealPropertyExport(simulation, source, "i");
            }
            else
            {

                ExportImpl = new ComplexPropertyExport(simulation, source, "i");
            }
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (A)";

        /// <summary>
        /// Get the name based on the properties
        /// </summary>
        public override string Name => "i(" + Source + ")";

        public RealPropertyExport ExportRealImpl { get; }
        public ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Read the voltage and write to the output
        /// </summary>
        public override double Extract()
        {
            return ExportRealImpl != null ? ExportRealImpl.Value : ExportImpl.Value.Real;
        }
    }
}

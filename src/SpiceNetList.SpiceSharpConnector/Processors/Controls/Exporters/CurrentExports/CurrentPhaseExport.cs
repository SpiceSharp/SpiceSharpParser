using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Phase of a complex current export.
    /// </summary>
    public class CurrentPhaseExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        protected readonly ComplexPropertyExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reference">Negative reference node</param>
        public CurrentPhaseExport(Simulation simulation, Identifier source)
        {
            Source = source;

            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "degrees";

        /// <summary>
        /// Gets get the name
        /// </summary>
        public override string Name => "ip(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            return ExportImpl.Value.Phase;
        }
    }
}

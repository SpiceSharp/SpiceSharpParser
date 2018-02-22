using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Real part of a complex current export.
    /// </summary>
    public class CurrentRealExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        protected RealPropertyExport ExportImpl { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentRealExport(Simulation simulation, Identifier source)
        {
            Source = source;

            ExportImpl = new RealPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "ir(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            return ExportImpl.Value;
        }
    }
}

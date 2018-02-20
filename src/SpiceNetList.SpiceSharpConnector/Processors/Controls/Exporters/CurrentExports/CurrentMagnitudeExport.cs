using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Magnitude of a complex current export.
    /// </summary>
    public class CurrentMagnitudeExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        protected readonly ComplexPropertyExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentMagnitudeExport(Simulation simulation, Identifier source)
        {
            Source = source;

            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "im(" + Source + ")";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            return ExportImpl.Value.Magnitude;
        }
    }
}

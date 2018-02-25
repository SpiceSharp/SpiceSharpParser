using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Imaginary part of a complex current export.
    /// </summary>
    public class CurrentImaginaryExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        private readonly ComplexPropertyExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public CurrentImaginaryExport(Simulation simulation, Identifier source)
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
        public override string Name => "ii(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Amps";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            return ExportImpl.Value.Imaginary;
        }
    }
}

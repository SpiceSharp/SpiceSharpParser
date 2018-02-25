using System;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Magnitude in decibels of a complex current export.
    /// </summary>
    public class CurrentDecibelExport : Export
    {
        /// <summary>
        /// The main node
        /// </summary>
        public Identifier Source { get; }

        private readonly ComplexPropertyExport ExportImpl;

        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentDecibelExport(Simulation simulation, Identifier source)
        {
            Source = source;
            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Get the type name
        /// </summary>
        public override string TypeName => "none";

        /// <summary>
        /// Get the name
        /// </summary>
        public override string Name => "idb(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (db A)";

        /// <summary>
        /// Extract
        /// </summary>
        public override double Extract()
        {
            //TODO: Verify with Sven
            return 20.0 * Math.Log10(ExportImpl.Value.Real);
        }
    }
}

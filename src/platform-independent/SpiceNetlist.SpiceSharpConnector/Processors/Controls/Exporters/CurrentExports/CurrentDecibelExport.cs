using System;
using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Magnitude of a complex current export.
    /// </summary>
    public class CurrentDecibelExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentDecibelExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentDecibelExport(Simulation simulation, Identifier source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "none";

        /// <summary>
        /// Gets the name
        /// </summary>
        public override string Name => "idb(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (db A)";

        /// <summary>
        /// Gets the complex property export
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current magnitude value
        /// </summary>
        /// <returns>
        /// Current magnitude value
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Current decibel export '{Name}' is invalid");
            }

            //TODO: Verify with Sven....
            return 20.0 * Math.Log10(ExportImpl.Value.Magnitude);
        }
    }
}

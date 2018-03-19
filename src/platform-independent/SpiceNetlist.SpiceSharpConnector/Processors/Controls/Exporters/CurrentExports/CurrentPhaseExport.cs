using System;
using SpiceNetlist.SpiceSharpConnector.Exceptions;
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
        /// Initializes a new instance of the <see cref="CurrentPhaseExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentPhaseExport(Simulation simulation, Identifier source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            Source = source;
            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "degrees";

        /// <summary>
        /// Gets get the name
        /// </summary>
        public override string Name => "ip(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current phase (radians)";

        /// <summary>
        /// Gets the complex property export
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current phase
        /// </summary>
        /// <returns>
        /// Current phase
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Current phase export '{Name}' is invalid");
            }

            return ExportImpl.Value.Phase;
        }
    }
}

using System;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Real part of a complex current export.
    /// </summary>
    public class CurrentRealExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentRealExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier of source</param>
        public CurrentRealExport(Simulation simulation, Identifier source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            ExportImpl = new RealPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "current";

        /// <summary>
        /// Gets the name
        /// </summary>
        public override string Name => "ir(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (A)";

        /// <summary>
        /// Gets the real property export
        /// </summary>
        protected RealPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts the current (real)
        /// </summary>
        /// <returns>
        /// The current value
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Current real export '{Name}' is invalid");
            }

            return ExportImpl.Value;
        }
    }
}

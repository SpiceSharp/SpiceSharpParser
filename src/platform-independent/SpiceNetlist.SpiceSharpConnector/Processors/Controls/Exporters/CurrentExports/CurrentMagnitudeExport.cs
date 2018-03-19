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
    public class CurrentMagnitudeExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentMagnitudeExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentMagnitudeExport(Simulation simulation, Identifier source)
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
        public override string TypeName => "current";

        /// <summary>
        /// Gets the name
        /// </summary>
        public override string Name => "im(" + Source + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (A)";

        /// <summary>
        /// Gets the complex property export
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current magnitude
        /// </summary>
        /// <returns>
        /// Current magnitude
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Current magnitude export '{Name}' is invalid");
            }

            return ExportImpl.Value.Magnitude;
        }
    }
}

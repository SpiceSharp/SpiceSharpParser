using System;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Imaginary of a complex current export.
    /// </summary>
    public class CurrentImaginaryExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentImaginaryExport"/> class.
        /// </summary>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentImaginaryExport(Simulation simulation, Identifier source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            ExportImpl = new ComplexPropertyExport(simulation, source, "i");

            Name = "ii(" + Source + ")";
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
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Current (A)";

        /// <summary>
        /// Gets the complex property export
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current imaginary value
        /// </summary>
        /// <returns>
        /// Current imaginary value
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                throw new GeneralConnectorException($"Current imaginary export '{Name}' is invalid");
            }

            return ExportImpl.Value.Imaginary;
        }
    }
}

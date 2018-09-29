using System;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Magnitude of a complex current export.
    /// </summary>
    public class CurrentMagnitudeExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentMagnitudeExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation.</param>
        /// <param name="source">An identifier.</param>
        public CurrentMagnitudeExport(string name, Simulation simulation, Identifier source)
            : base(simulation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
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
                if (ExceptionsEnabled)
                {
                    throw new GeneralReaderException($"Current magnitude export '{Name}' is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value.Magnitude;
        }
    }
}

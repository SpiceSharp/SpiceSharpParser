using System;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
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
            : base(simulation)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            ExportImpl = new RealPropertyExport(simulation, source, "i");

            Name = "ir(" + Source + ")";
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
                throw new GeneralReaderException($"Current real export '{Name}' is invalid");
            }

            return ExportImpl.Value;
        }
    }
}

using SpiceSharp.Simulations;
using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Real part of a complex current export.
    /// </summary>
    public class CurrentRealExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentRealExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier of source</param>
        public CurrentRealExport(string name, Simulation simulation, string source)
            : base(simulation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ExportImpl = new RealPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public string Source { get; }

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
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Current real export '{Name}' is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value;
        }
    }
}
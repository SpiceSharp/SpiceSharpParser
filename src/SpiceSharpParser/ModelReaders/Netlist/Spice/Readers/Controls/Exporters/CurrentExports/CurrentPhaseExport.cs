using SpiceSharp.Simulations;
using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Phase of a complex current export.
    /// </summary>
    public class CurrentPhaseExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentPhaseExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation</param>
        /// <param name="source">An identifier</param>
        public CurrentPhaseExport(string name, Simulation simulation, string source)
            : base(simulation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public string Source { get; }

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
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Current phase export '{Name}' is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value.Phase;
        }
    }
}
using SpiceSharp.Simulations;
using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Imaginary of a complex current export.
    /// </summary>
    public class CurrentImaginaryExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentImaginaryExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation.</param>
        /// <param name="source">A name of current source.</param>
        public CurrentImaginaryExport(string name, Simulation simulation, string source)
            : base(simulation)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ExportImpl = new ComplexPropertyExport(simulation, source, "i");
        }

        /// <summary>
        /// Gets the main node.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the quantity unit.
        /// </summary>
        public override string QuantityUnit => "Current (A)";

        /// <summary>
        /// Gets the complex property export.
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts current imaginary value.
        /// </summary>
        /// <returns>
        /// Current imaginary value.
        /// </returns>
        public override double Extract()
        {
            if (!ExportImpl.IsValid)
            {
                if (ExceptionsEnabled)
                {
                    throw new SpiceSharpParserException($"Current imaginary export '{Name}' is invalid");
                }

                return double.NaN;
            }

            return ExportImpl.Value.Imaginary;
        }
    }
}
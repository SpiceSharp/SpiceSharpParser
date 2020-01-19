using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports
{
    /// <summary>
    /// Current export.
    /// </summary>
    public class CurrentExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">A simulation.</param>
        /// <param name="source">A name of current source.</param>
        public CurrentExport(string name, Simulation simulation, string source)
            : base(simulation)
        {
            Name = name ?? throw new System.NullReferenceException(nameof(name));
            Source = source ?? throw new System.NullReferenceException(nameof(source));

            if (simulation is FrequencySimulation)
            {
                ExportImpl = new ComplexPropertyExport(simulation, source, "i");
            }
            else
            {
                ExportRealImpl = new RealPropertyExport(simulation, source, "i");
            }
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
        /// Gets the real exporter.
        /// </summary>
        protected RealPropertyExport ExportRealImpl { get; }

        /// <summary>
        /// Gets the complex exporter.
        /// </summary>
        protected ComplexPropertyExport ExportImpl { get; }

        /// <summary>
        /// Extracts the current value.
        /// </summary>
        /// <returns>
        /// A current value at the source.
        /// </returns>
        public override double Extract()
        {
            if (ExportRealImpl != null)
            {
                if (!ExportRealImpl.IsValid)
                {
                    if (ExceptionsEnabled)
                    {
                        throw new SpiceSharpParserException($"Current export {Name} is invalid");
                    }

                    return double.NaN;
                }

                return ExportRealImpl.Value;
            }
            else
            {
                if (!ExportImpl.IsValid)
                {
                    if (ExceptionsEnabled)
                    {
                        throw new SpiceSharpParserException($"Current export {Name} is invalid");
                    }

                    return double.NaN;
                }

                return ExportImpl.Value.Real;
            }
        }
    }
}
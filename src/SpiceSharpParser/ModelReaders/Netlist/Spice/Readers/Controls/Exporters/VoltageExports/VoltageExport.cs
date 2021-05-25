using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Voltage export.
    /// </summary>
    public class VoltageExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="simulation">Simulation.</param>
        /// <param name="node">Positive node.</param>
        /// <param name="reference">Negative reference node.</param>
        public VoltageExport(string name, Simulation simulation, string node, string reference = null)
            : base(simulation)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;

            if (simulation is FrequencySimulation fs)
            {
                ExportImpl = new ComplexVoltageExport(fs, node, reference);
            }
            else
            {
                ExportRealImpl = new RealVoltageExport((IBiasingSimulation)simulation, node, reference);
            }
        }

        /// <summary>
        /// Gets the main node.
        /// </summary>
        public string Node { get; }

        /// <summary>
        /// Gets the reference node.
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Gets the quantity unit.
        /// </summary>
        public override string QuantityUnit => "Voltage (V)";

        /// <summary>
        /// Gets the complex voltage export.
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Gets the real voltage export.
        /// </summary>
        protected RealVoltageExport ExportRealImpl { get; }

        /// <summary>
        /// Extracts the voltage value.
        /// </summary>
        /// <returns>
        /// A voltage value at the main node.
        /// </returns>
        public override double Extract()
        {
            if (ExportImpl != null)
            {
                if (!ExportImpl.IsValid)
                {
                    return double.NaN;
                }

                return ExportImpl.Value.Real;
            }
            else
            {
                if (!ExportRealImpl.IsValid)
                {
                    return double.NaN;
                }

                return ExportRealImpl.Value;
            }
        }
    }
}
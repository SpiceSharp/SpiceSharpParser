using SpiceNetlist.SpiceSharpConnector.Exceptions;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports
{
    /// <summary>
    /// Voltage export.
    /// </summary>
    public class VoltageExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="node">Positive node</param>
        /// <param name="reference">Negative reference node</param>
        public VoltageExport(Simulation simulation, Identifier node, Identifier reference = null)
        {
            if (simulation == null)
            {
                throw new System.ArgumentNullException(nameof(simulation));
            }

            Node = node ?? throw new System.ArgumentNullException(nameof(node));
            Reference = reference;

            if (simulation is FrequencySimulation)
            {
                ExportImpl = new ComplexVoltageExport(simulation, node, reference);
            }
            else
            {
                ExportRealImpl = new RealVoltageExport(simulation, node, reference);
            }
        }

        /// <summary>
        /// Gets the main node
        /// </summary>
        public Identifier Node { get; }

        /// <summary>
        /// Gets the reference node
        /// </summary>
        public Identifier Reference { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => "voltage";

        /// <summary>
        /// Gets the name based on the properties
        /// </summary>
        public override string Name => "v(" + Node.ToString() + (Reference == null ? string.Empty : ", " + Reference.ToString()) + ")";

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public override string QuantityUnit => "Voltage (V)";

        /// <summary>
        /// Gets the comple voltage export
        /// </summary>
        protected ComplexVoltageExport ExportImpl { get; }

        /// <summary>
        /// Gets the real voltage export
        /// </summary>
        protected RealVoltageExport ExportRealImpl { get; }

        /// <summary>
        /// Extracts the voltage value
        /// </summary>
        /// <returns>
        /// A voltage value at the main node
        /// </returns>
        public override double Extract()
        {
            if (ExportImpl != null)
            {
                if (!ExportImpl.IsValid)
                {
                    throw new GeneralConnectorException($"Voltage export {Name} is invalid");
                }

                return ExportImpl.Value.Real;
            }
            else
            {
                if (!ExportRealImpl.IsValid)
                {
                    throw new GeneralConnectorException($"Voltage export {Name} is invalid");
                }

                return ExportRealImpl.Value;
            }
        }
    }
}

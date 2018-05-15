using SpiceSharp.IntegrationMethods;

namespace SpiceSharpParser.Connector.Context
{
    public class SimulationConfiguration
    {
        public double? AbsoluteTolerance { get; set; }

        public double? RelTolerance { get; set; }

        public double? Gmin { get; set; }

        public int? DCMaxIterations { get; set; }

        public int? SweepMaxIterations { get; set; }

        public int? TranMaxIterations { get; set; }

        public Trapezoidal Method { get; set; }

        public bool? KeepOpInfo { get; set; }

        /// <summary>
        /// Gets or sets the temperature for this circuit.
        /// </summary>
        public double? TemperatureInKelvins { get; set; }

        /// <summary>
        /// Gets or sets the nominal temperature for the circuit
        /// Used for model parameters as the default.
        /// </summary>
        public double? NominalTemperatureInKelvins { get; set; }
    }
}

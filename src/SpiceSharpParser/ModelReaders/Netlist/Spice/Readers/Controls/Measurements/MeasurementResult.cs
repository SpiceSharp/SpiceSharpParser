namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements
{
    /// <summary>
    /// Represents the result of a single .MEAS/.MEASURE evaluation.
    /// </summary>
    public class MeasurementResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementResult"/> class.
        /// </summary>
        /// <param name="name">The measurement name.</param>
        /// <param name="value">The computed measurement value.</param>
        /// <param name="success">Whether the measurement succeeded.</param>
        /// <param name="measurementType">The type of measurement performed.</param>
        /// <param name="simulationName">The name of the simulation that produced this result.</param>
        public MeasurementResult(string name, double value, bool success, string measurementType, string simulationName)
        {
            Name = name;
            Value = value;
            Success = success;
            MeasurementType = measurementType;
            SimulationName = simulationName;
        }

        /// <summary>
        /// Gets the measurement name (user-defined identifier).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the computed measurement value.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Gets a value indicating whether the measurement was computed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the type of measurement that was performed (e.g. "TRIG_TARG", "MIN", "MAX", "AVG", "WHEN", "FIND_WHEN", "RMS", "PP", "INTEG", "DERIV", "PARAM").
        /// </summary>
        public string MeasurementType { get; }

        /// <summary>
        /// Gets the name of the simulation that produced this result.
        /// Useful for identifying sweep points when used with .STEP.
        /// </summary>
        public string SimulationName { get; }
    }
}

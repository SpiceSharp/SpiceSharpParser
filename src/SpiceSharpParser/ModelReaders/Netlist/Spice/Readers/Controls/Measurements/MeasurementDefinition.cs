using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements
{
    /// <summary>
    /// The type of measurement to perform.
    /// </summary>
    public enum MeasType
    {
        /// <summary>Measure time/x between trigger and target threshold crossings.</summary>
        TrigTarg,

        /// <summary>Find x-value where a condition is met.</summary>
        When,

        /// <summary>Find the value of one signal at the x-value where another signal crosses a threshold.</summary>
        FindWhen,

        /// <summary>Find the minimum value of a signal.</summary>
        Min,

        /// <summary>Find the maximum value of a signal.</summary>
        Max,

        /// <summary>Compute the average value of a signal.</summary>
        Avg,

        /// <summary>Compute the RMS value of a signal.</summary>
        Rms,

        /// <summary>Compute the peak-to-peak value of a signal.</summary>
        Pp,

        /// <summary>Integrate a signal over the simulation range.</summary>
        Integ,

        /// <summary>Compute the derivative of a signal at a point.</summary>
        Deriv,

        /// <summary>Compute a parameter expression from other measurement results.</summary>
        Param,
    }

    /// <summary>
    /// The type of edge to detect for threshold crossings.
    /// </summary>
    public enum EdgeType
    {
        /// <summary>Rising edge (signal crosses threshold going up).</summary>
        Rise,

        /// <summary>Falling edge (signal crosses threshold going down).</summary>
        Fall,

        /// <summary>Any crossing (rising or falling).</summary>
        Cross,
    }

    /// <summary>
    /// Represents the parsed definition of a .MEAS/.MEASURE statement.
    /// </summary>
    public class MeasurementDefinition
    {
        /// <summary>
        /// Gets or sets the user-defined measurement name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the analysis type (e.g. "TRAN", "AC", "DC", "NOISE", "OP").
        /// </summary>
        public string AnalysisType { get; set; }

        /// <summary>
        /// Gets or sets the type of measurement.
        /// </summary>
        public MeasType Type { get; set; }

        // --- TRIG/TARG properties ---

        /// <summary>Gets or sets the trigger signal parameter.</summary>
        public Parameter TrigSignal { get; set; }

        /// <summary>Gets or sets the trigger threshold value.</summary>
        public double TrigVal { get; set; }

        /// <summary>Gets or sets the trigger edge type.</summary>
        public EdgeType TrigEdge { get; set; } = EdgeType.Cross;

        /// <summary>Gets or sets the trigger edge number (1-based).</summary>
        public int TrigEdgeNumber { get; set; } = 1;

        /// <summary>Gets or sets the trigger time delay offset.</summary>
        public double? TrigTd { get; set; }

        /// <summary>Gets or sets the target signal parameter.</summary>
        public Parameter TargSignal { get; set; }

        /// <summary>Gets or sets the target threshold value.</summary>
        public double TargVal { get; set; }

        /// <summary>Gets or sets the target edge type.</summary>
        public EdgeType TargEdge { get; set; } = EdgeType.Cross;

        /// <summary>Gets or sets the target edge number (1-based).</summary>
        public int TargEdgeNumber { get; set; } = 1;

        /// <summary>Gets or sets the target time delay offset.</summary>
        public double? TargTd { get; set; }

        // --- WHEN properties ---

        /// <summary>Gets or sets the WHEN signal parameter.</summary>
        public Parameter WhenSignal { get; set; }

        /// <summary>Gets or sets the WHEN threshold value.</summary>
        public double WhenVal { get; set; }

        /// <summary>Gets or sets the WHEN edge type.</summary>
        public EdgeType WhenEdge { get; set; } = EdgeType.Cross;

        /// <summary>Gets or sets the WHEN edge number (1-based).</summary>
        public int WhenEdgeNumber { get; set; } = 1;

        // --- FIND/WHEN properties ---

        /// <summary>Gets or sets the FIND signal parameter (signal to evaluate at the WHEN crossing point).</summary>
        public Parameter FindSignal { get; set; }

        // --- Windowing ---

        /// <summary>Gets or sets the FROM bound of the measurement window (inclusive).</summary>
        public double? From { get; set; }

        /// <summary>Gets or sets the TO bound of the measurement window (inclusive).</summary>
        public double? To { get; set; }

        // --- DERIV ---

        /// <summary>Gets or sets the AT point for DERIV measurements.</summary>
        public double? At { get; set; }

        // --- Statistical measurement signal ---

        /// <summary>Gets or sets the signal parameter for MIN/MAX/AVG/RMS/PP/INTEG/DERIV measurements.</summary>
        public Parameter Signal { get; set; }

        // --- PARAM ---

        /// <summary>Gets or sets the expression string for PARAM measurements.</summary>
        public string ParamExpression { get; set; }
    }
}

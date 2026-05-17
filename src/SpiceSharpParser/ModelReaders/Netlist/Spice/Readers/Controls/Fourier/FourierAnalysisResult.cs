using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier
{
    /// <summary>
    /// Result of one signal analyzed by a .FOUR statement for one transient simulation.
    /// </summary>
    public class FourierAnalysisResult
    {
        /// <summary>
        /// Gets or sets the analyzed signal name, for example V(OUT).
        /// </summary>
        public string SignalName { get; set; }

        /// <summary>
        /// Gets or sets the simulation name that produced this result.
        /// </summary>
        public string SimulationName { get; set; }

        /// <summary>
        /// Gets or sets the requested fundamental frequency in hertz.
        /// </summary>
        public double FundamentalFrequency { get; set; }

        /// <summary>
        /// Gets or sets the total harmonic distortion percentage.
        /// </summary>
        public double TotalHarmonicDistortionPercent { get; set; }

        /// <summary>
        /// Gets the DC and harmonic rows.
        /// </summary>
        public List<FourierHarmonic> Harmonics { get; } = new List<FourierHarmonic>();

        /// <summary>
        /// Gets or sets a value indicating whether the analysis completed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the failure reason when <see cref="Success"/> is false.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

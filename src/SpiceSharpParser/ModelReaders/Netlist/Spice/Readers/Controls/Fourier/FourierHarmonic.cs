namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier
{
    /// <summary>
    /// A single DC or harmonic row produced by a .FOUR analysis.
    /// </summary>
    public class FourierHarmonic
    {
        /// <summary>
        /// Gets or sets the harmonic number. Zero is the DC component.
        /// </summary>
        public int HarmonicNumber { get; set; }

        /// <summary>
        /// Gets or sets the harmonic frequency in hertz.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Gets or sets the harmonic magnitude.
        /// </summary>
        public double Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the cosine-referenced phase in degrees.
        /// </summary>
        public double PhaseDegrees { get; set; }

        /// <summary>
        /// Gets or sets the magnitude normalized to harmonic 1.
        /// </summary>
        public double NormalizedMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the normalized magnitude in decibels.
        /// </summary>
        public double NormalizedMagnitudeDecibels { get; set; }
    }
}

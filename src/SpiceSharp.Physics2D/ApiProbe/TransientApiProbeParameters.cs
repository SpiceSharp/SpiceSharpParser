using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.ApiProbe
{
    /// <summary>
    /// Parameters for a <see cref="TransientApiProbe"/>.
    /// </summary>
    public sealed class TransientApiProbeParameters : ParameterSet<TransientApiProbeParameters>
    {
        /// <summary>
        /// Gets or sets the initial value of state A.
        /// </summary>
        [ParameterName("initiala"), ParameterInfo("Initial value of state A")]
        public double InitialA { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the initial value of state B.
        /// </summary>
        [ParameterName("initialb"), ParameterInfo("Initial value of state B")]
        public double InitialB { get; set; }

        /// <summary>
        /// Gets or sets the optional name of another probe to resolve during behavior construction.
        /// </summary>
        [ParameterName("linkedprobe"), ParameterInfo("Optional linked probe name")]
        public string LinkedProbeName { get; set; }
    }
}

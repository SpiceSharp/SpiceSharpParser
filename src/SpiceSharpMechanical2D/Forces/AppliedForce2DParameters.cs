using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="AppliedForce2D"/>.
    /// </summary>
    public sealed class AppliedForce2DParameters : ParameterSet<AppliedForce2DParameters>
    {
        /// <summary>Gets or sets the constant world x-force.</summary>
        [ParameterName("forcex"), ParameterName("fx"), ParameterInfo("World x-force")]
        public double ForceX { get; set; }

        /// <summary>Gets or sets the constant world y-force.</summary>
        [ParameterName("forcey"), ParameterName("fy"), ParameterInfo("World y-force")]
        public double ForceY { get; set; }

        /// <summary>
        /// Gets or sets the optional deterministic transient force function.
        /// </summary>
        public ForceFunction2D ForceFunction { get; set; }
    }
}

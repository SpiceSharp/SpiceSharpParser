using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="AppliedTorque2D"/>.
    /// </summary>
    public sealed class AppliedTorque2DParameters : ParameterSet<AppliedTorque2DParameters>
    {
        /// <summary>Gets or sets the world torque.</summary>
        [ParameterName("torque"), ParameterName("tau"), ParameterInfo("World torque")]
        public double Torque { get; set; }
    }
}

using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="AngularDrag2D"/>.
    /// </summary>
    public sealed class AngularDrag2DParameters : ParameterSet<AngularDrag2DParameters>
    {
        /// <summary>Gets or sets the angular damping coefficient.</summary>
        [ParameterName("damping"), ParameterName("c"), ParameterInfo("Angular damping coefficient")]
        public double Damping { get; set; }

        /// <summary>Gets or sets the angular velocity of the surrounding medium.</summary>
        [ParameterName("mediumangularvelocity"), ParameterName("mediumomega"), ParameterInfo("Medium angular velocity")]
        public double MediumAngularVelocity { get; set; }
    }
}

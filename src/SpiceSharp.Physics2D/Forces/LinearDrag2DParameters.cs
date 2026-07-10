using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="LinearDrag2D"/>.
    /// </summary>
    public sealed class LinearDrag2DParameters : ParameterSet<LinearDrag2DParameters>
    {
        /// <summary>Gets or sets the linear damping coefficient.</summary>
        [ParameterName("damping"), ParameterName("c"), ParameterInfo("Linear damping coefficient")]
        public double Damping { get; set; }

        /// <summary>Gets or sets the world x-velocity of the surrounding medium.</summary>
        [ParameterName("mediumvelocityx"), ParameterName("mediumvx"), ParameterInfo("Medium x-velocity")]
        public double MediumVelocityX { get; set; }

        /// <summary>Gets or sets the world y-velocity of the surrounding medium.</summary>
        [ParameterName("mediumvelocityy"), ParameterName("mediumvy"), ParameterInfo("Medium y-velocity")]
        public double MediumVelocityY { get; set; }
    }
}

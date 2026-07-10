using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="Gravity2D"/>.
    /// </summary>
    public sealed class Gravity2DParameters : ParameterSet<Gravity2DParameters>
    {
        /// <summary>Gets or sets the world x-acceleration.</summary>
        [ParameterName("accelerationx"), ParameterName("gx"), ParameterInfo("World x-acceleration")]
        public double AccelerationX { get; set; }

        /// <summary>Gets or sets the world y-acceleration.</summary>
        [ParameterName("accelerationy"), ParameterName("gy"), ParameterInfo("World y-acceleration")]
        public double AccelerationY { get; set; } = -9.80665;
    }
}

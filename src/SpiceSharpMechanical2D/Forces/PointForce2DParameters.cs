using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Parameters for <see cref="PointForce2D"/>.
    /// </summary>
    public sealed class PointForce2DParameters : ParameterSet<PointForce2DParameters>
    {
        /// <summary>Gets or sets the body-local application-point x-coordinate.</summary>
        [ParameterName("localpointx"), ParameterName("px"), ParameterInfo("Local point x-coordinate")]
        public double LocalPointX { get; set; }

        /// <summary>Gets or sets the body-local application-point y-coordinate.</summary>
        [ParameterName("localpointy"), ParameterName("py"), ParameterInfo("Local point y-coordinate")]
        public double LocalPointY { get; set; }

        /// <summary>Gets or sets the configured force x-component.</summary>
        [ParameterName("forcex"), ParameterName("fx"), ParameterInfo("Configured force x-component")]
        public double ForceX { get; set; }

        /// <summary>Gets or sets the configured force y-component.</summary>
        [ParameterName("forcey"), ParameterName("fy"), ParameterInfo("Configured force y-component")]
        public double ForceY { get; set; }

        /// <summary>Gets or sets the coordinate system of the configured force.</summary>
        public ForceCoordinateSystem2D ForceCoordinates { get; set; } =
            ForceCoordinateSystem2D.World;
    }
}

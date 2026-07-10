using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using SpiceSharpMechanical2D.Core;

namespace SpiceSharpMechanical2D.Bodies
{
    /// <summary>
    /// Parameters for a <see cref="RigidBody2D"/>.
    /// </summary>
    public sealed class RigidBody2DParameters : ParameterSet<RigidBody2DParameters>
    {
        /// <summary>
        /// Gets or sets the translational mass.
        /// </summary>
        [ParameterName("mass"), ParameterInfo("Translational mass")]
        public double Mass { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the rotational moment of inertia about the center of mass.
        /// </summary>
        [ParameterName("inertia"), ParameterInfo("Rotational inertia")]
        public double Inertia { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the initial world x-coordinate of the center of mass.
        /// </summary>
        [ParameterName("initialpositionx"), ParameterName("x0"), ParameterInfo("Initial x-position")]
        public double InitialPositionX { get; set; }

        /// <summary>
        /// Gets or sets the initial world y-coordinate of the center of mass.
        /// </summary>
        [ParameterName("initialpositiony"), ParameterName("y0"), ParameterInfo("Initial y-position")]
        public double InitialPositionY { get; set; }

        /// <summary>
        /// Gets or sets the initial unbounded counterclockwise angle in radians.
        /// </summary>
        [ParameterName("initialangle"), ParameterName("angle0"), ParameterInfo("Initial angle")]
        public double InitialAngle { get; set; }

        /// <summary>
        /// Gets or sets the initial world x-velocity of the center of mass.
        /// </summary>
        [ParameterName("initialvelocityx"), ParameterName("vx0"), ParameterInfo("Initial x-velocity")]
        public double InitialVelocityX { get; set; }

        /// <summary>
        /// Gets or sets the initial world y-velocity of the center of mass.
        /// </summary>
        [ParameterName("initialvelocityy"), ParameterName("vy0"), ParameterInfo("Initial y-velocity")]
        public double InitialVelocityY { get; set; }

        /// <summary>
        /// Gets or sets the initial counterclockwise angular velocity.
        /// </summary>
        [ParameterName("initialangularvelocity"), ParameterName("omega0"), ParameterInfo("Initial angular velocity")]
        public double InitialAngularVelocity { get; set; }

        /// <summary>
        /// Gets or sets the initial-condition policy.
        /// </summary>
        public MechanicalInitialConditionMode InitialConditionMode { get; set; } =
            MechanicalInitialConditionMode.HoldSpecifiedStateDuringOperatingPoint;
    }
}

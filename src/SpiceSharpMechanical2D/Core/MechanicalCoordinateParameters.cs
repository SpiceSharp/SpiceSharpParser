using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SpiceSharpMechanical2D.Core
{
    /// <summary>
    /// Parameters for a <see cref="MechanicalCoordinate"/>.
    /// </summary>
    public sealed class MechanicalCoordinateParameters : ParameterSet<MechanicalCoordinateParameters>
    {
        /// <summary>
        /// Gets or sets the generalized mass or rotational inertia.
        /// </summary>
        [ParameterName("mass"), ParameterName("generalizedmass"), ParameterInfo("Generalized mass")]
        public double GeneralizedMass { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the requested initial generalized position.
        /// </summary>
        [ParameterName("initialposition"), ParameterName("q0"), ParameterInfo("Initial generalized position")]
        public double InitialPosition { get; set; }

        /// <summary>
        /// Gets or sets the requested initial generalized velocity.
        /// </summary>
        [ParameterName("initialvelocity"), ParameterName("u0"), ParameterInfo("Initial generalized velocity")]
        public double InitialVelocity { get; set; }

        /// <summary>
        /// Gets or sets the initial-condition policy.
        /// </summary>
        public MechanicalInitialConditionMode InitialConditionMode { get; set; } =
            MechanicalInitialConditionMode.HoldSpecifiedStateDuringOperatingPoint;
    }
}

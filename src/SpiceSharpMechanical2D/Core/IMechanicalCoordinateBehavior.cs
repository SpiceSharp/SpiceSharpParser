using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharpMechanical2D.Core
{
    /// <summary>
    /// Exposes the solver state and diagnostics of one generalized coordinate.
    /// </summary>
    /// <remarks>
    /// Connected components use the two variables to cache their own matrix
    /// locations during behavior construction. Mutable solver storage is not
    /// exposed.
    /// </remarks>
    public interface IMechanicalCoordinateBehavior : IBehavior
    {
        /// <summary>
        /// Gets the generalized-position solver variable.
        /// </summary>
        IVariable<double> PositionVariable { get; }

        /// <summary>
        /// Gets the generalized-velocity solver variable. Its solver row is the
        /// generalized dynamics equation.
        /// </summary>
        IVariable<double> VelocityVariable { get; }

        /// <summary>
        /// Gets the current generalized position.
        /// </summary>
        double Position { get; }

        /// <summary>
        /// Gets the current generalized velocity.
        /// </summary>
        double Velocity { get; }

        /// <summary>
        /// Gets the generalized mass or rotational inertia.
        /// </summary>
        double GeneralizedMass { get; }

        /// <summary>
        /// Gets the requested initial generalized position.
        /// </summary>
        double InitialPosition { get; }

        /// <summary>
        /// Gets the requested initial generalized velocity.
        /// </summary>
        double InitialVelocity { get; }

        /// <summary>
        /// Gets the instantaneous kinetic energy.
        /// </summary>
        double KineticEnergy { get; }
    }
}

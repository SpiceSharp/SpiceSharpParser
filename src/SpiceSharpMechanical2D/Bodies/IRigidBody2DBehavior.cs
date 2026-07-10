using SpiceSharp.Behaviors;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;

namespace SpiceSharpMechanical2D.Bodies
{
    /// <summary>
    /// Exposes the solver state and kinematics of one planar rigid body.
    /// </summary>
    /// <remarks>
    /// Connected components map the six variables during behavior construction
    /// and own their force, torque, and Jacobian stamps. Mutable solver storage
    /// is not exposed.
    /// </remarks>
    public interface IRigidBody2DBehavior : IBehavior
    {
        /// <summary>Gets the world x-position solver variable.</summary>
        IVariable<double> PositionXVariable { get; }

        /// <summary>Gets the world y-position solver variable.</summary>
        IVariable<double> PositionYVariable { get; }

        /// <summary>Gets the unbounded angle solver variable.</summary>
        IVariable<double> AngleVariable { get; }

        /// <summary>Gets the world x-velocity solver variable and dynamics row.</summary>
        IVariable<double> VelocityXVariable { get; }

        /// <summary>Gets the world y-velocity solver variable and dynamics row.</summary>
        IVariable<double> VelocityYVariable { get; }

        /// <summary>Gets the angular-velocity solver variable and dynamics row.</summary>
        IVariable<double> AngularVelocityVariable { get; }

        /// <summary>Gets the current world x-position.</summary>
        double PositionX { get; }

        /// <summary>Gets the current world y-position.</summary>
        double PositionY { get; }

        /// <summary>Gets the current unbounded counterclockwise angle.</summary>
        double Angle { get; }

        /// <summary>Gets the current world x-velocity.</summary>
        double VelocityX { get; }

        /// <summary>Gets the current world y-velocity.</summary>
        double VelocityY { get; }

        /// <summary>Gets the current counterclockwise angular velocity.</summary>
        double AngularVelocity { get; }

        /// <summary>Gets the current world position.</summary>
        Vector2D Position { get; }

        /// <summary>Gets the current world linear velocity.</summary>
        Vector2D LinearVelocity { get; }

        /// <summary>Gets the translational mass.</summary>
        double Mass { get; }

        /// <summary>Gets the rotational moment of inertia.</summary>
        double Inertia { get; }

        /// <summary>Gets the translational kinetic energy.</summary>
        double LinearKineticEnergy { get; }

        /// <summary>Gets the rotational kinetic energy.</summary>
        double AngularKineticEnergy { get; }

        /// <summary>Gets the total kinetic energy.</summary>
        double KineticEnergy { get; }

        /// <summary>Transforms a body-local point to world coordinates.</summary>
        Vector2D LocalPointToWorld(Vector2D localPoint);

        /// <summary>Transforms a body-local vector to world coordinates.</summary>
        Vector2D LocalVectorToWorld(Vector2D localVector);

        /// <summary>Transforms a world point to body-local coordinates.</summary>
        Vector2D WorldPointToLocal(Vector2D worldPoint);

        /// <summary>Transforms a world vector to body-local coordinates.</summary>
        Vector2D WorldVectorToLocal(Vector2D worldVector);

        /// <summary>Gets the world velocity of a body-local point.</summary>
        Vector2D GetPointVelocity(Vector2D localPoint);

        /// <summary>
        /// Computes the counterclockwise torque about the center of mass from a
        /// world force applied at a body-local point.
        /// </summary>
        double ComputeTorque(Vector2D localPoint, Vector2D worldForce);
    }
}

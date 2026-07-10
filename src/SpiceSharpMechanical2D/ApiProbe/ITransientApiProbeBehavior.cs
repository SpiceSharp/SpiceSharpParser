using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharpMechanical2D.ApiProbe
{
    /// <summary>
    /// Exposes the solver state owned by a <see cref="TransientApiProbe"/> behavior.
    /// </summary>
    public interface ITransientApiProbeBehavior : IBehavior
    {
        /// <summary>
        /// Gets the private solver variable for state A.
        /// </summary>
        IVariable<double> AVariable { get; }

        /// <summary>
        /// Gets the private solver variable for state B.
        /// </summary>
        IVariable<double> BVariable { get; }

        /// <summary>
        /// Gets the current value of state A.
        /// </summary>
        double A { get; }

        /// <summary>
        /// Gets the current value of state B.
        /// </summary>
        double B { get; }

        /// <summary>
        /// Gets the optional behavior resolved during setup.
        /// </summary>
        ITransientApiProbeBehavior LinkedBehavior { get; }
    }
}

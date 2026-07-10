namespace SpiceSharp.Physics2D.Core
{
    /// <summary>
    /// Defines how a mechanical coordinate establishes its initial state.
    /// </summary>
    public enum MechanicalInitialConditionMode
    {
        /// <summary>
        /// Hold the requested position and velocity while SpiceSharp solves the
        /// transient operating point, then initialize integration history from
        /// that solved state.
        /// </summary>
        HoldSpecifiedStateDuringOperatingPoint,
    }
}

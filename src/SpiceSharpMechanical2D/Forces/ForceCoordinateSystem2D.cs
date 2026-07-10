namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Specifies the frame in which a point-force vector is defined.
    /// </summary>
    public enum ForceCoordinateSystem2D
    {
        /// <summary>
        /// The force vector is fixed in the world frame.
        /// </summary>
        World,

        /// <summary>
        /// The force vector is fixed in the body's local frame and rotates with it.
        /// </summary>
        BodyLocal,
    }
}

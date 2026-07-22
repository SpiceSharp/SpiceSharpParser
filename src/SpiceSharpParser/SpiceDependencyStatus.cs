namespace SpiceSharpParser
{
    /// <summary>
    /// Describes the outcome of resolving an external SPICE dependency.
    /// </summary>
    public enum SpiceDependencyStatus
    {
        /// <summary>
        /// The dependency was found and read.
        /// </summary>
        Resolved,

        /// <summary>
        /// The resolved dependency path did not exist.
        /// </summary>
        NotFound,

        /// <summary>
        /// The dependency existed but could not be read.
        /// </summary>
        Unreadable,
    }
}

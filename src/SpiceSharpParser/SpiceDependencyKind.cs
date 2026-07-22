namespace SpiceSharpParser
{
    /// <summary>
    /// Identifies the directive that introduced an external SPICE dependency.
    /// </summary>
    public enum SpiceDependencyKind
    {
        /// <summary>
        /// An .INCLUDE or .INC directive.
        /// </summary>
        Include,

        /// <summary>
        /// A file-backed .LIB directive.
        /// </summary>
        Library,
    }
}

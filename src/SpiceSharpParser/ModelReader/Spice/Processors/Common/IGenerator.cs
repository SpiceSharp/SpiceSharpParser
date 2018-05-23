namespace SpiceSharpParser.ModelReader.Spice.Processors.Common
{
    /// <summary>
    /// Generator for Spice element
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        string TypeName { get; }
    }
}

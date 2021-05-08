namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names
{
    /// <summary>
    /// Interface for all object name generators.
    /// </summary>
    public interface IObjectNameGenerator
    {
        string Separator { get; }

        /// <summary>
        /// Generates entity object name.
        /// </summary>
        /// <param name="entityName">Name of entity.</param>
        /// <returns>
        /// A object name for entity.
        /// </returns>
        string Generate(string entityName);

        /// <summary>
        /// Creates a new child object name generator.
        /// </summary>
        /// <param name="childGeneratorName">Name of generator.</param>
        /// <returns>
        /// A new object name generator.
        /// </returns>
        IObjectNameGenerator CreateChildGenerator(string childGeneratorName);
    }
}
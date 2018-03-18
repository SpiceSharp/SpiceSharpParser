namespace SpiceNetlist.SpiceSharpConnector.Context
{
    /// <summary>
    /// Interface for all node name generators
    /// </summary>
    public interface INodeNameGenerator
    {
        /// <summary>
        /// Generates a node name for circuit
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// A node name
        /// </returns>
        string Generate(string pinName);
    }
}

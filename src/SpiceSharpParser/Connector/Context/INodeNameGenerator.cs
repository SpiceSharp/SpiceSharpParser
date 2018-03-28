using System.Collections.Generic;

namespace SpiceSharpParser.Connector.Context
{
    /// <summary>
    /// Interface for all node name generators
    /// </summary>
    public interface INodeNameGenerator
    {
        /// <summary>
        /// Gets the globals
        /// </summary>
        IEnumerable<string> Globals { get; }

        /// <summary>
        /// Generates a node name for circuit
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// A node name
        /// </returns>
        string Generate(string pinName);

        /// <summary>
        /// Makes a pin name a global pin name
        /// </summary>
        /// <param name="pinName">Pin name</param>
        void SetGlobal(string pinName);
    }
}

using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// Interface for all node name generators
    /// </summary>
    public interface INodeNameGenerator
    {
        /// <summary>
        /// Gets the root name
        /// </summary>
        string RootName { get; }

        /// <summary>
        /// Gets the globals
        /// </summary>
        IEnumerable<string> Globals { get; }

        /// <summary>
        /// Gets children of node name generator
        /// </summary>
        List<INodeNameGenerator> Children { get; }

        /// <summary>
        /// Generates a node name for circuit
        /// </summary>
        /// <param name="pinName">Pin name</param>
        /// <returns>
        /// A node name
        /// </returns>
        string Generate(string pinName);

        /// <summary>
        /// Parses a path and generate a node name
        /// </summary>
        /// <param name="path">Node path</param>
        /// <returns>
        /// A node name
        /// </returns>
        string Parse(string path);

        /// <summary>
        /// Makes a pin name a global pin name
        /// </summary>
        /// <param name="pinName">Pin name</param>
        void SetGlobal(string pinName);
    }
}

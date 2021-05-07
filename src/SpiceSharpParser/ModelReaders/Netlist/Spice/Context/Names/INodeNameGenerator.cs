using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names
{
    /// <summary>
    /// Interface for all node name generators.
    /// </summary>
    public interface INodeNameGenerator
    {
        /// <summary>
        /// Gets the root name.
        /// </summary>
        string RootName { get; }

        string Separator { get; }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        IEnumerable<string> Globals { get; }

        /// <summary>
        /// Gets children of node name generator.
        /// </summary>
        List<INodeNameGenerator> Children { get; }

        /// <summary>
        /// Generates a node id for circuit.
        /// </summary>
        /// <param name="nodeName">Node name.</param>
        /// <returns>
        /// A node identifier.
        /// </returns>
        string Generate(string nodeName);

        /// <summary>
        /// Parses a path and generate a node id.
        /// </summary>
        /// <param name="path">Node path.</param>
        /// <returns>
        /// A node name.
        /// </returns>
        string Parse(string path);

        /// <summary>
        /// Makes a pin name a global pin name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        void SetGlobal(string pinName);
    }
}
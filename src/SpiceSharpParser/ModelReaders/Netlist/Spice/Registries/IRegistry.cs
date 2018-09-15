using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Interface for all registries.
    /// </summary>
    public interface IRegistry<TElement> : IEnumerable<TElement>
    {
        /// <summary>
        /// Gets a value indicating whether a specified control is in registry
        /// </summary>
        /// <param name="spiceName">Type of control</param>
        /// <returns>
        /// A value indicating whether a specified control is in registry
        /// </returns>
        bool Supports(string spiceName);

        TElement Get(string spiceName);

        void Bind(string spiceName, TElement element);

        void Bind(string[] spiceNames, TElement element);
    }
}

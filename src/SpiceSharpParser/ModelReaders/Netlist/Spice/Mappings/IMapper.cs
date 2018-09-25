using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Interface for all mappers that have a string key and <typeparamref name="TElement"/> as element.
    /// </summary>
    public interface IMapper<TElement> : IEnumerable<TElement>
    {
        /// <summary>
        /// Gets a value indicating whether a element with given key is in the mapper.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>
        /// A value indicating whether a element with specified key is in mapper.
        /// </returns>
        bool Contains(string key);

        /// <summary>
        /// Gets an element with given key.
        /// </summary>
        /// <param name="key">A key of the element.</param>
        /// <returns>
        /// The element or exception.
        /// </returns>
        TElement Get(string key);

        /// <summary>
        /// Maps the key with given element.
        /// </summary>
        /// <param name="key">A key of the element.</param>
        /// <param name="element">An element.</param>
        void Map(string key, TElement element);
        
        /// <summary>
        /// Maps the keys with given element.
        /// </summary>
        /// <param name="key">Keys of the element.</param>
        /// <param name="element">An element.</param>
        void Map(string[] key, TElement element);
    }
}

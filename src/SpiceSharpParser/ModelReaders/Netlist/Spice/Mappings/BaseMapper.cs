using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Mapper with base functionalities.
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of the element.
    /// </typeparam>
    public class BaseMapper<TElement> : IEnumerable<TElement>, IMapper<TElement>
        where TElement : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMapper{TElement}"/> class.
        /// </summary>
        public BaseMapper()
        {
        }

        /// <summary>
        /// Gets the count of elements in registry.
        /// </summary>
        public int Count
        {
            get
            {
                return Elements.Keys.Count;
            }
        }

        /// <summary>
        /// Gets the mapping type to element.
        /// </summary>
        protected Dictionary<string, TElement> Elements { get; } = new Dictionary<string, TElement>();

        /// <summary>
        /// Binds key with element.
        /// </summary>
        /// <param name="key">Key of the element.</param>
        /// <param name="element">Element to add.</param>
        public virtual void Map(string key, TElement element)
        {
            Elements[key] = element;
        }

        /// <summary>
        /// Binds the element to the mapper.
        /// </summary>
        /// <param name="element">Element to add</param>
        public virtual void Map(string[] keys, TElement element)
        {
            foreach (var key in keys)
            {
                Elements[key] = element;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the mapper has a element for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// A key.
        /// </param>
        /// <returns>
        /// A value indicating whether the mapper has a element with given <paramref name="key"/>.
        /// </returns>
        public bool Contains(string key)
        {
            return Elements.ContainsKey(key);
        }

        /// <summary>
        /// Gets the element for given type.
        /// </summary>
        /// <param name="type">A type of element.</param>
        /// <returns>
        /// A reference to the element.
        /// </returns>
        public TElement Get(string type)
        {
            return Elements[type];
        }

        /// <summary>
        /// Gets the typed enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// The enumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
    }
}

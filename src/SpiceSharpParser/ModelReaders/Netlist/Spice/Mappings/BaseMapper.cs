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
    public class BaseMapper<TElement> :  IMapper<TElement>
        where TElement : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMapper{TElement}"/> class.
        /// </summary>
        public BaseMapper()
        {
        }

        /// <summary>
        /// Gets the count of elements in mapper.
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

        public IEnumerator<KeyValuePair<string, TElement>> GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }
    }
}
